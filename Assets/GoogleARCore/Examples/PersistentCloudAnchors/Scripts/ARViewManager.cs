//-----------------------------------------------------------------------
// <copyright file="ARViewManager.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.PersistentCloudAnchors
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using GoogleARCore.CrossPlatform;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

#if ARCORE_IOS_SUPPORT
    using UnityEngine.XR.iOS;
#endif

    /// <summary>
    /// A manager component that helps with hosting and resolving Cloud Anchors.
    /// </summary>
    public class ARViewManager : MonoBehaviour
    {
        /// <summary>
        /// The main controller for Persistent Cloud Anchors sample.
        /// </summary>
        public PersistentCloudAnchorsController Controller;

        /// <summary>
        /// The 3D object that represents a Cloud Anchor.
        /// </summary>
        public GameObject CloudAnchorPrefab;

        /// <summary>
        /// The game object that includes <see cref="MapQualityIndicator"/> to visualize
        /// map quality result.
        /// </summary>
        public GameObject MapQualityIndicatorPrefab;

        /// <summary>
        /// The UI element that displays the instructions to guide hosting experience.
        /// </summary>
        public GameObject InstructionBar;

        /// <summary>
        /// The UI panel that allows the user to name the Cloud Anchor.
        /// </summary>
        public GameObject NamePanel;

        /// <summary>
        /// The UI panel that allows the user to copy the Cloud Anchor Id and share it.
        /// </summary>
        public GameObject CopyPanel;

        /// <summary>
        /// The UI element that displays warning message for invalid input name.
        /// </summary>
        public GameObject InputFieldWarning;

        /// <summary>
        /// The input field for naming Cloud Anchor.
        /// </summary>
        public InputField NameField;

        /// <summary>
        /// The instruction text in the top instruction bar.
        /// </summary>
        public Text InstructionText;

        /// <summary>
        /// The debug text in bottom snack bar.
        /// </summary>
        public Text DebugText;

        /// <summary>
        /// The button to save the typed name.
        /// </summary>
        public Button SaveButton;

        /// <summary>
        /// The button to save current cloud anchor id into clipboard.
        /// </summary>
        public Button ShareButton;

        /// <summary>
        /// The time between enters AR View and ARCore session starts to host or resolve.
        /// </summary>
        private const float _startPrepareTime = 3.0f;

        /// <summary>
        /// The timer to indicate whether the AR View has passed the start prepare time.
        /// </summary>
        private float _timeSinceStart;

        /// <summary>
        /// True if the app is in the process of returning to home page due to an invalid state,
        /// otherwise false.
        /// </summary>
        private bool _isReturning;

        /// <summary>
        /// True if ARCore is hosting the anchor given by previous hit test result.
        /// </summary>
        private bool _isHosting;

        /// <summary>
        /// The MapQualityIndicator that attaches to the placed object.
        /// </summary>
        private MapQualityIndicator _qualityIndicator = null;

        /// <summary>
        /// The history data that represents the current hosted Cloud Anchor.
        /// </summary>
        private CloudAnchorHistory _hostedCloudAnchor;

        /// <summary>
        /// The hit pose from platfrom-specific hit test result.
        /// </summary>
        private Pose? _hitPose = null;

        /// <summary>
        /// A platform-specific component indicating the pawn object has been placed
        /// on a flat surface.
        /// </summary>
        private Component _anchorComponent = null;

        /// <summary>
        /// A list for caching all resolved results.
        /// </summary>
        private List<Component> _cachedComponents = new List<Component>();

        /// <summary>
        /// A set for caching all pending resolving AsyncTasks.
        /// </summary>
        private HashSet<string> _pendingTask = new HashSet<string>();

        private Color _activeColor;

#if ARCORE_IOS_SUPPORT
        private List<ARHitTestResult> _hitResultList = new List<ARHitTestResult>();
        private Dictionary<string, ARPlaneAnchorAlignment> _arPlaneAligmentMapping =
            new Dictionary<string, ARPlaneAnchorAlignment>();
#endif

        /// <summary>
        /// Get the camera pose for the current frame.
        /// </summary>
        /// <returns>The camera pose of the current frame.</returns>
        public static Pose GetCameraPose()
        {
            Pose framePose = new Pose();
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if ARCORE_IOS_SUPPORT
                var session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
                if (session != null)
                {
                    var matrix = session.GetCameraPose();
                    framePose.position = UnityARMatrixOps.GetPosition(matrix);
                    framePose.rotation = UnityARMatrixOps.GetRotation(matrix);
                }
#endif
            }
            else
            {
                framePose = Frame.Pose;
            }

            return framePose;
        }

        /// <summary>
        /// Callback handling the validaton of the input field.
        /// </summary>
        /// <param name="inputString">The current value of the input field.</param>
        public void OnInputFieldValueChanged(string inputString)
        {
            // Cloud Anchor name should only contains: letters, numbers, hyphen(-), underscore(_).
            var regex = new Regex("^[a-zA-Z0-9-_]*$");
            InputFieldWarning.SetActive(!regex.IsMatch(inputString));
            SetSaveButtonActive(!InputFieldWarning.activeSelf && inputString.Length > 0);
        }

        /// <summary>
        /// Callback handling "Ok" button click event for input field.
        /// </summary>
        public void OnSaveButtonClicked()
        {
            _hostedCloudAnchor.Name = NameField.text;
            Controller.SaveCloudAnchorHistory(_hostedCloudAnchor);

            DebugText.text = string.Format("Saved Cloud Anchor:\n{0}.", _hostedCloudAnchor.Name);
            ShareButton.gameObject.SetActive(true);
            NamePanel.SetActive(false);
        }

        /// <summary>
        /// Callback handling "Share" button click event.
        /// </summary>
        public void OnShareButtonClicked()
        {
#if UNITY_2018_4_OR_NEWER
            GUIUtility.systemCopyBuffer = _hostedCloudAnchor.Id;
#else
            // On 2017.4, GUIUtility.systemCopyBuffer doesn't support Android or iOS.
            // Pops up a text field and let user to manually copy the Cloud Anchor Id.
            var textField = CopyPanel.GetComponentInChildren<InputField>();
            textField.text = _hostedCloudAnchor.Id;
            CopyPanel.SetActive(true);
#endif
            DebugText.text = "Copied cloud id: " + _hostedCloudAnchor.Id;
        }

        /// <summary>
        /// Callback handling "Done" button click event in copy panel.
        /// </summary>
        public void OnCopyCompleted()
        {
            CopyPanel.SetActive(false);
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _activeColor = SaveButton.GetComponentInChildren<Text>().color;
#if ARCORE_IOS_SUPPORT
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                UnityARSessionNativeInterface.ARAnchorAddedEvent += AddARPlane;
                UnityARSessionNativeInterface.ARAnchorRemovedEvent += RemoveARPlane;
            }
