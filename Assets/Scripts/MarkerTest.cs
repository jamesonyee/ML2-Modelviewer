using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.MagicLeapSupport;

public class MarkerTrackerExample : MonoBehaviour
{
    [Tooltip("Set the XR Origin so that the marker appears relative to headset's origin. If null, the script will try to find the component automatically.")]
    public XROrigin XROrigin;

    [Tooltip("If Not Null, this is the object that will be created at the position of each detected marker.")]
    public GameObject DefaultMarkerPrefab;
    public GameObject JetPrefab;
    public GameObject CarPrefab;

    public MagicLeapMarkerUnderstandingFeature.ArucoType ArucoType =
        MagicLeapMarkerUnderstandingFeature.ArucoType.Dictionary_5x5_50;

    public MagicLeapMarkerUnderstandingFeature.MarkerDetectorProfile DetectorProfile =
        MagicLeapMarkerUnderstandingFeature.MarkerDetectorProfile.Default;

    private MagicLeapMarkerUnderstandingFeature.MarkerDetectorSettings _detectorSettings;
    private MagicLeapMarkerUnderstandingFeature _markerFeature;
    private readonly Dictionary<string, GameObject> _markerObjectById = new Dictionary<string, GameObject>();

    // Dictionary to map ArUco codes to prefabs
    public Dictionary<int, GameObject> MarkerPrefabById = new Dictionary<int, GameObject>();

    private GameObject _currentActiveMarker;

    private void OnValidate()
    {
        // Automatically find the XROrigin component if it's present in the scene
        if (XROrigin == null)
        {
            XROrigin = FindAnyObjectByType<XROrigin>();
        }
    }

    private void Start()
    {
        _markerFeature = OpenXRSettings.Instance.GetFeature<MagicLeapMarkerUnderstandingFeature>();

        if (_markerFeature == null || _markerFeature.enabled == false)
        {
            Debug.LogError("The Magic Leap 2 Marker Understanding OpenXR Feature is missing or disabled. Disabling Script.");
            this.enabled = false;
            return;
        }

        if (XROrigin == null)
        {
            Debug.LogError("No XR Origin Found, markers sample will not work. Disabling Script.");
            this.enabled = false;
        }

        // Configure a generic detector with QR and Aruco Detector settings 
        _detectorSettings.QRSettings.EstimateQRLength = true;
        _detectorSettings.ArucoSettings.EstimateArucoLength = true;
        _detectorSettings.ArucoSettings.ArucoType = ArucoType;

        _detectorSettings.MarkerDetectorProfile = DetectorProfile;

        // We use the same settings on all 3 of the 
        // different detectors and target the specific marker by setting the Marker Type before creating the detector 

        // Create Aruco detector
        _detectorSettings.MarkerType = MagicLeapMarkerUnderstandingFeature.MarkerType.Aruco;
        _markerFeature.CreateMarkerDetector(_detectorSettings);

        // Create QRCode Detector
        _detectorSettings.MarkerType = MagicLeapMarkerUnderstandingFeature.MarkerType.QR;
        _markerFeature.CreateMarkerDetector(_detectorSettings);

        // Create UPCA Detector
        _detectorSettings.MarkerType = MagicLeapMarkerUnderstandingFeature.MarkerType.UPCA;
        _markerFeature.CreateMarkerDetector(_detectorSettings);

        MarkerPrefabById[0] = DefaultMarkerPrefab;
        MarkerPrefabById[1] = JetPrefab;
        MarkerPrefabById[2] = CarPrefab;
    }

    private void OnDestroy()
    {
        if (_markerFeature != null)
        {
            _markerFeature.DestroyAllMarkerDetectors();
        }
    }

    void Update()
    {
        // Update the marker detector
        _markerFeature.UpdateMarkerDetectors();

        // Iterate through all of the marker detectors
        for (int i = 0; i < _markerFeature.MarkerDetectors.Count; i++)
        {
            // Verify that the marker detector is running
            if (_markerFeature.MarkerDetectors[i].Status == MagicLeapMarkerUnderstandingFeature.MarkerDetectorStatus.Ready)
            {
                // Cycle through the detector's data and log it to the debug log
                MagicLeapMarkerUnderstandingFeature.MarkerDetector currentDetector = _markerFeature.MarkerDetectors[i];
                OnUpdateDetector(currentDetector);
            }
        }
    }

    private void OnUpdateDetector(MagicLeapMarkerUnderstandingFeature.MarkerDetector detector)
    {
        for (int i = 0; i < detector.Data.Count; i++)
        {
            string id = "";
            float markerSize = .05f;
            var data = detector.Data[i];
            switch (detector.Settings.MarkerType)
            {
                case MagicLeapMarkerUnderstandingFeature.MarkerType.Aruco:
                    id = data.MarkerNumber.ToString();
                    markerSize = data.MarkerLength;
                    break;
                case MagicLeapMarkerUnderstandingFeature.MarkerType.QR:
                    id = data.MarkerString;
                    markerSize = data.MarkerLength;
                    break;
                case MagicLeapMarkerUnderstandingFeature.MarkerType.UPCA:
                    Debug.Log("No pose is given for marker type UPCA, value is " + id);
                    break;
            }

            if (!data.MarkerPose.HasValue)
            {
                //Do not create a marker until the pose is valid
                return;
            }

            if (!string.IsNullOrEmpty(id) && markerSize > 0)
            {
                int markerId = int.Parse(id);
                GameObject markerPrefab = DefaultMarkerPrefab;

                // Check if a specific prefab is mapped to the detected marker ID
                if (MarkerPrefabById.ContainsKey(markerId))
                {
                    markerPrefab = MarkerPrefabById[markerId];
                }

                // Hide the current active marker if it's different from the new marker
                if (_currentActiveMarker != null && _currentActiveMarker != markerPrefab)
                {
                    _currentActiveMarker.SetActive(false);
                }

                // If the marker ID has not been tracked create a new marker object
                if (!_markerObjectById.ContainsKey(id))
                {
                    // Create the marker object based on the detected ID
                    GameObject newMarker = Instantiate(markerPrefab);
                    _markerObjectById.Add(id, newMarker);
                }

                GameObject marker = _markerObjectById[id];
                SetTransformToMarkerPose(marker.transform, data.MarkerPose.Value, markerSize);

                // Set the new marker as the current active marker and show it
                _currentActiveMarker = marker;
                _currentActiveMarker.SetActive(true);
            }
        }
    }

    private void SetTransformToMarkerPose(Transform marker, Pose markerPose, float markerSize)
    {
        Transform originTransform = XROrigin.CameraFloorOffsetObject.transform;

        // Set the position of the marker. Since the pose is given relative to the XR Origin,
        // we need to transform it to world coordinates.
        marker.position = originTransform.TransformPoint(markerPose.position);

        // Add a 90-degree rotation around the X-axis to ensure the model is correctly oriented
        Quaternion additionalRotation = Quaternion.Euler(-90, 0, 0);
        marker.rotation = originTransform.rotation * markerPose.rotation * additionalRotation;

        // When marker size estimation is enabled, markers may take a few frames to scale to their appropriate size.
        if (marker.transform.localScale.x != markerSize)
        {
            marker.localScale = new Vector3(markerSize, markerSize, markerSize);
        }
    }
}
