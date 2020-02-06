//-----------------------------------------------------------------------
// <copyright file="ARPrestoCallbackManager.cs" company="Google">
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
        private static ARPrestoCallbackManager s_Instance;

        private static IAndroidPermissionsCheck s_AndroidPermissionCheck;

        private CheckApkAvailabilityResultCallback m_CheckApkAvailabilityResultCallback;

        private RequestApkInstallationResultCallback m_RequestApkInstallationResultCallback;

        private CameraPermissionRequestProvider m_RequestCameraPermissionCallback;

        private EarlyUpdateCallback m_EarlyUpdateCallback;

        private OnBeforeSetConfigurationCallback m_OnBeforeSetConfigurationCallback;

        private OnBeforeResumeSessionCallback m_OnBeforeResumeSessionCallback;

        private List<Action<ApkAvailabilityStatus>> m_PendingAvailabilityCheckCallbacks =
            new List<Action<ApkAvailabilityStatus>>();

        private List<Action<ApkInstallationStatus>> m_PendingInstallationRequestCallbacks =
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
                if (s_Instance == null)
                {
                    if (s_AndroidPermissionCheck == null &&
                        !InstantPreviewManager.IsProvidingPlatform)
                    {
                        s_AndroidPermissionCheck = AndroidPermissionsManager.GetInstance();
                    }

                    s_Instance = new ARPrestoCallbackManager();
                    s_Instance._Initialize();
                }

                return s_Instance;
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

            ExternApi.ArPresto_checkApkAvailability(m_CheckApkAvailabilityResultCallback,
                IntPtr.Zero);

            m_PendingAvailabilityCheckCallbacks.Add(onComplete);

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
                m_RequestApkInstallationResultCallback, IntPtr.Zero);

            m_PendingInstallationRequestCallbacks.Add(onComplete);

            return task;
        }

        internal static void ResetInstance()
        {
            s_Instance = null;
            s_AndroidPermissionCheck = null;
        }

        internal static void SetAndroidPermissionCheck(
            IAndroidPermissionsCheck androidPermissionsCheck)
        {
            s_AndroidPermissionCheck = androidPermissionsCheck;
        }

        [AOT.MonoPInvokeCallback(typeof(CheckApkAvailabilityResultCallback))]
        private static void _OnCheckApkAvailabilityResultTrampoline(
            ApiAvailability status, IntPtr context)
        {
            Instance._OnCheckApkAvailabilityResult(status.ToApkAvailabilityStatus());
        }

        [AOT.MonoPInvokeCallback(typeof(RequestApkInstallationResultCallback))]
        private static void _OnApkInstallationResultTrampoline(
            ApiApkInstallationStatus status, IntPtr context)
        {
            Instance._OnRequestApkInstallationResult(status.ToApkInstallationStatus());
        }

        [AOT.MonoPInvokeCallback(typeof(CameraPermissionRequestProvider))]
        private static void _RequestCameraPermissionTrampoline(
            CameraPermissionsResultCallback onComplete, IntPtr context)
        {
            Instance._RequestCameraPermission(onComplete, context);
        }

        [AOT.MonoPInvokeCallback(typeof(EarlyUpdateCallback))]
        private static void _EarlyUpdateTrampoline()
        {
            if (Instance.EarlyUpdate != null)
            {
                Instance.EarlyUpdate();
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnBeforeSetConfigurationCallback))]
        private static void _BeforeSetConfigurationTrampoline(
            IntPtr sessionHandle, IntPtr configHandle)
        {
            if (Instance.OnSetConfiguration != null)
            {
                Instance.OnSetConfiguration(sessionHandle, configHandle);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnBeforeResumeSessionCallback))]
        private static void _BeforeResumeSessionTrampoline(IntPtr sessionHandle)
        {
            if (Instance.BeforeResumeSession != null)
            {
                Instance.BeforeResumeSession(sessionHandle);
            }
        }

        private void _Initialize()
        {
            m_EarlyUpdateCallback = new EarlyUpdateCallback(_EarlyUpdateTrampoline);

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                // Instant preview does not support updated function signature returning 'bool'.
                ExternApi.ArCoreUnity_setArPrestoInitialized(m_EarlyUpdateCallback);
            }
            else if (!ExternApi.ArCoreUnity_setArPrestoInitialized(m_EarlyUpdateCallback))
            {
                Debug.LogError(
                    "Missing Unity Engine ARCore support.  Please ensure that the Unity project " +
                    "has the 'Player Settings > XR Settings > ARCore Supported' checkbox enabled.");
            }

            IntPtr javaVMHandle = IntPtr.Zero;
            IntPtr activityHandle = IntPtr.Zero;
            ExternApi.ArCoreUnity_getJniInfo(ref javaVMHandle, ref activityHandle);

            m_CheckApkAvailabilityResultCallback =
                new CheckApkAvailabilityResultCallback(_OnCheckApkAvailabilityResultTrampoline);

            m_RequestApkInstallationResultCallback =
                new RequestApkInstallationResultCallback(_OnApkInstallationResultTrampoline);

            m_RequestCameraPermissionCallback =
                new CameraPermissionRequestProvider(_RequestCameraPermissionTrampoline);

            m_OnBeforeSetConfigurationCallback =
                new OnBeforeSetConfigurationCallback(_BeforeSetConfigurationTrampoline);

            m_OnBeforeResumeSessionCallback =
                new OnBeforeResumeSessionCallback(_BeforeResumeSessionTrampoline);

            ExternApi.ArPresto_initialize(
                javaVMHandle, activityHandle, m_RequestCameraPermissionCallback,
                m_OnBeforeSetConfigurationCallback, m_OnBeforeResumeSessionCallback);
        }

        private void _OnCheckApkAvailabilityResult(ApkAvailabilityStatus status)
        {
            foreach (var onComplete in m_PendingAvailabilityCheckCallbacks)
            {
                onComplete(status);
            }

            m_PendingAvailabilityCheckCallbacks.Clear();
        }

        private void _OnRequestApkInstallationResult(ApkInstallationStatus status)
        {
            foreach (var onComplete in m_PendingInstallationRequestCallbacks)
            {
                onComplete(status);
            }

            m_PendingInstallationRequestCallbacks.Clear();
        }

        private void _RequestCameraPermission(CameraPermissionsResultCallback onComplete,
            IntPtr context)
        {
            const string cameraPermissionName = "android.permission.CAMERA";

            if (s_AndroidPermissionCheck != null)
            {
                s_AndroidPermissionCheck.RequestAndroidPermission(cameraPermissionName)
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