#endif
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _timeSinceStart = 0.0f;
            _isReturning = false;
            _isHosting = false;
            _hitPose = null;
            _anchorComponent = null;
            _qualityIndicator = null;
            _cachedComponents.Clear();

            InstructionBar.SetActive(true);
            NamePanel.SetActive(false);
            CopyPanel.SetActive(false);
            InputFieldWarning.SetActive(false);
            ShareButton.gameObject.SetActive(false);
            Controller.PlaneGenerator.SetActive(true);

            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Ready:
                    ReturnToHomePage("Invalid application mode, returning to home page...");
                    break;
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    InstructionText.text = "Detecting flat surface...";
                    DebugText.text = "ARCore is preparing for " + Controller.Mode;
                    break;
            }
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            if (_pendingTask.Count > 0)
            {
                Debug.LogFormat("Cancelling pending tasks for {0} Cloud Anchor(s): {1}",
                    _pendingTask.Count,
                    string.Join(",", new List<string>(_pendingTask).ToArray()));
                foreach (string id in _pendingTask)
                {
                    XPSession.CancelCloudAnchorAsyncTask(id);
                }

                _pendingTask.Clear();
            }

            if (_qualityIndicator != null)
            {
                Destroy(_qualityIndicator.gameObject);
                _qualityIndicator = null;
            }

            if (_anchorComponent != null)
            {
                Destroy(_anchorComponent.gameObject);
                _anchorComponent = null;
            }

            if (_cachedComponents.Count > 0)
            {
                foreach (var anchor in _cachedComponents)
                {
                    Destroy(anchor.gameObject);
                }

                _cachedComponents.Clear();
            }
        }

