//-----------------------------------------------------------------------
// <copyright file="PointCloudApi.cs" company="Google">
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
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    using Marshal = System.Runtime.InteropServices.Marshal;

    internal class PointCloudApi
    {
        private NativeSession m_NativeSession;

        private float[] m_CachedVector = new float[4];

        public PointCloudApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public long GetTimestamp(IntPtr pointCloudHandle)
        {
            long timestamp = 0;
            ExternApi.ArPointCloud_getTimestamp(
                m_NativeSession.SessionHandle, pointCloudHandle, ref timestamp);
            return timestamp;
        }

        public int GetNumberOfPoints(IntPtr pointCloudHandle)
        {
            int pointCount = 0;
            ExternApi.ArPointCloud_getNumberOfPoints(
                m_NativeSession.SessionHandle, pointCloudHandle, ref pointCount);

            return pointCount;
        }

        public PointCloudPoint GetPoint(IntPtr pointCloudHandle, int index)
        {
            // Get a reference to the pointcloud data to extract position and condfidence of point
            // at index.
            IntPtr pointCloudDataHandle = IntPtr.Zero;
            ExternApi.ArPointCloud_getData(
                m_NativeSession.SessionHandle, pointCloudHandle, ref pointCloudDataHandle);
            IntPtr pointDataHandle = new IntPtr(pointCloudDataHandle.ToInt64() +
                (Marshal.SizeOf(typeof(Vector4)) * index));
            Marshal.Copy(pointDataHandle, m_CachedVector, 0, 4);

            // Negate z axis because points are returned in OpenGl space.
            Vector3 position = new Vector3(
                m_CachedVector[0], m_CachedVector[1], -m_CachedVector[2]);
            float confidence = m_CachedVector[3];

            return new PointCloudPoint(_GetPointId(pointCloudHandle, index), position, confidence);
        }

        public void Release(IntPtr pointCloudHandle)
        {
            ExternApi.ArPointCloud_release(pointCloudHandle);
        }

#if !UNITY_EDITOR
        private int _GetPointId(IntPtr pointCloudHandle, int index)
        {
            IntPtr pointCloudIdsHandle = IntPtr.Zero;
            ExternApi.ArPointCloud_getPointIds(
                m_NativeSession.SessionHandle, pointCloudHandle, ref pointCloudIdsHandle);
            IntPtr pointIdHandle =
                new IntPtr(pointCloudIdsHandle.ToInt64() + (Marshal.SizeOf(typeof(int)) * index));
            return Marshal.ReadInt32(pointIdHandle);
        }
#else
        private int _GetPointId(IntPtr pointCloudHandle, int index)
        {
            return 0;
        }
#endif

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_getTimestamp(
                IntPtr session, IntPtr pointCloudHandle, ref long timestamp);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_getNumberOfPoints(
                IntPtr session, IntPtr pointCloudHandle, ref int pointCount);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_getData(
                IntPtr session, IntPtr pointCloudHandle, ref IntPtr pointCloudData);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_getPointIds(
                IntPtr session, IntPtr pointCloudHandle, ref IntPtr pointIds);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_release(IntPtr pointCloudHandle);
#pragma warning restore 626
        }
    }
}
