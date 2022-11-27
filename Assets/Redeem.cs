using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Redeem : MonoBehaviour
{
    private Button currentButton;
    private int coinAmount;

    private void Start()
    {
        currentButton = GetComponent<Button>();
    }

    public void RedeemPrize(int cost) {

        coinAmount = AuthManager.Instance.GetCurrentUser().coinAmount;

        if (coinAmount < cost)
        {
            HomeSceneManager.Instance.UpdateRedeemCoin(coinAmount, "Koin kamu tidak cukup!\nJumlah koin kamu : ");
            return;
        }

        currentButton.interactable = false;
        
        coinAmount -= cost;
        
        var currentUser = AuthManager.Instance.GetCurrentUser();
        currentUser.coinAmount = coinAmount;

        AuthManager.Instance.SetUserData(currentUser);
        AuthManager.Instance.PostUser(currentUser, currentUser.userID, () => { });

        HomeSceneManager.Instance.UpdateUserCoin(coinAmount);
        HomeSceneManager.Instance.UpdateRedeemCoin(coinAmount, "Jumlah koin kamu : ");
    }

}
