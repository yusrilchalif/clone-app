using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Database;
using System;
using ARLocation;

public class TreasureHuntSceneManager : MonoBehaviour
{

    public static TreasureHuntSceneManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinText;
    private UserData currentUser;

    private List<Coin> coins = new List<Coin>();
    private List<GameObject> allCoins = new List<GameObject>();

    private void Awake()
    {
        if (Instance != this && Instance != null)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentUser = AuthManager.Instance.GetCurrentUser();
        UpdateCoinText(currentUser.coinAmount);

        Debug.Log(GameObject.FindGameObjectWithTag("MainCamera"));
        AuthManager.Instance.coinDBRef.ValueChanged += OnDatabaseValueChanged;
        ARLocationManager.Instance.Camera = Camera.main;
        ARLocationManager.Instance.OnARTrackingRestored( () => {
            ARLocationManager.Instance.ResetARSession();
            ARLocationManager.Instance.Restart();
        });
    }


    public void UpdateCoinText(int coinAmt) {
        coinText.text = coinAmt.ToString();
    }

    public void OnDatabaseValueChanged(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        foreach (var coin in allCoins)
        {
            Destroy(coin);
        }

        StartSpawningCoin();
    }

    public void StartSpawningCoin() {
        AuthManager.Instance.GetCoinListFromDB( (coinsUpdate)=> { });
        coins = AuthManager.Instance.GetCoinList();

        Debug.Log($"Instantiating coins with total of {coins.Count}");

        allCoins.Clear();
        allCoins = new List<GameObject>();

        foreach (var item in coins)
        {
            Debug.Log($"Instantiating coins....");
            if (!item.isAvailable)
                continue;

            var coinObj = Instantiate(AuthManager.Instance.GetCoinGameObject(item).gameObject);
            var coinref = coinObj.GetComponentInChildren<CoinScript>();
            coinref.InitCoin(OnCoinInteracted);
            allCoins.Add(coinObj.gameObject);
        }

    }

    private void OnCoinInteracted(CoinScript coins)
    {
        Debug.Log("Coin function invoked!");
        AuthManager.Instance.SetCoinsAvailability(coins.MarkerID, false);
    }

    private void OnDestroy()
    {
        foreach (var coin in allCoins)
        {
            Destroy(coin);
        }
        Instance = null;
        AuthManager.Instance.coinDBRef.ValueChanged -= OnDatabaseValueChanged;
    }

    public void ResetARSession(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        ARLocationManager.Instance.ResetARSession();
        ARLocationManager.Instance.Restart();
    }
}
