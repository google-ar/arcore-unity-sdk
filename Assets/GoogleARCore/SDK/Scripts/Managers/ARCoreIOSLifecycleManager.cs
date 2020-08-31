//-----------------------------------------------------------------------
// <copyright file="ARCoreIOSLifecycleManager.cs" company="Google LLC">
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
#if UNITY_IOS
namespace GoogleARCoreInternal
{
    using System;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using GoogleARCoreInternal.CrossPlatform;
    using UnityEngine;

    internal class ARCoreIOSLifecycleManager : ILifecycleManager
    {
        private static ARCoreIOSLifecycleManager _instance;
#if ARCORE_IOS_SUPPORT

        internal static IARCoreiOSHelper _arCoreiOSHelper = null;

        private IntPtr _sessionHandle = IntPtr.Zero;

        private IntPtr _frameHandle = IntPtr.Zero;

        private bool _sessionEnabled = false;

        private IntPtr _realArKitSessionHandle = IntPtr.Zero;
#endif // ARCORE_IOS_SUPPORT
        // Avoid warnings for fields that are unused on when ARCORE_IOS_SUPPORT is not defined.
#pragma warning disable 67, 414

        public event Action EarlyUpdate;

        public event Action UpdateSessionFeatures;

        public event Action<bool> OnSessionSetEnabled;

        public event Action<IntPtr, IntPtr> OnSetConfiguration;

        public event Action OnResetInstance;
#pragma warning restore 67, 414

        public static ARCoreIOSLifecycleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ARCoreIOSLifecycleManager();
#if ARCORE_IOS_SUPPORT
                    if (_arCoreiOSHelper == null)
                    {
                        _arCoreiOSHelper = new ARCoreiOSHelper();
                    }
                    _instance._Initialize();
#endif
                }

                return _instance;
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

            SessionComponent = sessionComponent;
            _realArKitSessionHandle = _arCoreiOSHelper.GetARKitSessionPtr();
            string apiKey = _arCoreiOSHelper.GetCloudServicesApiKey();
            var status =
                ExternApi.ArSession_create(apiKey, null, ref _sessionHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat(
                    "Could not create a cross platform ARCore session ({0}).", status);
                return;
            }

            NativeSession = new NativeSession(_sessionHandle, IntPtr.Zero);
            _arCoreiOSHelper.RegisterFrameUpdateEvent(_instance._OnARKitFrameUpdated);
#else
            Debug.Log("ARCore iOS Support is not enabled. ARCore will be disabled on iOS device.");
            return;
#endif
        }

        public void EnableSession()
        {
#if ARCORE_IOS_SUPPORT
            _sessionEnabled = true;
            SessionStatus = SessionStatus.Tracking;
#endif
        }

        public void DisableSession()
        {
#if ARCORE_IOS_SUPPORT
            _sessionEnabled = false;
            SessionStatus = SessionStatus.NotTracking;
#endif
        }

        public void ResetSession()
        {
#if ARCORE_IOS_SUPPORT
            if (_sessionHandle != IntPtr.Zero)
            {
                if (_frameHandle != IntPtr.Zero)
                {
                    NativeSession.FrameApi.Release(_frameHandle);
                    _frameHandle = IntPtr.Zero;
                }

                ExternApi.ArSession_destroy(_sessionHandle);
                _sessionHandle = IntPtr.Zero;
            }

            if (NativeSession != null)
            {
                NativeSession.MarkDestroyed();
            }

            _Initialize();
#endif
        }

        /// <summary>
        /// Force reset the singleton instance to null. Should only be used in Unit Test.
        /// </summary>
        internal static void ResetInstance()
        {
#if ARCORE_IOS_SUPPORT
            if (_instance != null && _instance.OnResetInstance != null)
            {
                _instance.OnResetInstance();
            }

            _instance = null;
#endif
        }

#if ARCORE_IOS_SUPPORT
        private void _OnARKitFrameUpdated(UnityEngine.XR.iOS.UnityARCamera camera)
        {
            if (_frameHandle != IntPtr.Zero)
            {
                NativeSession.FrameApi.Release(_frameHandle);
                _frameHandle = IntPtr.Zero;
            }

            if (_sessionEnabled)
            {
                _frameHandle = _arCoreiOSHelper.GetARKitFramePtr(_realArKitSessionHandle);
                ExternApi.ArSession_updateAndAcquireArFrame(
                    _sessionHandle, _frameHandle, ref _frameHandle);
            }

            if (NativeSession != null)
            {
                NativeSession.OnUpdate(_frameHandle);
            }

            if (EarlyUpdate != null)
            {
                EarlyUpdate();
            }
        }

        private void _Initialize()
        {
            _sessionEnabled = false;
            SessionStatus = SessionStatus.NotTracking;
            LostTrackingReason = LostTrackingReason.None;
            IsSessionChangedThisFrame = false;
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_create(
                string apiKey, string bundleIdentifier, ref IntPtr sessionHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_destroy(IntPtr session);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_updateAndAcquireArFrame(
                IntPtr sessionHandle, IntPtr arkitFrameHandle, ref IntPtr arFrame);
        }
#endif // ARCORE_IOS_SUPPORT
    }
}
#endif
