//-----------------------------------------------------------------------
// <copyright file="PointApi.cs" company="Google LLC">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class PointApi
    {
        private NativeSession _nativeSession;

        public PointApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public Pose GetPose(IntPtr pointHandle)
        {
            var poseHandle = _nativeSession.PoseApi.Create();
            ExternApi.ArPoint_getPose(_nativeSession.SessionHandle, pointHandle, poseHandle);
            Pose resultPose = _nativeSession.PoseApi.ExtractPoseValue(poseHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public Pose GetInstantPlacementPointPose(IntPtr instantPlacementPointHandle)
        {
            var poseHandle = _nativeSession.PoseApi.Create();
            ExternApi.ArInstantPlacementPoint_getPose(
                _nativeSession.SessionHandle, instantPlacementPointHandle, poseHandle);
            Pose resultPose = _nativeSession.PoseApi.ExtractPoseValue(poseHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public InstantPlacementPointTrackingMethod GetInstantPlacementPointTrackingMethod(
        IntPtr instantPlacementPointHandle)
        {
            InstantPlacementPointTrackingMethod trackingMethod =
                InstantPlacementPointTrackingMethod.NotTracking;
            ExternApi.ArInstantPlacementPoint_getTrackingMethod(
                _nativeSession.SessionHandle, instantPlacementPointHandle, ref trackingMethod);
            return trackingMethod;
        }

        public FeaturePointOrientationMode GetOrientationMode(IntPtr pointHandle)
        {
            ApiFeaturePointOrientationMode orientationMode =
                ApiFeaturePointOrientationMode.Identity;
            ExternApi.ArPoint_getOrientationMode(_nativeSession.SessionHandle, pointHandle,
                ref orientationMode);
            return orientationMode.ToFeaturePointOrientationMode();
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPoint_getPose(
                IntPtr session, IntPtr point, IntPtr out_pose);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPoint_getOrientationMode(
                IntPtr session, IntPtr point, ref ApiFeaturePointOrientationMode orientationMode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArInstantPlacementPoint_getPose(
                IntPtr session, IntPtr instantPlacementPoint, IntPtr out_pose);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArInstantPlacementPoint_getTrackingMethod(
                IntPtr session, IntPtr instantPlacementPoint,
                ref InstantPlacementPointTrackingMethod trackingMethod);
#pragma warning restore 626
        }
    }
}
