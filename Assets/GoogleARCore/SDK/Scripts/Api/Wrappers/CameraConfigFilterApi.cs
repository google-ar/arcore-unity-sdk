//-----------------------------------------------------------------------
// <copyright file="CameraConfigFilterApi.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class CameraConfigFilterApi
    {
        private NativeSession _nativeSession;

        public CameraConfigFilterApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules",
                         "SA1118:ParameterMustNotSpanMultipleLines",
                         Justification = "Bypass source check.")]
        public IntPtr Create(
            DeviceCameraDirection direction,
            ARCoreCameraConfigFilter filter)
        {
            IntPtr cameraConfigFilterHandle = IntPtr.Zero;
            ExternApi.ArCameraConfigFilter_create(
                _nativeSession.SessionHandle, ref cameraConfigFilterHandle);
            ExternApi.ArCameraConfigFilter_setFacingDirection(
                _nativeSession.SessionHandle, cameraConfigFilterHandle, direction);

            if (filter != null)
            {
                if (filter.TargetCameraFramerate != null)
                {
                    ExternApi.ArCameraConfigFilter_setTargetFps(_nativeSession.SessionHandle,
                        cameraConfigFilterHandle, ConvertToFpsFilter(filter.TargetCameraFramerate));
                }

                if (filter.DepthSensorUsage != null)
                {
                    ExternApi.ArCameraConfigFilter_setDepthSensorUsage(_nativeSession.SessionHandle,
                        cameraConfigFilterHandle, ConvertToDepthFilter(filter.DepthSensorUsage));
                }

                if (filter.StereoCameraUsage != null)
                {
                    ExternApi.ArCameraConfigFilter_setStereoCameraUsage(
                        _nativeSession.SessionHandle, cameraConfigFilterHandle,
                        ConvertToStereoFilter(filter.StereoCameraUsage));
                }
            }

            return cameraConfigFilterHandle;
        }

        public void Destroy(IntPtr cameraConfigListHandle)
        {
            ExternApi.ArCameraConfigFilter_destroy(cameraConfigListHandle);
        }

        private int ConvertToFpsFilter(
            ARCoreCameraConfigFilter.TargetCameraFramerateFilter targetCameraFramerate)
        {
            int fpsFilter = 0;
            if (targetCameraFramerate.Target30FPS)
            {
                fpsFilter |= (int)ApiCameraConfigTargetFps.TargetFps30;
            }

            if (targetCameraFramerate.Target60FPS)
            {
                fpsFilter |= (int)ApiCameraConfigTargetFps.TargetFps60;
            }

            return fpsFilter;
        }

        private int ConvertToDepthFilter(
            ARCoreCameraConfigFilter.DepthSensorUsageFilter depthSensorUsage)
        {
            int depthFilter = 0;
            if (depthSensorUsage.RequireAndUse)
            {
                depthFilter |= (int)CameraConfigDepthSensorUsage.RequireAndUse;
            }

            if (depthSensorUsage.DoNotUse)
            {
                depthFilter |= (int)CameraConfigDepthSensorUsage.DoNotUse;
            }

            return depthFilter;
        }

        private int ConvertToStereoFilter(
            ARCoreCameraConfigFilter.StereoCameraUsageFilter stereoCameraUsage)
        {
            int stereoFilter = 0;
            if (stereoCameraUsage.RequireAndUse)
            {
                stereoFilter |= (int)CameraConfigStereoCameraUsage.RequireAndUse;
            }

            if (stereoCameraUsage.DoNotUse)
            {
                stereoFilter |= (int)CameraConfigStereoCameraUsage.DoNotUse;
            }

            return stereoFilter;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_create(IntPtr sessionHandle,
                ref IntPtr cameraConfigFilterHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_destroy(IntPtr cameraConfigFilterHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setFacingDirection(
                IntPtr session, IntPtr filter, DeviceCameraDirection facing_direction_filter);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setTargetFps(IntPtr sessionHandle,
                IntPtr cameraConfigFilterHandle, int fpsFilter);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setDepthSensorUsage(IntPtr sessionHandle,
                IntPtr cameraConfigFilterHandle, int depthFilter);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setStereoCameraUsage(
                IntPtr sessionHandle, IntPtr cameraConfigFilterHandle, int stereoFilter);
#pragma warning restore 626
        }
    }
}
