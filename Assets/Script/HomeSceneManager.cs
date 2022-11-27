using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using System.Text;

public class HomeSceneManager : MonoBehaviour
{
    public static HomeSceneManager Instance { get; private set; }

    //public TextMeshProUGUI fyiText;
    public TextMeshProUGUI userName;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI coinRedeemText;
    [SerializeField] private SO_InformationLibrary informationLibrary;

    private UserData currentUserData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Init User
        //Get User based from userID
        //TIDY THIS SOON!!!!! (SO MESSY, ADD LOADING SCREEN TO LOAD USER FIRST!)
        string userID = RemoveSpecialChar(AuthManager.Instance.user.Email);
        AuthManager.Instance.GetUser(userID, 
            (userData) => {
            Debug.Log($"User obtained! UserID : {userID}");

            //Check if current user data is already filled
            //CHANGE SOON!!! (If user is changing into another account 
            // then there's possibility of newly logged account data will be overriden)
            currentUserData = userData;

            //Update Home UI
            int indexLibrary = Random.Range(0, informationLibrary.infoLibrary.Count);
            string text = informationLibrary.infoLibrary[indexLibrary].info;
            //UpdateInfoText(text);

            UpdateUsername(userData.userName);
            UpdateUserCoin(userData.coinAmount);
            UpdateRedeemCoin(userData.coinAmount);
            },
            () => {
                Debug.Log("Created new user.");

                currentUserData = AuthManager.Instance.GetCurrentUser();

                UpdateUsername(currentUserData.userName);
                UpdateUserCoin(currentUserData.coinAmount);
                UpdateRedeemCoin(currentUserData.coinAmount);
/*
                InitNewUser();

                AuthManager.Instance.PostUser(currentUserData, currentUserData.userID, () =>
                {
                    Debug.Log("Posted, check the db"); 
                        
                    if (AuthManager.Instance.GetCurrentUser() is null)
                        AuthManager.Instance.SetUserData(currentUserData);
                    else
                        currentUserData = AuthManager.Instance.GetCurrentUser();

                    //Update Home UI
                    int indexLibrary = Random.Range(0, informationLibrary.infoLibrary.Count);
                    string text = informationLibrary.infoLibrary[indexLibrary].info;
                    //UpdateInfoText(text);

                    UpdateUsername(currentUserData.userName);
                    UpdateUserCoin(currentUserData.coinAmount);
                    UpdateRedeemCoin(currentUserData.coinAmount);
                });*/
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUser(UserData user) {
        this.currentUserData = user;
    }

    //public void UpdateInfoText(string infoText)
    //{
    //    fyiText.text = infoText;
    //}

    public void UpdateUsername(string username)
    {
        userName.text = $"Halo, {username} si Penjelajah";
    }

    public void UpdateUserCoin(int coinAmt) {
        coinText.text = coinAmt.ToString();
    }

    public void UpdateRedeemCoin(int coinAmt) {
        coinRedeemText.text = $"Jumlah koin kamu : {coinAmt}";
    }

    public void UpdateRedeemCoin(int coinAmt, string updateText) {
        coinRedeemText.text = $"{updateText} {coinAmt}";
    }

    public void InitNewUser() {
        string userID = RemoveSpecialChar(AuthManager.Instance.auth.CurrentUser.Email);
        
        // Build Only
        string tempUserName = AuthManager.Instance.auth.CurrentUser.DisplayName;

        //Demo Only
        if (string.IsNullOrEmpty(tempUserName)) {
            tempUserName = AuthManager.Instance.user.Email;

            char splittedWords = '@';
            string[] username = tempUserName.Split(splittedWords);
            tempUserName = username[0];
        }

        int coinAmt = 0;

        currentUserData = new UserData(userID, tempUserName, coinAmt);
    }

    public static string RemoveSpecialChar(string input) {
        string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };

        for (int i = 0; i < chars.Length; i++)
        {
            if (input.Contains(chars[i]))
                input = input.Replace(chars[i], "");
        }
        return input;
    }

    public static void GetUserCallback(UserData userdata) {
        HomeSceneManager.Instance.SetUser(userdata);
        Debug.Log(userdata.userName + " " + userdata.userID);
    }

}
