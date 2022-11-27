using System;

[Serializable]
public class UserData
{
    public string userID;
    public string userName;
    public int coinAmount;

    public UserData(string userID, string username, int coinAmt) {
        this.userID = userID;
        this.userName = username;
        this.coinAmount = coinAmt;
    }
}