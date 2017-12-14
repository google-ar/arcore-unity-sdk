//-----------------------------------------------------------------------
// <copyright file="PointCloudApi.cs" company="Google">
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
    public class PointCloudApi
    {
        private NativeApi m_NativeApi;

        private float[] m_CachedVector = new float[4];

        public PointCloudApi(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public long GetTimestamp(IntPtr pointCloudHandle)
        {
            long timestamp = 0;
            ExternApi.ArPointCloud_getTimestamp(m_NativeApi.SessionHandle, pointCloudHandle, ref timestamp);
            return timestamp;
        }

        public int GetNumberOfPoints(IntPtr pointCloudHandle)
        {
            int pointCount = 0;
            ExternApi.ArPointCloud_getNumberOfPoints(m_NativeApi.SessionHandle, pointCloudHandle, ref pointCount);

            // TODO (xuguo): remove the divide by 4 after b/69389164 is fixed.
            return pointCount / 4;
        }

        public Vector4 GetPoint(IntPtr pointCloudHandle, int index)
        {
            IntPtr pointCloudNativeHandle = IntPtr.Zero;
            ExternApi.ArPointCloud_getData(m_NativeApi.SessionHandle, pointCloudHandle, ref pointCloudNativeHandle);
            IntPtr pointHandle = new IntPtr(pointCloudNativeHandle.ToInt64() +
                                            (Marshal.SizeOf(typeof(Vector4)) * index));
            Marshal.Copy(pointHandle, m_CachedVector, 0, 4);

            // Negate z axis because points are returned in OpenGl space.
            return new Vector4(m_CachedVector[0], m_CachedVector[1], -m_CachedVector[2], m_CachedVector[3]);
        }

        public void CopyPoints(IntPtr pointCloudHandle, List<Vector4> points)
        {
            points.Clear();

            IntPtr pointCloudNativeHandle = IntPtr.Zero;
            int pointCloudSize = GetNumberOfPoints(pointCloudHandle);

            ExternApi.ArPointCloud_getData(m_NativeApi.SessionHandle, pointCloudHandle, ref pointCloudNativeHandle);

            MarshalingHelper.AddUnmanagedStructArrayToList<Vector4>(pointCloudNativeHandle,
                    pointCloudSize, points);

            for (int i = 0; i < pointCloudSize; ++i)
            {
                // Negate z axis because points are returned in OpenGl space.
                points[i] = new Vector4(points[i].x, points[i].y,
                        -points[i].z, points[i].w);
            }
        }

        public void Release(IntPtr pointCloudHandle)
        {
            ExternApi.ArPointCloud_release(pointCloudHandle);
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_getTimestamp(IntPtr session, IntPtr pointCloudHandle,
                ref long timestamp);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_getNumberOfPoints(IntPtr session, IntPtr pointCloudHandle,
                ref int pointCount);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_getData(IntPtr session, IntPtr pointCloudHandle,
                ref IntPtr pointCloudData);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPointCloud_release(IntPtr pointCloudHandle);
        }
    }
}
