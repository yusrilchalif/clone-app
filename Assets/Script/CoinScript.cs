using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ARLocation;

public class CoinScript : MonoBehaviour
{
    [SerializeField] private int coinWorth;
    [SerializeField] private float turnRate;
    private bool isRotating = true;
    private bool isTaskDone = false;

    [SerializeField] private GameObject coinVFX;
    private Action<CoinScript> OnInteract;
    public string MarkerID => locationScript.Location.Label;

    [SerializeField] private PlaceAtLocation locationScript;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(isRotating)
            RotateCoin(turnRate);
    }

    public void InitCoin(Action<CoinScript> onInteract) {
        OnInteract = onInteract;
    }

    public void InitCoin(Action<CoinScript> onInteract, Sprite imageLogo)
    {
        OnInteract = onInteract;
    }

    public void RotateCoin(float rotateSpeed) {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnMouseDown() {
        
        //Coin Function
        OnInteract?.Invoke(this);
        AddCoin(coinWorth);
        //SFX

        //Destroy After Done
        StartCoroutine(DestroyDelay());
    }

    private IEnumerator DestroyDelay() {
        while (true)
        {
           if(isTaskDone) break;

           isTaskDone = true;

           yield return null;
        }
        //VFX
        Instantiate(coinVFX, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.1f);

        Destroy(this.gameObject);
    }

    private void AddCoin(int amount) {
        //idk, this seems so unsafe for security and privacy reason
        //just update to a better version soon
        UserData currentUser = AuthManager.Instance.GetCurrentUser();
        currentUser.coinAmount += amount;
        TreasureHuntSceneManager.Instance.UpdateCoinText(currentUser.coinAmount);

        AuthManager.Instance.GetCoinFromDatabase( (coinLeft) => {
            Debug.Log($"Coin in db before is : {coinLeft}");
            coinLeft -= amount;
            AuthManager.Instance.SetCoinInDB(coinLeft);
        });

        AuthManager.Instance.SetUserData(currentUser);
        AuthManager.Instance.PostUser(currentUser, currentUser.userID, () => {
            Debug.Log($"Coins gained! Current coins now are {currentUser.coinAmount}");
            TreasureHuntSceneManager.Instance.UpdateCoinText(currentUser.coinAmount);
        });
    }
}
