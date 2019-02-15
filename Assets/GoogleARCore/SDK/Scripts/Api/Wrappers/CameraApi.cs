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

#if UNITY_IOS && !UNITY_EDITOR
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

        public LostTrackingReason GetLostTrackingReason(IntPtr cameraHandle)
        {
            ApiTrackingFailureReason apiTrackingFailureReason = ApiTrackingFailureReason.None;
            ExternApi.ArCamera_getTrackingFailureReason(m_NativeSession.SessionHandle,
                cameraHandle, ref apiTrackingFailureReason);
            return apiTrackingFailureReason.ToLostTrackingReason();
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

        public CameraIntrinsics GetTextureIntrinsics(IntPtr cameraHandle)
        {
            IntPtr cameraIntrinsicsHandle = IntPtr.Zero;
            ExternApi.ArCameraIntrinsics_create(m_NativeSession.SessionHandle, ref cameraIntrinsicsHandle);

            ExternApi.ArCamera_getTextureIntrinsics(
                m_NativeSession.SessionHandle, cameraHandle, cameraIntrinsicsHandle);

            CameraIntrinsics textureIntrinsics = _GetCameraIntrinsicsFromHandle(cameraIntrinsicsHandle);
            ExternApi.ArCameraIntrinsics_destroy(cameraIntrinsicsHandle);

            return textureIntrinsics;
        }

        public CameraIntrinsics GetImageIntrinsics(IntPtr cameraHandle)
        {
            IntPtr cameraIntrinsicsHandle = IntPtr.Zero;
            ExternApi.ArCameraIntrinsics_create(m_NativeSession.SessionHandle, ref cameraIntrinsicsHandle);

            ExternApi.ArCamera_getImageIntrinsics(m_NativeSession.SessionHandle, cameraHandle,
                                                  cameraIntrinsicsHandle);

            CameraIntrinsics textureIntrinsics = _GetCameraIntrinsicsFromHandle(cameraIntrinsicsHandle);
            ExternApi.ArCameraIntrinsics_destroy(cameraIntrinsicsHandle);

            return textureIntrinsics;
        }

        public void Release(IntPtr cameraHandle)
        {
            ExternApi.ArCamera_release(cameraHandle);
        }

        private CameraIntrinsics _GetCameraIntrinsicsFromHandle(IntPtr intrinsicsHandle)
        {
            float fx, fy, px, py;
            fx = fy = px = py = 0;
            int width, height;
            width = height = 0;

            ExternApi.ArCameraIntrinsics_getFocalLength(m_NativeSession.SessionHandle, intrinsicsHandle,
                ref fx, ref fy);
            ExternApi.ArCameraIntrinsics_getPrincipalPoint(m_NativeSession.SessionHandle, intrinsicsHandle,
                ref px, ref py);
            ExternApi.ArCameraIntrinsics_getImageDimensions(m_NativeSession.SessionHandle, intrinsicsHandle,
                ref width, ref height);

            return new CameraIntrinsics(new Vector2(fx, fy), new Vector2(px, py), new Vector2Int(width, height));
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getTrackingState(IntPtr sessionHandle, IntPtr cameraHandle,
                ref ApiTrackingState outTrackingState);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getTrackingFailureReason(IntPtr sessionHandle, IntPtr cameraHandle,
                ref ApiTrackingFailureReason outTrackingState);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getDisplayOrientedPose(
                IntPtr sessionHandle, IntPtr cameraHandle, IntPtr outPose);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getProjectionMatrix(IntPtr sessionHandle, IntPtr cameraHandle,
                float near, float far, ref Matrix4x4 outMatrix);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getTextureIntrinsics(IntPtr sessionHandle, IntPtr cameraHandle,
                IntPtr outCameraIntrinsics);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_getImageIntrinsics(IntPtr sessionHandle, IntPtr cameraHandle,
                IntPtr outCameraIntrinsics);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCamera_release(IntPtr cameraHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraIntrinsics_create(IntPtr sessionHandle, ref IntPtr outCameraIntrinsics);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraIntrinsics_getFocalLength(IntPtr sessionHandle, IntPtr intrinsicsHandle,
                ref float outFx, ref float outFy);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraIntrinsics_getPrincipalPoint(
                IntPtr sessionHandle, IntPtr intrinsicsHandle, ref float outCx, ref float outCy);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraIntrinsics_getImageDimensions(
                IntPtr sessionHandle, IntPtr intrinsicsHandle, ref int outWidth, ref int outWeight);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraIntrinsics_destroy(IntPtr intrinsicsHandle);
#pragma warning restore 626
        }
    }
}
