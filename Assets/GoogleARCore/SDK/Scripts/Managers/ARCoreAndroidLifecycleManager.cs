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
    using System.Collections.Generic;
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
        private static ARCoreAndroidLifecycleManager s_Instance = null;

        private IntPtr m_CachedSessionHandle = IntPtr.Zero;

        private IntPtr m_CachedFrameHandle = IntPtr.Zero;

        private Dictionary<IntPtr, NativeSession> m_NativeSessions =
            new Dictionary<IntPtr, NativeSession>(new IntPtrEqualityComparer());

        private DeviceCameraDirection? m_CachedCameraDirection = null;

        private ARCoreSessionConfig m_CachedConfig = null;

        private ScreenOrientation? m_CachedScreenOrientation = null;

        private bool? m_DesiredSessionState = null;

        // Only care about disable to enable transition here (ignore enable to disable transition)
        // because it will triggier _OnBeforeResumeSession which links to a public API
        // RegisterChooseCameraConfigurationCallback.
        private bool m_HaveDisableToEnableTransition = false;

        private AndroidNativeHelper.AndroidSurfaceRotation m_CachedDisplayRotation =
            AndroidNativeHelper.AndroidSurfaceRotation.Rotation0;

        private List<IntPtr> m_TempCameraConfigHandles = new List<IntPtr>();

        private List<CameraConfig> m_TempCameraConfigs = new List<CameraConfig>();

        public event Action EarlyUpdate;

        public event Action<bool> OnSessionSetEnabled;

        public static ARCoreAndroidLifecycleManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ARCoreAndroidLifecycleManager();
                    s_Instance._Initialize();
                    ARPrestoCallbackManager.Instance.EarlyUpdate += s_Instance._OnEarlyUpdate;
                    ARPrestoCallbackManager.Instance.BeforeResumeSession +=
                        s_Instance._OnBeforeResumeSession;
                }

                return s_Instance;
            }
        }

        public SessionStatus SessionStatus { get; private set; }

        public LostTrackingReason LostTrackingReason { get; private set; }

        public ARCoreSession SessionComponent { get; private set; }

        public NativeSession NativeSession
        {
            get
            {
                if (m_CachedSessionHandle == IntPtr.Zero)
                {
                    return null;
                }

                return _GetNativeSession(m_CachedSessionHandle);
            }
        }

        public bool IsSessionChangedThisFrame { get; private set; }

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
            if (m_DesiredSessionState.HasValue && !m_DesiredSessionState.Value)
            {
                m_HaveDisableToEnableTransition = true;
            }

            m_DesiredSessionState = true;
        }

        public void DisableSession()
        {
            m_DesiredSessionState = false;
        }

        public void ResetSession()
        {
            _FireOnSessionSetEnabled(false);
            _Initialize();
            ExternApi.ArPresto_reset();
        }

        private void _OnBeforeResumeSession(IntPtr sessionHandle)
        {
            if (SessionComponent == null || sessionHandle == IntPtr.Zero ||
                SessionComponent.GetChooseCameraConfigurationCallback() == null)
            {
                return;
            }

            NativeSession tempNativeSession = _GetNativeSession(sessionHandle);

            var listHandle = tempNativeSession.CameraConfigListApi.Create();
            tempNativeSession.SessionApi.GetSupportedCameraConfigurations(
                listHandle, m_TempCameraConfigHandles, m_TempCameraConfigs,
                SessionComponent.DeviceCameraDirection);

            if (m_TempCameraConfigHandles.Count == 0)
            {
                Debug.LogWarning(
                    "Unable to choose a custom camera configuration because none are available.");
            }
            else
            {
                var configIndex =
                    SessionComponent.GetChooseCameraConfigurationCallback()(m_TempCameraConfigs);
                if (configIndex >= 0 && configIndex < m_TempCameraConfigHandles.Count)
                {
                    var status = tempNativeSession.SessionApi.SetCameraConfig(
                        m_TempCameraConfigHandles[configIndex]);
                    if (status != ApiArStatus.Success)
                    {
                        Debug.LogErrorFormat(
                            "Failed to set the ARCore camera configuration: {0}", status);
                    }
                }

                for (int i = 0; i < m_TempCameraConfigHandles.Count; i++)
                {
                    tempNativeSession.CameraConfigApi.Destroy(m_TempCameraConfigHandles[i]);
                }
            }

            // clean up
            tempNativeSession.CameraConfigListApi.Destroy(listHandle);

            m_TempCameraConfigHandles.Clear();
            m_TempCameraConfigs.Clear();
        }

        private void _OnEarlyUpdate()
        {
            // Update session activity before EarlyUpdate.
            if (m_HaveDisableToEnableTransition)
            {
                _SetSessionEnabled(false);
                _SetSessionEnabled(true);
                m_HaveDisableToEnableTransition = false;

                // Avoid firing session enable event twice.
                if (m_DesiredSessionState.HasValue && m_DesiredSessionState.Value)
                {
                    m_DesiredSessionState = null;
                }
            }

            if (m_DesiredSessionState.HasValue)
            {
                _SetSessionEnabled(m_DesiredSessionState.Value);
                m_DesiredSessionState = null;
            }

            // Perform updates before calling ArPresto_update.
            _UpdateDisplayGeometry();
            if (SessionComponent != null)
            {
                _SetCameraDirection(SessionComponent.DeviceCameraDirection);
                _SetConfiguration(SessionComponent.SessionConfig);
            }

            // Update ArPresto and potentially ArCore.
            ExternApi.ArPresto_update();

            // Get state information from ARPresto.
            ApiPrestoStatus prestoStatus = ApiPrestoStatus.Uninitialized;
            ExternApi.ArPresto_getStatus(ref prestoStatus);
            SessionStatus = prestoStatus.ToSessionStatus();

            LostTrackingReason = LostTrackingReason.None;
            if (NativeSession != null && SessionStatus == SessionStatus.LostTracking)
            {
                var cameraHandle = NativeSession.FrameApi.AcquireCamera();
                LostTrackingReason = NativeSession.CameraApi.GetLostTrackingReason(cameraHandle);
                NativeSession.CameraApi.Release(cameraHandle);
            }

            // Get the current session from presto and note if it has changed.
            IntPtr sessionHandle = IntPtr.Zero;
            ExternApi.ArPresto_getSession(ref sessionHandle);
            IsSessionChangedThisFrame = m_CachedSessionHandle != sessionHandle;
            m_CachedSessionHandle = sessionHandle;

            ExternApi.ArPresto_getFrame(ref m_CachedFrameHandle);

            // Update the native session with the newest frame.
            if (NativeSession != null)
            {
                NativeSession.OnUpdate(m_CachedFrameHandle);
            }

            _UpdateTextureIfNeeded();

            if (EarlyUpdate != null)
            {
                EarlyUpdate();
            }
        }

        private void _Initialize()
        {
            if (m_NativeSessions != null)
            {
                foreach (var nativeSession in m_NativeSessions.Values)
                {
                    nativeSession.MarkDestroyed();
                }
            }

            m_NativeSessions = new Dictionary<IntPtr, NativeSession>(new IntPtrEqualityComparer());
            m_CachedSessionHandle = IntPtr.Zero;
            m_CachedFrameHandle = IntPtr.Zero;
            m_CachedConfig = null;
            m_DesiredSessionState = null;
            m_HaveDisableToEnableTransition = false;
            BackgroundTexture = null;
            SessionComponent = null;
            IsSessionChangedThisFrame = true;
            SessionStatus = SessionStatus.None;
            LostTrackingReason = LostTrackingReason.None;
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
                BackgroundTexture = Texture2D.CreateExternalTexture(
                    0, 0, TextureFormat.ARGB32, false, false, new IntPtr(backgroundTextureId));
                return;
            }

            BackgroundTexture.UpdateExternalTexture(new IntPtr(backgroundTextureId));
        }

        private void _SetSessionEnabled(bool sessionEnabled)
        {
            if (sessionEnabled && SessionComponent == null)
            {
                return;
            }

            _FireOnSessionSetEnabled(sessionEnabled);
            ExternApi.ArPresto_setEnabled(sessionEnabled);
        }

        private void _SetCameraDirection(DeviceCameraDirection cameraDirection)
        {
            // The camera direction has not changed.
            if (m_CachedCameraDirection.HasValue &&
                m_CachedCameraDirection.Value == cameraDirection)
            {
                return;
            }

            if (InstantPreviewManager.IsProvidingPlatform &&
                cameraDirection == DeviceCameraDirection.BackFacing)
            {
                return;
            }
            else if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage(
                    "enable front-facing (selfie) camera");
                m_CachedCameraDirection = DeviceCameraDirection.BackFacing;
                if (SessionComponent != null)
                {
                    SessionComponent.DeviceCameraDirection = DeviceCameraDirection.BackFacing;
                }

                return;
            }

            m_CachedCameraDirection = cameraDirection;
            var apiCameraDirection =
                cameraDirection == DeviceCameraDirection.BackFacing ?
                    ApiPrestoDeviceCameraDirection.BackFacing :
                    ApiPrestoDeviceCameraDirection.FrontFacing;

            _FireOnSessionSetEnabled(false);
            ExternApi.ArPresto_setDeviceCameraDirection(apiCameraDirection);
            _FireOnSessionSetEnabled(true);
        }

        private void _SetConfiguration(ARCoreSessionConfig config)
        {
            // There is no configuration to set.
            if (config == null)
            {
                return;
            }

            // The configuration has not been updated.
            if (m_CachedConfig != null && config.Equals(m_CachedConfig) &&
                (config.AugmentedImageDatabase == null ||
                 !config.AugmentedImageDatabase.m_IsDirty) &&
                !ExperimentManager.Instance.IsConfigurationDirty)
            {
                return;
            }

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                if (config.PlaneFindingMode == DetectedPlaneFindingMode.Disabled)
                {
                    InstantPreviewManager.LogLimitedSupportMessage("disable 'Plane Finding'");
                    config.PlaneFindingMode = DetectedPlaneFindingMode.Horizontal;
                }

                if (!config.EnableLightEstimation)
                {
                    InstantPreviewManager.LogLimitedSupportMessage("disable 'Light Estimation'");
                    config.EnableLightEstimation = true;
                }

                if (config.CameraFocusMode == CameraFocusMode.Auto)
                {
                    InstantPreviewManager.LogLimitedSupportMessage("enable camera Auto Focus mode");
                    config.CameraFocusMode = CameraFocusMode.Fixed;
                }

                if (config.AugmentedImageDatabase != null)
                {
                    InstantPreviewManager.LogLimitedSupportMessage("enable 'Augmented Images'");
                    config.AugmentedImageDatabase = null;
                }

                if (config.AugmentedFaceMode == AugmentedFaceMode.Mesh)
                {
                    InstantPreviewManager.LogLimitedSupportMessage("enable 'Augmented Faces'");
                    config.AugmentedFaceMode = AugmentedFaceMode.Disabled;
                }
            }

            var prestoConfig = new ApiPrestoConfig(config);
            ExternApi.ArPresto_setConfiguration(ref prestoConfig);
            m_CachedConfig = ScriptableObject.CreateInstance<ARCoreSessionConfig>();
            m_CachedConfig.CopyFrom(config);
        }

        private void _UpdateDisplayGeometry()
        {
            IntPtr sessionHandle = IntPtr.Zero;
            ExternApi.ArPresto_getSession(ref sessionHandle);

            if (sessionHandle == IntPtr.Zero)
            {
                return;
            }

            if (!m_CachedScreenOrientation.HasValue ||
                Screen.orientation != m_CachedScreenOrientation)
            {
                m_CachedScreenOrientation = Screen.orientation;
                m_CachedDisplayRotation = AndroidNativeHelper.GetDisplayRotation();
            }

            ExternApi.ArSession_setDisplayGeometry(
                sessionHandle, (int)m_CachedDisplayRotation, Screen.width, Screen.height);
        }

        private NativeSession _GetNativeSession(IntPtr sessionHandle)
        {
            NativeSession nativeSession;
            if (!m_NativeSessions.TryGetValue(sessionHandle, out nativeSession))
            {
                nativeSession = new NativeSession(sessionHandle, m_CachedFrameHandle);
                m_NativeSessions.Add(sessionHandle, nativeSession);
            }

            return nativeSession;
        }

        private void _FireOnSessionSetEnabled(bool isEnabled)
        {
            if (OnSessionSetEnabled != null)
            {
                OnSessionSetEnabled(isEnabled);
            }
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setDisplayGeometry(
                IntPtr sessionHandle, int rotation, int width, int height);

            [AndroidImport(ApiConstants.ARCoreShimApi)]
            public static extern int ArCoreUnity_getBackgroundTextureId();

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_getSession(ref IntPtr sessionHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_setDeviceCameraDirection(
                ApiPrestoDeviceCameraDirection cameraDirection);

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