#if ARCORE_IOS_SUPPORT
        /// <summary>
        /// The Unity OnDestroy() method.
        /// </summary>
        public void OnDestroy()
        {
            _arPlaneAligmentMapping.Clear();
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                UnityARSessionNativeInterface.ARAnchorAddedEvent -= AddARPlane;
                UnityARSessionNativeInterface.ARAnchorRemovedEvent -= RemoveARPlane;
            }

        }
#endif

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // Give ARCore some time to prepare for hosting or resolving.
            if (_timeSinceStart < _startPrepareTime)
            {
                _timeSinceStart += Time.deltaTime;
                if (_timeSinceStart >= _startPrepareTime)
                {
                    UpdateInitialInstruction();
                }

                return;
            }

            ARCoreLifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Resolving)
            {
                ResolvingCloudAnchors();
            }
            else if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Hosting)
            {
                // Perform hit test and place an anchor on the hit test result.
                if (_hitPose == null)
                {
                    // If the player has not touched the screen then the update is complete.
                    Touch touch;
                    if (Input.touchCount < 1 ||
                        (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                    {
                        return;
                    }

                    // Ignore the touch if it's pointing on UI objects.
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return;
                    }

                    // Perform hit test and place a pawn object.
                    PerformHitTest(touch.position);
                }

                HostingCloudAnchor();
            }
        }

        private void PerformHitTest(Vector2 touchPos)
        {
            var planeType = DetectedPlaneType.HorizontalUpwardFacing;
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if ARCORE_IOS_SUPPORT
                var session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
                var viewportPoint = Controller.MainCamera.ScreenToViewportPoint(touchPos);
                ARPoint arPoint = new ARPoint
                {
                    x = viewportPoint.x,
                    y = viewportPoint.y
                };

                _hitResultList = session.HitTest(arPoint,
                    ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
                if (_hitResultList.Count > 0)
                {
                    // Fetch the closest hit result.
                    int minDistanceIndex = GetMinDistanceIndex(_hitResultList);

                    string identifier = _hitResultList[minDistanceIndex].anchorIdentifier;
                    if (_arPlaneAligmentMapping.ContainsKey(identifier))
                    {
                        planeType = _arPlaneAligmentMapping[identifier] ==
                            ARPlaneAnchorAlignment.ARPlaneAnchorAlignmentVertical ?
                            DetectedPlaneType.Vertical : DetectedPlaneType.HorizontalUpwardFacing;
                    }
                    else
                    {
                        Debug.LogWarningFormat("Didn't find anchor identifier: {0}", identifier);
                        return;
                    }

                    Pose hitPose = new Pose();
                    hitPose.position = UnityARMatrixOps.GetPosition(
                        _hitResultList[minDistanceIndex].worldTransform);
                    if (planeType == DetectedPlaneType.Vertical)
                    {
                        hitPose.rotation = UnityARMatrixOps.GetRotation(
                            _hitResultList[minDistanceIndex].worldTransform);
                    }
                    else
                    {
                        // Point the hitPose rotation roughly away from the raycast/camera
                        // to match ARCore.
                        hitPose.rotation.eulerAngles =
                            new Vector3(0.0f, Controller.MainCamera.transform.eulerAngles.y, 0.0f);
                    }

                    _hitPose = hitPose;
                    var anchorGO = new GameObject("ARUserAnchor");
                    _anchorComponent = anchorGO.AddComponent<UnityARUserAnchorComponent>();
                    anchorGO.transform.position = hitPose.position;
                    anchorGO.transform.rotation = hitPose.rotation;
                }
#endif
            }
            else
            {
                TrackableHit arcoreHitResult = new TrackableHit();
                if (Frame.Raycast(touchPos.x, touchPos.y, TrackableHitFlags.PlaneWithinPolygon,
                    out arcoreHitResult))
                {
                    DetectedPlane plane = arcoreHitResult.Trackable as DetectedPlane;
                    if (plane == null)
                    {
                        Debug.LogWarning("Hit test result has invalid trackable type: " +
                            arcoreHitResult.Trackable.GetType());
                        return;
                    }

                    planeType = plane.PlaneType;
                    _hitPose = arcoreHitResult.Pose;
                    _anchorComponent =
                        arcoreHitResult.Trackable.CreateAnchor(arcoreHitResult.Pose);
                }
            }

            if (_anchorComponent != null)
            {
                Instantiate(CloudAnchorPrefab, _anchorComponent.transform);

                // Attach map quality indicator to this pawn.
                var indicatorGO =
                    Instantiate(MapQualityIndicatorPrefab, _anchorComponent.transform);
                _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                _qualityIndicator.DrawIndicator(planeType, Controller.MainCamera);

                InstructionText.text = " To save this location, walk around the object to " +
                    "capture it from different angles";
                DebugText.text = "Waiting for sufficient mapping quaility...";

                // Hide plane generator so users can focus on the object they placed.
                Controller.PlaneGenerator.SetActive(false);
            }
        }

        private void HostingCloudAnchor()
        {
            // There is no anchor for hosting.
            if (_anchorComponent == null)
            {
                return;
            }

            // There is a pending hosting task.
            if (_isHosting)
            {
                return;
            }

            // Hosting instructions:
            var cameraDist = (_qualityIndicator.transform.position -
                Controller.MainCamera.transform.position).magnitude;
            if (cameraDist < _qualityIndicator.Radius * 1.5f)
            {
                InstructionText.text = "You are too close, move backward.";
                return;
            }
            else if (cameraDist > 10.0f)
            {
                InstructionText.text = "You are too far, come closer.";
                return;
            }
            else if (_qualityIndicator.ReachTopviewAngle)
            {
                InstructionText.text =
                    "You are looking from the top view, move around from all sides.";
                return;
            }
            else if (!_qualityIndicator.ReachQualityThreshold)
            {
                InstructionText.text = "Save the object here by capturing it from all sides.";
                // Can pass in ANY valid camera pose to the mapping quality API.
                // Ideally, the pose should represent users’ expected perspectives.
                DebugText.text = "Current mapping quality: " +
                    XPSession.EstimateFeatureMapQualityForHosting(GetCameraPose());
                return;
            }

            // Start hosting:
            _isHosting = true;
            InstructionText.text = "Processing...";
            DebugText.text = "Mapping quality has reached sufficient threshold, " +
                "creating Cloud Anchor.";
            DebugText.text = string.Format(
                "FeatureMapQuality has reached {0}, triggering CreateCloudAnchor.",
                XPSession.EstimateFeatureMapQualityForHosting(GetCameraPose()));

#if ARCORE_IOS_SUPPORT
            var anchor = (UnityARUserAnchorComponent)_anchorComponent;
#else
            var anchor = (Anchor)_anchorComponent;
#endif

            // Creating a Cloud Anchor with lifetime = 1 day.
            // This is configurable up to 365 days when keyless authentication is used.
            XPSession.CreateCloudAnchor(anchor, 1).ThenAction(result =>
            {
                if (!_isHosting)
                {
                    // This is the pending task from previous session.
                    return;
                }

                if (result.Response != CloudServiceResponse.Success)
                {
                    Debug.LogFormat("Failed to host cloud anchor: {0}", result.Response);
                    OnAnchorHostedFinished(false, result.Response.ToString());
                }
                else
                {
                    Debug.LogFormat("Succeed to host cloud anchor: {0}", result.Anchor.CloudId);
                    int count = Controller.LoadCloudAnchorHistory().Collection.Count;
                    _hostedCloudAnchor =
                        new CloudAnchorHistory("CloudAnchor" + count, result.Anchor.CloudId);
                    OnAnchorHostedFinished(true, result.Anchor.CloudId);
                }
            });
        }

        private void ResolvingCloudAnchors()
        {
            // No Cloud Anchor for resolving.
            if (Controller.ResolvingSet.Count == 0)
            {
                return;
            }

            // ARCore session is not ready for resolving.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            Debug.LogFormat("Attempting to resolve {0} anchor(s): {1}",
                Controller.ResolvingSet.Count,
                string.Join(",", new List<string>(Controller.ResolvingSet).ToArray()));
            foreach (string cloudId in Controller.ResolvingSet)
            {
                _pendingTask.Add(cloudId);
                XPSession.ResolveCloudAnchor(cloudId).ThenAction(result =>
                {
                    _pendingTask.Remove(cloudId);
                    if (result.Response != CloudServiceResponse.Success)
                    {
                        Debug.LogFormat("Faild to resolve cloud anchor {0} for {1}",
                            cloudId, result.Response);
                        OnAnchorResolvedFinished(false, result.Response.ToString());
                    }
                    else
                    {
                        Debug.LogFormat("Succeed to resolve cloud anchor: {0}", cloudId);
                        OnAnchorResolvedFinished(true, cloudId);
                        Instantiate(CloudAnchorPrefab, result.Anchor.transform);
                        _cachedComponents.Add(result.Anchor);
                    }
                });
            }

            Controller.ResolvingSet.Clear();
        }

        private void OnAnchorHostedFinished(bool success, string response)
        {
            if (success)
            {
                InstructionText.text = "Finish!";
                Invoke("DoHideInstructionBar", 1.5f);
                DebugText.text = "Succeed to host cloud anchor: " + response;

                // Display name panel and hide instruction bar.
                NameField.text = _hostedCloudAnchor.Name;
                NamePanel.SetActive(true);
                SetSaveButtonActive(true);
            }
            else
            {
                InstructionText.text = "Host failed.";
                DebugText.text = "Failed to host cloud anchor: " + response;
            }
        }

        private void OnAnchorResolvedFinished(bool success, string response)
        {
            if (success)
            {
                InstructionText.text = "Resolve success!";
                DebugText.text = "Succeed to resolve cloud anchor: " + response;
            }
            else
            {
                InstructionText.text = "Resolve failed.";
                DebugText.text = "Failed to resolve cloud anchor: " + response;
            }
        }

        private void UpdateInitialInstruction()
        {
            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                    // Initial instruction for hosting flow:
                    InstructionText.text = "Tap to place an object.";
                    DebugText.text = "Tap a vertical or horizontal plane...";
                    return;
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    // Initial instruction for resolving flow:
                    InstructionText.text =
                        "Look at the location you expect to see the AR experience appear.";
                    DebugText.text = string.Format("Attempting to resolve {0} anchors...",
                        Controller.ResolvingSet.Count);
                    return;
                default:
                    return;
            }
        }

        private void ARCoreLifecycleUpdate()
        {
            var sleepTimeout = SleepTimeout.NeverSleep;
#if !UNITY_IOS
            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }
