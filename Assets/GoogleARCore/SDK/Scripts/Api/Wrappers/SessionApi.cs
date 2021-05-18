//-----------------------------------------------------------------------
// <copyright file="SessionApi.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using GoogleARCore.CrossPlatform;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class SessionApi
    {
        private NativeSession _nativeSession;

        public SessionApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public void ReportEngineType()
        {
            ExternApi.ArSession_reportEngineType(
                _nativeSession.SessionHandle, "Unity", Application.unityVersion);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules",
                         "SA1118:ParameterMustNotSpanMultipleLines",
                         Justification = "Bypass source check.")]
        public void GetSupportedCameraConfigurationsWithFilter(
            ARCoreCameraConfigFilter cameraConfigFilter,
            IntPtr cameraConfigListHandle, List<IntPtr> supportedCameraConfigHandles,
            List<CameraConfig> supportedCameraConfigs, DeviceCameraDirection cameraFacingDirection)
        {
            IntPtr cameraConfigFilterHandle = _nativeSession.CameraConfigFilterApi.Create(
                cameraFacingDirection,
                cameraConfigFilter);
            ExternApi.ArSession_getSupportedCameraConfigsWithFilter(
                _nativeSession.SessionHandle,
                cameraConfigFilterHandle,
                cameraConfigListHandle);
            _nativeSession.CameraConfigFilterApi.Destroy(cameraConfigFilterHandle);

            supportedCameraConfigHandles.Clear();
            supportedCameraConfigs.Clear();
            int listSize = _nativeSession.CameraConfigListApi.GetSize(cameraConfigListHandle);

            for (int i = 0; i < listSize; i++)
            {
                IntPtr cameraConfigHandle = _nativeSession.CameraConfigApi.Create();
                _nativeSession.CameraConfigListApi.GetItemAt(
                    cameraConfigListHandle, i, cameraConfigHandle);
                supportedCameraConfigHandles.Add(cameraConfigHandle);
                supportedCameraConfigs.Add(CreateCameraConfig(cameraConfigHandle));
            }
        }

        public ApiArStatus SetCameraConfig(IntPtr cameraConfigHandle)
        {
            return ExternApi.ArSession_setCameraConfig(
                _nativeSession.SessionHandle, cameraConfigHandle);
        }

        public CameraConfig GetCameraConfig()
        {
            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("access camera config");
                return new CameraConfig();
            }

            IntPtr cameraConfigHandle = _nativeSession.CameraConfigApi.Create();
            ExternApi.ArSession_getCameraConfig(_nativeSession.SessionHandle, cameraConfigHandle);
            CameraConfig currentCameraConfig = CreateCameraConfig(cameraConfigHandle);
            _nativeSession.CameraConfigApi.Destroy(cameraConfigHandle);
            return currentCameraConfig;
        }

        public void GetAllTrackables(List<Trackable> trackables)
        {
            IntPtr listHandle = _nativeSession.TrackableListApi.Create();
            ExternApi.ArSession_getAllTrackables(
                _nativeSession.SessionHandle, ApiTrackableType.BaseTrackable, listHandle);

            trackables.Clear();
            int count = _nativeSession.TrackableListApi.GetCount(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackableHandle =
                    _nativeSession.TrackableListApi.AcquireItem(listHandle, i);

                Trackable trackable = _nativeSession.TrackableFactory(trackableHandle);
                if (trackable != null)
                {
                    trackables.Add(trackable);
                }
                else
                {
                    _nativeSession.TrackableApi.Release(trackableHandle);
                }
            }

            _nativeSession.TrackableListApi.Destroy(listHandle);
        }

        public void SetDisplayGeometry(ScreenOrientation orientation, int width, int height)
        {
            const int androidRotation0 = 0;
            const int androidRotation90 = 1;
            const int androidRotation180 = 2;
            const int androidRotation270 = 3;

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

            ExternApi.ArSession_setDisplayGeometry(
                _nativeSession.SessionHandle, androidOrientation, width, height);
        }

        public Anchor CreateAnchor(Pose pose)
        {
            IntPtr poseHandle = _nativeSession.PoseApi.Create(pose);
            IntPtr anchorHandle = IntPtr.Zero;
            ExternApi.ArSession_acquireNewAnchor(
                _nativeSession.SessionHandle, poseHandle, ref anchorHandle);
            var anchorResult = Anchor.Factory(_nativeSession, anchorHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return anchorResult;
        }

        public ApiArStatus CreateCloudAnchor(
            IntPtr platformAnchorHandle, out IntPtr cloudAnchorHandle)
        {
            cloudAnchorHandle = IntPtr.Zero;
            var result =
                ExternApi.ArSession_hostAndAcquireNewCloudAnchor(
                    _nativeSession.SessionHandle, platformAnchorHandle, ref cloudAnchorHandle);
            return result;
        }

        public ApiArStatus ResolveCloudAnchor(String cloudAnchorId, out IntPtr cloudAnchorHandle)
        {
            cloudAnchorHandle = IntPtr.Zero;
            return ExternApi.ArSession_resolveAndAcquireNewCloudAnchor(
                _nativeSession.SessionHandle, cloudAnchorId, ref cloudAnchorHandle);
        }

        public bool IsDepthModeSupported(ApiDepthMode depthMode)
        {
            int isSupported = 0;
            ExternApi.ArSession_isDepthModeSupported(
                _nativeSession.SessionHandle, depthMode, ref isSupported);
            return isSupported != 0;
        }

        public ApiArStatus HostCloudAnchor(IntPtr platformAnchorHandle, int ttlDays,
            out IntPtr cloudAnchorHandle)
        {
            cloudAnchorHandle = IntPtr.Zero;
            var result = ExternApi.ArSession_hostAndAcquireNewCloudAnchorWithTtl(
                _nativeSession.SessionHandle, platformAnchorHandle, ttlDays,
                ref cloudAnchorHandle);
            return result;
        }

        public void SetAuthToken(String authToken)
        {
            ExternApi.ArSession_setAuthToken(_nativeSession.SessionHandle, authToken);
        }

        public FeatureMapQuality EstimateFeatureMapQualityForHosting(Pose pose)
        {
            IntPtr poseHandle = _nativeSession.PoseApi.Create(pose);
            int featureMapQuality = (int)FeatureMapQuality.Insufficient;
            var status = ExternApi.ArSession_estimateFeatureMapQualityForHosting(
                _nativeSession.SessionHandle, poseHandle, ref featureMapQuality);
            _nativeSession.PoseApi.Destroy(poseHandle);

            if (status != ApiArStatus.Success)
            {
                Debug.LogWarningFormat("Failed to estimate feature map quality with status {0}.",
                    status);
            }

            return (FeatureMapQuality)featureMapQuality;
        }

        public PlaybackStatus GetPlaybackStatus()
        {
            ApiPlaybackStatus status = ApiPlaybackStatus.None;
            ExternApi.ArSession_getPlaybackStatus(
                _nativeSession.SessionHandle, ref status);
            return status.ToPlaybackStatus();
        }

        public PlaybackResult SetPlaybackDataset(String datasetFilepath)
        {
            ApiArStatus status = ExternApi.ArSession_setPlaybackDataset(
                _nativeSession.SessionHandle,
                datasetFilepath);

            // Only specific ArStatus responses are expected.
            switch (status)
            {
                case ApiArStatus.Success:
                    return PlaybackResult.OK;
                case ApiArStatus.ErrorSessionNotPaused:
                    return PlaybackResult.ErrorSessionNotPaused;
                case ApiArStatus.ErrorSessionUnsupported:
                    return PlaybackResult.ErrorSessionUnsupported;
                case ApiArStatus.ErrorPlaybackFailed:
                    return PlaybackResult.ErrorPlaybackFailed;
                default:
                    Debug.LogErrorFormat("Attempt to set a playback dataset path failed with "
                                         + "unexpected status: {0}", status);
                    break;
            }

            return PlaybackResult.ErrorPlaybackFailed;
        }

        public RecordingStatus GetRecordingStatus()
        {
            ApiRecordingStatus status = ApiRecordingStatus.None;
            ExternApi.ArSession_getRecordingStatus(
                _nativeSession.SessionHandle, ref status);
            return status.ToRecordingStatus();
        }

        public RecordingResult StartRecording(ARCoreRecordingConfig config)
        {
            IntPtr recordingConfigHandle =
                _nativeSession.RecordingConfigApi.Create(config);
            ApiArStatus status = ExternApi.ArSession_startRecording(
                _nativeSession.SessionHandle, recordingConfigHandle);

            _nativeSession.RecordingConfigApi.Destory(recordingConfigHandle);

            // Only specific ArStatus responses are expected.
            switch (status)
            {
                case ApiArStatus.Success:
                    return RecordingResult.OK;
                case ApiArStatus.ErrorIllegalState:
                    return RecordingResult.ErrorIllegalState;
                case ApiArStatus.ErrorInvalidArgument:
                    return RecordingResult.ErrorInvalidArgument;
                case ApiArStatus.ErrorRecordingFailed:
                    return RecordingResult.ErrorRecordingFailed;
                default:
                    Debug.LogErrorFormat("Attempt to start a recording failed with unexpected " +
                        "status: {0}", status);
                    break;
            }

            return RecordingResult.ErrorRecordingFailed;
        }

        public RecordingResult StopRecording()
        {
            ApiArStatus status = ExternApi.ArSession_stopRecording(_nativeSession.SessionHandle);

            // Only specific ArStatus responses are expected.
            switch (status)
            {
                case ApiArStatus.Success:
                    return RecordingResult.OK;
                case ApiArStatus.ErrorRecordingFailed:
                    return RecordingResult.ErrorRecordingFailed;
                default:
                    Debug.LogErrorFormat("Attempt to stop recording failed with unexpected " +
                        "status: {0}", status);
                    break;
            }

            return RecordingResult.ErrorRecordingFailed;
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules",
                         "SA1118:ParameterMustNotSpanMultipleLines",
                         Justification = "Bypass source check.")]
        private CameraConfig CreateCameraConfig(IntPtr cameraConfigHandle)
        {
            DeviceCameraDirection facingDirection =
                _nativeSession.CameraConfigApi.GetFacingDirection(cameraConfigHandle);
            int imageWidth = 0;
            int imageHeight = 0;
            int textureWidth = 0;
            int textureHeight = 0;
            int minFps = 0;
            int maxFps = 0;
            CameraConfigDepthSensorUsage depthSensorUsage =
                _nativeSession.CameraConfigApi.GetDepthSensorUsage(cameraConfigHandle);
            CameraConfigStereoCameraUsage stereoCameraUsage =
                _nativeSession.CameraConfigApi.GetStereoCameraUsage(cameraConfigHandle);
            _nativeSession.CameraConfigApi.GetImageDimensions(
                cameraConfigHandle, out imageWidth, out imageHeight);
            _nativeSession.CameraConfigApi.GetTextureDimensions(
                cameraConfigHandle, out textureWidth, out textureHeight);
            _nativeSession.CameraConfigApi.GetFpsRange(
                cameraConfigHandle, out minFps, out maxFps);
            return new CameraConfig(
                facingDirection,
                new Vector2(imageWidth, imageHeight),
                new Vector2(textureWidth, textureHeight),
                minFps,
                maxFps,
                stereoCameraUsage,
                depthSensorUsage);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArSession_configure(IntPtr sessionHandle, IntPtr config);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getSupportedCameraConfigsWithFilter(
                IntPtr sessionHandle, IntPtr cameraConfigFilterHandle,
                IntPtr cameraConfigListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_setCameraConfig(
                IntPtr sessionHandle, IntPtr cameraConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getCameraConfig(
                IntPtr sessionHandle, IntPtr cameraConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getAllTrackables(
                IntPtr sessionHandle, ApiTrackableType filterType, IntPtr trackableList);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setDisplayGeometry(
                IntPtr sessionHandle, int rotation, int width, int height);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArSession_acquireNewAnchor(
                IntPtr sessionHandle, IntPtr poseHandle, ref IntPtr anchorHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_isDepthModeSupported(
                IntPtr sessionHandle, ApiDepthMode depthMode, ref int isSupported);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_reportEngineType(
                IntPtr sessionHandle, string engineType, string engineVersion);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_resolveAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                String cloudAnchorId,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchorWithTtl(
                IntPtr sessionHandle, IntPtr anchorHandle, int ttlDays,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setAuthToken(
                IntPtr sessionHandle, String authToken);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_estimateFeatureMapQualityForHosting(
                IntPtr sessionHandle, IntPtr poseHandle, ref int featureMapQuality);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getRecordingStatus(
                IntPtr sessionHandle, ref ApiRecordingStatus recordingStatus);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_startRecording(
                IntPtr sessionHandle, IntPtr recordingConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_stopRecording(
                IntPtr sessionHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getPlaybackStatus(
                IntPtr sessionHandle, ref ApiPlaybackStatus playbackStatus);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_setPlaybackDataset(
              IntPtr sessionHandle, String mp4DatasetFilePath);
#pragma warning restore 626
        }
    }
}
