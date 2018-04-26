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

#if UNITY_IOS
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

        public event LifecycleManager.EarlyUpdateDelegate EarlyUpdateEvent;

        public static ARCoreAndroidLifecycleManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ARCoreAndroidLifecycleManager();
                    s_Instance._Initialize();
                }

                return s_Instance;
            }
        }

        public Texture2D BackgroundTexture { get; private set; }

        public bool IsTracking { get; private set; }

        public NativeSession NativeSession { get; private set; }

        public ARCoreSession SessionComponent { get; private set; }

        public SessionStatus SessionStatus { get; private set; }

        public void SetConfiguration(ARCoreSessionConfig config)
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
                handle.Free();
            }
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
            ARPrestoCallbackManager.Instance.InitializeIfNeeded();
            EnableSession();
        }

        public void EnableSession()
        {
            // No session component in the scene and thus no session to enable.
            if (SessionComponent == null)
            {
                return;
            }

            SetConfiguration(SessionComponent.SessionConfig);
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

        internal void EarlyUpdate()
        {
            // Get the latest session handle from presto.
            IntPtr sessionHandle = IntPtr.Zero;
            ExternApi.ArPresto_getSession(ref sessionHandle);

            // Update the display geometry if there is a non-null session.
            if (sessionHandle != IntPtr.Zero)
            {
                _SetDisplayGeometry(sessionHandle, Screen.orientation, Screen.width, Screen.height);
            }

            // Update ArPresto and potentially ArCore.
            ExternApi.ArPresto_update();

            // Update pending AsyncTasks.
            AsyncTask.OnUpdate();

            // Update the lifecycle state.
            _UpdateState();

            // Return if there is no session component in the scene.
            if (SessionComponent == null)
            {
                return;
            }

            ExternApi.ArPresto_getSession(ref sessionHandle);

            IntPtr frameHandle = IntPtr.Zero;
            ExternApi.ArPresto_getFrame(ref frameHandle);

            if (NativeSession == null && sessionHandle != IntPtr.Zero)
            {
                NativeSession = new NativeSession(sessionHandle, frameHandle);
                NativeSession.SessionApi.ReportEngineType();
            }

            if (NativeSession != null)
            {
                NativeSession.OnUpdate(frameHandle);
            }

            SetConfiguration(SessionComponent.SessionConfig);

            _UpdateTextureIfNeeded();

            InstantPreviewManager.OnEarlyUpdate();

            if (EarlyUpdateEvent != null)
            {
                EarlyUpdateEvent();
            }
        }

        private void _Initialize()
        {
            m_CachedConfig = null;
            BackgroundTexture = null;
            NativeSession = null;
            SessionComponent = null;
            IsTracking = false;
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

        private void _UpdateState()
        {
            ApiPrestoStatus prestoStatus = ApiPrestoStatus.Uninitialized;
            ExternApi.ArPresto_getStatus(ref prestoStatus);

            IsTracking = prestoStatus == ApiPrestoStatus.Resumed;
            SessionStatus = prestoStatus.ToSessionStatus();
        }

        private void _SetDisplayGeometry(IntPtr sessionHandle, ScreenOrientation orientation, int width, int height)
        {
            const int androidRotation0 = 0;
            const int androidRotation90 = 1;
            const int androidRotation180 = 2;
            const int androidRotation270 = 3;

            if (sessionHandle == IntPtr.Zero)
            {
                return;
            }

            int androidOrientation = 0;
            switch (orientation)
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

            ExternApi.ArSession_setDisplayGeometry(sessionHandle, androidOrientation, width, height);
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
