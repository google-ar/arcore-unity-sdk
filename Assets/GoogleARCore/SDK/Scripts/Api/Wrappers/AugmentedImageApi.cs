//-----------------------------------------------------------------------
// <copyright file="AugmentedImageApi.cs" company="Google LLC">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using GoogleARCoreInternal;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class AugmentedImageApi
    {
        private NativeSession _nativeSession;

        public AugmentedImageApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public int GetDatabaseIndex(IntPtr augmentedImageHandle)
        {
            int outIndex = -1;
            ExternApi.ArAugmentedImage_getIndex(_nativeSession.SessionHandle, augmentedImageHandle,
                ref outIndex);
            return outIndex;
        }

        public Pose GetCenterPose(IntPtr augmentedImageHandle)
        {
            IntPtr poseHandle = _nativeSession.PoseApi.Create();
            ExternApi.ArAugmentedImage_getCenterPose(
                _nativeSession.SessionHandle, augmentedImageHandle, poseHandle);
            Pose result = _nativeSession.PoseApi.ExtractPoseValue(poseHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return result;
        }

        public float GetExtentX(IntPtr augmentedImageHandle)
        {
            float outExtentX = 0f;
            ExternApi.ArAugmentedImage_getExtentX(
                _nativeSession.SessionHandle, augmentedImageHandle, ref outExtentX);
            return outExtentX;
        }

        public float GetExtentZ(IntPtr augmentedImageHandle)
        {
            float outExtentZ = 0f;
            ExternApi.ArAugmentedImage_getExtentZ(
                _nativeSession.SessionHandle, augmentedImageHandle, ref outExtentZ);
            return outExtentZ;
        }

        public string GetName(IntPtr augmentedImageHandle)
        {
            IntPtr outName = IntPtr.Zero;
            ExternApi.ArAugmentedImage_acquireName(
                _nativeSession.SessionHandle, augmentedImageHandle, ref outName);
            string name = Marshal.PtrToStringAnsi(outName);
            ExternApi.ArString_release(outName);
            return name;
        }

        public AugmentedImageTrackingMethod GetTrackingMethod(IntPtr augmentedImageHandle)
        {
            AugmentedImageTrackingMethod trackingMethod = AugmentedImageTrackingMethod.NotTracking;
            ExternApi.ArAugmentedImage_getTrackingMethod(
                _nativeSession.SessionHandle, augmentedImageHandle, ref trackingMethod);
            return trackingMethod;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImage_getIndex(IntPtr sessionHandle,
                IntPtr augmentedImageHandle, ref int outIndex);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImage_getCenterPose(IntPtr sessionHandle,
                IntPtr augmentedImageHandle, IntPtr outPoseHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImage_getExtentX(IntPtr sessionHandle,
                IntPtr augmentedImageHandle, ref float outExtentX);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImage_getExtentZ(IntPtr sessionHandle,
                IntPtr augmentedImageHandle, ref float outExtentZ);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImage_acquireName(IntPtr sessionHandle,
                IntPtr augmentedImageHandle, ref IntPtr outName);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImage_getTrackingMethod(IntPtr sessionHandle,
                IntPtr augmentedImageHandle, ref AugmentedImageTrackingMethod trackingMethod);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArString_release(IntPtr str);
#pragma warning restore 626
        }
    }
}
