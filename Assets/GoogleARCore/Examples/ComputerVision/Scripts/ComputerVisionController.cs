//-----------------------------------------------------------------------
// <copyright file="ComputerVisionController.cs" company="Google LLC">
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
        private static float _frameRateUpdateInterval = 2.0f;

        /// <summary>
        /// A buffer that stores the result of performing edge detection on the camera image each
        /// frame.
        /// </summary>
        private byte[] _edgeDetectionResultImage = null;

        /// <summary>
        /// Texture created from the result of running edge detection on the camera image bytes.
        /// </summary>
        private Texture2D _edgeDetectionBackgroundTexture = null;

        /// <summary>
        /// These UVs are applied to the background material to crop and rotate
        /// '_edgeDetectionBackgroundTexture' to match the aspect ratio and rotation of the device
        /// display.
        /// </summary>
        private DisplayUvCoords _cameraImageToDisplayUvTransformation;

        private ScreenOrientation? _cachedOrientation = null;
        private Vector2 _cachedScreenDimensions = Vector2.zero;
        private bool _isQuitting = false;
        private bool _useHighResCPUTexture = false;
        private ARCoreSession.OnChooseCameraConfigurationDelegate _onChoseCameraConfiguration =
            null;

        private int _highestResolutionConfigIndex = 0;
        private int _lowestResolutionConfigIndex = 0;
        private bool _resolutioninitialized = false;
        private Text _imageTextureToggleText;
        private float _renderingFrameRate = 0f;
        private float _renderingFrameTime = 0f;
        private int _frameCounter = 0;
        private float _framePassedTime = 0.0f;

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
            _onChoseCameraConfiguration = ChooseCameraConfiguration;
            ARSessionManager.RegisterChooseCameraConfigurationCallback(
                _onChoseCameraConfiguration);
        }

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            ImageTextureToggle.OnPointClickDetected += OnBackgroundClicked;

            _imageTextureToggleText = ImageTextureToggle.GetComponentInChildren<Text>();
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

            QuitOnConnectionErrors();
            UpdateFrameRate();

            // Change the CPU resolution checkbox visibility.
            LowResConfigToggle.gameObject.SetActive(EdgeDetectionBackgroundImage.enabled);
            HighResConfigToggle.gameObject.SetActive(EdgeDetectionBackgroundImage.enabled);
            _imageTextureToggleText.text = EdgeDetectionBackgroundImage.enabled ?
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

                OnImageAvailable(image.Width, image.Height, image.YRowStride, image.Y, 0);
            }

            var cameraIntrinsics = EdgeDetectionBackgroundImage.enabled
                ? Frame.CameraImage.ImageIntrinsics : Frame.CameraImage.TextureIntrinsics;
            string intrinsicsType =
                EdgeDetectionBackgroundImage.enabled ? "CPU Image" : "GPU Texture";
            CameraIntrinsicsOutput.text =
                CameraIntrinsicsToString(cameraIntrinsics, intrinsicsType);
        }

        /// <summary>
        /// Handles the low resolution checkbox toggle changing.
        /// </summary>
        /// <param name="newValue">The new value for the checkbox.</param>
        public void OnLowResolutionCheckboxValueChanged(bool newValue)
        {
            _useHighResCPUTexture = !newValue;
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
            _useHighResCPUTexture = newValue;
            LowResConfigToggle.isOn = !newValue;

            // Pause and resume the ARCore session to apply the camera configuration.
            ARSessionManager.enabled = false;
            ARSessionManager.enabled = true;
        }

        /// <summary>
        /// Handles the auto focus checkbox value changed.
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
        private void OnBackgroundClicked()
        {
            EdgeDetectionBackgroundImage.enabled = !EdgeDetectionBackgroundImage.enabled;
        }

        private void UpdateFrameRate()
        {
            _frameCounter++;
            _framePassedTime += Time.deltaTime;
            if (_framePassedTime > _frameRateUpdateInterval)
            {
                _renderingFrameTime = 1000 * _framePassedTime / _frameCounter;
                _renderingFrameRate = 1000 / _renderingFrameTime;
                _framePassedTime = 0f;
                _frameCounter = 0;
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
        private void OnImageAvailable(
            int width, int height, int rowStride, IntPtr pixelBuffer, int bufferSize)
        {
            if (!EdgeDetectionBackgroundImage.enabled)
            {
                return;
            }

            if (_edgeDetectionBackgroundTexture == null ||
                _edgeDetectionResultImage == null ||
                _edgeDetectionBackgroundTexture.width != width ||
                _edgeDetectionBackgroundTexture.height != height)
            {
                _edgeDetectionBackgroundTexture =
                    new Texture2D(width, height, TextureFormat.R8, false, false);
                _edgeDetectionResultImage = new byte[width * height];
                _cameraImageToDisplayUvTransformation = Frame.CameraImage.ImageDisplayUvs;
            }

            if (_cachedOrientation != Screen.orientation ||
                _cachedScreenDimensions.x != Screen.width ||
                _cachedScreenDimensions.y != Screen.height)
            {
                _cameraImageToDisplayUvTransformation = Frame.CameraImage.ImageDisplayUvs;
                _cachedOrientation = Screen.orientation;
                _cachedScreenDimensions = new Vector2(Screen.width, Screen.height);
            }

            // Detect edges within the image.
            if (EdgeDetector.Detect(
                _edgeDetectionResultImage, pixelBuffer, width, height, rowStride))
            {
                // Update the rendering texture with the edge image.
                _edgeDetectionBackgroundTexture.LoadRawTextureData(_edgeDetectionResultImage);
                _edgeDetectionBackgroundTexture.Apply();
                EdgeDetectionBackgroundImage.material.SetTexture(
                    "_ImageTex", _edgeDetectionBackgroundTexture);

                const string TOP_LEFT_RIGHT = "_UvTopLeftRight";
                const string BOTTOM_LEFT_RIGHT = "_UvBottomLeftRight";
                EdgeDetectionBackgroundImage.material.SetVector(TOP_LEFT_RIGHT, new Vector4(
                    _cameraImageToDisplayUvTransformation.TopLeft.x,
                    _cameraImageToDisplayUvTransformation.TopLeft.y,
                    _cameraImageToDisplayUvTransformation.TopRight.x,
                    _cameraImageToDisplayUvTransformation.TopRight.y));
                EdgeDetectionBackgroundImage.material.SetVector(BOTTOM_LEFT_RIGHT, new Vector4(
                    _cameraImageToDisplayUvTransformation.BottomLeft.x,
                    _cameraImageToDisplayUvTransformation.BottomLeft.y,
                    _cameraImageToDisplayUvTransformation.BottomRight.x,
                    _cameraImageToDisplayUvTransformation.BottomRight.y));
            }
        }

        /// <summary>
        /// Quit the application if there was a connection error for the ARCore session.
        /// </summary>
        private void QuitOnConnectionErrors()
        {
            if (_isQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                ShowAndroidToastMessage("Camera permission is needed to run this application.");
                _isQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
            else if (Session.Status == SessionStatus.FatalError)
            {
                ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                _isQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void ShowAndroidToastMessage(string message)
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
        private void DoQuit()
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
        private string CameraIntrinsicsToString(CameraIntrinsics intrinsics, string intrinsicsType)
        {
            float fovX = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
                intrinsics.ImageDimensions.x, 2 * intrinsics.FocalLength.x);
            float fovY = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
                intrinsics.ImageDimensions.y, 2 * intrinsics.FocalLength.y);

            string frameRateTime = _renderingFrameRate < 1 ? "Calculating..." :
                string.Format("{0}ms ({1}fps)", _renderingFrameTime.ToString("0.0"),
                    _renderingFrameRate.ToString("0.0"));

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
        private int ChooseCameraConfiguration(List<CameraConfig> supportedConfigurations)
        {
            if (!_resolutioninitialized)
            {
                _highestResolutionConfigIndex = 0;
                _lowestResolutionConfigIndex = 0;
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
                        _highestResolutionConfigIndex = index;
                        maximalConfig = config;
                    }

                    if ((config.ImageSize.x < minimalConfig.ImageSize.x &&
                         config.ImageSize.y < minimalConfig.ImageSize.y) ||
                        (config.ImageSize.x == minimalConfig.ImageSize.x &&
                         config.ImageSize.y == minimalConfig.ImageSize.y &&
                         config.MaxFPS > minimalConfig.MaxFPS))
                    {
                        _lowestResolutionConfigIndex = index;
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
                _resolutioninitialized = true;
            }

            if (_useHighResCPUTexture)
            {
                return _highestResolutionConfigIndex;
            }

            return _lowestResolutionConfigIndex;
        }
    }
}
