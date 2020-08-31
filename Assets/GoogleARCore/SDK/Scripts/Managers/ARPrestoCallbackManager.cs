//-----------------------------------------------------------------------
// <copyright file="ARPrestoCallbackManager.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class ARPrestoCallbackManager
    {
        private static ARPrestoCallbackManager _instance;

        private static IAndroidPermissionsCheck _androidPermissionCheck;

        private CheckApkAvailabilityResultCallback _checkApkAvailabilityResultCallback;

        private RequestApkInstallationResultCallback _requestApkInstallationResultCallback;

        private CameraPermissionRequestProvider _requestCameraPermissionCallback;

        private EarlyUpdateCallback _earlyUpdateCallback;

        private OnBeforeSetConfigurationCallback _onBeforeSetConfigurationCallback;

        private OnBeforeResumeSessionCallback _onBeforeResumeSessionCallback;

        private List<Action<ApkAvailabilityStatus>> _pendingAvailabilityCheckCallbacks =
            new List<Action<ApkAvailabilityStatus>>();

        private List<Action<ApkInstallationStatus>> _pendingInstallationRequestCallbacks =
            new List<Action<ApkInstallationStatus>>();

        public delegate void EarlyUpdateCallback();

        private delegate void OnBeforeSetConfigurationCallback(
            IntPtr sessionHandle, IntPtr configHandle);

        private delegate void OnBeforeResumeSessionCallback(IntPtr sessionHandle);

        private delegate void CameraPermissionRequestProvider(
            CameraPermissionsResultCallback onComplete, IntPtr context);

        private delegate void CameraPermissionsResultCallback(bool granted,
            IntPtr context);

        private delegate void CheckApkAvailabilityResultCallback(ApiAvailability status,
            IntPtr context);

        private delegate void RequestApkInstallationResultCallback(
            ApiApkInstallationStatus status, IntPtr context);

        private delegate void SessionCreationResultCallback(
            IntPtr sessionHandle, IntPtr frameHandle, IntPtr context, ApiArStatus status);

        public event Action EarlyUpdate;

        public event Action<IntPtr> BeforeResumeSession;

        public event Action<IntPtr, IntPtr> OnSetConfiguration;

        public static ARPrestoCallbackManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (_androidPermissionCheck == null &&
                        !InstantPreviewManager.IsProvidingPlatform)
                    {
                        _androidPermissionCheck = AndroidPermissionsManager.GetInstance();
                    }

                    _instance = new ARPrestoCallbackManager();
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        public AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
            Action<ApkAvailabilityStatus> onComplete;
            AsyncTask<ApkAvailabilityStatus> task =
                new AsyncTask<ApkAvailabilityStatus>(out onComplete);

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("determine ARCore APK " +
                    "availability");
                return task;
            }

            ExternApi.ArPresto_checkApkAvailability(_checkApkAvailabilityResultCallback,
                IntPtr.Zero);

            _pendingAvailabilityCheckCallbacks.Add(onComplete);

            return task;
        }

        public AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested)
        {
            Action<ApkInstallationStatus> onComplete;
            AsyncTask<ApkInstallationStatus> task =
                new AsyncTask<ApkInstallationStatus>(out onComplete);

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("request installation of ARCore " +
                    "APK");
                return task;
            }

            ExternApi.ArPresto_requestApkInstallation(userRequested,
                _requestApkInstallationResultCallback, IntPtr.Zero);

            _pendingInstallationRequestCallbacks.Add(onComplete);

            return task;
        }

        internal static void ResetInstance()
        {
            _instance = null;
            _androidPermissionCheck = null;
        }

        internal static void SetAndroidPermissionCheck(
            IAndroidPermissionsCheck androidPermissionsCheck)
        {
            _androidPermissionCheck = androidPermissionsCheck;
        }

        [AOT.MonoPInvokeCallback(typeof(CheckApkAvailabilityResultCallback))]
        private static void OnCheckApkAvailabilityResultTrampoline(
            ApiAvailability status, IntPtr context)
        {
            Instance.OnCheckApkAvailabilityResult(status.ToApkAvailabilityStatus());
        }

        [AOT.MonoPInvokeCallback(typeof(RequestApkInstallationResultCallback))]
        private static void OnApkInstallationResultTrampoline(
            ApiApkInstallationStatus status, IntPtr context)
        {
            Instance.OnRequestApkInstallationResult(status.ToApkInstallationStatus());
        }

        [AOT.MonoPInvokeCallback(typeof(CameraPermissionRequestProvider))]
        private static void RequestCameraPermissionTrampoline(
            CameraPermissionsResultCallback onComplete, IntPtr context)
        {
            Instance.RequestCameraPermission(onComplete, context);
        }

        [AOT.MonoPInvokeCallback(typeof(EarlyUpdateCallback))]
        private static void EarlyUpdateTrampoline()
        {
            if (Instance.EarlyUpdate != null)
            {
                Instance.EarlyUpdate();
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnBeforeSetConfigurationCallback))]
        private static void BeforeSetConfigurationTrampoline(
            IntPtr sessionHandle, IntPtr configHandle)
        {
            if (Instance.OnSetConfiguration != null)
            {
                Instance.OnSetConfiguration(sessionHandle, configHandle);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnBeforeResumeSessionCallback))]
        private static void BeforeResumeSessionTrampoline(IntPtr sessionHandle)
        {
            if (Instance.BeforeResumeSession != null)
            {
                Instance.BeforeResumeSession(sessionHandle);
            }
        }

        private void Initialize()
        {
            _earlyUpdateCallback = new EarlyUpdateCallback(EarlyUpdateTrampoline);

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                // Instant preview does not support updated function signature returning 'bool'.
                ExternApi.ArCoreUnity_setArPrestoInitialized(_earlyUpdateCallback);
            }
            else if (!ExternApi.ArCoreUnity_setArPrestoInitialized(_earlyUpdateCallback))
            {
                Debug.LogError(
                    "Missing Unity Engine ARCore support.  Please ensure that the Unity project " +
                    "has the 'Player Settings > XR Settings > ARCore Supported' checkbox enabled.");
            }

            IntPtr javaVMHandle = IntPtr.Zero;
            IntPtr activityHandle = IntPtr.Zero;
            ExternApi.ArCoreUnity_getJniInfo(ref javaVMHandle, ref activityHandle);

            _checkApkAvailabilityResultCallback =
                new CheckApkAvailabilityResultCallback(OnCheckApkAvailabilityResultTrampoline);

            _requestApkInstallationResultCallback =
                new RequestApkInstallationResultCallback(OnApkInstallationResultTrampoline);

            _requestCameraPermissionCallback =
                new CameraPermissionRequestProvider(RequestCameraPermissionTrampoline);

            _onBeforeSetConfigurationCallback =
                new OnBeforeSetConfigurationCallback(BeforeSetConfigurationTrampoline);

            _onBeforeResumeSessionCallback =
                new OnBeforeResumeSessionCallback(BeforeResumeSessionTrampoline);

            ExternApi.ArPresto_initialize(
                javaVMHandle, activityHandle, _requestCameraPermissionCallback,
                _onBeforeSetConfigurationCallback, _onBeforeResumeSessionCallback);
        }

        private void OnCheckApkAvailabilityResult(ApkAvailabilityStatus status)
        {
            foreach (var onComplete in _pendingAvailabilityCheckCallbacks)
            {
                onComplete(status);
            }

            _pendingAvailabilityCheckCallbacks.Clear();
        }

        private void OnRequestApkInstallationResult(ApkInstallationStatus status)
        {
            foreach (var onComplete in _pendingInstallationRequestCallbacks)
            {
                onComplete(status);
            }

            _pendingInstallationRequestCallbacks.Clear();
        }

        private void RequestCameraPermission(CameraPermissionsResultCallback onComplete,
            IntPtr context)
        {
            const string cameraPermissionName = "android.permission.CAMERA";

            if (_androidPermissionCheck != null)
            {
                _androidPermissionCheck.RequestAndroidPermission(cameraPermissionName)
                    .ThenAction((grantResult) =>
                    {
                        onComplete(grantResult.IsAllGranted, context);
                    });
            }
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArCoreUnity_getJniInfo(
                ref IntPtr javaVM, ref IntPtr activity);

            [AndroidImport(ApiConstants.ARCoreShimApi)]
            public static extern bool ArCoreUnity_setArPrestoInitialized(
                EarlyUpdateCallback onEarlyUpdate);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_initialize(
                IntPtr javaVM, IntPtr activity,
                CameraPermissionRequestProvider requestCameraPermission,
                OnBeforeSetConfigurationCallback onBeforeSetConfiguration,
                OnBeforeResumeSessionCallback onBeforeResumeSession);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_checkApkAvailability(
                CheckApkAvailabilityResultCallback onResult, IntPtr context);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_requestApkInstallation(
                bool user_requested, RequestApkInstallationResultCallback onResult, IntPtr context);
#pragma warning restore 626
        }
    }
}
