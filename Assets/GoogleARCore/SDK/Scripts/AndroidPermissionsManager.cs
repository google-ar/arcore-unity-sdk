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
        private static AndroidPermissionsManager s_Instance;
        private static AndroidJavaObject s_Activity;
        private static AndroidJavaObject s_PermissionService;
        private static AsyncTask<AndroidPermissionsRequestResult> s_CurrentRequest = null;
        private static Action<AndroidPermissionsRequestResult> s_OnPermissionsRequestFinished;

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Constructs a new AndroidPermissionsManager.
        /// </summary>
        public AndroidPermissionsManager() : base("com.unity3d.plugin.UnityAndroidPermissions$IPermissionRequestResult")
        {
        }

        /// @endcond

        /// <summary>
        /// Checks if an Android permission is granted to the application.
        /// </summary>
        /// <param name="permissionName">The full name of the Android permission to check (e.g.
        /// android.permission.CAMERA).</param>
        /// <returns><c>true</c> if <c>permissionName</c> is granted to the application, otherwise
        /// <c>false</c>.</returns>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "Allocates new objects the first time is called")]
        public static bool IsPermissionGranted(string permissionName)
        {
            if (Application.isEditor)
            {
                return true;
            }

            return GetPermissionsService().Call<bool>("IsPermissionGranted", GetUnityActivity(), permissionName);
        }

        /// <summary>
        /// Requests an Android permission from the user.
        /// </summary>
        /// <param name="permissionName">The permission to be requested (e.g. android.permission.CAMERA).</param>
        /// <returns>An asynchronous task the completes when the user has accepted/rejected the requested permission
        /// and yields a {@link AndroidPermissionsRequestResult} that summarizes the result.  If this method is called
        /// when another permissions request is pending <c>null</c> will be returned instead.</returns>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "Allocates new objects the first time is called")]
        public static AsyncTask<AndroidPermissionsRequestResult> RequestPermission(string permissionName)
        {
            if (AndroidPermissionsManager.IsPermissionGranted(permissionName))
            {
                return new AsyncTask<AndroidPermissionsRequestResult>(new AndroidPermissionsRequestResult(
                    new string[] { permissionName }, new bool[] { true }));
            }

            if (s_CurrentRequest != null)
            {
                ARDebug.LogError("Attempted to make simultaneous Android permissions requests.");
                return null;
            }

            GetPermissionsService().Call("RequestPermissionAsync", GetUnityActivity(),
                new[] { permissionName }, GetInstance());
            s_CurrentRequest = new AsyncTask<AndroidPermissionsRequestResult>(out s_OnPermissionsRequestFinished);

            return s_CurrentRequest;
        }

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired when a permission is granted.
        /// </summary>
        /// <param name="permissionName">The name of the permission that was granted.</param>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "Requires further investigation.")]
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
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "Requires further investigation.")]
        public virtual void OnPermissionDenied(string permissionName)
        {
            _OnPermissionResult(permissionName, false);
        }

        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired on an Android activity result (unused part of UnityAndroidPermissions interface).
        /// </summary>
        public virtual void OnActivityResult()
        {
        }

        private static AndroidPermissionsManager GetInstance()
        {
            if (s_Instance == null)
            {
                s_Instance = new AndroidPermissionsManager();
            }

            return s_Instance;
        }

        private static AndroidJavaObject GetUnityActivity()
        {
            if (s_Activity == null)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                s_Activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            return s_Activity;
        }

        private static AndroidJavaObject GetPermissionsService()
        {
            if (s_PermissionService == null)
            {
                s_PermissionService = new AndroidJavaObject("com.unity3d.plugin.UnityAndroidPermissions");
            }

            return s_PermissionService;
        }

        /// @endcond

        /// <summary>
        /// Callback fired on an Android permission result.
        /// </summary>
        /// <param name="permissionName">The name of the permission.</param>
        /// <param name="granted">If permission is granted or not.</param>
        private void _OnPermissionResult(string permissionName, bool granted)
        {
            if (s_OnPermissionsRequestFinished == null)
            {
                Debug.LogErrorFormat("AndroidPermissionsManager received an unexpected permissions result {0}",
                    permissionName);
                return;
            }

            // Cache completion method and reset request state.
            var onRequestFinished = s_OnPermissionsRequestFinished;
            s_CurrentRequest = null;
            s_OnPermissionsRequestFinished = null;

            onRequestFinished(new AndroidPermissionsRequestResult(new string[] { permissionName },
                new bool[] { granted }));
        }
    }
}
