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

        private bool m_DisabledSessionOnErrorState = false;

        // Only care about disable to enable transition here (ignore enable to disable transition)
        // because it will triggier _OnBeforeResumeSession which links to a public API
        // RegisterChooseCameraConfigurationCallback.
        private bool m_HaveDisableToEnableTransition = false;

        private AndroidNativeHelper.AndroidSurfaceRotation m_CachedDisplayRotation =
            AndroidNativeHelper.AndroidSurfaceRotation.Rotation0;

        private List<IntPtr> m_TempCameraConfigHandles = new List<IntPtr>();

        private List<CameraConfig> m_TempCameraConfigs = new List<CameraConfig>();

        public event Action UpdateSessionFeatures;

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

                    ExperimentManager.Instance.Initialize();
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
            if (SessionComponent == null || sessionHandle == IntPtr.Zero)
            {
                return;
            }

            NativeSession tempNativeSession = _GetNativeSession(sessionHandle);

            var listHandle = tempNativeSession.CameraConfigListApi.Create();
            tempNativeSession.SessionApi.GetSupportedCameraConfigurationsWithFilter(
                SessionComponent.CameraConfigFilter,
                listHandle, m_TempCameraConfigHandles, m_TempCameraConfigs,
                SessionComponent.DeviceCameraDirection);

            if (m_TempCameraConfigHandles.Count == 0)
            {
                Debug.LogWarning(
                    "Unable to choose a custom camera configuration because none are available.");
            }
            else
            {
                var configIndex = 0;
                if (SessionComponent.GetChooseCameraConfigurationCallback() != null)
                {
                    configIndex = SessionComponent.GetChooseCameraConfigurationCallback()(
                        m_TempCameraConfigs);
                }

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
            if (SessionComponent != null)
            {
                IntPtr previousSession = IntPtr.Zero;
                ExternApi.ArPresto_getSession(ref previousSession);

                if (UpdateSessionFeatures != null)
                {
                    UpdateSessionFeatures();
                }

                _SetCameraDirection(SessionComponent.DeviceCameraDirection);

                IntPtr currentSession = IntPtr.Zero;
                ExternApi.ArPresto_getSession(ref currentSession);

                // Fire the session enabled event when the underlying session has been changed
                // due to session feature update(camera direction etc).
                if (previousSession != currentSession)
                {
                    _FireOnSessionSetEnabled(false);
                    _FireOnSessionSetEnabled(true);
                }

                _SetConfiguration(SessionComponent.SessionConfig);
            }

            _UpdateDisplayGeometry();

            // Update ArPresto and potentially ArCore.
            ExternApi.ArPresto_update();

            SessionStatus previousSessionStatus = SessionStatus;

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

            // If the current status is an error, check if the SessionStatus error state changed.
            if (SessionStatus.IsError() &&
                previousSessionStatus.IsError() != SessionStatus.IsError())
            {
                // Disable internal session bits so we properly pause the session due to error.
                _FireOnSessionSetEnabled(false);
                m_DisabledSessionOnErrorState = true;
            }
            else if (SessionStatus.IsValid() && m_DisabledSessionOnErrorState)
            {
                if (SessionComponent.enabled)
                {
                    _FireOnSessionSetEnabled(true);
                }

                m_DisabledSessionOnErrorState = false;
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

            // If the session status is an error, do not fire the callback itself; but do
            // ArPresto_setEnabled to signal the intention to resume once the session status is
            // valid.
            if (!SessionStatus.IsError())
            {
                _FireOnSessionSetEnabled(sessionEnabled);
            }

            ExternApi.ArPresto_setEnabled(sessionEnabled);
        }

        private bool _SetCameraDirection(DeviceCameraDirection cameraDirection)
        {
            // The camera direction has not changed.
            if (m_CachedCameraDirection.HasValue &&
                m_CachedCameraDirection.Value == cameraDirection)
            {
                return false;
            }

            if (InstantPreviewManager.IsProvidingPlatform &&
                cameraDirection == DeviceCameraDirection.BackFacing)
            {
                return false;
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

                return false;
            }

            m_CachedCameraDirection = cameraDirection;
            var apiCameraDirection =
                cameraDirection == DeviceCameraDirection.BackFacing ?
                    ApiPrestoDeviceCameraDirection.BackFacing :
                    ApiPrestoDeviceCameraDirection.FrontFacing;

            ExternApi.ArPresto_setDeviceCameraDirection(apiCameraDirection);

            return true;
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
                if (config.LightEstimationMode != LightEstimationMode.Disabled)
                {
                    InstantPreviewManager.LogLimitedSupportMessage("enable 'Light Estimation'");
                    config.LightEstimationMode = LightEstimationMode.Disabled;
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
            if (!m_CachedScreenOrientation.HasValue ||
                Screen.orientation != m_CachedScreenOrientation)
            {
                m_CachedScreenOrientation = Screen.orientation;
                m_CachedDisplayRotation = AndroidNativeHelper.GetDisplayRotation();
            }

            ExternApi.ArPresto_setDisplayGeometry(
                m_CachedDisplayRotation, Screen.width, Screen.height);
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
            [AndroidImport(ApiConstants.ARCoreShimApi)]
            public static extern int ArCoreUnity_getBackgroundTextureId();

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_setDisplayGeometry(
                AndroidNativeHelper.AndroidSurfaceRotation rotation, int width, int height);

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
