//-----------------------------------------------------------------------
// <copyright file="ComputerVisionController.cs" company="Google">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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

namespace GoogleARCore.Examples.ComputerVision
{
    using System;
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.UI;

    #if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
    #endif  // UNITY_EDITOR

    /// <summary>
    /// Controller for the ComputerVision example that accesses the CPU camera image (i.e. image
    /// bytes), performs edge detection on the image, and renders an overlay to the screen.
    /// </summary>
    public class ComputerVisionController : MonoBehaviour
    {
        /// <summary>
        /// The ARCoreSession monobehavior that manages the ARCore session.
        /// </summary>
        public ARCoreSession ARSessionManager;

        /// <summary>
        /// An image using a material with EdgeDetectionBackground.shader to render a percentage of
        /// the edge detection background to the screen over the standard camera background.
        /// </summary>
        public Image EdgeDetectionBackgroundImage;

        /// <summary>
        /// A Text box that is used to output the camera intrinsics values.
        /// </summary>
        public Text CameraIntrinsicsOutput;

        /// <summary>
        /// A Text box that is used to show messages at runtime.
        /// </summary>
        public Text SnackbarText;

        /// <summary>
        /// A toggle that is used to select the low resolution CPU camera configuration.
        /// </summary>
        public Toggle LowResConfigToggle;

        /// <summary>
        /// A toggle that is used to select the high resolution CPU camera configuration.
        /// </summary>
        public Toggle HighResConfigToggle;

        /// <summary>
        /// A toggle that is used to toggle between CPU image and GPU texture.
        /// </summary>
        public PointClickHandler ImageTextureToggle;

        /// <summary>
        /// A toggle that is used to toggle between Fixed and Auto focus modes.
        /// </summary>
        public Toggle AutoFocusToggle;

        /// <summary>
        /// The frame rate update interval.
        /// </summary>
        private static float s_FrameRateUpdateInterval = 2.0f;

        /// <summary>
        /// A buffer that stores the result of performing edge detection on the camera image each
        /// frame.
        /// </summary>
        private byte[] m_EdgeDetectionResultImage = null;

        /// <summary>
        /// Texture created from the result of running edge detection on the camera image bytes.
        /// </summary>
        private Texture2D m_EdgeDetectionBackgroundTexture = null;

        /// <summary>
        /// These UVs are applied to the background material to crop and rotate
        /// 'm_EdgeDetectionBackgroundTexture' to match the aspect ratio and rotation of the device
        /// display.
        /// </summary>
        private DisplayUvCoords m_CameraImageToDisplayUvTransformation;

        private ScreenOrientation? m_CachedOrientation = null;
        private Vector2 m_CachedScreenDimensions = Vector2.zero;
        private bool m_IsQuitting = false;
        private bool m_UseHighResCPUTexture = false;
        private ARCoreSession.OnChooseCameraConfigurationDelegate m_OnChoseCameraConfiguration =
            null;

        private int m_HighestResolutionConfigIndex = 0;
        private int m_LowestResolutionConfigIndex = 0;
        private bool m_Resolutioninitialized = false;
        private Text m_ImageTextureToggleText;
        private float m_RenderingFrameRate = 0f;
        private float m_RenderingFrameTime = 0f;
        private int m_FrameCounter = 0;
        private float m_FramePassedTime = 0.0f;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            // Lock screen to portrait.
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.Portrait;

            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;

            // Register the callback to set camera config before arcore session is enabled.
            m_OnChoseCameraConfiguration = _ChooseCameraConfiguration;
            ARSessionManager.RegisterChooseCameraConfigurationCallback(
                m_OnChoseCameraConfiguration);
        }

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            ImageTextureToggle.OnPointClickDetected += _OnBackgroundClicked;

