using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscountParent : MonoBehaviour
{
    [SerializeField, Tooltip("Please use the same name as the marker name")] private string markerName;
    [SerializeField] private List<DiscountObject> allDiscountObjects;

    // Start is called before the first frame update
    void Awake()
    {
        if (string.IsNullOrEmpty(markerName))
            markerName = gameObject.name;
    }

    private void OnEnable()
    {
        InitObject();
    }

    public void InitObject() {
        int randomAR = Random.Range(0, allDiscountObjects.Count);
        Instantiate(allDiscountObjects[randomAR], transform);
        Debug.Log("Child Instantiated");
    }

    public List<DiscountObject> GetDiscountObjects() {
        return allDiscountObjects;
    }

    public string GetMarkerName() {
        if (string.IsNullOrEmpty(markerName))
            return gameObject.name;
        else
            return markerName;
    }
}