#endif

            Screen.sleepTimeout = sleepTimeout;

            if (_isReturning)
            {
                return;
            }

            // Return to home page if ARCore is in error status.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                ReturnToHomePage("Camera permission is needed to run this application.");
            }
            else if (Session.Status.IsError())
            {
                ReturnToHomePage("ARCore encountered a problem connecting. " +
                    "Please start the app again.");
            }
        }

        private void ReturnToHomePage(string reason)
        {
            Debug.Log("Returning home for reason: " + reason);
            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            DebugText.text = reason;
            Invoke("DoReturnToHomePage", 3.0f);
        }

        private void DoReturnToHomePage()
        {
            Controller.SwitchToHomePage();
        }

        private void DoHideInstructionBar()
        {
            InstructionBar.SetActive(false);
        }

        private void SetSaveButtonActive(bool active)
        {
            SaveButton.enabled = active;
            SaveButton.GetComponentInChildren<Text>().color = active ? _activeColor : Color.gray;
        }

#if ARCORE_IOS_SUPPORT
        private void AddARPlane(ARPlaneAnchor arPlane)
        {
            _arPlaneAligmentMapping[arPlane.identifier] = arPlane.alignment;
        }

        private void RemoveARPlane(ARPlaneAnchor arPlane)
        {
            _arPlaneAligmentMapping.Remove(arPlane.identifier);
        }

        private int GetMinDistanceIndex(List<ARHitTestResult> results)
        {
            if (results.Count == 0)
            {
                return -1;
            }

            int minDistanceIndex = 0;
            for (int i = 1; i < results.Count; i++)
            {
                if (results[i].distance < results[minDistanceIndex].distance)
                {
                    minDistanceIndex = i;
                }
            }

            return minDistanceIndex;
        }
#endif
    }
}
