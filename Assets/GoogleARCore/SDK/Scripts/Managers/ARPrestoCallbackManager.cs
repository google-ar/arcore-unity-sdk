//-----------------------------------------------------------------------
// <copyright file="ARPrestoCallbackManager.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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

#if UNITY_IOS
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class ARPrestoCallbackManager
    {
        private static ARPrestoCallbackManager s_Instance;

        private CheckApkAvailabilityResultCallback m_CheckApkAvailabilityResultCallback;

        private RequestApkInstallationResultCallback m_RequestApkInstallationResultCallback;

        private CameraPermissionRequestProvider m_RequestCameraPermissionCallback;

        private EarlyUpdateCallback m_EarlyUpdateCallback;

        private OnBeforeSetConfigurationCallback m_OnBeforeSetConfigurationCallback;

        private List<Action<ApkAvailabilityStatus>> m_PendingAvailabilityCheckCallbacks =
            new List<Action<ApkAvailabilityStatus>>();

        private List<Action<ApkInstallationStatus>> m_PendingInstallationRequestCallbacks =
            new List<Action<ApkInstallationStatus>>();

        public delegate void EarlyUpdateCallback();

        private delegate void OnBeforeSetConfigurationCallback(IntPtr sessionHandhle, IntPtr configHandle);

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

        public static ARPrestoCallbackManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ARPrestoCallbackManager();
                }

                return s_Instance;
            }
        }

        public void InitializeIfNeeded()
        {
            if (m_EarlyUpdateCallback != null)
            {
                return;
            }

            m_EarlyUpdateCallback = new EarlyUpdateCallback(_EarlyUpdateTrampoline);
            ExternApi.ArCoreUnity_setArPrestoInitialized(m_EarlyUpdateCallback);

            IntPtr javaVMHandle = IntPtr.Zero;
            IntPtr activityHandle = IntPtr.Zero;
            ExternApi.ArCoreUnity_getJniInfo(ref javaVMHandle, ref activityHandle);

            m_CheckApkAvailabilityResultCallback =
                new CheckApkAvailabilityResultCallback(_OnCheckApkAvailabilityResultTrampoline);

            m_RequestApkInstallationResultCallback =
                new RequestApkInstallationResultCallback(_OnApkInstallationResultTrampoline);

            m_RequestCameraPermissionCallback =
                new CameraPermissionRequestProvider(_RequestCameraPermissionTrampoline);

            m_OnBeforeSetConfigurationCallback = new OnBeforeSetConfigurationCallback(_BeforeSetConfigurationTrampoline);
            ExternApi.ArPresto_initialize(javaVMHandle, activityHandle, m_RequestCameraPermissionCallback,
                m_OnBeforeSetConfigurationCallback);
        }

        public AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
            InitializeIfNeeded();
            Action<ApkAvailabilityStatus> onComplete;
            AsyncTask<ApkAvailabilityStatus> task =
                new AsyncTask<ApkAvailabilityStatus>(out onComplete);

            ExternApi.ArPresto_checkApkAvailability(m_CheckApkAvailabilityResultCallback,
                IntPtr.Zero);

            m_PendingAvailabilityCheckCallbacks.Add(onComplete);

            return task;
        }

        public AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested)
        {
            InitializeIfNeeded();
            Action<ApkInstallationStatus> onComplete;
            AsyncTask<ApkInstallationStatus> task =
                new AsyncTask<ApkInstallationStatus>(out onComplete);

            ExternApi.ArPresto_requestApkInstallation(userRequested,
                m_RequestApkInstallationResultCallback, IntPtr.Zero);

            m_PendingInstallationRequestCallbacks.Add(onComplete);

            return task;
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
            ARCoreAndroidLifecycleManager.Instance.EarlyUpdate();
        }

        [AOT.MonoPInvokeCallback(typeof(OnBeforeSetConfigurationCallback))]
        private static void _BeforeSetConfigurationTrampoline(IntPtr sessionHandhle, IntPtr configHandle)
        {
            ExperimentManager.Instance.OnBeforeSetConfiguration(sessionHandhle, configHandle);
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
            AndroidPermissionsManager.RequestPermission(cameraPermissionName).ThenAction((grantResult) =>
            {
                onComplete(grantResult.IsAllGranted, context);
            });
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArCoreUnity_getJniInfo(ref IntPtr javaVM, ref IntPtr activity);

            [AndroidImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArCoreUnity_setArPrestoInitialized(EarlyUpdateCallback onEarlyUpdate);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_initialize(IntPtr javaVM, IntPtr activity,
                CameraPermissionRequestProvider requestCameraPermission,
                OnBeforeSetConfigurationCallback onBeforeSetConfiguration);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_checkApkAvailability(
                CheckApkAvailabilityResultCallback onResult, IntPtr context);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_requestApkInstallation(bool user_requested,
                RequestApkInstallationResultCallback onResult, IntPtr context);
#pragma warning restore 626
        }
    }
}