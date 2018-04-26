//-----------------------------------------------------------------------
// <copyright file="CameraApi.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
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

#if UNITY_IOS
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class CameraApi
    {
        private NativeSession m_NativeSession;

        public CameraApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public TrackingState GetTrackingState(IntPtr cameraHandle)
        {
            ApiTrackingState apiTrackingState = ApiTrackingState.Stopped;
            ExternApi.ArCamera_getTrackingState(m_NativeSession.SessionHandle,
                cameraHandle, ref apiTrackingState);
            return apiTrackingState.ToTrackingState();
        }

        public Pose GetPose(IntPtr cameraHandle)
        {
            if (cameraHandle == IntPtr.Zero)
            {
                return Pose.identity;
            }

            IntPtr poseHandle = m_NativeSession.PoseApi.Create();
            ExternApi.ArCamera_getDisplayOrientedPose(m_NativeSession.SessionHandle, cameraHandle,
                poseHandle);
            Pose resultPose = m_NativeSession.PoseApi.ExtractPoseValue(poseHandle);
            m_NativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public Matrix4x4 GetProjectionMatrix(IntPtr cameraHandle, float near, float far)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            ExternApi.ArCamera_getProjectionMatrix(m_NativeSession.SessionHandle, cameraHandle,
                near, far, ref matrix);
            return matrix;
        }

        public void Release(IntPtr cameraHandle)
        {
            ExternApi.ArCamera_release(cameraHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getTrackingState(IntPtr sessionHandle, IntPtr cameraHandle,
                ref ApiTrackingState outTrackingState);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getDisplayOrientedPose(IntPtr sessionHandle, IntPtr cameraHandle, IntPtr outPose);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getProjectionMatrix(IntPtr sessionHandle, IntPtr cameraHandle,
                float near, float far, ref Matrix4x4 outMatrix);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_release(IntPtr cameraHandle);
#pragma warning restore 626
        }
    }
}
