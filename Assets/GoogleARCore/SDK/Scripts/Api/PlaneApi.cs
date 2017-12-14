//-----------------------------------------------------------------------
// <copyright file="PlaneApi.cs" company="Google">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
    Justification = "Internal")]
    public class PlaneApi
    {
        private const int k_MaxPolygonSize = 1024;
        private NativeApi m_NativeApi;
        private float[] m_TmpPoints;
        private GCHandle m_TmpPointsHandle;

        public PlaneApi(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
            m_TmpPoints = new float[k_MaxPolygonSize * 2];
            m_TmpPointsHandle = GCHandle.Alloc(m_TmpPoints, GCHandleType.Pinned);
        }

        ~PlaneApi()
        {
            m_TmpPointsHandle.Free();
        }

        public Pose GetCenterPose(IntPtr planeHandle)
        {
            var poseHandle = m_NativeApi.Pose.Create();
            ExternApi.ArPlane_getCenterPose(m_NativeApi.SessionHandle, planeHandle, poseHandle);
            Pose resultPose = m_NativeApi.Pose.ExtractPoseValue(poseHandle);
            m_NativeApi.Pose.Destroy(poseHandle);
            return resultPose;
        }

        public float GetExtentX(IntPtr planeHandle)
        {
            float extentX = 0.0f;
            ExternApi.ArPlane_getExtentX(m_NativeApi.SessionHandle, planeHandle, ref extentX);
            return extentX;
        }

        public float GetExtentZ(IntPtr planeHandle)
        {
            float extentZ = 0.0f;
            ExternApi.ArPlane_getExtentZ(m_NativeApi.SessionHandle, planeHandle, ref extentZ);
            return extentZ;
        }

        public void GetPolygon(IntPtr planeHandle, List<Vector3> points)
        {
            points.Clear();
            int pointCount = 0;
            ExternApi.ArPlane_getPolygonSize(m_NativeApi.SessionHandle, planeHandle, ref pointCount);
            if (pointCount < 1)
            {
                return;
            }
            else if (pointCount > k_MaxPolygonSize)
            {
                Debug.LogError("GetPolygon::Plane polygon size exceeds buffer capacity.");
                pointCount = k_MaxPolygonSize;
            }

            ExternApi.ArPlane_getPolygon(m_NativeApi.SessionHandle, planeHandle, m_TmpPointsHandle.AddrOfPinnedObject());

            var planeCenter = GetCenterPose(planeHandle);
            var unityWorldTPlane = Matrix4x4.TRS(planeCenter.position, planeCenter.rotation, Vector3.one);
            for (int i = pointCount - 2; i >= 0; i -= 2)
            {
                var point = unityWorldTPlane.MultiplyPoint3x4(new Vector3(m_TmpPoints[i], 0, -m_TmpPoints[i + 1]));
                points.Add(point);
            }
        }

        public TrackedPlane GetSubsumedBy(IntPtr planeHandle)
        {
            IntPtr subsumerHandle = IntPtr.Zero;
            ExternApi.ArPlane_acquireSubsumedBy(m_NativeApi.SessionHandle, planeHandle, ref subsumerHandle);
            return m_NativeApi.TrackableFactory(subsumerHandle) as TrackedPlane;
        }

        public bool IsPoseInExtents(IntPtr planeHandle, Pose pose)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent a boolean.
            int isPoseInExtents = 0;
            var poseHandle = m_NativeApi.Pose.Create(pose);
            ExternApi.ArPlane_isPoseInExtents(m_NativeApi.SessionHandle, planeHandle, poseHandle, ref isPoseInExtents);
            m_NativeApi.Pose.Destroy(poseHandle);
            return isPoseInExtents != 0;
        }

        public bool IsPoseInExtents(IntPtr planeHandle, IntPtr poseHandle)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent a boolean.
            int isPoseInExtents = 0;
            ExternApi.ArPlane_isPoseInExtents(m_NativeApi.SessionHandle, planeHandle, poseHandle, ref isPoseInExtents);
            return isPoseInExtents != 0;
        }

        public bool IsPoseInPolygon(IntPtr planeHandle, Pose pose)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent a boolean.
            int isPoseInPolygon = 0;
            var poseHandle = m_NativeApi.Pose.Create(pose);
            ExternApi.ArPlane_isPoseInPolygon(m_NativeApi.SessionHandle, planeHandle, poseHandle, ref isPoseInPolygon);
            m_NativeApi.Pose.Destroy(poseHandle);
            return isPoseInPolygon != 0;
        }

        public bool IsPoseInPolygon(IntPtr planeHandle, IntPtr poseHandle)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent a boolean.
            int isPoseInPolygon = 0;
            ExternApi.ArPlane_isPoseInPolygon(m_NativeApi.SessionHandle, planeHandle, poseHandle, ref isPoseInPolygon);
            return isPoseInPolygon != 0;
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getCenterPose(IntPtr sessionHandle, IntPtr planeHandle,
                IntPtr poseHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_acquireSubsumedBy(IntPtr sessionHandle, IntPtr planeHandle,
                ref IntPtr subsumerHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getExtentX(IntPtr sessionHandle, IntPtr planeHandle,
                ref float extentX);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getExtentZ(IntPtr sessionHandle, IntPtr planeHandle,
                ref float extentZ);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getPolygonSize(IntPtr sessionHandle, IntPtr planeHandle,
                ref int polygonSize);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getPolygon(IntPtr sessionHandle, IntPtr planeHandle,
                IntPtr polygonXZ);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_isPoseInExtents(IntPtr sessionHandle, IntPtr planeHandle,
                IntPtr poseHandle, ref int isPoseInExtents);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_isPoseInPolygon(IntPtr sessionHandle, IntPtr planeHandle,
                IntPtr poseHandle, ref int isPoseInPolygon);
        }
    }
}
