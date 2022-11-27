using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Database;
using TMPro;
using Proyecto26;
using RSG;
using System;
using Newtonsoft.Json;
using System.Linq;
using ARLocation;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Header("Firestore")]
    private FirebaseFirestore firestore;
    private DocumentReference coinDbAmount;
    public static string collectionID = "coins";
    public static string documentID = "coinsID";

    [Header("Firebase Realtime Database")]
    [SerializeField, Tooltip("Always makes sure this is filled!")] private SO_CoinDatabase coinDatabase;
    [SerializeField] private List<PlaceAtLocation> coinObjectsBasedFromEnum;
    private static readonly string databaseURL = $"https://mandiriproject-94c0c-default-rtdb.asia-southeast1.firebasedatabase.app/";
    public DatabaseReference coinDBRef;
    private UserData currentUserData;
    private int coinLeft = 0;
    private List<Coin> coinDBList = new List<Coin>();

    private void Awake()
    {
        if (Instance != this && Instance != null)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitFirebase();
            }
            else
            {
                Debug.LogError("Firebase not resolve" + dependencyStatus);
            }
        });
    }

    private void Start()
    {
        //Start pulling coin database from firebase
        GetCoinListFromDB((coinList) => {
            Debug.Log($"Coin DB instances pulled from firebase! with result : {coinList}");

            if (coinDBList == null || coinDBList.Count == 0) {
                CreateCoinDatabase(() => {
                    Debug.Log("Coin DB created based on scriptable object!");
                });
            }
        });
    }

    void InitFirebase()
    {
        //Base Firebase API
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        //Init Firebase Auth
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        //Init Firebase Firestore
        firestore = FirebaseFirestore.DefaultInstance;
        coinDbAmount = firestore.Collection(collectionID).Document(documentID);
        Debug.Log($"Trying to get coin amount reference : {coinDbAmount}");
        /*
        var coindbRef = FirebaseDatabase.DefaultInstance.GetReference("coins").GetValueAsync().ContinueWithOnMainThread( task => { 
            if(task.IsFaulted)
                Debug.Log($"Trying to get coin db reference : {task.Result}, but failed");

            Debug.Log($"Trying to get coin db reference : {task.Result}");
        });*/
        coinDBRef = FirebaseDatabase.DefaultInstance.GetReference("coins");
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    public void Login(string email, string password, TextMeshProUGUI errorText) {
        StartCoroutine(LoginAsync(email, password, errorText));
    }

    private IEnumerator LoginAsync(string email, string password, TextMeshProUGUI errorText)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);
        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;
            string failedMessage = "Login gagal : ";
            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email tidak ditemukan!";
                    errorText.gameObject.SetActive(true);
                    errorText.text = failedMessage.ToString();
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Password Salah!";
                    errorText.gameObject.SetActive(true);
                    errorText.text = failedMessage.ToString();
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email kosong!";
                    errorText.gameObject.SetActive(true);
                    errorText.text = failedMessage.ToString();
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password kosong!";
                    errorText.gameObject.SetActive(true);
                    errorText.text = failedMessage.ToString();
                    break;
                default:
                    failedMessage = "Akun tidak ditemukan!";
                    errorText.gameObject.SetActive(true);
                    errorText.text = failedMessage.ToString();
                    break;
            }
            Debug.Log(failedMessage);
        }
        else
        {
            user = loginTask.Result;
            Debug.LogFormat($"You Are Successfully Logged In {0}", user.Email);
            //References.userName = user.DisplayName;
            SceneManager.LoadScene("Home 1");
        }
    }

    public delegate void PostCoinCallback(int coinAmt);

    public void GetCoinFromDatabase(PostCoinCallback callback) {

        /*
        //Old Function
        coinDbAmount.Listen( snapshot => {
            snapshot.TryGetValue<int>("coinsAmount", out coinLeft);
            callback();
        });
        */

        /*
        coinDbAmount.GetSnapshotAsync().ContinueWithOnMainThread( task => {
            DocumentSnapshot snapshot = task.Result;
            int coinLeft = 0;

            if (!snapshot.Exists) {
                Debug.Log($"Document {snapshot.Id} doesnt exist!");
                return;
            }

            Dictionary<string, object> coinData = snapshot.ToDictionary();
            foreach (var item in coinData)
            {
                Debug.Log($"{item.Key} contains: {item.Value}");
                coinLeft = (int)item.Value;

                callback(coinLeft);
                Debug.Log($"Coin left : {coinLeft}");
            }
        });*/

        WaitForDBInfo().Then(() => {
            callback(coinLeft);
        });
    }

    public void SetCoinInDB(int coinTotal) {
        if (coinTotal <= 0)
            coinTotal = 0;

        Dictionary<string, object> coinLeft = new Dictionary<string, object> {
            {"coinsAmount", coinTotal}
        };

        coinDbAmount.SetAsync(coinLeft).ContinueWithOnMainThread(task => {
            Debug.Log($"Coin data overwritten! Coin left should be : {coinTotal}");
        });
    }

    public void SetUserData(UserData newUser) {
        currentUserData = newUser;
    }

    public UserData GetCurrentUser() {
        return currentUserData;
    }

    public delegate void PostUserCallback();
    public delegate void GetUserCallbackSuccesful(UserData user);
    public delegate void GetUserCallbackFailed();

    public void PostUser(UserData user, string userID, PostUserCallback callback) {
        Debug.Log($"Posting to DB to : {databaseURL} with userID : {userID} ");
        RestClient.Put<UserData>($"{databaseURL}users/{userID}.json", user).Then(response => {
            callback();
        });
    }

    public void GetUser(string userID, GetUserCallbackSuccesful success, GetUserCallbackFailed failed)
    {
        Debug.Log($"Getting user from DB : {databaseURL} with userID : {userID} ");
        RestClient.Get<UserData>($"{databaseURL}users/{userID}.json").Done(
            response => {
                Debug.Log("Succesful");
                currentUserData = response;
                success(response);
            },
            (response2) => {
                Debug.Log("rejected");
                string tempUsername = userID;
                char splittedWords = '@';
                string[] username = tempUsername.Split(splittedWords);
                tempUsername = username[0];
                UserData newUser = new UserData(userID, tempUsername, 0);
                currentUserData = newUser;
                PostUser(newUser, newUser.userID, () => { failed(); });
            });
    }

    public IPromise WaitForDBInfo() {
        var promise = new Promise();
        coinDbAmount.GetSnapshotAsync().ContinueWith(
            task => {
                DocumentSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.Log($"Document {snapshot.Id} doesnt exist!");
                    return;
                }

                Dictionary<string, object> coinData = snapshot.ToDictionary();
                foreach (var item in coinData)
                {
                    Debug.Log($"{item.Key} contains: {item.Value}");
                    coinLeft = Convert.ToInt32(item.Value);
                    Debug.Log($" Coin in local : {coinLeft}");
                }

                if (task.IsFaulted || task.IsCanceled)
                {
                    promise.Reject(new Exception("failed"));
                    return;
                }
                //initalze data
                promise.Resolve();
            });

        return promise;
    }

    public delegate void PostCoinDBCallback();
    public delegate void GetCoinDbCallback(List<Coin> coinDB);

    public void CreateCoinDatabase(PostCoinDBCallback callback) {
        Debug.Log($"Posting coin DB to : {databaseURL} ");
        foreach (Coin item in coinDatabase.coinList)
        {
            RestClient.Put<Coin>($"{databaseURL}coins/{item.markerNameID}.json", item).Then(response => {
                callback();
            });
        }
    }

    public void UpdateCoinDatabase(Coin coinObj) {
        var coinJson = JsonUtility.ToJson(coinObj);
        coinDBRef.Child(coinObj.markerNameID).SetRawJsonValueAsync(coinJson);
        Debug.Log($"Coin with id {coinObj.markerNameID} are updated!");
    }

    public void GetCoinListFromDB(GetCoinDbCallback callback) {
        RestClient.Get($"{databaseURL}coins.json").Then(response => {
            Debug.Log("Trying to parse json to coin list...");
            Debug.Log($"Parsing {response.Text}");
            var responseJson = response.Text;
            var coinDict = JsonConvert.DeserializeObject<Dictionary<string, Coin>>(responseJson);
            coinDBList = coinDict.Values.ToList();

            Debug.Log($"Parsed with result : {coinDBList.Count}");
            callback(coinDBList);
        });
    }

    public List<Coin> GetCoinList() {
        return coinDBList;
    }

    public void SetCoinsAvailability(string coinsMarkerID, bool status) {
        var coin = coinDBList.First(item => item.markerNameID == coinsMarkerID);
        coin.isAvailable = status;
        UpdateCoinDatabase(coin);
    }

    public PlaceAtLocation GetCoinGameObject(Coin coin) {
        PlaceAtLocation item = coinObjectsBasedFromEnum[(int)coin.coinType];
        item.Location = coin.coinLocation;
        return item;
    }

    public static string RemoveSpecialChar(string input)
    {
        string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };

        for (int i = 0; i < chars.Length; i++)
        {
            if (input.Contains(chars[i]))
                input = input.Replace(chars[i], "");
        }
        return input;
    }
}