            m_ImageTextureToggleText = ImageTextureToggle.GetComponentInChildren<Text>();
#if UNITY_EDITOR
            AutoFocusToggle.GetComponentInChildren<Text>().text += "\n(Not supported in editor)";
            HighResConfigToggle.GetComponentInChildren<Text>().text +=
                "\n(Not supported in editor)";
            SnackbarText.text =
                "Use mouse/keyboard in the editor Game view to toggle settings.\n" +
                "(Tapping on the device screen will not work while running in the editor)";
#else
            SnackbarText.text = string.Empty;
#endif
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            _QuitOnConnectionErrors();
            _UpdateFrameRate();

            // Change the CPU resolution checkbox visibility.
            LowResConfigToggle.gameObject.SetActive(EdgeDetectionBackgroundImage.enabled);
            HighResConfigToggle.gameObject.SetActive(EdgeDetectionBackgroundImage.enabled);
            m_ImageTextureToggleText.text = EdgeDetectionBackgroundImage.enabled ?
                    "Switch to GPU Texture" : "Switch to CPU Image";

            if (!Session.Status.IsValid())
            {
                return;
            }

            using (var image = Frame.CameraImage.AcquireCameraImageBytes())
            {
                if (!image.IsAvailable)
                {
                    return;
                }

                _OnImageAvailable(image.Width, image.Height, image.YRowStride, image.Y, 0);
            }

