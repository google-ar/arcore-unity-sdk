//-----------------------------------------------------------------------
// <copyright file="PlaneDiscoveryGuide.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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
        private const float k_OnStartDelay = 1f;

        /// <summary>
        /// The time to delay, after a at least one plane is tracked by ARCore, hiding the plane discovery guide.
        /// </summary>
        private const float k_HideGuideDelay = 0.75f;

        /// <summary>
        /// The duration of the hand animation fades.
        /// </summary>
        private const float k_AnimationFadeDuration = 0.15f;

        /// <summary>
        /// The Game Object that provides feature points visualization.
        /// </summary>
        [Tooltip("The Game Object that provides feature points visualization.")]
        [SerializeField] private GameObject m_FeaturePoints = null;

        /// <summary>
        /// The RawImage that provides rotating hand animation.
        /// </summary>
        [Tooltip("The RawImage that provides rotating hand animation.")]
        [SerializeField] private RawImage m_HandAnimation = null;

        /// <summary>
        /// The snackbar Game Object.
        /// </summary>
        [Tooltip("The snackbar Game Object.")]
        [SerializeField] private GameObject m_SnackBar = null;

        /// <summary>
        /// The snackbar text.
        /// </summary>
        [Tooltip("The snackbar text.")]
        [SerializeField] private Text m_SnackBarText = null;

        /// <summary>
        /// The Game Object that contains the button to open the help window.
        /// </summary>
        [Tooltip("The Game Object that contains the button to open the help window.")]
        [SerializeField] private GameObject m_OpenButton = null;

        /// <summary>
        /// The Game Object that contains the window with more instructions on how to find a plane.
        /// </summary>
        [Tooltip(
            "The Game Object that contains the window with more instructions on how to find " +
            "a plane.")]
        [SerializeField] private GameObject m_MoreHelpWindow = null;

        /// <summary>
        /// The Game Object that contains the button to close the help window.
        /// </summary>
        [Tooltip("The Game Object that contains the button to close the help window.")]
        [SerializeField] private Button m_GotItButton = null;

        /// <summary>
        /// The elapsed time ARCore has been detecting at least one plane.
        /// </summary>
        private float m_DetectedPlaneElapsed;

        /// <summary>
        /// The elapsed time ARCore has been tracking but not detected any planes.
        /// </summary>
        private float m_NotDetectedPlaneElapsed;

        /// <summary>
        /// Indicates whether a lost tracking reason is displayed.
        /// </summary>
        private bool m_IsLostTrackingDisplayed;

        /// <summary>
        /// A list to hold detected planes ARCore is tracking in the current frame.
        /// </summary>
        private List<DetectedPlane> m_DetectedPlanes = new List<DetectedPlane>();

        /// <summary>
        /// Unity's Start() method.
        /// </summary>
        public void Start()
        {
            m_OpenButton.GetComponent<Button>().onClick.AddListener(_OnOpenButtonClicked);
            m_GotItButton.onClick.AddListener(_OnGotItButtonClicked);

            _CheckFieldsAreNotNull();
            m_MoreHelpWindow.SetActive(false);
            m_IsLostTrackingDisplayed = false;
            m_NotDetectedPlaneElapsed = DisplayGuideDelay - k_OnStartDelay;
        }

        /// <summary>
        /// Unity's OnDestroy() method.
        /// </summary>
        public void OnDestroy()
        {
            m_OpenButton.GetComponent<Button>().onClick.RemoveListener(_OnOpenButtonClicked);
            m_GotItButton.onClick.RemoveListener(_OnGotItButtonClicked);
        }

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            _UpdateDetectedPlaneTrackingState();
            _UpdateUI();
        }

        /// <summary>
        /// Callback executed when the open button has been clicked by the user.
        /// </summary>
        private void _OnOpenButtonClicked()
        {
            m_MoreHelpWindow.SetActive(true);

            enabled = false;
            m_FeaturePoints.SetActive(false);
            m_HandAnimation.enabled = false;
            m_SnackBar.SetActive(false);
        }

        /// <summary>
        /// Callback executed when the got-it button has been clicked by the user.
        /// </summary>
        private void _OnGotItButtonClicked()
        {
            m_MoreHelpWindow.SetActive(false);
            enabled = true;
        }

        /// <summary>
        /// Checks whether at least one plane being actively tracked exists.
        /// </summary>
        private void _UpdateDetectedPlaneTrackingState()
        {
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            Session.GetTrackables<DetectedPlane>(m_DetectedPlanes, TrackableQueryFilter.All);
            foreach (DetectedPlane plane in m_DetectedPlanes)
            {
                if (plane.TrackingState == TrackingState.Tracking)
                {
                    m_DetectedPlaneElapsed += Time.deltaTime;
                    m_NotDetectedPlaneElapsed = 0f;
                    return;
                }
            }

            m_DetectedPlaneElapsed = 0f;
            m_NotDetectedPlaneElapsed += Time.deltaTime;
        }

        /// <summary>
        /// Hides or shows the UI based on the existence of a plane being currently tracked.
        /// </summary>
        private void _UpdateUI()
        {
            if (Session.Status == SessionStatus.LostTracking &&
                Session.LostTrackingReason != LostTrackingReason.None)
            {
                // The session has lost tracking.
                m_FeaturePoints.SetActive(false);
                m_HandAnimation.enabled = false;
                m_SnackBar.SetActive(true);
                switch (Session.LostTrackingReason)
                {
                    case LostTrackingReason.InsufficientLight:
                        m_SnackBarText.text = "Too dark. Try moving to a well-lit area.";
                        break;
                    case LostTrackingReason.InsufficientFeatures:
                        m_SnackBarText.text = "Aim device at a surface with more texture or color.";
                        break;
                    case LostTrackingReason.ExcessiveMotion:
                        m_SnackBarText.text = "Moving too fast. Slow down.";
                        break;
                    case LostTrackingReason.CameraUnavailable:
                        m_SnackBarText.text = "Another app is using the camera. Tap on this app " +
                            "or try closing the other one.";
                        break;
                    default:
                        m_SnackBarText.text = "Motion tracking is lost.";
                        break;
                }

                m_OpenButton.SetActive(false);
                m_IsLostTrackingDisplayed = true;
                return;
            }
            else if (m_IsLostTrackingDisplayed)
            {
                // The session has moved from the lost tracking state.
                m_SnackBar.SetActive(false);
                m_IsLostTrackingDisplayed = false;
            }

            if (m_NotDetectedPlaneElapsed > DisplayGuideDelay)
            {
                // The session has been tracking but no planes have been found by
                // 'DisplayGuideDelay'.
                m_FeaturePoints.SetActive(true);

                if (!m_HandAnimation.enabled)
                {
                    m_HandAnimation.GetComponent<CanvasRenderer>().SetAlpha(0f);
                    m_HandAnimation.CrossFadeAlpha(1f, k_AnimationFadeDuration, false);
                }

                m_HandAnimation.enabled = true;
                m_SnackBar.SetActive(true);

                if (m_NotDetectedPlaneElapsed > OfferDetailedInstructionsDelay)
                {
                    m_SnackBarText.text = "Need Help?";
                    m_OpenButton.SetActive(true);
                }
                else
                {
                    m_SnackBarText.text = "Point your camera to where you want to place an object.";
                    m_OpenButton.SetActive(false);
                }
            }
            else if (m_NotDetectedPlaneElapsed > 0f || m_DetectedPlaneElapsed > k_HideGuideDelay)
            {
                // The session is tracking but no planes have been found in less than
                // 'DisplayGuideDelay' or at least one plane has been tracking for more than
                // 'k_HideGuideDelay'.
                m_FeaturePoints.SetActive(false);
                m_SnackBar.SetActive(false);
                m_OpenButton.SetActive(false);

                if (m_HandAnimation.enabled)
                {
                    m_HandAnimation.GetComponent<CanvasRenderer>().SetAlpha(1f);
                    m_HandAnimation.CrossFadeAlpha(0f, k_AnimationFadeDuration, false);
                }

                m_HandAnimation.enabled = false;
            }
        }

        /// <summary>
        /// Checks the required fields are not null, and logs a Warning otherwise.
        /// </summary>
        private void _CheckFieldsAreNotNull()
        {
            if (m_MoreHelpWindow == null)
            {
                Debug.LogError("MoreHelpWindow is null");
            }

            if (m_GotItButton == null)
            {
                Debug.LogError("GotItButton is null");
            }

            if (m_SnackBarText == null)
            {
                Debug.LogError("SnackBarText is null");
            }

            if (m_SnackBar == null)
            {
                Debug.LogError("SnackBar is null");
            }

            if (m_OpenButton == null)
            {
                Debug.LogError("OpenButton is null");
            }
            else if (m_OpenButton.GetComponent<Button>() == null)
            {
                Debug.LogError("OpenButton does not have a Button Component.");
            }

            if (m_HandAnimation == null)
            {
                Debug.LogError("HandAnimation is null");
            }

            if (m_FeaturePoints == null)
            {
                Debug.LogError("FeaturePoints is null");
            }
        }
    }
}
