//-----------------------------------------------------------------------
// <copyright file="ARCoreAndroidLifecycleManager.cs" company="Google">
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
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class ARCoreAndroidLifecycleManager : ILifecycleManager
    {
        private static ARCoreAndroidLifecycleManager s_Instance;

        private ARCoreSessionConfig m_CachedConfig = null;

        public event Action EarlyUpdate;

        public static ARCoreAndroidLifecycleManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ARCoreAndroidLifecycleManager();
                    s_Instance._Initialize();
                    ARPrestoCallbackManager.Instance.EarlyUpdate += s_Instance._OnEarlyUpdate;
                    s_Instance.EarlyUpdate += InstantPreviewManager.OnEarlyUpdate;
                }

                return s_Instance;
            }
        }

        public SessionStatus SessionStatus { get; private set; }

        public ARCoreSession SessionComponent { get; private set; }

        public NativeSession NativeSession { get; private set; }

        public Texture2D BackgroundTexture { get; private set; }

        public AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
            return ARPrestoCallbackManager.Instance.CheckApkAvailability();
        }

        public AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested)
        {
            return ARPrestoCallbackManager.Instance.RequestApkInstallation(userRequested);
        }

        public void CreateSession(ARCoreSession sessionComponent)
        {
            sessionComponent.StartCoroutine(InstantPreviewManager.InitializeIfNeeded());

            if (SessionComponent != null)
            {
                Debug.LogError("Multiple ARCore session components cannot exist in the scene. " +
                    "Destroying the newest.");
                GameObject.Destroy(sessionComponent);
                return;
            }

            SessionComponent = sessionComponent;
        }

        public void EnableSession()
        {
            // No session component in the scene and thus no session to enable.
            if (SessionComponent == null)
            {
                return;
            }

            _SetConfiguration(SessionComponent.SessionConfig);
            ExternApi.ArPresto_setEnabled(true);
        }

        public void DisableSession()
        {
            ExternApi.ArPresto_setEnabled(false);
        }

        public void ResetSession()
        {
            _Initialize();
            ExternApi.ArPresto_reset();
        }

        private void _OnEarlyUpdate()
        {
            // Perform updates before calling ArPresto_update.
            _UpdateDisplayGeometry();
            if (SessionComponent != null)
            {
                _SetConfiguration(SessionComponent.SessionConfig);
            }

            // Update ArPresto and potentially ArCore.
            ExternApi.ArPresto_update();

            // Get state information from ARPresto.
            ApiPrestoStatus prestoStatus = ApiPrestoStatus.Uninitialized;
            IntPtr sessionHandle = IntPtr.Zero;
            IntPtr frameHandle = IntPtr.Zero;
            ExternApi.ArPresto_getStatus(ref prestoStatus);
            ExternApi.ArPresto_getSession(ref sessionHandle);
            ExternApi.ArPresto_getFrame(ref frameHandle);

            SessionStatus = prestoStatus.ToSessionStatus();

            // Update native session reference to match presto.
            if (sessionHandle == IntPtr.Zero)
            {
                NativeSession = null;
            }
            else if (NativeSession == null)
            {
                NativeSession = new NativeSession(sessionHandle, frameHandle);
            }

            // Update the native session with the newest frame.
            if (NativeSession != null)
            {
                NativeSession.OnUpdate(frameHandle);
            }

            _UpdateTextureIfNeeded();

            if (EarlyUpdate != null)
            {
                EarlyUpdate();
            }
        }

        private void _Initialize()
        {
            m_CachedConfig = null;
            BackgroundTexture = null;
            NativeSession = null;
            SessionComponent = null;
            SessionStatus = SessionStatus.None;
        }

        private void _UpdateTextureIfNeeded()
        {
            // If running in editor, updates background texture from Instant Preview only.
            Texture2D previewBackgroundTexture = BackgroundTexture;
            if (InstantPreviewManager.UpdateBackgroundTextureIfNeeded(ref previewBackgroundTexture))
            {
                BackgroundTexture = previewBackgroundTexture;
                return;
            }

            IntPtr frameHandle = IntPtr.Zero;
            ExternApi.ArPresto_getFrame(ref frameHandle);

            int backgroundTextureId = ExternApi.ArCoreUnity_getBackgroundTextureId();

            if (frameHandle == IntPtr.Zero)
            {
                // This prevents using a texture that has not been filled out by ARCore.
                return;
            }
            else if (backgroundTextureId == -1)
            {
                return;
            }
            else if (BackgroundTexture != null &&
                BackgroundTexture.GetNativeTexturePtr().ToInt32() == backgroundTextureId)
            {
                return;
            }
            else if (BackgroundTexture == null)
            {
                // The Unity-cached size and format of the texture (0x0, ARGB) is not the
                // actual format of the texture. This is okay because the texture is not
                // accessed by pixels, it is accessed with UV coordinates.
                BackgroundTexture = Texture2D.CreateExternalTexture(0, 0, TextureFormat.ARGB32, false,
                    false, new IntPtr(backgroundTextureId));
                return;
            }

            BackgroundTexture.UpdateExternalTexture(new IntPtr(backgroundTextureId));
        }

        private void _SetConfiguration(ARCoreSessionConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (m_CachedConfig == null || !config.Equals(m_CachedConfig) ||
                ExperimentManager.Instance.IsConfigurationDirty)
            {
                GCHandle handle;
                var prestoConfig = new ApiPrestoConfig(config, out handle);
                ExternApi.ArPresto_setConfiguration(ref prestoConfig);
                m_CachedConfig = ScriptableObject.CreateInstance<ARCoreSessionConfig>();
                m_CachedConfig.CopyFrom(config);

                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        private void _UpdateDisplayGeometry()
        {
            const int androidRotation0 = 0;
            const int androidRotation90 = 1;
            const int androidRotation180 = 2;
            const int androidRotation270 = 3;

            IntPtr sessionHandle = IntPtr.Zero;
            ExternApi.ArPresto_getSession(ref sessionHandle);

            if (sessionHandle == IntPtr.Zero)
            {
                return;
            }

            int androidOrientation = 0;
            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    androidOrientation = androidRotation90;
                    break;
                case ScreenOrientation.LandscapeRight:
                    androidOrientation = androidRotation270;
                    break;
                case ScreenOrientation.Portrait:
                    androidOrientation = androidRotation0;
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    androidOrientation = androidRotation180;
                    break;
            }

            ExternApi.ArSession_setDisplayGeometry(sessionHandle, androidOrientation, Screen.width, Screen.height);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setDisplayGeometry(IntPtr sessionHandle, int rotation, int width,
                int height);

            [AndroidImport(ApiConstants.ARCoreShimApi)]
            public static extern int ArCoreUnity_getBackgroundTextureId();

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_getSession(ref IntPtr sessionHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_setConfiguration(ref ApiPrestoConfig config);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_setEnabled(bool isEnabled);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_getFrame(ref IntPtr frameHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_getStatus(ref ApiPrestoStatus prestoStatus);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_update();

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_reset();
#pragma warning restore 626
        }
    }
}
