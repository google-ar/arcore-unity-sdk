//-----------------------------------------------------------------------
// <copyright file="AndroidPermissionsManager.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Manages Android permissions for the Unity application.
    /// </summary>
    public class AndroidPermissionsManager : AndroidJavaProxy, IAndroidPermissionsCheck
    {
        private static AndroidPermissionsManager _instance;
        private static AndroidJavaObject _activity;
        private static AndroidJavaObject _permissionService;
        private static AsyncTask<AndroidPermissionsRequestResult> _currentRequest = null;
        private static Action<AndroidPermissionsRequestResult> _onPermissionsRequestFinished;

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Constructs a new AndroidPermissionsManager.
        /// </summary>
        public AndroidPermissionsManager() : base(
            "com.unity3d.plugin.UnityAndroidPermissions$IPermissionRequestResult")
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
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Allocates new objects the first time is called")]
        public static bool IsPermissionGranted(string permissionName)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return true;
            }

            return GetPermissionsService().Call<bool>(
                "IsPermissionGranted", GetUnityActivity(), permissionName);
        }

        /// <summary>
        /// Requests an Android permission from the user.
        /// </summary>
        /// <param name="permissionName">The permission to be requested (e.g.
        /// android.permission.CAMERA).</param>
        /// <returns>An asynchronous task that completes when the user has accepted or rejected the
        /// requested permission and yields a <see cref="AndroidPermissionsRequestResult"/> that
        /// summarizes the result. If this method is called when another permissions request is
        /// pending, <c>null</c> will be returned instead.</returns>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Allocates new objects the first time is called")]
        public static AsyncTask<AndroidPermissionsRequestResult> RequestPermission(
            string permissionName)
        {
            if (AndroidPermissionsManager.IsPermissionGranted(permissionName))
            {
                return new AsyncTask<AndroidPermissionsRequestResult>(
                    new AndroidPermissionsRequestResult(
                        new string[] { permissionName }, new bool[] { true }));
            }

            if (_currentRequest != null)
            {
                ARDebug.LogError("Attempted to make simultaneous Android permissions requests.");
                return null;
            }

            GetPermissionsService().Call("RequestPermissionAsync", GetUnityActivity(),
                new[] { permissionName }, GetInstance());
            _currentRequest =
                new AsyncTask<AndroidPermissionsRequestResult>(out _onPermissionsRequestFinished);

            return _currentRequest;
        }

        /// <summary>
        /// Requests an Android permission from the user.
        /// </summary>
        /// <param name="permissionName">The permission to be requested (e.g.
        /// android.permission.CAMERA).</param>
        /// <returns>An asynchronous task that completes when the user has accepted or rejected the
        /// requested permission and yields a <see cref="AndroidPermissionsRequestResult"/> that
        /// summarizes the result. If this method is called when another permissions request is
        /// pending, <c>null</c> will be returned instead.</returns>
        public AsyncTask<AndroidPermissionsRequestResult> RequestAndroidPermission(
            string permissionName)
        {
            return RequestPermission(permissionName);
        }

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired when a permission is granted.
        /// </summary>
        /// <param name="permissionName">The name of the permission that was granted.</param>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Implements java object interface.")]
        public virtual void OnPermissionGranted(string permissionName)
        {
            OnPermissionResult(permissionName, true);
        }

        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired when a permission is denied.
        /// </summary>
        /// <param name="permissionName">The name of the permission that was denied.</param>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Implements java object interface.")]
        public virtual void OnPermissionDenied(string permissionName)
        {
            OnPermissionResult(permissionName, false);
        }

        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Callback fired on an Android activity result (unused part of UnityAndroidPermissions
        /// interface).
        /// </summary>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Implements java object interface.")]
        public virtual void OnActivityResult()
        {
        }

        internal static AndroidPermissionsManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AndroidPermissionsManager();
            }

            return _instance;
        }

        private static AndroidJavaObject GetUnityActivity()
        {
            if (_activity == null)
            {
                AndroidJavaClass unityPlayer =
                    new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            return _activity;
        }

        private static AndroidJavaObject GetPermissionsService()
        {
            if (_permissionService == null)
            {
                _permissionService =
                    new AndroidJavaObject("com.unity3d.plugin.UnityAndroidPermissions");
            }

            return _permissionService;
        }

        /// @endcond

        /// <summary>
        /// Callback fired on an Android permission result.
        /// </summary>
        /// <param name="permissionName">The name of the permission.</param>
        /// <param name="granted">If permission is granted or not.</param>
        private void OnPermissionResult(string permissionName, bool granted)
        {
            if (_onPermissionsRequestFinished == null)
            {
                Debug.LogErrorFormat(
                    "AndroidPermissionsManager received an unexpected permissions result {0}",
                    permissionName);
                return;
            }

            // Cache completion method and reset request state.
            var onRequestFinished = _onPermissionsRequestFinished;
            _currentRequest = null;
            _onPermissionsRequestFinished = null;

            onRequestFinished(new AndroidPermissionsRequestResult(new string[] { permissionName },
                new bool[] { granted }));
        }
    }
}