            var cameraIntrinsics = EdgeDetectionBackgroundImage.enabled
                ? Frame.CameraImage.ImageIntrinsics : Frame.CameraImage.TextureIntrinsics;
            string intrinsicsType =
                EdgeDetectionBackgroundImage.enabled ? "CPU Image" : "GPU Texture";
            CameraIntrinsicsOutput.text =
                _CameraIntrinsicsToString(cameraIntrinsics, intrinsicsType);
        }

        /// <summary>
        /// Handles the low resolution checkbox toggle changing.
        /// </summary>
        /// <param name="newValue">The new value for the checkbox.</param>
        public void OnLowResolutionCheckboxValueChanged(bool newValue)
        {
            m_UseHighResCPUTexture = !newValue;
            HighResConfigToggle.isOn = !newValue;

            // Pause and resume the ARCore session to apply the camera configuration.
            ARSessionManager.enabled = false;
            ARSessionManager.enabled = true;
        }

        /// <summary>
        /// Handles the high resolution checkbox toggle changing.
        /// </summary>
        /// <param name="newValue">The new value for the checkbox.</param>
        public void OnHighResolutionCheckboxValueChanged(bool newValue)
        {
            m_UseHighResCPUTexture = newValue;
            LowResConfigToggle.isOn = !newValue;

            // Pause and resume the ARCore session to apply the camera configuration.
            ARSessionManager.enabled = false;
            ARSessionManager.enabled = true;
        }

        /// <summary>
        /// Hanldes the auto focus checkbox value changed.
        /// </summary>
        /// <param name="autoFocusEnabled">If set to <c>true</c> auto focus will be enabled.</param>
        public void OnAutoFocusCheckboxValueChanged(bool autoFocusEnabled)
        {
            var config = ARSessionManager.SessionConfig;
            if (config != null)
            {
                config.CameraFocusMode =
                    autoFocusEnabled ? CameraFocusMode.AutoFocus : CameraFocusMode.FixedFocus;
            }
        }

        /// <summary>
        /// Function get called when the background image got clicked.
        /// </summary>
        private void _OnBackgroundClicked()
        {
            EdgeDetectionBackgroundImage.enabled = !EdgeDetectionBackgroundImage.enabled;
        }

        private void _UpdateFrameRate()
        {
            m_FrameCounter++;
            m_FramePassedTime += Time.deltaTime;
            if (m_FramePassedTime > s_FrameRateUpdateInterval)
            {
                m_RenderingFrameTime = 1000 * m_FramePassedTime / m_FrameCounter;
                m_RenderingFrameRate = 1000 / m_RenderingFrameTime;
                m_FramePassedTime = 0f;
                m_FrameCounter = 0;
            }
        }

        /// <summary>
        /// Handles a new CPU image.
        /// </summary>
        /// <param name="width">Width of the image, in pixels.</param>
        /// <param name="height">Height of the image, in pixels.</param>
        /// <param name="rowStride">Row stride of the image, in pixels.</param>
        /// <param name="pixelBuffer">Pointer to raw image buffer.</param>
        /// <param name="bufferSize">The size of the image buffer, in bytes.</param>
        private void _OnImageAvailable(
            int width, int height, int rowStride, IntPtr pixelBuffer, int bufferSize)
        {
            if (!EdgeDetectionBackgroundImage.enabled)
            {
                return;
            }

            if (m_EdgeDetectionBackgroundTexture == null ||
                m_EdgeDetectionResultImage == null ||
                m_EdgeDetectionBackgroundTexture.width != width ||
                m_EdgeDetectionBackgroundTexture.height != height)
            {
                m_EdgeDetectionBackgroundTexture =
                    new Texture2D(width, height, TextureFormat.R8, false, false);
                m_EdgeDetectionResultImage = new byte[width * height];
                m_CameraImageToDisplayUvTransformation = Frame.CameraImage.ImageDisplayUvs;
            }

            if (m_CachedOrientation != Screen.orientation ||
                m_CachedScreenDimensions.x != Screen.width ||
                m_CachedScreenDimensions.y != Screen.height)
            {
                m_CameraImageToDisplayUvTransformation = Frame.CameraImage.ImageDisplayUvs;
                m_CachedOrientation = Screen.orientation;
                m_CachedScreenDimensions = new Vector2(Screen.width, Screen.height);
            }

            // Detect edges within the image.
            if (EdgeDetector.Detect(
                m_EdgeDetectionResultImage, pixelBuffer, width, height, rowStride))
            {
                // Update the rendering texture with the edge image.
                m_EdgeDetectionBackgroundTexture.LoadRawTextureData(m_EdgeDetectionResultImage);
                m_EdgeDetectionBackgroundTexture.Apply();
                EdgeDetectionBackgroundImage.material.SetTexture(
                    "_ImageTex", m_EdgeDetectionBackgroundTexture);

                const string TOP_LEFT_RIGHT = "_UvTopLeftRight";
                const string BOTTOM_LEFT_RIGHT = "_UvBottomLeftRight";
                EdgeDetectionBackgroundImage.material.SetVector(TOP_LEFT_RIGHT, new Vector4(
                    m_CameraImageToDisplayUvTransformation.TopLeft.x,
                    m_CameraImageToDisplayUvTransformation.TopLeft.y,
                    m_CameraImageToDisplayUvTransformation.TopRight.x,
                    m_CameraImageToDisplayUvTransformation.TopRight.y));
                EdgeDetectionBackgroundImage.material.SetVector(BOTTOM_LEFT_RIGHT, new Vector4(
                    m_CameraImageToDisplayUvTransformation.BottomLeft.x,
                    m_CameraImageToDisplayUvTransformation.BottomLeft.y,
                    m_CameraImageToDisplayUvTransformation.BottomRight.x,
                    m_CameraImageToDisplayUvTransformation.BottomRight.y));
            }
        }

        /// <summary>
        /// Quit the application if there was a connection error for the ARCore session.
        /// </summary>
        private void _QuitOnConnectionErrors()
        {
            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status == SessionStatus.FatalError)
            {
                _ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Generate string to print the value in CameraIntrinsics.
        /// </summary>
        /// <param name="intrinsics">The CameraIntrinsics to generate the string from.</param>
        /// <param name="intrinsicsType">The string that describe the type of the
        /// intrinsics.</param>
        /// <returns>The generated string.</returns>
        private string _CameraIntrinsicsToString(CameraIntrinsics intrinsics, string intrinsicsType)
        {
            float fovX = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
                intrinsics.ImageDimensions.x, 2 * intrinsics.FocalLength.x);
            float fovY = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
                intrinsics.ImageDimensions.y, 2 * intrinsics.FocalLength.y);

            string frameRateTime = m_RenderingFrameRate < 1 ? "Calculating..." :
                string.Format("{0}ms ({1}fps)", m_RenderingFrameTime.ToString("0.0"),
                    m_RenderingFrameRate.ToString("0.0"));

            string message = string.Format(
                "Unrotated Camera {4} Intrinsics:{0}  Focal Length: {1}{0}  " +
                "Principal Point: {2}{0}  Image Dimensions: {3}{0}  " +
                "Unrotated Field of View: ({5}°, {6}°){0}" +
                "Render Frame Time: {7}",
                Environment.NewLine, intrinsics.FocalLength.ToString(),
                intrinsics.PrincipalPoint.ToString(), intrinsics.ImageDimensions.ToString(),
                intrinsicsType, fovX, fovY, frameRateTime);
            return message;
        }

        /// <summary>
        /// Select the desired camera configuration.
        /// If high resolution toggle is checked, select the camera configuration
        /// with highest cpu image and highest FPS.
        /// If low resolution toggle is checked, select the camera configuration
        /// with lowest CPU image and highest FPS.
        /// </summary>
        /// <param name="supportedConfigurations">A list of all supported camera
        /// configuration.</param>
        /// <returns>The desired configuration index.</returns>
        private int _ChooseCameraConfiguration(List<CameraConfig> supportedConfigurations)
        {
            if (!m_Resolutioninitialized)
            {
                m_HighestResolutionConfigIndex = 0;
                m_LowestResolutionConfigIndex = 0;
                CameraConfig maximalConfig = supportedConfigurations[0];
                CameraConfig minimalConfig = supportedConfigurations[0];
                for (int index = 1; index < supportedConfigurations.Count; index++)
                {
                    CameraConfig config = supportedConfigurations[index];
                    if ((config.ImageSize.x > maximalConfig.ImageSize.x &&
                         config.ImageSize.y > maximalConfig.ImageSize.y) ||
                        (config.ImageSize.x == maximalConfig.ImageSize.x &&
                         config.ImageSize.y == maximalConfig.ImageSize.y &&
                         config.MaxFPS > maximalConfig.MaxFPS))
                    {
                        m_HighestResolutionConfigIndex = index;
                        maximalConfig = config;
                    }

                    if ((config.ImageSize.x < minimalConfig.ImageSize.x &&
                         config.ImageSize.y < minimalConfig.ImageSize.y) ||
                        (config.ImageSize.x == minimalConfig.ImageSize.x &&
                         config.ImageSize.y == minimalConfig.ImageSize.y &&
                         config.MaxFPS > minimalConfig.MaxFPS))
                    {
                        m_LowestResolutionConfigIndex = index;
                        minimalConfig = config;
                    }
                }

                LowResConfigToggle.GetComponentInChildren<Text>().text = string.Format(
                    "Low Resolution CPU Image ({0} x {1}), Target FPS: ({2} - {3}), " +
                    "Depth Sensor Usage: {4}",
                    minimalConfig.ImageSize.x, minimalConfig.ImageSize.y,
                    minimalConfig.MinFPS, minimalConfig.MaxFPS, minimalConfig.DepthSensorUsage);
                HighResConfigToggle.GetComponentInChildren<Text>().text = string.Format(
                    "High Resolution CPU Image ({0} x {1}), Target FPS: ({2} - {3}), " +
                    "Depth Sensor Usage: {4}",
                    maximalConfig.ImageSize.x, maximalConfig.ImageSize.y,
                    maximalConfig.MinFPS, maximalConfig.MaxFPS, maximalConfig.DepthSensorUsage);
                m_Resolutioninitialized = true;
            }

            if (m_UseHighResCPUTexture)
            {
                return m_HighestResolutionConfigIndex;
            }

            return m_LowestResolutionConfigIndex;
        }
    }
}
