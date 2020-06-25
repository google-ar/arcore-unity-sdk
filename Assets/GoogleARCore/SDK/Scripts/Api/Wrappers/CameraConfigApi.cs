//-----------------------------------------------------------------------
// <copyright file="CameraConfigApi.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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

    internal class CameraConfigApi
    {
        private NativeSession m_NativeSession;

        public CameraConfigApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public IntPtr Create()
        {
            IntPtr cameraConfigHandle = IntPtr.Zero;

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("create ARCamera config");
                return cameraConfigHandle;
            }

            ExternApi.ArCameraConfig_create(m_NativeSession.SessionHandle, ref cameraConfigHandle);
            return cameraConfigHandle;
        }

        public void Destroy(IntPtr cameraConfigHandle)
        {
            ExternApi.ArCameraConfig_destroy(cameraConfigHandle);
        }

        public void GetImageDimensions(IntPtr cameraConfigHandle, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("access ARCamera image dimensions");
                return;
            }

            ExternApi.ArCameraConfig_getImageDimensions(
                m_NativeSession.SessionHandle, cameraConfigHandle, ref width, ref height);
        }

        public void GetTextureDimensions(IntPtr cameraConfigHandle, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage(
                    "access ARCamera texture dimensions");
                return;
            }

            ExternApi.ArCameraConfig_getTextureDimensions(
                m_NativeSession.SessionHandle, cameraConfigHandle, ref width, ref height);
        }

        public ApiCameraConfigFacingDirection GetFacingDirection(IntPtr cameraConfigHandle)
        {
            ApiCameraConfigFacingDirection direction = ApiCameraConfigFacingDirection.Back;

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("access ARCamera facing direction");
                return direction;
            }

            ExternApi.ArCameraConfig_getFacingDirection(
                m_NativeSession.SessionHandle, cameraConfigHandle, ref direction);

            return direction;
        }

        public void GetFpsRange(IntPtr cameraConfigHandle, out int minFps, out int maxFps)
        {
            minFps = 0;
            maxFps = 0;

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("access ARCamera FpsRange");
                return;
            }

            ExternApi.ArCameraConfig_getFpsRange(
                m_NativeSession.SessionHandle, cameraConfigHandle, ref minFps, ref maxFps);
        }

        public CameraConfigDepthSensorUsages GetDepthSensorUsage(IntPtr cameraConfigHandle)
        {
            int depthSensorUsage = (int)CameraConfigDepthSensorUsages.DoNotUse;

            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage("access ARCamera DepthSensorUsage");
                return (CameraConfigDepthSensorUsages)depthSensorUsage;
            }

            ExternApi.ArCameraConfig_getDepthSensorUsage(
                m_NativeSession.SessionHandle, cameraConfigHandle, ref depthSensorUsage);
            return (CameraConfigDepthSensorUsages)depthSensorUsage;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_create(IntPtr sessionHandle,
                ref IntPtr cameraConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_destroy(IntPtr cameraConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getImageDimensions(IntPtr sessionHandle,
                IntPtr cameraConfigHandle, ref int width, ref int height);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getTextureDimensions(IntPtr sessionHandle,
                IntPtr cameraConfigHandle, ref int width, ref int height);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getFacingDirection(
                IntPtr sessionHandle, IntPtr cameraConfigHandle,
                ref ApiCameraConfigFacingDirection facingDirection);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getFpsRange(
                IntPtr sessionHandle, IntPtr cameraConfigHandle, ref int minFps, ref int maxFps);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfig_getDepthSensorUsage(
                IntPtr sessionHandle, IntPtr cameraConfigHandle, ref int depthSensorUsage);
#pragma warning restore 626
        }
    }
}
