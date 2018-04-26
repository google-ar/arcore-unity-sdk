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

#if UNITY_IOS
    using UnityEngine.XR.iOS;
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class ARCoreIOSLifecycleManager : ILifecycleManager
    {
        private const string k_CloudServicesApiKeyPath = "RuntimeSettings/CloudServicesApiKey";
        private static ARCoreIOSLifecycleManager s_Instance;
        private IntPtr m_SessionHandle = IntPtr.Zero;
        private string m_CloudServicesApiKey;

        // Avoid warnings for fields that are unused on Android platform.
#pragma warning disable 67, 414
        private IntPtr m_RealArKitSessionHandle = IntPtr.Zero;

        private IntPtr m_FrameHandle = IntPtr.Zero;

        public event LifecycleManager.EarlyUpdateDelegate EarlyUpdateEvent;
#pragma warning restore 67, 414

        public static ARCoreIOSLifecycleManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ARCoreIOSLifecycleManager();
                    s_Instance.m_CloudServicesApiKey = (Resources.Load(k_CloudServicesApiKeyPath) as TextAsset).text;
                }

                return s_Instance;
            }
        }

        public bool IsTracking { get; private set; }

        public ARCoreSession SessionComponent { get; private set; }

        public NativeSession NativeSession { get; private set; }

        public void SetConfiguration(ARCoreSessionConfig config)
        {
        }

        public void CreateSession(ARCoreSession sessionComponent)
        {
            if (SessionComponent != null)
            {
                Debug.LogError("Multiple ARCore session components cannot exist in the scene. " +
                    "Destroying the newest.");
                GameObject.Destroy(sessionComponent);
                return;
            }

            m_RealArKitSessionHandle = _GetSessionHandleFromArkitPlugin();
            SessionComponent = sessionComponent;
            EnableSession();
        }

        public void EnableSession()
        {
            if (m_SessionHandle == IntPtr.Zero)
            {
                var status = ExternApi.ArSession_create(m_CloudServicesApiKey, null, ref m_SessionHandle);
                if (status != ApiArStatus.Success)
                {
                    Debug.LogErrorFormat("Could not create a cross platform ARCore session ({0}).", status);
                    return;
                }

                NativeSession = new NativeSession(m_SessionHandle, IntPtr.Zero);
            }

            _RegisterFrameUpdatedEvent();
        }

        public void DisableSession()
        {
            _UnRegisterFrameUpdatedEvent();
        }

        public void ResetSession()
        {
            DisableSession();

            if (m_SessionHandle != IntPtr.Zero)
            {
                ExternApi.ArSession_destroy(m_SessionHandle);
                m_SessionHandle = IntPtr.Zero;
            }
        }

#if UNITY_IOS
        private void _OnARKitFrameUpdated(UnityEngine.XR.iOS.UnityARCamera camera)
        {
            if (m_FrameHandle != IntPtr.Zero)
            {
                NativeSession.FrameApi.Release(m_FrameHandle);
                m_FrameHandle = IntPtr.Zero;
            }

            m_FrameHandle = ExternApi.ARCoreARKitIntegration_getCurrentFrame(m_RealArKitSessionHandle);
            ExternApi.ArSession_updateAndAcquireArFrame(m_SessionHandle, m_FrameHandle, ref m_FrameHandle);

            NativeSession.OnUpdate(m_FrameHandle);
            AsyncTask.OnUpdate();

            if (EarlyUpdateEvent != null)
            {
                EarlyUpdateEvent();
            }
        }
#endif

        private void _RegisterFrameUpdatedEvent()
        {
#if UNITY_IOS
            UnityEngine.XR.iOS.UnityARSessionNativeInterface.ARFrameUpdatedEvent += _OnARKitFrameUpdated;
#endif
        }

        private void _UnRegisterFrameUpdatedEvent()
        {
#if UNITY_IOS
            UnityEngine.XR.iOS.UnityARSessionNativeInterface.ARFrameUpdatedEvent -= _OnARKitFrameUpdated;
#endif
        }

        private IntPtr _GetSessionHandleFromArkitPlugin()
        {
            IntPtr result = IntPtr.Zero;
#if UNITY_IOS
            var m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
            var sessionField = m_session.GetType().GetField("m_NativeARSession", BindingFlags.NonPublic | BindingFlags.Instance);
            var val = sessionField.GetValue(m_session);
            result = ExternApi.ARCoreARKitIntegration_castUnitySessionToARKitSession((System.IntPtr)val);
#endif
            return result;
        }

        private struct ExternApi
        {
            [DllImport("__Internal")]
            public static extern IntPtr ARCoreARKitIntegration_castUnitySessionToARKitSession(IntPtr sessionToCast);

            [DllImport("__Internal")]
            public static extern IntPtr ARCoreARKitIntegration_getCurrentFrame(IntPtr arkitSessionHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_create(string apiKey, string bundleIdentifier, ref IntPtr sessionHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_destroy(IntPtr session);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_updateAndAcquireArFrame(IntPtr sessionHandle,
                IntPtr arkitFrameHandle, ref IntPtr arFrame);
        }
    }
}
