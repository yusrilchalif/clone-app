using UnityEngine;
using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Firestore;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance {get; private set;}

    private FirebaseFirestore firestoreDb;

    public static string storageLink = "https://console.firebase.google.com/project/mandiriproject-94c0c/database/mandiriproject-94c0c-default-rtdb/data/~2F";

    private void Awake() {
        
        if(Instance != this && Instance != null)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);

        firestoreDb = FirebaseFirestore.DefaultInstance;
    }

    public int GetCoinAmount() {

        int coinAmount = 0;

        CollectionReference coinRef = firestoreDb.Collection("coins");
        coinRef.GetSnapshotAsync().ContinueWithOnMainThread( task => {
            QuerySnapshot snapshots = task.Result;

            foreach (DocumentSnapshot document in snapshots.Documents)
            {
                Debug.Log(string.Format("Coins in : {0}", document.Id));
            }

        });

        return coinAmount;
    }

}