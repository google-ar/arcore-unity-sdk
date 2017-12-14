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
    using UnityEngine.Rendering;

    /// <summary>
    /// Controlls the ComputerVision example.
    /// </summary>
    public class ComputerVisionController : MonoBehaviour
    {
        /// <summary>
        /// The TextureReader component instance.
        /// </summary>
        public TextureReader TextureReaderComponent;

        /// <summary>
        /// Background renderer to inject our texture into.
        /// </summary>
        public ARCoreBackgroundRenderer BackgroundRenderer;
        
        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

        /// <summary>
        /// Texture created from filtered camera image.
        /// </summary>
        private Texture2D m_TextureToRender = null;
        private int m_ImageWidth = 0;
        private int m_ImageHeight = 0;
        private byte[] m_EdgeImage = null;
        private float m_SwipeMomentum = 0.0f;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        public void Start()
        {
            // Registers the TextureReader callback.
            TextureReaderComponent.OnImageAvailableCallback += OnImageAvailable;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            _QuitOnConnectionErrors();
            _HandleTouchInput();
        }

        /// <summary>
        /// TextureReader callback handler.
        /// </summary>
        /// <param name="format">The format of the image.</param>
        /// <param name="width">Width of the image, in pixels.</param>
        /// <param name="height">Height of the image, in pixels.</param>
        /// <param name="pixelBuffer">Pointer to raw image buffer.</param>
        /// <param name="bufferSize">The size of the image buffer, in bytes.</param>
        public void OnImageAvailable(TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer, int bufferSize)
        {
            if (format != TextureReaderApi.ImageFormatType.ImageFormatGrayscale)
            {
                Debug.Log("No edge detected due to incorrect image format.");
                return;
            }

            if (m_TextureToRender == null || m_EdgeImage == null || m_ImageWidth != width || m_ImageHeight != height)
            {
                m_TextureToRender = new Texture2D(width, height, TextureFormat.R8, false, false);
                m_EdgeImage = new byte[width * height];
                m_ImageWidth = width;
                m_ImageHeight = height;
            }

            // Detect edges within the image.
            if (EdgeDetector.Detect(m_EdgeImage, pixelBuffer, width, height))
            {
                // Update the rendering texture with the edge image.
                m_TextureToRender.LoadRawTextureData(m_EdgeImage);
                m_TextureToRender.Apply();
                BackgroundRenderer.BackgroundMaterial.SetTexture("_ImageTex", m_TextureToRender);
            }
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private static void _ShowAndroidToastMessage(string message)
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
        /// Handles detecting touch input to control the edge detection effect.
        /// </summary>
        private void _HandleTouchInput()
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

            var overlayPercentage = BackgroundRenderer.BackgroundMaterial.GetFloat("_OverlayPercentage");
            overlayPercentage -= m_SwipeMomentum;
            BackgroundRenderer.BackgroundMaterial.SetFloat("_OverlayPercentage", Mathf.Clamp(overlayPercentage, 0.0f, 1.0f));
        }

        /// <summary>
        /// Gets the delta touch as a percentage of the screen.
        /// </summary>
        /// <returns>The delta touch as a percentage of the screen.</returns>
        private float _GetTouchDelta()
        {
            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    return -Input.GetTouch(0).deltaPosition.x / Screen.width;
                case ScreenOrientation.LandscapeRight:
                    return Input.GetTouch(0).deltaPosition.x / Screen.width;
                case ScreenOrientation.Portrait:
                    return Input.GetTouch(0).deltaPosition.y / Screen.height;
                case ScreenOrientation.PortraitUpsideDown:
                    return -Input.GetTouch(0).deltaPosition.y / Screen.height;
                default:
                    return 0;
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
        /// Quit the application if there was a connection error for the ARCore session.
        /// </summary>
        private void _QuitOnConnectionErrors()
        {
            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
            else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
        }
    }
}
