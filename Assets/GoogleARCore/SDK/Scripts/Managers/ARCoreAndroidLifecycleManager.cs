//-----------------------------------------------------------------------
// <copyright file="ARCoreAndroidLifecycleManager.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    using System.Linq;
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
        private const int _mtNumTextureIds = 4;

        private const int _invalidTextureId = -1;

        private const int _nullTextureId = 0;

        private static ARCoreAndroidLifecycleManager _instance = null;

        private IntPtr _cachedSessionHandle = IntPtr.Zero;

        private IntPtr _cachedFrameHandle = IntPtr.Zero;

        private Dictionary<IntPtr, NativeSession> _nativeSessions =
            new Dictionary<IntPtr, NativeSession>(new IntPtrEqualityComparer());

        private DeviceCameraDirection? _cachedCameraDirection = null;

        private ARCoreSessionConfig _cachedConfig = null;

        private ScreenOrientation? _cachedScreenOrientation = null;

        private bool? _desiredSessionState = null;

        private bool _disabledSessionOnErrorState = false;

        // Only care about disable to enable transition here (ignore enable to disable transition)
        // because it will triggier OnBeforeResumeSession which links to a public API
        // RegisterChooseCameraConfigurationCallback.
        private bool _haveDisableToEnableTransition = false;

        private AsyncTask<AndroidPermissionsRequestResult>
            _androidPermissionRequest = null;

        private HashSet<string> _requiredPermissionNames = new HashSet<string>();

        private AndroidNativeHelper.AndroidSurfaceRotation _cachedDisplayRotation =
            AndroidNativeHelper.AndroidSurfaceRotation.Rotation0;

        private List<IntPtr> _tempCameraConfigHandles = new List<IntPtr>();

        private List<CameraConfig> _tempCameraConfigs = new List<CameraConfig>();

        // List of OpenGL ES texture IDs for camera generated during OnEarlyUpdate
        private int[] _cameraTextureIds = null;
        private Dictionary<int, Texture2D> _textureIdToTexture2D =
            new Dictionary<int, Texture2D>();

        public event Action UpdateSessionFeatures;

        public event Action EarlyUpdate;

        public event Action<bool> OnSessionSetEnabled;

        public event Action<IntPtr, IntPtr> OnSetConfiguration;

        public event Action OnResetInstance;

        public static ARCoreAndroidLifecycleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ARCoreAndroidLifecycleManager();
                    _instance.Initialize();
                    ARPrestoCallbackManager.Instance.EarlyUpdate += _instance.OnEarlyUpdate;
                    ARPrestoCallbackManager.Instance.BeforeResumeSession +=
                        _instance.OnBeforeResumeSession;
                    ARPrestoCallbackManager.Instance.OnSetConfiguration +=
                        _instance.SetSessionConfiguration;

                    ExperimentManager.Instance.Initialize();
                }

                return _instance;
            }
        }

        public SessionStatus SessionStatus { get; private set; }

        public LostTrackingReason LostTrackingReason { get; private set; }

        public ARCoreSession SessionComponent { get; private set; }

        public NativeSession NativeSession
        {
            get
            {
                if (_cachedSessionHandle == IntPtr.Zero)
                {
                    return null;
                }

                return GetNativeSession(_cachedSessionHandle);
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
            if (_desiredSessionState.HasValue && !_desiredSessionState.Value)
            {
                _haveDisableToEnableTransition = true;
            }

            _desiredSessionState = true;
        }

        public void DisableSession()
        {
            _desiredSessionState = false;
        }

        public void ResetSession()
        {
            FireOnSessionSetEnabled(false);
            Initialize();
            ExternApi.ArPresto_reset();
        }

        /// <summary>
        /// Force reset the singleton instance to null. Should only be used in Unit Test.
        /// </summary>
        internal static void ResetInstance()
        {
            if (_instance != null && _instance.OnResetInstance != null)
            {
                _instance.OnResetInstance();
            }

            _instance = null;
        }

        private ApiPrestoCallbackResult OnBeforeResumeSession(IntPtr sessionHandle)
        {
            ApiPrestoCallbackResult result = ApiPrestoCallbackResult.InvalidCameraConfig;
            if (SessionComponent == null || sessionHandle == IntPtr.Zero)
            {
                return result;
            }

            NativeSession tempNativeSession = GetNativeSession(sessionHandle);
            var listHandle = tempNativeSession.CameraConfigListApi.Create();
            tempNativeSession.SessionApi.GetSupportedCameraConfigurationsWithFilter(
                SessionComponent.CameraConfigFilter,
                listHandle, _tempCameraConfigHandles, _tempCameraConfigs,
                SessionComponent.DeviceCameraDirection);

            if (_tempCameraConfigHandles.Count == 0)
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
                        _tempCameraConfigs);
                }

                if (configIndex >= 0 && configIndex < _tempCameraConfigHandles.Count)
                {
                    var status = tempNativeSession.SessionApi.SetCameraConfig(
                        _tempCameraConfigHandles[configIndex]);
                    if (status != ApiArStatus.Success)
                    {
                        Debug.LogErrorFormat(
                            "Failed to set the ARCore camera configuration: {0}", status);
                    }
                    else
                    {
                        result = ApiPrestoCallbackResult.Success;

                        // sync the session configuration with the new camera direction.
                        ExternApi.ArPresto_setConfigurationDirty();
                    }
                }

                for (int i = 0; i < _tempCameraConfigHandles.Count; i++)
                {
                    tempNativeSession.CameraConfigApi.Destroy(_tempCameraConfigHandles[i]);
                }
            }

            // clean up
            tempNativeSession.CameraConfigListApi.Destroy(listHandle);
            _tempCameraConfigHandles.Clear();
            _tempCameraConfigs.Clear();
            return result;
        }

        private void OnEarlyUpdate()
        {
            SetCameraTextureNameIfNecessary();

            // Update session activity before EarlyUpdate.
            if (_haveDisableToEnableTransition)
            {
                SetSessionEnabled(false);
                SetSessionEnabled(true);
                _haveDisableToEnableTransition = false;

                // Avoid firing session enable event twice.
                if (_desiredSessionState.HasValue && _desiredSessionState.Value)
                {
                    _desiredSessionState = null;
                }
            }

            if (_desiredSessionState.HasValue)
            {
                SetSessionEnabled(_desiredSessionState.Value);
                _desiredSessionState = null;
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

                SetCameraDirection(SessionComponent.DeviceCameraDirection);

                IntPtr currentSession = IntPtr.Zero;
                ExternApi.ArPresto_getSession(ref currentSession);

                // Fire the session enabled event when the underlying session has been changed
                // due to session feature update(camera direction etc).
                if (previousSession != currentSession)
                {
                    FireOnSessionSetEnabled(false);
                    FireOnSessionSetEnabled(true);
                }

                // Validate and convert the SessionConfig to a Instant Preview supported config by
                // logging and disabling limited supported features.
                if (InstantPreviewManager.IsProvidingPlatform &&
                    SessionComponent.SessionConfig != null &&
                    !InstantPreviewManager.ValidateSessionConfig(SessionComponent.SessionConfig))
                {
                    // A new SessionConfig object will be created based on the original
                    // SessionConfig with all limited support features disabled.
                    SessionComponent.SessionConfig =
                        InstantPreviewManager.GenerateInstantPreviewSupportedConfig(
                            SessionComponent.SessionConfig);
                }

                if (_requiredPermissionNames.Count > 0)
                {
                    RequestPermissions();
                }

                UpdateConfiguration(SessionComponent.SessionConfig);
            }

            UpdateDisplayGeometry();

            // Update ArPresto and potentially ArCore.
            ExternApi.ArPresto_update();
            if (SystemInfo.graphicsMultiThreaded && !InstantPreviewManager.IsProvidingPlatform)
            {
                // Synchronize render thread with update call.
                ExternApi.ARCoreRenderingUtils_CreatePostUpdateFence();
            }

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
                FireOnSessionSetEnabled(false);
                _disabledSessionOnErrorState = true;
            }
            else if (SessionStatus.IsValid() && _disabledSessionOnErrorState)
            {
                if (SessionComponent.enabled)
                {
                    FireOnSessionSetEnabled(true);
                }

                _disabledSessionOnErrorState = false;
            }

            // Get the current session from presto and note if it has changed.
            IntPtr sessionHandle = IntPtr.Zero;
            ExternApi.ArPresto_getSession(ref sessionHandle);
            IsSessionChangedThisFrame = _cachedSessionHandle != sessionHandle;
            _cachedSessionHandle = sessionHandle;

            ExternApi.ArPresto_getFrame(ref _cachedFrameHandle);

            // Update the native session with the newest frame.
            if (NativeSession != null)
            {
                NativeSession.OnUpdate(_cachedFrameHandle);
            }

            UpdateTextureIfNeeded();

            if (EarlyUpdate != null)
            {
                EarlyUpdate();
            }
        }

        private void SetCameraTextureNameIfNecessary()
        {
            if (InstantPreviewManager.IsProvidingPlatform)
            {
                return;
            }

            if (_cameraTextureIds == null)
            {
                GenerateCameraTextureNames();
            }

            // Check if ARCore has a valid texture name. If not, set it.
            if (NativeSession != null)
            {
                if (!ArCoreHasValidTextureName())
                {
                    ExternApi.ArPresto_setCameraTextureNames(
                        _cameraTextureIds.Length, _cameraTextureIds);
                }
            }
        }

        private bool ArCoreHasValidTextureName()
        {
            int textureName = NativeSession.FrameApi.GetCameraTextureName();
            return textureName != _invalidTextureId && textureName != _nullTextureId;
        }

        private void GenerateCameraTextureNames()
        {
            int textureNum = SystemInfo.graphicsMultiThreaded ? _mtNumTextureIds : 1;
            Debug.LogFormat("Using {0} textures for ARCore session", textureNum);
            _cameraTextureIds = new int[textureNum];
            OpenGL.glGenTextures(_cameraTextureIds.Length, _cameraTextureIds);
            int error = OpenGL.glGetError();
            if (error != 0)
            {
                Debug.LogErrorFormat("OpenGL glGenTextures error: {0}", error);
            }

            foreach (int textureId in _cameraTextureIds)
            {
                OpenGL.glBindTexture(OpenGL.Target.GL_TEXTURE_EXTERNAL_OES,
                                     textureId);
                Texture2D texture2d = Texture2D.CreateExternalTexture(
                    0, 0, TextureFormat.ARGB32, false, false, new IntPtr(textureId));
                _textureIdToTexture2D[textureId] = texture2d;
            }
        }

        private void Initialize()
        {
            if (_nativeSessions != null)
            {
                foreach (var nativeSession in _nativeSessions.Values)
                {
                    nativeSession.MarkDestroyed();
                }
            }

            _nativeSessions = new Dictionary<IntPtr, NativeSession>(new IntPtrEqualityComparer());
            _cachedSessionHandle = IntPtr.Zero;
            _cachedFrameHandle = IntPtr.Zero;
            _cachedConfig = null;
            _desiredSessionState = null;
            _haveDisableToEnableTransition = false;
            _requiredPermissionNames = new HashSet<string>();
            _androidPermissionRequest = null;
            BackgroundTexture = null;
            SessionComponent = null;
            IsSessionChangedThisFrame = true;
            SessionStatus = SessionStatus.None;
            LostTrackingReason = LostTrackingReason.None;
        }

        private void UpdateTextureIfNeeded()
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
            if (frameHandle == IntPtr.Zero)
            {
                // This prevents using a texture that has not been filled out by ARCore.
                return;
            }

            int backgroundTextureId = NativeSession.FrameApi.GetCameraTextureName();
            Texture2D texture2d = null;
            if (_textureIdToTexture2D.TryGetValue(backgroundTextureId, out texture2d))
            {
                BackgroundTexture = texture2d;
            }
        }

        private void SetSessionEnabled(bool sessionEnabled)
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
                FireOnSessionSetEnabled(sessionEnabled);
            }

            ExternApi.ArPresto_setEnabled(sessionEnabled);
        }

        private bool SetCameraDirection(DeviceCameraDirection cameraDirection)
        {
            // The camera direction has not changed.
            if (_cachedCameraDirection.HasValue &&
                _cachedCameraDirection.Value == cameraDirection)
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
                _cachedCameraDirection = DeviceCameraDirection.BackFacing;
                if (SessionComponent != null)
                {
                    SessionComponent.DeviceCameraDirection = DeviceCameraDirection.BackFacing;
                }

                return false;
            }

            _cachedCameraDirection = cameraDirection;
            ApiPrestoStatus prestoStatus = ApiPrestoStatus.Uninitialized;
            ExternApi.ArPresto_getStatus(ref prestoStatus);
            if (prestoStatus == ApiPrestoStatus.ErrorInvalidCameraConfig)
            {
                // if the session is paused by invalid camera configuration,
                // attempt to recover by changing the camera direction:
                OnBeforeResumeSession(_cachedSessionHandle);
            }

            return true;
        }

        private void SetSessionConfiguration(IntPtr sessionHandle, IntPtr configHandle)
        {
            if (configHandle == IntPtr.Zero)
            {
                Debug.LogWarning("Cannot set configuration for invalid configHanlde.");
                return;
            }

            if (sessionHandle == IntPtr.Zero && !InstantPreviewManager.IsProvidingPlatform)
            {
                Debug.LogWarning("Cannot set configuration for invalid sessionHandle.");
                return;
            }

            if (_cachedConfig == null)
            {
                return;
            }

            // Disable depth if the device doesn't support it.
            if (_cachedConfig.DepthMode != DepthMode.Disabled)
            {
                NativeSession tempNativeSession = GetNativeSession(sessionHandle);
                if (!tempNativeSession.SessionApi.IsDepthModeSupported(
                    _cachedConfig.DepthMode.ToApiDepthMode()))
                {
                    _cachedConfig.DepthMode = DepthMode.Disabled;
                }
            }

            // Don't set session config before the camera direction has changed to desired one.
            NativeSession nativeSession = GetNativeSession(sessionHandle);
            if (_cachedCameraDirection != null &&
                nativeSession.SessionApi.GetCameraConfig().FacingDirection !=
                _cachedCameraDirection)
            {
                _cachedConfig = null;
                return;
            }

            SessionConfigApi.UpdateApiConfigWithARCoreSessionConfig(
                sessionHandle, configHandle, _cachedConfig);

            if (OnSetConfiguration != null)
            {
                OnSetConfiguration(sessionHandle, configHandle);
            }
        }

        private void UpdateConfiguration(ARCoreSessionConfig config)
        {
            // There is no configuration to set.
            if (config == null)
            {
                return;
            }

            // The configuration has not been updated.
            if (_cachedConfig != null && config.Equals(_cachedConfig) &&
                (config.AugmentedImageDatabase == null ||
                    !config.AugmentedImageDatabase._isDirty) &&
                !ExperimentManager.Instance.IsConfigurationDirty)
            {
                return;
            }

            _cachedConfig = ScriptableObject.CreateInstance<ARCoreSessionConfig>();
            _cachedConfig.CopyFrom(config);

            if (_requiredPermissionNames.Count > 0)
            {
                RequestPermissions();
                return;
            }

            ExternApi.ArPresto_setConfigurationDirty();
        }

        private void UpdateDisplayGeometry()
        {
            if (!_cachedScreenOrientation.HasValue ||
                Screen.orientation != _cachedScreenOrientation)
            {
                _cachedScreenOrientation = Screen.orientation;
                _cachedDisplayRotation = AndroidNativeHelper.GetDisplayRotation();
            }

            ExternApi.ArPresto_setDisplayGeometry(
                _cachedDisplayRotation, Screen.width, Screen.height);
        }

        private NativeSession GetNativeSession(IntPtr sessionHandle)
        {
            NativeSession nativeSession;
            if (!_nativeSessions.TryGetValue(sessionHandle, out nativeSession))
            {
                nativeSession = new NativeSession(sessionHandle, _cachedFrameHandle);
                _nativeSessions.Add(sessionHandle, nativeSession);
            }

            return nativeSession;
        }

        private void FireOnSessionSetEnabled(bool isEnabled)
        {
            if (OnSessionSetEnabled != null)
            {
                OnSessionSetEnabled(isEnabled);
            }
        }

        private void RequestPermissions()
        {
            // All required permissions are granted.
            if (_requiredPermissionNames.Count == 0)
            {
                return;
            }

            // Waiting for camera permission or there is another pending request.
            if (!ARPrestoCallbackManager.Instance.IsCameraPermissionGranted() ||
                _androidPermissionRequest != null)
            {
                return;
            }

            _androidPermissionRequest = AndroidPermissionsManager
                .RequestPermission(_requiredPermissionNames.First());
            if (_androidPermissionRequest != null)
            {
                _requiredPermissionNames.Remove(_requiredPermissionNames.First());
                _androidPermissionRequest.ThenAction(result =>
                {
                    _androidPermissionRequest = null;
                    ExternApi.ArPresto_setConfigurationDirty();
                });
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
            public static extern void ArPresto_setCameraTextureNames(
                int numberOfTextures, int[] textureIds);

            [AndroidImport(ApiConstants.ARRenderingUtilsApi)]
            public static extern void ARCoreRenderingUtils_CreatePostUpdateFence();

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_setEnabled(bool isEnabled);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_getFrame(ref IntPtr frameHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_getStatus(ref ApiPrestoStatus prestoStatus);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_update();

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_setConfigurationDirty();

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPresto_reset();
#pragma warning restore 626
        }
    }
}
