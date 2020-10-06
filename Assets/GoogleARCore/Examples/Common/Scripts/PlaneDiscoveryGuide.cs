//-----------------------------------------------------------------------
// <copyright file="PlaneDiscoveryGuide.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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

namespace GoogleARCore.Examples.Common
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    /// <summary>
    /// Provides plane discovery visuals that guide users to scan surroundings and discover planes.
    /// This consists of a hand animation and a snackbar with short instructions. If no plane is found
    /// after a certain ammount of time, the snackbar shows a button that offers to open a help window with
    /// more detailed instructions on how to find a plane when pressed.
    /// </summary>
    public class PlaneDiscoveryGuide : MonoBehaviour
    {
        /// <summary>
        /// The time to delay, after ARCore loses tracking of any planes, showing the plane
        /// discovery guide.
        /// </summary>
        [Tooltip("The time to delay, after ARCore loses tracking of any planes, showing the plane " +
                 "discovery guide.")]
        public float DisplayGuideDelay = 3.0f;

        /// <summary>
        /// The time to delay, after displaying the plane discovery guide, offering more detailed
        /// instructions on how to find a plane.
        /// </summary>
        [Tooltip("The time to delay, after displaying the plane discovery guide, offering more detailed " +
                 "instructions on how to find a plane.")]
        public float OfferDetailedInstructionsDelay = 8.0f;

        /// <summary>
        /// The time to delay, after Unity Start, showing the plane discovery guide.
        /// </summary>
        private const float _onStartDelay = 1f;

        /// <summary>
        /// The time to delay, after a at least one plane is tracked by ARCore, hiding the plane discovery guide.
        /// </summary>
        private const float _hideGuideDelay = 0.75f;

        /// <summary>
        /// The duration of the hand animation fades.
        /// </summary>
        private const float _animationFadeDuration = 0.15f;

        /// <summary>
        /// The Game Object that provides feature points visualization.
        /// </summary>
        [Tooltip("The Game Object that provides feature points visualization.")]
        [FormerlySerializedAs("m_FeaturePoints")]
        [SerializeField]
        private GameObject _featurePoints = null;

        /// <summary>
        /// The RawImage that provides rotating hand animation.
        /// </summary>
        [Tooltip("The RawImage that provides rotating hand animation.")]
        [FormerlySerializedAs("m_HandAnimation")]
        [SerializeField]
        private RawImage _handAnimation = null;

        /// <summary>
        /// The snackbar Game Object.
        /// </summary>
        [Tooltip("The snackbar Game Object.")]
        [FormerlySerializedAs("m_SnackBar")]
        [SerializeField]
        private GameObject _snackBar = null;

        /// <summary>
        /// The snackbar text.
        /// </summary>
        [Tooltip("The snackbar text.")]
        [FormerlySerializedAs("m_SnackBarText")]
        [SerializeField]
        private Text _snackBarText = null;

        /// <summary>
        /// The Game Object that contains the button to open the help window.
        /// </summary>
        [Tooltip("The Game Object that contains the button to open the help window.")]
        [FormerlySerializedAs("m_OpenButton")]
        [SerializeField]
        private GameObject _openButton = null;

        /// <summary>
        /// The Game Object that contains the window with more instructions on how to find a plane.
        /// </summary>
        [Tooltip(
            "The Game Object that contains the window with more instructions on how to find " +
            "a plane.")]
        [FormerlySerializedAs("m_MoreHelpWindow")]
        [SerializeField]
        private GameObject _moreHelpWindow = null;

        /// <summary>
        /// The Game Object that contains the button to close the help window.
        /// </summary>
        [Tooltip("The Game Object that contains the button to close the help window.")]
        [FormerlySerializedAs("m_GotItButton")]
        [SerializeField]
        private Button _gotItButton = null;

        /// <summary>
        /// The elapsed time ARCore has been detecting at least one plane.
        /// </summary>
        private float _detectedPlaneElapsed;

        /// <summary>
        /// The elapsed time ARCore has been tracking but not detected any planes.
        /// </summary>
        private float _notDetectedPlaneElapsed;

        /// <summary>
        /// Indicates whether a lost tracking reason is displayed.
        /// </summary>
        private bool _isLostTrackingDisplayed;

        /// <summary>
        /// A list to hold detected planes ARCore is tracking in the current frame.
        /// </summary>
        private List<DetectedPlane> _detectedPlanes = new List<DetectedPlane>();

        /// <summary>
        /// Unity's Start() method.
        /// </summary>
        public void Start()
        {
            _openButton.GetComponent<Button>().onClick.AddListener(OnOpenButtonClicked);
            _gotItButton.onClick.AddListener(OnGotItButtonClicked);

            CheckFieldsAreNotNull();
            _moreHelpWindow.SetActive(false);
            _isLostTrackingDisplayed = false;
            _notDetectedPlaneElapsed = DisplayGuideDelay - _onStartDelay;
        }

        /// <summary>
        /// Unity's OnDestroy() method.
        /// </summary>
        public void OnDestroy()
        {
            _openButton.GetComponent<Button>().onClick.RemoveListener(OnOpenButtonClicked);
            _gotItButton.onClick.RemoveListener(OnGotItButtonClicked);
        }

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            UpdateDetectedPlaneTrackingState();
            UpdateUI();
        }

        /// <summary>
        /// Enable or Disable this Plane Discovery Guide.
        /// </summary>
        /// <param name="guideEnabled">Enable/Disable the guide.</param>
        public void EnablePlaneDiscoveryGuide(bool guideEnabled)
        {
            if (guideEnabled)
            {
                enabled = true;
            }
            else
            {
                enabled = false;
                _featurePoints.SetActive(false);
                _handAnimation.enabled = false;
                _snackBar.SetActive(false);
            }
        }

        /// <summary>
        /// Callback executed when the open button has been clicked by the user.
        /// </summary>
        private void OnOpenButtonClicked()
        {
            _moreHelpWindow.SetActive(true);

            enabled = false;
            _featurePoints.SetActive(false);
            _handAnimation.enabled = false;
            _snackBar.SetActive(false);
        }

        /// <summary>
        /// Callback executed when the got-it button has been clicked by the user.
        /// </summary>
        private void OnGotItButtonClicked()
        {
            _moreHelpWindow.SetActive(false);
            enabled = true;
        }

        /// <summary>
        /// Checks whether at least one plane being actively tracked exists.
        /// </summary>
        private void UpdateDetectedPlaneTrackingState()
        {
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            Session.GetTrackables<DetectedPlane>(_detectedPlanes, TrackableQueryFilter.All);
            foreach (DetectedPlane plane in _detectedPlanes)
            {
                if (plane.TrackingState == TrackingState.Tracking)
                {
                    _detectedPlaneElapsed += Time.deltaTime;
                    _notDetectedPlaneElapsed = 0f;
                    return;
                }
            }

            _detectedPlaneElapsed = 0f;
            _notDetectedPlaneElapsed += Time.deltaTime;
        }

        /// <summary>
        /// Hides or shows the UI based on the existence of a plane being currently tracked.
        /// </summary>
        private void UpdateUI()
        {
            if (Session.Status == SessionStatus.LostTracking &&
                Session.LostTrackingReason != LostTrackingReason.None)
            {
                // The session has lost tracking.
                _featurePoints.SetActive(false);
                _handAnimation.enabled = false;
                _snackBar.SetActive(true);
                switch (Session.LostTrackingReason)
                {
                    case LostTrackingReason.InsufficientLight:
                        _snackBarText.text = "Too dark. Try moving to a well-lit area.";
                        break;
                    case LostTrackingReason.InsufficientFeatures:
                        _snackBarText.text = "Aim device at a surface with more texture or color.";
                        break;
                    case LostTrackingReason.ExcessiveMotion:
                        _snackBarText.text = "Moving too fast. Slow down.";
                        break;
                    case LostTrackingReason.CameraUnavailable:
                        _snackBarText.text = "Another app is using the camera. Tap on this app " +
                            "or try closing the other one.";
                        break;
                    default:
                        _snackBarText.text = "Motion tracking is lost.";
                        break;
                }

                _openButton.SetActive(false);
                _isLostTrackingDisplayed = true;
                return;
            }
            else if (_isLostTrackingDisplayed)
            {
                // The session has moved from the lost tracking state.
                _snackBar.SetActive(false);
                _isLostTrackingDisplayed = false;
            }

            if (_notDetectedPlaneElapsed > DisplayGuideDelay)
            {
                // The session has been tracking but no planes have been found by
                // 'DisplayGuideDelay'.
                _featurePoints.SetActive(true);

                if (!_handAnimation.enabled)
                {
                    _handAnimation.GetComponent<CanvasRenderer>().SetAlpha(0f);
                    _handAnimation.CrossFadeAlpha(1f, _animationFadeDuration, false);
                }

                _handAnimation.enabled = true;
                _snackBar.SetActive(true);

                if (_notDetectedPlaneElapsed > OfferDetailedInstructionsDelay)
                {
                    _snackBarText.text = "Need Help?";
                    _openButton.SetActive(true);
                }
                else
                {
                    _snackBarText.text = "Point your camera to where you want to place an object.";
                    _openButton.SetActive(false);
                }
            }
            else if (_notDetectedPlaneElapsed > 0f || _detectedPlaneElapsed > _hideGuideDelay)
            {
                // The session is tracking but no planes have been found in less than
                // 'DisplayGuideDelay' or at least one plane has been tracking for more than
                // '_hideGuideDelay'.
                _featurePoints.SetActive(false);
                _snackBar.SetActive(false);
                _openButton.SetActive(false);

                if (_handAnimation.enabled)
                {
                    _handAnimation.GetComponent<CanvasRenderer>().SetAlpha(1f);
                    _handAnimation.CrossFadeAlpha(0f, _animationFadeDuration, false);
                }

                _handAnimation.enabled = false;
            }
        }

        /// <summary>
        /// Checks the required fields are not null, and logs a Warning otherwise.
        /// </summary>
        private void CheckFieldsAreNotNull()
        {
            if (_moreHelpWindow == null)
            {
                Debug.LogError("MoreHelpWindow is null");
            }

            if (_gotItButton == null)
            {
                Debug.LogError("GotItButton is null");
            }

            if (_snackBarText == null)
            {
                Debug.LogError("SnackBarText is null");
            }

            if (_snackBar == null)
            {
                Debug.LogError("SnackBar is null");
            }

            if (_openButton == null)
            {
                Debug.LogError("OpenButton is null");
            }
            else if (_openButton.GetComponent<Button>() == null)
            {
                Debug.LogError("OpenButton does not have a Button Component.");
            }

            if (_handAnimation == null)
            {
                Debug.LogError("HandAnimation is null");
            }

            if (_featurePoints == null)
            {
                Debug.LogError("FeaturePoints is null");
            }
        }
    }
}
