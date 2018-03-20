//-----------------------------------------------------------------------
// <copyright file="ComputerVisionController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
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

namespace GoogleARCore.TextureReader
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
    /// Controller for the ComputerVision example that accesses the CPU camera image (i.e. image bytes), performs
    /// edge detection on the image, and renders an overlay to the screen.
    /// </summary>
    public class ComputerVisionController : MonoBehaviour
    {
        /// <summary>
        /// An image using a material with EdgeDetectionBackground.shader to render a
        /// percentage of the edge detection background to the screen over the standard camera background.
        /// </summary>
        public Image EdgeDetectionBackgroundImage;

        /// <summary>
        /// When false, uses ARCore's <c>CameraImage.TryAcquireCameraImage</c> for CPU image access at the default
        /// resolution (640x480). When true, enables the attached TextureReader component to copy the camera texture
        /// to the CPU at a custom resolution set on the component. This has the advantage of providing a custom
        /// resolution CPU camera image but incurs latency for the blit to complete.
        /// </summary>
        public bool UseCustomResolutionImage = false;

        /// <summary>
        /// A buffer that stores the result of performing edge detection on the camera image each frame.
        /// </summary>
        private byte[] m_EdgeDetectionResultImage = null;

        /// <summary>
        /// Texture created from the result of running edge detection on the camera image bytes.
        /// </summary>
        private Texture2D m_EdgeDetectionBackgroundTexture = null;

        /// <summary>
        /// These UVs are applied to the background material to crop and rotate 'm_EdgeDetectionBackgroundTexture'
        /// to match the aspect ratio and rotation of the device display.
        /// </summary>
        private DisplayUvCoords m_CameraImageToDisplayUvTransformation;

        private TextureReader m_CachedTextureReader;
        private float m_SwipeMomentum = 0.0f;
        private ScreenOrientation m_CachedOrientation = ScreenOrientation.Unknown;
        private Vector2 m_CachedScreenDimensions = Vector2.zero;
        private bool m_IsQuitting = false;

        /// <summary>
        /// Instant Preview Input does not support deltaPosition, so we keep track of the most recent
        /// touch position and calculate deltas based on this.
        /// TODO (b/74777449): Remove when deltaPosition is fixed.
        /// </summary>
        private Vector2 m_PreviousTouch = Vector2.zero;

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            m_CachedTextureReader = GetComponent<TextureReader>();
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
            _HandleSwipeInput();

            if (!Session.Status.IsValid())
            {
                return;
            }

            if (UseCustomResolutionImage)
            {
                return;
            }

            using (var image = Frame.CameraImage.AcquireCameraImageBytes())
            {
                if (!image.IsAvailable)
                {
                    return;
                }

                _OnImageAvailable(TextureReaderApi.ImageFormatType.ImageFormatGrayscale,
                    image.Width, image.Height, image.Y, 0);
            }
        }

        /// <summary>
        /// Handles the custom resolution checkbox toggle changing.
        /// </summary>
        /// <param name="newValue">The new value for the checkbox.</param>
        public void OnCustomResolutionCheckboxValueChanged(bool newValue)
        {
            UseCustomResolutionImage = newValue;
            if (newValue == true)
            {
                m_CachedTextureReader.enabled = true;
                m_CachedTextureReader.OnImageAvailableCallback += _OnImageAvailable;
            }
            else
            {
                m_CachedTextureReader.enabled = false;
                m_CachedTextureReader.OnImageAvailableCallback -= _OnImageAvailable;
            }
        }

        /// <summary>
        /// Adjusts the percentage of the edge detection overlay shown based on user swipes.
        /// </summary>
        private void _HandleSwipeInput()
        {
            const float SWIPE_SCALING_FACTOR = 1.15f;
            const float INTERTIAL_CANCELING_FACTOR = 2.0f;
            const float MINIMUM_MOMENTUM = .01f;

            if (Input.touchCount == 0)
            {
                m_SwipeMomentum /= INTERTIAL_CANCELING_FACTOR;
            }
            else
            {
                m_SwipeMomentum = _GetTouchDelta();
                m_SwipeMomentum *= SWIPE_SCALING_FACTOR;
            }

            if (Mathf.Abs(m_SwipeMomentum) < MINIMUM_MOMENTUM)
            {
                m_SwipeMomentum = 0;
            }

            var overlayPercentage = EdgeDetectionBackgroundImage.material.GetFloat("_OverlayPercentage");
            overlayPercentage -= m_SwipeMomentum;
            EdgeDetectionBackgroundImage.material.SetFloat("_OverlayPercentage", Mathf.Clamp(overlayPercentage, 0.0f, 1.0f));
        }

        /// <summary>
        /// Gets the delta touch as a percentage of the screen.
        /// </summary>
        /// <returns>The delta touch as a percentage of the screen.</returns>
        private float _GetTouchDelta()
        {
            Vector2 newTouch = Input.GetTouch(0).position;
            Vector2 deltaPosition = newTouch - m_PreviousTouch;
            m_PreviousTouch = newTouch;
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
              return 0;
            }

            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    return -deltaPosition.x / Screen.width;
                case ScreenOrientation.LandscapeRight:
                    return deltaPosition.x / Screen.width;
                case ScreenOrientation.Portrait:
                    return deltaPosition.y / Screen.height;
                case ScreenOrientation.PortraitUpsideDown:
                    return -deltaPosition.y / Screen.height;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Handles a new CPU image.
        /// </summary>
        /// <param name="format">The format of the image.</param>
        /// <param name="width">Width of the image, in pixels.</param>
        /// <param name="height">Height of the image, in pixels.</param>
        /// <param name="pixelBuffer">Pointer to raw image buffer.</param>
        /// <param name="bufferSize">The size of the image buffer, in bytes.</param>
        private void _OnImageAvailable(TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer, int bufferSize)
        {
            if (format != TextureReaderApi.ImageFormatType.ImageFormatGrayscale)
            {
                Debug.Log("No edge detected due to incorrect image format.");
                return;
            }

            if (m_EdgeDetectionBackgroundTexture == null || m_EdgeDetectionResultImage == null ||
                m_EdgeDetectionBackgroundTexture.width != width || m_EdgeDetectionBackgroundTexture.height != height)
            {
                m_EdgeDetectionBackgroundTexture = new Texture2D(width, height, TextureFormat.R8, false, false);
                m_EdgeDetectionResultImage = new byte[width * height];
                _UpdateCameraImageToDisplayUVs();
            }

            if (m_CachedOrientation != Screen.orientation || m_CachedScreenDimensions.x != Screen.width ||
                m_CachedScreenDimensions.y != Screen.height)
            {
                _UpdateCameraImageToDisplayUVs();
                m_CachedOrientation = Screen.orientation;
                m_CachedScreenDimensions = new Vector2(Screen.width, Screen.height);
            }

            // Detect edges within the image.
            if (EdgeDetector.Detect(m_EdgeDetectionResultImage, pixelBuffer, width, height))
            {
                // Update the rendering texture with the edge image.
                m_EdgeDetectionBackgroundTexture.LoadRawTextureData(m_EdgeDetectionResultImage);
                m_EdgeDetectionBackgroundTexture.Apply();
                EdgeDetectionBackgroundImage.material.SetTexture("_ImageTex", m_EdgeDetectionBackgroundTexture);

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
        /// Updates the uv transformation from the camera image orientation and aspect to the display's.
        /// </summary>
        private void _UpdateCameraImageToDisplayUVs()
        {
            int cameraToDisplayRotation = _GetCameraImageToDisplayRotation();

            float uBorder;
            float vBorder;
            _GetUvBorders(out uBorder, out vBorder);

            switch (cameraToDisplayRotation)
            {
            case 90:
                m_CameraImageToDisplayUvTransformation.TopLeft = new Vector2(1 - uBorder, 1 - vBorder);
                m_CameraImageToDisplayUvTransformation.TopRight = new Vector2(1 - uBorder, vBorder);
                m_CameraImageToDisplayUvTransformation.BottomRight = new Vector2(uBorder, vBorder);
                m_CameraImageToDisplayUvTransformation.BottomLeft = new Vector2(uBorder, 1 - vBorder);
                break;
            case 180:
                m_CameraImageToDisplayUvTransformation.TopLeft = new Vector2(uBorder, 1 - vBorder);
                m_CameraImageToDisplayUvTransformation.TopRight = new Vector2(1 - uBorder, 1 - vBorder);
                m_CameraImageToDisplayUvTransformation.BottomRight = new Vector2(1 - uBorder, vBorder);
                m_CameraImageToDisplayUvTransformation.BottomLeft = new Vector2(uBorder, vBorder);
                break;
            case 270:
                m_CameraImageToDisplayUvTransformation.TopLeft = new Vector2(uBorder, vBorder);
                m_CameraImageToDisplayUvTransformation.TopRight = new Vector2(uBorder, 1 - vBorder);
                m_CameraImageToDisplayUvTransformation.BottomRight = new Vector2(1 - uBorder, 1 - vBorder);
                m_CameraImageToDisplayUvTransformation.BottomLeft = new Vector2(1 - uBorder, vBorder);
                break;
            default:
            case 0:
                m_CameraImageToDisplayUvTransformation.TopLeft = new Vector2(1 - uBorder, vBorder);
                m_CameraImageToDisplayUvTransformation.TopRight = new Vector2(uBorder, vBorder);
                m_CameraImageToDisplayUvTransformation.BottomRight = new Vector2(uBorder, 1 - vBorder);
                m_CameraImageToDisplayUvTransformation.BottomLeft = new Vector2(1 - uBorder, 1 - vBorder);
                break;
            }
        }

        /// <summary>
        /// Gets the rotation that needs to be applied to the device camera image in order for it to match
        /// the current orientation of the display.
        /// </summary>
        /// <returns>The needed rotation.</returns>
        private int _GetCameraImageToDisplayRotation()
        {
#if !UNITY_EDITOR
            AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
            AndroidJavaClass cameraInfoClass = new AndroidJavaClass("android.hardware.Camera$CameraInfo");
            AndroidJavaObject cameraInfo = new AndroidJavaObject("android.hardware.Camera$CameraInfo");
            cameraClass.CallStatic("getCameraInfo", cameraInfoClass.GetStatic<int>("CAMERA_FACING_BACK"),
                cameraInfo);
            int cameraRotationToNaturalDisplayOrientation = cameraInfo.Get<int>("orientation");

            AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject windowManager =
                unityActivity.Call<AndroidJavaObject>("getSystemService",
                contextClass.GetStatic<string>("WINDOW_SERVICE"));

            AndroidJavaClass surfaceClass = new AndroidJavaClass("android.view.Surface");
            int displayRotationFromNaturalEnum = windowManager
                .Call<AndroidJavaObject>("getDefaultDisplay").Call<int>("getRotation");

            int displayRotationFromNatural = 0;
            if (displayRotationFromNaturalEnum == surfaceClass.GetStatic<int>("ROTATION_90"))
            {
                displayRotationFromNatural = 90;
            }
            else if (displayRotationFromNaturalEnum == surfaceClass.GetStatic<int>("ROTATION_180"))
            {
                displayRotationFromNatural = 180;
            }
            else if (displayRotationFromNaturalEnum == surfaceClass.GetStatic<int>("ROTATION_270"))
            {
                displayRotationFromNatural = 270;
            }

            return (cameraRotationToNaturalDisplayOrientation + displayRotationFromNatural) % 360;
#else  // !UNITY_EDITOR
            // Using Instant Preview in the Unity Editor, the display orientation is always portrait.
            return 0;
#endif  // !UNITY_EDITOR
        }

        /// <summary>
        /// Gets the percentage of space needed to be cropped on the device camera image to match the display
        /// aspect ratio.
        /// </summary>
        /// <param name="uBorder">The cropping of the 'u' dimension.</param>
        /// <param name="vBorder">The cropping of the 'v' dimension.<</param>
        private void _GetUvBorders(out float uBorder, out float vBorder)
        {
            int imageWidth = m_EdgeDetectionBackgroundTexture.width;
            int imageHeight = m_EdgeDetectionBackgroundTexture.height;

            float screenAspectRatio;
            var cameraToDisplayRotation = _GetCameraImageToDisplayRotation();
            if (cameraToDisplayRotation == 90 || cameraToDisplayRotation == 270)
            {
                screenAspectRatio = (float)Screen.height / Screen.width;
            }
            else
            {
                screenAspectRatio = (float)Screen.width / Screen.height;
            }

            var imageAspectRatio = (float)imageWidth / imageHeight;
            var croppedWidth = 0.0f;
            var croppedHeight = 0.0f;

            if (screenAspectRatio < imageAspectRatio)
            {
                croppedWidth = imageHeight * screenAspectRatio;
                croppedHeight = imageHeight;
            }
            else
            {
                croppedWidth = imageWidth;
                croppedHeight = imageWidth / screenAspectRatio;
            }

            uBorder = (imageWidth - croppedWidth) / imageWidth / 2.0f;
            vBorder = (imageHeight - croppedHeight) / imageHeight / 2.0f;
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

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status == SessionStatus.FatalError)
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
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
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
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
    }
}
