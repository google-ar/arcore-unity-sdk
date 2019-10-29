//-----------------------------------------------------------------------
// <copyright file="CameraConfigFilterApi.cs" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
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
        private NativeSession m_NativeSession;

        public CameraConfigFilterApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public IntPtr Create(ARCoreCameraConfigFilter filter)
        {
            IntPtr cameraConfigFilterHandle = IntPtr.Zero;
            ExternApi.ArCameraConfigFilter_create(
                m_NativeSession.SessionHandle, ref cameraConfigFilterHandle);

            if (filter != null)
            {
                ExternApi.ArCameraConfigFilter_setTargetFps(m_NativeSession.SessionHandle,
                    cameraConfigFilterHandle, _ConvertToFpsFilter(filter.TargetCameraFramerate));
                ExternApi.ArCameraConfigFilter_setDepthSensorUsage(m_NativeSession.SessionHandle,
                    cameraConfigFilterHandle, _ConvertToDepthFilter(filter.DepthSensorUsage));
            }

            return cameraConfigFilterHandle;
        }

        public void Destroy(IntPtr cameraConfigListHandle)
        {
            ExternApi.ArCameraConfigFilter_destroy(cameraConfigListHandle);
        }

        private int _ConvertToFpsFilter(
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

        private int _ConvertToDepthFilter(
            ARCoreCameraConfigFilter.DepthSensorUsageFilter depthSensorUsage)
        {
            int depthFilter = 0;
            if (depthSensorUsage.RequireAndUse)
            {
                depthFilter |= (int)CameraConfigDepthSensorUsages.RequireAndUse;
            }

            if (depthSensorUsage.DoNotUse)
            {
                depthFilter |= (int)CameraConfigDepthSensorUsages.DoNotUse;
            }

            return depthFilter;
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
            public static extern void ArCameraConfigFilter_setTargetFps(IntPtr sessionHandle,
                IntPtr cameraConfigFilterHandle, int fpsFilter);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigFilter_setDepthSensorUsage(IntPtr sessionHandle,
                IntPtr cameraConfigFilterHandle, int depthFilter);
#pragma warning restore 626
        }
    }
}
