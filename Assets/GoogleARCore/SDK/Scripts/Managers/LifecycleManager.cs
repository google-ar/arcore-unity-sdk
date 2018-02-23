//-----------------------------------------------------------------------
// <copyright file="LifecycleManager.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class LifecycleManager
    {
        private static LifecycleManager s_Instance = new LifecycleManager();

        private CheckApkAvailabilityResultCallback m_CheckApkAvailabilityResultCallback;

        private RequestApkInstallationResultCallback m_RequestApkInstallationResultCallback;

        private CameraPermissionRequestProvider m_RequestCameraPermissionCallback;

        private EarlyUpdateCallback m_EarlyUpdateCallback;

        private Texture2D m_BackgroundTexture;

        private ARCoreSession m_SessionComponent;

        private NativeSession m_NativeSession;

        private List<Action<ApkAvailabilityStatus>> m_PendingAvailabilityCheckCallbacks =
            new List<Action<ApkAvailabilityStatus>>();

        private List<Action<ApkInstallationStatus>> m_PendingInstallationRequestCallbacks =
            new List<Action<ApkInstallationStatus>>();

        static LifecycleManager()
        {
            Instance._Initialize();
        }

        private delegate void CheckApkAvailabilityResultCallback(ApiAvailability status,
            IntPtr context);

        private delegate void RequestApkInstallationResultCallback(
            ApiApkInstallationStatus status, IntPtr context);

        private delegate void CameraPermissionsResultCallback(bool granted,
            IntPtr context);

        private delegate void CameraPermissionRequestProvider(
            CameraPermissionsResultCallback onComplete, IntPtr context);

        private delegate void SessionCreationResultCallback(
            IntPtr sessionHandle, IntPtr frameHandle, IntPtr context, ApiArStatus status);

        private delegate void EarlyUpdateCallback();

        public static LifecycleManager Instance
        {
            get
            {
                return s_Instance;
            }
        }

        public Texture2D BackgroundTexture
        {
            get
            {
                return m_BackgroundTexture;
            }
        }

        public NativeSession NativeSession
        {
            get
            {
                if (m_NativeSession != null)
                {
                    return m_NativeSession;
                }

                IntPtr sessionHandle = IntPtr.Zero;
                ExternApi.ArPresto_getSession(ref sessionHandle);

                IntPtr frameHandle = IntPtr.Zero;
                ExternApi.ArPresto_getFrame(ref frameHandle);

                if (sessionHandle == IntPtr.Zero || frameHandle == IntPtr.Zero)
                {
                    return null;
                }

                m_NativeSession = new NativeSession(sessionHandle, frameHandle);
                return m_NativeSession;
            }
        }

        public SessionStatus SessionStatus
        {
            get
            {
                ApiPrestoStatus prestoStatus = ApiPrestoStatus.Uninitialized;
                ExternApi.ArPresto_getStatus(ref prestoStatus);
                return prestoStatus.ToSessionStatus();
            }
        }

        public void CreateSession(ARCoreSession session)
        {
            session.StartCoroutine(InstantPreviewManager.InitializeIfNeeded());

            if (m_SessionComponent != null)
            {
                Debug.LogError("Multiple session components cannot exist in the scene. " +
                    "Destroying the newest.");
                GameObject.Destroy(session);
                return;
            }

            m_SessionComponent = session;
        }

        public void EnableSession()
        {
            var config = m_SessionComponent.SessionConfig;
            if (config != null)
            {
                ExternApi.ArPresto_setConfiguration(new ApiPrestoConfig(config));
            }

            ExternApi.ArPresto_setEnabled(true);
        }

        public void DisableSession()
        {
            ExternApi.ArPresto_setEnabled(false);
        }

        public void DestroySession()
        {
            m_SessionComponent = null;
        }

        public AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
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
            Action<ApkInstallationStatus> onComplete;
            AsyncTask<ApkInstallationStatus> task =
                new AsyncTask<ApkInstallationStatus>(out onComplete);

            ExternApi.ArPresto_requestApkInstallation(userRequested,
                m_RequestApkInstallationResultCallback, IntPtr.Zero);

            m_PendingInstallationRequestCallbacks.Add(onComplete);

            return task;
        }

        [AOT.MonoPInvokeCallback(typeof(CheckApkAvailabilityResultCallback))]
        private static void OnCheckApkAvailabilityResultTrampoline(
            ApiAvailability status, IntPtr context)
        {
            Instance._OnCheckApkAvailabilityResult(status.ToApkAvailabilityStatus());
        }

        [AOT.MonoPInvokeCallback(typeof(RequestApkInstallationResultCallback))]
        private static void OnApkInstallationResultTrampoline(
            ApiApkInstallationStatus status, IntPtr context)
        {
            Instance._OnRequestApkInstallationResult(status.ToApkInstallationStatus());
        }

        [AOT.MonoPInvokeCallback(typeof(CameraPermissionRequestProvider))]
        private static void RequestCameraPermissionTrampoline(
            CameraPermissionsResultCallback onComplete, IntPtr context)
        {
            Instance._RequestCameraPermission(onComplete, context);
        }

        [AOT.MonoPInvokeCallback(typeof(EarlyUpdateCallback))]
        private static void EarlyUpdateTrampoline()
        {
            Instance._EarlyUpdate();
        }

        private void _Initialize()
        {
            m_EarlyUpdateCallback = new EarlyUpdateCallback(EarlyUpdateTrampoline);
            ExternApi.ArCoreUnity_setArPrestoInitialized(m_EarlyUpdateCallback);

            IntPtr javaVMHandle = IntPtr.Zero;
            IntPtr activityHandle = IntPtr.Zero;
            ExternApi.ArCoreUnity_getJniInfo(ref javaVMHandle, ref activityHandle);

            m_CheckApkAvailabilityResultCallback =
                new CheckApkAvailabilityResultCallback(OnCheckApkAvailabilityResultTrampoline);

            m_RequestApkInstallationResultCallback =
                new RequestApkInstallationResultCallback(OnApkInstallationResultTrampoline);

            m_RequestCameraPermissionCallback =
                new CameraPermissionRequestProvider(RequestCameraPermissionTrampoline);

            ExternApi.ArPresto_initialize(javaVMHandle, activityHandle,
                m_RequestCameraPermissionCallback);
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

        private void _EarlyUpdate()
        {
            AsyncTask.OnUpdate();
            _UpdateTextureIfNeeded();

            if (m_NativeSession != null)
            {
                m_NativeSession.SessionApi.SetDisplayGeometry(
                    Screen.orientation, Screen.width, Screen.height);
                m_NativeSession.OnUpdate();
            }
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

        private void _UpdateTextureIfNeeded()
        {
            // If running in editor, updates background texture from Instant Preview only.
            if (InstantPreviewManager.UpdateBackgroundTextureIfNeeded(ref m_BackgroundTexture))
            {
                return;
            }

            int backgroundTextureId = ExternApi.ArCoreUnity_getBackgroundTextureId();

            if (NativeSession == null)
            {
                // This prevents using a texture that has not been filled out by ARCore.
                return;
            }
            else if (backgroundTextureId == -1)
            {
                return;
            }
            else if (m_BackgroundTexture != null &&
                m_BackgroundTexture.GetNativeTexturePtr().ToInt32() == backgroundTextureId)
            {
                return;
            }
            else if (m_BackgroundTexture == null)
            {
                // The Unity-cached size and format of the texture (0x0, ARGB) is not the
                // actual format of the texture. This is okay because the texture is not
                // accessed by pixels, it is accessed with UV coordinates.
                m_BackgroundTexture = Texture2D.CreateExternalTexture(0, 0, TextureFormat.ARGB32, false,
                    false, new IntPtr(backgroundTextureId));
                return;
            }

            m_BackgroundTexture.UpdateExternalTexture(new IntPtr(backgroundTextureId));
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArCoreUnity_getJniInfo(ref IntPtr javaVM, ref IntPtr activity);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArCoreUnity_setArPrestoInitialized(EarlyUpdateCallback onEarlyUpdate);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern int ArCoreUnity_getBackgroundTextureId();

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_initialize(IntPtr javaVM, IntPtr activity,
                CameraPermissionRequestProvider requestCameraPermission);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_checkApkAvailability(
                CheckApkAvailabilityResultCallback onResult, IntPtr context);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_requestApkInstallation(bool user_requested,
                RequestApkInstallationResultCallback onResult, IntPtr context);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_getSession(ref IntPtr sessionHandle);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_setConfiguration(ApiPrestoConfig config);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_setEnabled(bool isEnabled);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_getFrame(ref IntPtr frameHandle);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArPresto_getStatus(ref ApiPrestoStatus prestoStatus);
        }
    }
}