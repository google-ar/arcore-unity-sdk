//-----------------------------------------------------------------------
// <copyright file="ARCoreSession.cs" company="Google">
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
    using System.Runtime.InteropServices;
    using GoogleARCoreInternal;
    using UnityEngine;
    using UnityEngine.XR;

    /// <summary>
    /// A component that manages the ARCore Session in a Unity scene.
    /// </summary>
    public class ARCoreSession : MonoBehaviour
    {
        /// <summary>
        /// A scriptable object specifying the ARCore session configuration.
        /// </summary>
        [Tooltip("A scriptable object specifying the ARCore session configuration.")]
        public ARCoreSessionConfig SessionConfig;

        /// <summary>
        /// Toggles whether the tango service should be automatically connected upon Awake.
        /// </summary>
        [Tooltip("Toggles whether the tango service should be automatically connected upon Awake.")]
        public bool ConnectOnAwake = false;

        private SessionManager m_SessionManager;

        /// <summary>
        /// Unity Awake.
        /// </summary>
        public void Awake()
        {
            if (Application.isEditor)
            {
                enabled = false;
                return;
            }

            if (FindObjectsOfType<ARCoreSession>().Length > 1)
            {
                ARDebug.LogError("Multiple SessionComponents present in the game scene.  Destroying the gameobject " +
                 "of the newest one.");
                Destroy(gameObject);
                return;
            }

            m_SessionManager = SessionManager.CreateSession();
            Session.Initialize(m_SessionManager);

            if (Session.ConnectionState != SessionConnectionState.Uninitialized)
            {
                ARDebug.LogError("Could not create an ARCore session.  The current Unity Editor may not support this " +
                    "version of ARCore.");
                return;
            }

            if (ConnectOnAwake)
            {
                Connect();
            }
        }

        /// <summary>
        /// Unity OnDestroy.
        /// </summary>
        public void OnDestroy()
        {
            Frame.Destroy();
            Session.Destroy();
        }

        /// <summary>
        /// Unity Update.
        /// </summary>
        public void Update()
        {
            if (m_SessionManager == null)
            {
                return;
            }

            AsyncTask.OnUpdate();
        }

        /// <summary>
        /// Connects an ARSession using {@link sessionConfig} configuration. Note that if user permissions are needed
        /// they will be requested and thus this is an asynchronous method.
        /// </summary>
        /// <returns>An {@link AsyncTask<T>} that completes when the connection has been made or failed. </returns>
        public AsyncTask<SessionConnectionState> Connect()
        {
            return Connect(SessionConfig);
        }

        /// <summary>
        /// Connects an ARSession.  Note that if user permissions are needed they will be requested and thus this is an
        /// asynchronous method.
        /// </summary>
        /// <param name="sessionConfig">The session configuration.</param>
        /// <returns>An {@link AsyncTask<T>} that completes when the connection has been made or failed. </returns>
        public AsyncTask<SessionConnectionState> Connect(ARCoreSessionConfig sessionConfig)
        {
            const string androidCameraPermissionName = "android.permission.CAMERA";

            if (m_SessionManager == null)
            {
                ARDebug.LogError("Cannot connect because ARCoreSession failed to initialize.");
                return new AsyncTask<SessionConnectionState>(SessionConnectionState.Uninitialized);
            }

            if (sessionConfig == null)
            {
                ARDebug.LogError("Unable to connect ARSession session due to missing ARSessionConfig.");
                m_SessionManager.ConnectionState = SessionConnectionState.MissingConfiguration;
                return new AsyncTask<SessionConnectionState>(Session.ConnectionState);
            }

            // We have already connected at least once.
            if (Session.ConnectionState != SessionConnectionState.Uninitialized)
            {
                ARDebug.LogError("Multiple attempts to connect to the ARSession.  Note that the ARSession connection " +
                    "spans the lifetime of the application and cannot be reconfigured.  This will change in future " +
                    "versions of ARCore.");
                return new AsyncTask<SessionConnectionState>(Session.ConnectionState);
            }

            // Create an asynchronous task for the potential permissions flow and service connection.
            Action<SessionConnectionState> onTaskComplete;
            var returnTask = new AsyncTask<SessionConnectionState>(out onTaskComplete);
            returnTask.ThenAction((connectionState) =>
            {
                m_SessionManager.ConnectionState = connectionState;
            });

            // Attempt service connection immediately if permissions are granted.
            if (AndroidPermissionsManager.IsPermissionGranted(androidCameraPermissionName))
            {
                _ResumeSession(sessionConfig, onTaskComplete);
                return returnTask;
            }

            // Request needed permissions and attempt service connection if granted.
            AndroidPermissionsManager.RequestPermission(androidCameraPermissionName).ThenAction((requestResult) =>
            {
                if (requestResult.IsAllGranted)
                {
                    _ResumeSession(sessionConfig, onTaskComplete);
                }
                else
                {
                    ARDebug.LogError("ARCore connection failed because a needed permission was rejected.");
                    onTaskComplete(SessionConnectionState.UserRejectedNeededPermission);
                }
            });

            return returnTask;
        }

        /// <summary>
        /// Connects to the ARCore service.
        /// </summary>
        /// <param name="sessionConfig">The session configuration to connect with.</param>
        /// <param name="onComplete">A callback for when the result of the connection attempt is known.</param>
        private void _ResumeSession(ARCoreSessionConfig sessionConfig, Action<SessionConnectionState> onComplete)
        {
            if (!m_SessionManager.CheckSupported(sessionConfig))
            {
                ARDebug.LogError("The requested ARCore session configuration is not supported.");
                onComplete(SessionConnectionState.InvalidConfiguration);
                return;
            }

            if (!m_SessionManager.SetConfiguration(sessionConfig))
            {
                ARDebug.LogError("ARCore connection failed because the current configuration is not supported.");
                onComplete(SessionConnectionState.InvalidConfiguration);
                return;
            }

            Frame.Initialize(m_SessionManager.FrameManager);

            // ArSession_resume needs to be called in the UI thread due to b/69682628.
            AsyncTask.PerformActionInUIThread(() =>
            {
                if (!m_SessionManager.Resume())
                {
                    onComplete(SessionConnectionState.ConnectToServiceFailed);
                }
                else
                {
                    onComplete(SessionConnectionState.Connected);
                }
            });
        }
    }
}
