//-----------------------------------------------------------------------
// <copyright file="SessionConfigApi.cs" company="Google LLC">
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
    using GoogleARCore;
    using GoogleARCoreInternal.CrossPlatform;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class SessionConfigApi
    {
        private NativeSession _nativeSession;

        public SessionConfigApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public static void UpdateApiConfigWithARCoreSessionConfig(IntPtr sessionHandle,
            IntPtr configHandle, ARCoreSessionConfig sessionConfig)
        {
            ApiLightEstimationMode lightingMode =
                sessionConfig.LightEstimationMode.ToApiLightEstimationMode();
            ExternApi.ArConfig_setLightEstimationMode(sessionHandle, configHandle, lightingMode);

            ApiPlaneFindingMode planeFindingMode =
                sessionConfig.PlaneFindingMode.ToApiPlaneFindingMode();
            ExternApi.ArConfig_setPlaneFindingMode(sessionHandle, configHandle, planeFindingMode);

            ApiUpdateMode updateMode = sessionConfig.MatchCameraFramerate ?
                ApiUpdateMode.Blocking : ApiUpdateMode.LatestCameraImage;
            ExternApi.ArConfig_setUpdateMode(sessionHandle, configHandle, updateMode);

            ApiCloudAnchorMode cloudAnchorMode =
                sessionConfig.CloudAnchorMode.ToApiCloudAnchorMode();
            ExternApi.ArConfig_setCloudAnchorMode(sessionHandle, configHandle, cloudAnchorMode);

            IntPtr augmentedImageDatabaseHandle = IntPtr.Zero;
            if (sessionConfig.AugmentedImageDatabase != null)
            {
                augmentedImageDatabaseHandle = sessionConfig.AugmentedImageDatabase._nativeHandle;
                ExternApi.ArConfig_setAugmentedImageDatabase(sessionHandle, configHandle,
                    augmentedImageDatabaseHandle);
            }
            else
            {
                ExternApi.ArConfig_setAugmentedImageDatabase(sessionHandle, configHandle,
                    IntPtr.Zero);
            }

            ApiAugmentedFaceMode augmentedFaceMode =
                sessionConfig.AugmentedFaceMode.ToApiAugmentedFaceMode();
            ExternApi.ArConfig_setAugmentedFaceMode(sessionHandle, configHandle, augmentedFaceMode);

            ApiCameraFocusMode focusMode = sessionConfig.CameraFocusMode.ToApiCameraFocusMode();
            ExternApi.ArConfig_setFocusMode(sessionHandle, configHandle, focusMode);

            if (!InstantPreviewManager.IsProvidingPlatform)
            {
                ApiDepthMode depthMode = sessionConfig.DepthMode.ToApiDepthMode();
                ExternApi.ArConfig_setDepthMode(sessionHandle, configHandle, depthMode);
            }

            if (!InstantPreviewManager.IsProvidingPlatform)
            {
                ExternApi.ArConfig_setInstantPlacementMode(sessionHandle, configHandle,
                    sessionConfig.InstantPlacementMode);
            }
        }

        public IntPtr Create()
        {
            IntPtr configHandle = IntPtr.Zero;
            ExternApi.ArConfig_create(_nativeSession.SessionHandle, ref configHandle);
            return configHandle;
        }

        public void Destroy(IntPtr configHandle)
        {
            ExternApi.ArConfig_destroy(configHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_create(IntPtr session, ref IntPtr out_config);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_destroy(IntPtr config);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setLightEstimationMode(
                IntPtr session, IntPtr config, ApiLightEstimationMode light_estimation_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setPlaneFindingMode(
                IntPtr session, IntPtr config, ApiPlaneFindingMode plane_finding_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setUpdateMode(
                IntPtr session, IntPtr config, ApiUpdateMode update_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setCloudAnchorMode(
                IntPtr session, IntPtr config, ApiCloudAnchorMode cloud_anchor_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setAugmentedImageDatabase(
                IntPtr session, IntPtr config, IntPtr augmented_image_database);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setAugmentedFaceMode(
                IntPtr session, IntPtr config, ApiAugmentedFaceMode augmented_face_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setFocusMode(
                IntPtr session, IntPtr config, ApiCameraFocusMode focus_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setDepthMode(
                IntPtr session, IntPtr config, ApiDepthMode mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setInstantPlacementMode(
                IntPtr session, IntPtr config, InstantPlacementMode instant_placement_mode);
#pragma warning restore 626
        }
    }
}
