using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageTracking : MonoBehaviour
{
    [SerializeField ] private List<DiscountParent> discountObject;
    public XRReferenceImageLibrary referenceLibrary;

    private ARTrackedImageManager arTracked;
    private ARSession arSession;

    private GameObject spawnedObject;
    private Dictionary<string, GameObject> discountList = new Dictionary<string, GameObject>();

    private void Awake()
    {
        arTracked = GetComponent<ARTrackedImageManager>(); 
        arSession = FindObjectOfType<ARSession>();

        arSession.Reset();
        arSession.enabled = true;

        foreach (var item in discountObject)
        {
            discountList.Add(item.GetMarkerName(), item.gameObject);
            Debug.Log($"Added marker with name {item.GetMarkerName()}");
        }
    }

    private void Start()
    {
        spawnedObject = new GameObject();
        arTracked.referenceLibrary = referenceLibrary;
    }

    private void OnEnable()
    {
        arTracked.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        arTracked.trackedImagesChanged -= ImageChanged;
    }

    private void OnDestroy()
    {
        arTracked.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            InitImage(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            Destroy(spawnedObject);
        }
        
    }

    private void InitImage(ARTrackedImage trackedImage)
    {
        Debug.Log($"Tracked image detected : {trackedImage.referenceImage.name}");
        spawnedObject = Instantiate(discountList[trackedImage.referenceImage.name], trackedImage.transform);
        spawnedObject.transform.Rotate(new Vector3(90, 0, 0));
        spawnedObject.SetActive(true);
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        if (spawnedObject == null)
            InitImage(trackedImage);

        spawnedObject.SetActive(true);
        spawnedObject.transform.position = trackedImage.transform.position;
    }

    public void ResetSession() {
        LoaderUtility.Deinitialize();
        LoaderUtility.Initialize();
    }
}
