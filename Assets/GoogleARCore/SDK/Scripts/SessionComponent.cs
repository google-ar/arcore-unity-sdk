//-----------------------------------------------------------------------
// <copyright file="SessionComponent.cs" company="Google">
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

namespace GoogleARCore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GoogleARCoreInternal;
    /// @cond EXCLUDE_FROM_DOXYGEN
    using UnityTango = GoogleAR.UnityNative;
    /// @endcond

    /// <summary>
    /// A component that manages the ARCore Session in a Unity scene.
    /// </summary>
    public class SessionComponent : MonoBehaviour
    {
        /// <summary>
        /// A scriptable object specifying tango service connection settings.
        /// </summary>
        [Tooltip("A scriptable object specifying tango service connection settings.")]
        public SessionConfig m_arSessionConfig;

        /// <summary>
        /// A camera used to render the scene from a first person view from the device.
        /// </summary>
        [Tooltip("A camera used to render the first person view from the Tango device.")]
        public Camera m_firstPersonCamera;

        /// <summary>
        /// Toggles whether the tango service should be automatically connected upon Awake.
        /// </summary>
        [Tooltip("Toggles whether the tango service should be automatically connected upon Awake.")]
        public bool m_connectOnAwake = true;

        /// <summary>
        /// The last known screen orientation.
        /// </summary>
        private ScreenOrientation m_cachedScreenOrientation = ScreenOrientation.Unknown;

        /// <summary>
        /// Indicates if the AR background is set up.
        /// </summary>
        private bool m_arBackgroundSetup = false;

        private int m_numberOfRecoveryAttempts;

        /// <summary>
        /// Gets a value indicating whether the first person camera is active.
        /// </summary>
        private bool IsFirstPersonCameraActive
        {
            get
            {
                return m_firstPersonCamera != null && m_firstPersonCamera.gameObject.activeInHierarchy;
            }
        }

        /// <summary>
        /// Unity Awake method.
        /// </summary>
        public void Awake()
        {
            if (FindObjectsOfType<SessionComponent>().Length > 1)
            {
                ARDebug.LogError("Multiple SessionComponents present in the game scene.  Destroying the gameobject " +
                 "of the newest one.");
                Destroy(gameObject);
                return;
            }

            // Launches a temporary recovery daemon for a known race condition in ARCore connection code.
            StartCoroutine(_AttemptRecoverIfInvalidState());

            if (m_connectOnAwake)
            {
                Connect();
            }
        }

        /// <summary>
        /// Unity Update method.
        /// </summary>
        public void Update()
        {
            // Unity defaults to targeting 30fps on Android which makes the color image look stuttery since it does
            // not come in at a perfect every 33ms and so every so often misses a frame. Target an extremely high
            // frame rate so that Unity instead updates every vblank.
            Application.targetFrameRate = 300;
            QualitySettings.vSyncCount = 0;

            AsyncTask.EarlyUpdate();

            if (SessionManager.ConnectionState != SessionConnectionState.Connected)
            {
                return;
            }

            SessionManager.Instance.EarlyUpdate();

            // Handle camera FOV changes needed upon orientation change when using the video overlay.
            if (m_arBackgroundSetup && m_cachedScreenOrientation != Screen.orientation)
            {
                _UpdateCameraFOV();
            }
        }

        public void OnApplicationPause(bool isPaused)
        {
            if (SessionManager.ConnectionState != SessionConnectionState.Connected)
            {
                return;
            }

            SessionManager.Instance.OnApplicationPause(isPaused);
        }

        /// <summary>
        /// Connects an ARSession using <c>m_arSessionConfig</c> configuration. Note that if user permissions are needed
        ///they will be requested and thus this is an asynchronous method.
        /// </summary>
         /// <returns>An <c>AsyncTask</c> that completes when the connection has been made or failed. </returns>

        public AsyncTask<SessionConnectionState> Connect()
        {
            return Connect(m_arSessionConfig);
        }

        /// <summary>
        /// Connects an ARSession.  Note that if user permissions are needed they will be requested and thus this is an
        /// asynchronous method.
        /// </summary>
        /// <param name="sessionConfig">The session configuration.</param>
        /// <returns>An <c>AsyncTask</c> that completes when the connection has been made or failed. </returns>
        public AsyncTask<SessionConnectionState> Connect(SessionConfig sessionConfig)
        {
            const string ANDROID_CAMERA_PERMISSION_NAME = "android.permission.CAMERA";

            if (sessionConfig == null)
            {
                ARDebug.LogError("Unable to connect ARSession session due to missing ARSessionConfig.");
                SessionManager.ConnectionState = SessionConnectionState.MissingConfiguration;
                return new AsyncTask<SessionConnectionState>(SessionManager.ConnectionState);
            }

            bool isSupported;
            ApiServiceErrorStatus status = TangoClientApi.TangoService_IsSupported(out isSupported);
            if (status.IsTangoFailure())
            {
                ARDebug.LogError("There was an error accessing the ARCore API.");
                SessionManager.ConnectionState = SessionConnectionState.ConnectToServiceFailed;
                return new AsyncTask<SessionConnectionState>(SessionManager.ConnectionState);
            }
            if (!isSupported)
            {
                ARDebug.LogError("Device does not support ARCore.");
                SessionManager.ConnectionState = SessionConnectionState.DeviceNotSupported;
                return new AsyncTask<SessionConnectionState>(SessionManager.ConnectionState);
            }

            // We have already connected at least once.
            if (SessionManager.ConnectionState != SessionConnectionState.Uninitialized)
            {
                ARDebug.LogError("Multiple attempts to connect to the ARSession.  Note that the ARSession connection " +
                    "spans the lifetime of the application and cannot be reconfigured.  This will change in future " +
                    "versions of ARCore.");
                return new AsyncTask<SessionConnectionState>(SessionManager.ConnectionState);
            }

            // Create an asynchronous task for the potential permissions flow and service connection.
            Action<SessionConnectionState> onTaskComplete;
            var returnTask = new AsyncTask<SessionConnectionState>(out onTaskComplete);

            // Attempt service connection immediately if permissions are granted.
            if (AndroidPermissionsManager.IsPermissionGranted(ANDROID_CAMERA_PERMISSION_NAME))
            {
                _ConnectToService(sessionConfig, onTaskComplete);
                return returnTask;
            }

            // Request needed permissions and attempt service connection if granted.
            var permissionsArray = new string[] { ANDROID_CAMERA_PERMISSION_NAME };
            AndroidPermissionsManager.RequestPermission(permissionsArray).ThenAction((requestResult) => {
                if (requestResult.IsAllGranted)
                {
                    _ConnectToService(sessionConfig, onTaskComplete);
                }
                else
                {
                    ARDebug.LogError("ARCore connection failed because a needed permission was rejected.");
                    SessionManager.ConnectionState = SessionConnectionState.UserRejectedNeededPermission;
                    onTaskComplete(SessionManager.ConnectionState);
                }
            });

            return returnTask;
        }

        /// <summary>
        /// Connects to the ARCore service.
        /// </summary>
        /// <param name="sessionConfig">The session configuration to connect with.</param>
        /// <param name="onComplete">A callback for when the result of the connection attempt is known.</param>
        private void _ConnectToService(SessionConfig sessionConfig, Action<SessionConnectionState> onComplete)
        {
            // Connect the ARCore session.
            UnityTango.Config tangoConfig = _GetSessionTangoConfiguration(sessionConfig);
            if (!UnityTango.Device.Connect(tangoConfig))
            {
                ARDebug.LogError("Failed to connect the ARSession.");
                SessionManager.ConnectionState = SessionConnectionState.ConnectToServiceFailed;
                onComplete(SessionConnectionState.ConnectToServiceFailed);
                return;
            }

            if(sessionConfig.m_enableARBackground)
            {
                _SetupVideoOverlay();
            }

            SessionManager.ConnectionState = SessionConnectionState.Connected;
            onComplete(SessionManager.ConnectionState);
        }

        /// <summary>
        /// Updates the first person camera FOV to match the field of view of the video overlay.
        /// </summary>
        private void _UpdateCameraFOV()
        {
            bool qurerySuccess = false;
            float fieldOfView;

            if (Screen.orientation == ScreenOrientation.Portrait ||
                Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            {
                qurerySuccess = UnityTango.Device.TryGetHorizontalFov(out fieldOfView);
            }
            else
            {
                qurerySuccess = UnityTango.Device.TryGetVerticalFov(out fieldOfView);
            }

            if (qurerySuccess)
            {
                m_firstPersonCamera.fieldOfView = fieldOfView;
                m_cachedScreenOrientation = Screen.orientation;
            }
        }

        /// <summary>
        /// Sets up the video overlay rendering.
        /// </summary>
        private void _SetupVideoOverlay()
        {
            if (m_arBackgroundSetup)
            {
                return;
            }

            if (!IsFirstPersonCameraActive)
            {
                Debug.LogError("The first person camera must be set for video overlay to work.");
                return;
            }

            var backgroundRender = new UnityEngine.XR.ARBackgroundRenderer();
            backgroundRender.backgroundMaterial =
                Resources.Load("Materials/ARBackground", typeof(Material)) as Material;
            backgroundRender.mode = UnityEngine.XR.ARRenderMode.MaterialAsBackground;
            if (m_firstPersonCamera != null)
            {
                backgroundRender.camera = m_firstPersonCamera;
            }

            UnityTango.Device.backgroundRenderer = backgroundRender;
            m_arBackgroundSetup = true;
        }

        private IEnumerator _AttemptRecoverIfInvalidState()
        {
            const string ANDROID_CAMERA_PERMISSION_NAME = "android.permission.CAMERA";
            const int MAX_RECOVERY_ATTEMPTS = 10;
            var permissionsArray = new string[] { ANDROID_CAMERA_PERMISSION_NAME };

            while (true)
            {
                if (TangoClientApi.CallsToConnectWithoutMatchingDisconnect())
                {
                    Debug.LogWarning("Recovery daemon detected ARCore connection problem.  Cycling pause resume.");

                    // Asking for the camera permission we already have forces a Pause/Resume "kick".
                    AndroidPermissionsManager.RequestPermission(permissionsArray);

                    // This avoids thrashing on recovery attempts.
                    if (++m_numberOfRecoveryAttempts >= MAX_RECOVERY_ATTEMPTS)
                    {
                        yield break;
                    }
                }

                yield return null;
            }
        }

        private UnityTango.Config _GetSessionTangoConfiguration(SessionConfig sessionConfig)
        {
            const string DRIFT_CORRECTION_FLAG = "config_enable_drift_correction";
            const string PLANE_DETECTION_FLAG = "config_experimental_enable_plane_detection";
            const string POINTCLOUD_FROM_VIO_FLAG = "config_experimental_enable_depth_from_vio";
            const string POINTCLOUD_FLAG = "config_enable_depth";
            const string POINTCLOUD_TYPE_FLAG = "config_depth_mode";
            const int XYZC_POINTCLOUD_MODE = 0;
            UnityTango.Config tangoConfig = new UnityTango.Config();

            // Set defaults
            tangoConfig.enableMotionTracking = true;
            tangoConfig.enableDepth = false;
            tangoConfig.enableColorCamera = sessionConfig.m_enableARBackground;
            tangoConfig.areaLearningMode = UnityTango.AreaLearningMode.None;
            tangoConfig.AddConfigParameter(DRIFT_CORRECTION_FLAG, true);
            tangoConfig.AddConfigParameter(POINTCLOUD_FROM_VIO_FLAG, sessionConfig.m_enablePointcloud);
            tangoConfig.AddConfigParameter(POINTCLOUD_TYPE_FLAG, XYZC_POINTCLOUD_MODE);
            tangoConfig.AddConfigParameter(POINTCLOUD_FLAG, sessionConfig.m_enablePointcloud);
            tangoConfig.AddConfigParameter(PLANE_DETECTION_FLAG, sessionConfig.m_enablePlaneFinding);

            return tangoConfig;
        }
    }
}
