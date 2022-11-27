using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class BargaintHunt : MonoBehaviour
{
    [SerializeField] private Text imageTrackedText;
    [SerializeField] private GameObject[] discountObject;
    [SerializeField] private Vector3 scaleObj = new Vector3(0.1f, 0.1f, 0.1f);
    private ARTrackedImageManager trackedManager;
    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedManager = GetComponent<ARTrackedImageManager>();

        foreach (GameObject go in discountObject)
        {
            GameObject newGO = Instantiate(go, Vector3.zero, Quaternion.identity);
            newGO.name = go.name;
            arObjects.Add(go.name, newGO);
        }
    }

    void OnEnable()
    {
        trackedManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.removed)
        {
            arObjects[trackedImage.name].SetActive(false);
        }
    }

    void UpdateARImage(ARTrackedImage trackedImage)
    {
        imageTrackedText.text = trackedImage.referenceImage.name;

        AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position);
    }

    void AssignGameObject(string name, Vector3 newPosition)
    {
        if(discountObject != null)
        {
            GameObject goAR = arObjects[name];
            goAR.SetActive(true);
            goAR.transform.position = newPosition;
            goAR.transform.localScale = scaleObj;

            foreach (GameObject go in arObjects.Values)
            {
                Debug.Log($"GO in arObjects.Values: {go.name}");
                if(go.name != name)
                {
                    go.SetActive(false);
                }
            }
        }
    }
}
