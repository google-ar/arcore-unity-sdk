//-----------------------------------------------------------------------
// <copyright file="ApiTypeExtensions.cs" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    using GoogleARCore;
    using GoogleARCore.CrossPlatform;
    using GoogleARCoreInternal.CrossPlatform;
    using UnityEngine;

    internal static class ApiTypeExtensions
    {
        public static ApkAvailabilityStatus ToApkAvailabilityStatus(this ApiAvailability apiStatus)
        {
            switch (apiStatus)
            {
                case ApiAvailability.UnknownError:
                    return ApkAvailabilityStatus.UnknownError;
                case ApiAvailability.UnknownChecking:
                    return ApkAvailabilityStatus.UnknownChecking;
                case ApiAvailability.UnknownTimedOut:
                    return ApkAvailabilityStatus.UnknownTimedOut;
                case ApiAvailability.UnsupportedDeviceNotCapable:
                    return ApkAvailabilityStatus.UnsupportedDeviceNotCapable;
                case ApiAvailability.SupportedNotInstalled:
                    return ApkAvailabilityStatus.SupportedNotInstalled;
                case ApiAvailability.SupportedApkTooOld:
                    return ApkAvailabilityStatus.SupportedApkTooOld;
                case ApiAvailability.SupportedInstalled:
                    return ApkAvailabilityStatus.SupportedInstalled;
                default:
                    Debug.LogErrorFormat("Unexpected ApiAvailability status {0}", apiStatus);
                    return ApkAvailabilityStatus.UnknownError;
            }
        }

        public static ApkInstallationStatus ToApkInstallationStatus(
            this ApiApkInstallationStatus apiStatus)
        {
            switch (apiStatus)
            {
                case ApiApkInstallationStatus.Uninitialized:
                    return ApkInstallationStatus.Uninitialized;
                case ApiApkInstallationStatus.Requested:
                    return ApkInstallationStatus.Requested;
                case ApiApkInstallationStatus.Success:
                    return ApkInstallationStatus.Success;
                case ApiApkInstallationStatus.Error:
                    return ApkInstallationStatus.Error;
                case ApiApkInstallationStatus.ErrorDeviceNotCompatible:
                    return ApkInstallationStatus.ErrorDeviceNotCompatible;
                case ApiApkInstallationStatus.ErrorUserDeclined:
                    return ApkInstallationStatus.ErrorUserDeclined;
                default:
                    Debug.LogErrorFormat("Unexpected ApiApkInstallStatus status {0}", apiStatus);
                    return ApkInstallationStatus.Error;
            }
        }

        public static SessionStatus ToSessionStatus(this ApiPrestoStatus prestoStatus)
        {
            switch (prestoStatus)
            {
                case ApiPrestoStatus.Uninitialized:
                    return SessionStatus.None;
                case ApiPrestoStatus.RequestingApkInstall:
                case ApiPrestoStatus.RequestingPermission:
                    return SessionStatus.Initializing;
                case ApiPrestoStatus.Resumed:
                    return SessionStatus.Tracking;
                case ApiPrestoStatus.ResumedNotTracking:
                    return SessionStatus.LostTracking;
                case ApiPrestoStatus.Paused:
                    return SessionStatus.NotTracking;
                case ApiPrestoStatus.ErrorFatal:
                    return SessionStatus.FatalError;
                case ApiPrestoStatus.ErrorApkNotAvailable:
                    return SessionStatus.ErrorApkNotAvailable;
                case ApiPrestoStatus.ErrorPermissionNotGranted:
                    return SessionStatus.ErrorPermissionNotGranted;
                case ApiPrestoStatus.ErrorSessionConfigurationNotSupported:
                    return SessionStatus.ErrorSessionConfigurationNotSupported;
                case ApiPrestoStatus.ErrorCameraNotAvailable:
                    return SessionStatus.ErrorCameraNotAvailable;
                case ApiPrestoStatus.ErrorIllegalState:
                    return SessionStatus.ErrorIllegalState;
                default:
                    Debug.LogErrorFormat("Unexpected presto status {0}", prestoStatus);
                    return SessionStatus.FatalError;
            }
        }

        public static TrackingState ToTrackingState(this ApiTrackingState apiTrackingState)
        {
            switch (apiTrackingState)
            {
                case ApiTrackingState.Tracking:
                    return TrackingState.Tracking;
                case ApiTrackingState.Paused:
                    return TrackingState.Paused;
                case ApiTrackingState.Stopped:
                    return TrackingState.Stopped;
                default:
                    return TrackingState.Stopped;
            }
        }

        public static XPTrackingState ToXPTrackingState(this TrackingState apiTrackingState)
        {
            switch (apiTrackingState)
            {
                case TrackingState.Tracking:
                    return XPTrackingState.Tracking;
                case TrackingState.Paused:
                    return XPTrackingState.Paused;
                case TrackingState.Stopped:
                    return XPTrackingState.Stopped;
                default:
                    return XPTrackingState.Stopped;
            }
        }

        public static LostTrackingReason ToLostTrackingReason(
            this ApiTrackingFailureReason apiTrackingFailureReason)
        {
            switch (apiTrackingFailureReason)
            {
                case ApiTrackingFailureReason.None:
                    return LostTrackingReason.None;
                case ApiTrackingFailureReason.BadState:
                    return LostTrackingReason.BadState;
                case ApiTrackingFailureReason.InsufficientLight:
                    return LostTrackingReason.InsufficientLight;
                case ApiTrackingFailureReason.ExcessiveMotion:
                    return LostTrackingReason.ExcessiveMotion;
                case ApiTrackingFailureReason.InsufficientFeatures:
                    return LostTrackingReason.InsufficientFeatures;
                case ApiTrackingFailureReason.CameraUnavailable:
                    return LostTrackingReason.CameraUnavailable;
                default:
                    return LostTrackingReason.None;
            }
        }

        public static DeviceCameraDirection ToDeviceCameraDirection(
            this ApiCameraConfigFacingDirection direction)
        {
            switch (direction)
            {
                case ApiCameraConfigFacingDirection.Front:
                    return DeviceCameraDirection.FrontFacing;
                case ApiCameraConfigFacingDirection.Back:
                default:
                    return DeviceCameraDirection.BackFacing;
            }
        }

        public static LightEstimateState ToLightEstimateState(this ApiLightEstimateState apiState)
        {
            switch (apiState)
            {
                case ApiLightEstimateState.NotValid:
                    return LightEstimateState.NotValid;
                case ApiLightEstimateState.Valid:
                    return LightEstimateState.Valid;
                default:
                    return LightEstimateState.NotValid;
            }
        }

        public static ApiLightEstimationMode ToApiLightEstimationMode(this LightEstimationMode mode)
        {
            switch (mode)
            {
                case LightEstimationMode.Disabled:
                    return ApiLightEstimationMode.Disabled;
                case LightEstimationMode.AmbientIntensity:
                    return ApiLightEstimationMode.AmbientIntensity;
                case LightEstimationMode.EnvironmentalHDRWithoutReflections:
                case LightEstimationMode.EnvironmentalHDRWithReflections:
                    return ApiLightEstimationMode.EnvironmentalHDR;
                default:
                    Debug.LogErrorFormat("Unexpected LightEstimationMode {0}", mode);
                    return ApiLightEstimationMode.Disabled;
            }
        }

        public static ApiPlaneFindingMode ToApiPlaneFindingMode(this DetectedPlaneFindingMode mode)
        {
            switch (mode)
            {
                case DetectedPlaneFindingMode.Disabled:
                    return ApiPlaneFindingMode.Disabled;
                case DetectedPlaneFindingMode.Horizontal:
                    return ApiPlaneFindingMode.Horizontal;
                case DetectedPlaneFindingMode.Vertical:
                    return ApiPlaneFindingMode.Vertical;
                case DetectedPlaneFindingMode.HorizontalAndVertical:
                    return ApiPlaneFindingMode.HorizontalAndVertical;
                default:
                    Debug.LogErrorFormat("Unexpected DetectedPlaneFindingMode {0}", mode);
                    return ApiPlaneFindingMode.Disabled;
            }
        }

        public static ApiAugmentedFaceMode ToApiAugmentedFaceMode(this AugmentedFaceMode mode)
        {
            switch (mode)
            {
                case AugmentedFaceMode.Disabled:
                    return ApiAugmentedFaceMode.Disabled;
                case AugmentedFaceMode.Mesh:
                    return ApiAugmentedFaceMode.Mesh3D;
                default:
                    Debug.LogErrorFormat("Unexpected AugmentedFaceMode {0}", mode);
                    return ApiAugmentedFaceMode.Disabled;
            }
        }

        public static ApiCameraFocusMode ToApiCameraFocusMode(this CameraFocusMode mode)
        {
            switch (mode)
            {
                case CameraFocusMode.FixedFocus:
                    return ApiCameraFocusMode.Fixed;
                case CameraFocusMode.AutoFocus:
                    return ApiCameraFocusMode.Auto;
                default:
                    Debug.LogErrorFormat("Unexpected CameraFocusMode {0}", mode);
                    return ApiCameraFocusMode.Fixed;
            }
        }

        public static DepthStatus ToDepthStatus(this ApiArStatus apiStatus)
        {
            switch (apiStatus)
            {
                case ApiArStatus.Success:
                    return DepthStatus.Success;
                case ApiArStatus.ErrorNotYetAvailable:
                    return DepthStatus.NotYetAvailable;
                case ApiArStatus.ErrorNotTracking:
                    return DepthStatus.NotTracking;
                case ApiArStatus.ErrorIllegalState:
                    return DepthStatus.IllegalState;
                case ApiArStatus.ErrorInvalidArgument:
                case ApiArStatus.ErrorResourceExhausted:
                case ApiArStatus.ErrorDeadlineExceeded:
                default:
                    return DepthStatus.InternalError;
            }
        }

        public static ApiDepthMode ToApiDepthMode(this DepthMode depthMode)
        {
            switch (depthMode)
            {
                case DepthMode.Automatic:
                    return ApiDepthMode.Automatic;
                case DepthMode.Disabled:
                default:
                    return ApiDepthMode.Disabled;
            }
        }

        public static ApiCloudAnchorMode ToApiCloudAnchorMode(this CloudAnchorMode mode)
        {
            switch (mode)
            {
                case CloudAnchorMode.Disabled:
                    return ApiCloudAnchorMode.Disabled;
                case CloudAnchorMode.Enabled:
                    return ApiCloudAnchorMode.Enabled;
                default:
                    Debug.LogErrorFormat("Unexpected CloudAnchorMode {0}", mode);
                    return ApiCloudAnchorMode.Disabled;
            }
        }

        public static DetectedPlaneType ToDetectedPlaneType(this ApiPlaneType apiPlaneType)
        {
            switch (apiPlaneType)
            {
                case ApiPlaneType.HorizontalUpwardFacing:
                    return DetectedPlaneType.HorizontalUpwardFacing;
                case ApiPlaneType.HorizontalDownwardFacing:
                    return DetectedPlaneType.HorizontalDownwardFacing;
                case ApiPlaneType.Vertical:
                    return DetectedPlaneType.Vertical;
                default:
                    return DetectedPlaneType.HorizontalUpwardFacing;
            }
        }

        public static DisplayUvCoords ToDisplayUvCoords(this ApiDisplayUvCoords apiCoords)
        {
            return new DisplayUvCoords(apiCoords.TopLeft, apiCoords.TopRight,
                apiCoords.BottomLeft, apiCoords.BottomRight);
        }

        public static FeaturePointOrientationMode ToFeaturePointOrientationMode(
            this ApiFeaturePointOrientationMode apiMode)
        {
            switch (apiMode)
            {
                case ApiFeaturePointOrientationMode.Identity:
                    return FeaturePointOrientationMode.Identity;
                case ApiFeaturePointOrientationMode.SurfaceNormal:
                    return FeaturePointOrientationMode.SurfaceNormal;
                default:
                    ARDebug.LogError("Invalid value for ApiFeaturePointOrientationMode.");
                    return FeaturePointOrientationMode.Identity;
            }
        }

        public static CloudServiceResponse ToCloudServiceResponse(this ApiArStatus status)
        {
            switch (status)
            {
                case ApiArStatus.Success:
                    return CloudServiceResponse.Success;
                case ApiArStatus.ErrorCloudAnchorsNotConfigured:
                    return CloudServiceResponse.ErrorNotSupportedByConfiguration;
                case ApiArStatus.ErrorNotTracking:
                case ApiArStatus.ErrorSessionPaused:
                    return CloudServiceResponse.ErrorNotTracking;
                case ApiArStatus.ErrorResourceExhausted:
                    return CloudServiceResponse.ErrorTooManyCloudAnchors;
                default:
                    return CloudServiceResponse.ErrorInternal;
            }
        }

        public static CloudServiceResponse ToCloudServiceResponse(
            this ApiCloudAnchorState anchorState)
        {
            switch (anchorState)
            {
                case ApiCloudAnchorState.Success:
                    return CloudServiceResponse.Success;
                case ApiCloudAnchorState.ErrorNotAuthorized:
                    return CloudServiceResponse.ErrorNotAuthorized;
                case ApiCloudAnchorState.ErrorResourceExhausted:
                    return CloudServiceResponse.ErrorApiQuotaExceeded;
                case ApiCloudAnchorState.ErrorHostingDatasetProcessingFailed:
                    return CloudServiceResponse.ErrorDatasetInadequate;
                case ApiCloudAnchorState.ErrorResolveingCloudIdNotFound:
                    return CloudServiceResponse.ErrorCloudIdNotFound;
                case ApiCloudAnchorState.ErrorResolvingSDKTooOld:
                    return CloudServiceResponse.ErrorSDKTooOld;
                case ApiCloudAnchorState.ErrorResolvingSDKTooNew:
                    return CloudServiceResponse.ErrorSDKTooNew;
                case ApiCloudAnchorState.ErrorHostingServiceUnavailable:
                    return CloudServiceResponse.ErrorHostingServiceUnavailable;
                case ApiCloudAnchorState.None:
                case ApiCloudAnchorState.TaskInProgress:
                case ApiCloudAnchorState.ErrorInternal:
                default:
                    return CloudServiceResponse.ErrorInternal;
            }
        }
    }
}
