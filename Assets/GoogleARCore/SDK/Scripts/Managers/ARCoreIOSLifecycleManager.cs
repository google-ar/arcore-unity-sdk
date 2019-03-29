//-----------------------------------------------------------------------
// <copyright file="ARCoreIOSLifecycleManager.cs" company="Google">
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
    using System.Reflection;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    internal class ARCoreIOSLifecycleManager : ILifecycleManager
    {
        private const string k_CloudServicesApiKeyPath = "RuntimeSettings/CloudServicesApiKey";

        private static ARCoreIOSLifecycleManager s_Instance;

        private IntPtr m_SessionHandle = IntPtr.Zero;

        private IntPtr m_FrameHandle = IntPtr.Zero;

        // Avoid warnings for fields that are unused on Android platform.
#pragma warning disable 67, 414
        private string m_CloudServicesApiKey;

        private bool m_SessionEnabled = false;

        private IntPtr m_RealArKitSessionHandle = IntPtr.Zero;

        public event Action EarlyUpdate;

        public event Action<bool> OnSessionSetEnabled;
#pragma warning restore 67, 414

        public static ARCoreIOSLifecycleManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ARCoreIOSLifecycleManager();
                    s_Instance._Initialize();
                    s_Instance.m_CloudServicesApiKey =
                        (Resources.Load(k_CloudServicesApiKeyPath) as TextAsset).text;
#if ARCORE_IOS_SUPPORT
                    UnityEngine.XR.iOS.UnityARSessionNativeInterface.ARFrameUpdatedEvent +=
                        s_Instance._OnARKitFrameUpdated;
#endif
                }

                return s_Instance;
            }
        }

        public SessionStatus SessionStatus { get; private set; }

        public LostTrackingReason LostTrackingReason { get; private set; }

        public ARCoreSession SessionComponent { get; private set; }

        public NativeSession NativeSession { get; private set; }

        public bool IsSessionChangedThisFrame { get; private set; }

        public AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
            return new AsyncTask<ApkAvailabilityStatus>(
                ApkAvailabilityStatus.UnsupportedDeviceNotCapable);
        }

        public AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested)
        {
            return new AsyncTask<ApkInstallationStatus>(ApkInstallationStatus.Error);
        }

        public void CreateSession(ARCoreSession sessionComponent)
        {
#if ARCORE_IOS_SUPPORT
            if (SessionComponent != null)
            {
                Debug.LogError("Multiple ARCore session components cannot exist in the scene. " +
                    "Destroying the newest.");
                GameObject.Destroy(sessionComponent);
                return;
            }

            m_RealArKitSessionHandle = _GetSessionHandleFromArkitPlugin();
            SessionComponent = sessionComponent;

            var status =
                ExternApi.ArSession_create(m_CloudServicesApiKey, null, ref m_SessionHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat(
                    "Could not create a cross platform ARCore session ({0}).", status);
                return;
            }

            NativeSession = new NativeSession(m_SessionHandle, IntPtr.Zero);
#else
            Debug.Log("ARCore iOS Support is not enabled. ARCore will be disabled on iOS device.");
            return;
#endif
        }

        public void EnableSession()
        {
            m_SessionEnabled = true;
            SessionStatus = SessionStatus.Tracking;
        }

        public void DisableSession()
        {
            m_SessionEnabled = false;
            SessionStatus = SessionStatus.NotTracking;
        }

        public void ResetSession()
        {
            if (m_SessionHandle != IntPtr.Zero)
            {
                if (m_FrameHandle != IntPtr.Zero)
                {
                    NativeSession.FrameApi.Release(m_FrameHandle);
                    m_FrameHandle = IntPtr.Zero;
                }

                ExternApi.ArSession_destroy(m_SessionHandle);
                m_SessionHandle = IntPtr.Zero;
            }

            _Initialize();
        }

#if ARCORE_IOS_SUPPORT
        private void _OnARKitFrameUpdated(UnityEngine.XR.iOS.UnityARCamera camera)
        {
            if (m_FrameHandle != IntPtr.Zero)
            {
                NativeSession.FrameApi.Release(m_FrameHandle);
                m_FrameHandle = IntPtr.Zero;
            }

            if (m_SessionEnabled)
            {
                m_FrameHandle =
                    ExternApi.ARCoreARKitIntegration_getCurrentFrame(m_RealArKitSessionHandle);
                ExternApi.ArSession_updateAndAcquireArFrame(
                    m_SessionHandle, m_FrameHandle, ref m_FrameHandle);
            }

            if (NativeSession != null)
            {
                NativeSession.OnUpdate(m_FrameHandle);
            }

            if (EarlyUpdate != null)
            {
                EarlyUpdate();
            }
        }
#endif

        private void _Initialize()
        {
            m_SessionEnabled = false;
            SessionStatus = SessionStatus.NotTracking;
            LostTrackingReason = LostTrackingReason.None;
            IsSessionChangedThisFrame = false;
        }

        private IntPtr _GetSessionHandleFromArkitPlugin()
        {
            IntPtr result = IntPtr.Zero;
#if ARCORE_IOS_SUPPORT
            var m_session =
                UnityEngine.XR.iOS.UnityARSessionNativeInterface.GetARSessionNativeInterface();
            var sessionField = m_session.GetType().GetField(
                "m_NativeARSession", BindingFlags.NonPublic | BindingFlags.Instance);
            var val = sessionField.GetValue(m_session);
            result =
                ExternApi.ARCoreARKitIntegration_castUnitySessionToARKitSession((System.IntPtr)val);
#endif
            return result;
        }

        private struct ExternApi
        {
#if UNITY_IOS
            [DllImport(ApiConstants.ARCoreARKitIntegrationApi)]
            public static extern IntPtr ARCoreARKitIntegration_castUnitySessionToARKitSession(
                IntPtr sessionToCast);

            [DllImport(ApiConstants.ARCoreARKitIntegrationApi)]
            public static extern IntPtr ARCoreARKitIntegration_getCurrentFrame(
                IntPtr arkitSessionHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_create(
                string apiKey, string bundleIdentifier, ref IntPtr sessionHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_destroy(IntPtr session);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_updateAndAcquireArFrame(
                IntPtr sessionHandle, IntPtr arkitFrameHandle, ref IntPtr arFrame);
#else
            public static IntPtr ARCoreARKitIntegration_castUnitySessionToARKitSession(
                IntPtr sessionToCast)
            {
                return IntPtr.Zero;
            }

            public static IntPtr ARCoreARKitIntegration_getCurrentFrame(IntPtr arkitSessionHandle)
            {
                return IntPtr.Zero;
            }

            public static ApiArStatus ArSession_create(string apiKey, string bundleIdentifier,
                ref IntPtr sessionHandle)
            {
                return ApiArStatus.Success;
            }

            public static void ArSession_destroy(IntPtr session)
            {
            }

            public static ApiArStatus ArSession_updateAndAcquireArFrame(IntPtr sessionHandle,
                IntPtr arkitFrameHandle, ref IntPtr arFrame)
            {
                return ApiArStatus.Success;
            }
#endif
        }
    }
}
