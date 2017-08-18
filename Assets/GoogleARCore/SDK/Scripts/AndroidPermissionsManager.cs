//-----------------------------------------------------------------------
// <copyright file="AndroidPermissionsManager.cs" company="Google">
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
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Manages Android permissions for the Unity application.
    /// </summary>
    public class AndroidPermissionsManager : AndroidJavaProxy
    {
        private static AndroidPermissionsManager m_instance;
        private static AndroidJavaObject m_Activity;
        private static AndroidJavaObject m_PermissionService;
        private static AsyncTask<AndroidPermissionsRequestResult> m_currentRequest = null;
        private static Action<AndroidPermissionsRequestResult> m_onPermissionsRequestFinished;

        private static AndroidPermissionsManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new AndroidPermissionsManager();
                }

                return m_instance;
            }
        }

        private static AndroidJavaObject UnityActivity
        {
            get
            {
                if (m_Activity == null)
                {
                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    m_Activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                }

                return m_Activity;
            }
        }

        private static AndroidJavaObject PermissionsService
        {
            get
            {
                if (m_PermissionService == null)
                {
                    m_PermissionService = new AndroidJavaObject("com.unity3d.player.UnityAndroidPermissions");
                }

                return m_PermissionService;
            }
        }

        /// <summary>
        /// Checks if an Android permission is granted to the application.
        /// </summary>
        /// <param name="permissionName">The full name of the Android permission to check (e.g.
        /// android.permission.CAMERA).</param>
        /// <returns><c>true</c> if <c>permissionName</c> is granted to the application, otherwise
        /// <c>false</c>.</returns>
        public static bool IsPermissionGranted(string permissionName)
        {
            return PermissionsService.Call<bool>("IsPermissionGranted", UnityActivity, permissionName);
        }

        /// <summary>
        /// Requests a set of Android permissions from the user.
        /// </summary>
        /// <param name="permissionNames">A collection of permissions to be requested (e.g. android.permission.CAMERA).
        /// <b>Currently only the first permission in the collection will be requested. The behavior of this parameter
        /// will change in later versions.</b>
        /// </param>
        /// <returns>An asynchronous task the completes when the user has accepted/rejected all requested permissions
        /// and yields a <c>AndroidPermissionsRequestResult</c> that summarizes the result.  If this method is called
        /// when another permissions request is pending <c>null</c> will be returned instead.</returns>
        public static AsyncTask<AndroidPermissionsRequestResult> RequestPermission(string[] permissionNames)
        {
            if (m_currentRequest != null)
            {
                ARDebug.LogError("Attempted to make simultaneous Android permissions requests.");
                return null;
            }

            // TODO (mtsmall): We currently only allow one permission to be requested because of the callback structure.
            PermissionsService.Call("RequestPermissionAsync", UnityActivity, new [] { permissionNames[0] }, Instance);
            m_currentRequest = new AsyncTask<AndroidPermissionsRequestResult>(out m_onPermissionsRequestFinished);

            return m_currentRequest;
        }

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Constructs a new AndroidPermissionsManager.
        /// </summary>
        public AndroidPermissionsManager() : base("com.unity3d.player.UnityAndroidPermissions$IPermissionRequestResult") {}
        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired when a permission is granted.
        /// </summary>
        /// <param name="permissionName">The name of the permission that was granted.</param>
        public virtual void OnPermissionGranted(string permissionName)
        {
            _OnPermissionResult(permissionName, true);
        }
        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired when a permission is denied.
        /// </summary>
        /// <param name="permissionName">The name of the permission that was denied.</param>
        public virtual void OnPermissionDenied(string permissionName)
        {
            _OnPermissionResult(permissionName, false);
        }
        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired on an Android activity result (unused part of UnityAndroidPermissions interface).
        /// </summary>
        public virtual void OnActivityResult() {}
        /// @endcond

        private void _OnPermissionResult(string permissionName, bool granted)
        {
            if (m_onPermissionsRequestFinished == null)
            {
                Debug.LogErrorFormat("AndroidPermissionsManager received an unexpected permissions result {0}",
                    permissionName);
                return;
            }

            // Cache completion method and reset request state.
            var onRequestFinished = m_onPermissionsRequestFinished;
            m_currentRequest = null;
            m_onPermissionsRequestFinished = null;

            onRequestFinished(new AndroidPermissionsRequestResult(new string[] { permissionName },
                new bool[] { granted }));
        }
    }
}
