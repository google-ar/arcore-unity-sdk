//-----------------------------------------------------------------------
// <copyright file="PlaneApi.cs" company="Google LLC">
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

    internal class PlaneApi
    {
        private const int _maxPolygonSize = 1024;
        private NativeSession _nativeSession;
        private float[] _tmpPoints;
        private System.Runtime.InteropServices.GCHandle _tmpPointsHandle;

        public PlaneApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
            _tmpPoints = new float[_maxPolygonSize * 2];
            _tmpPointsHandle = System.Runtime.InteropServices.GCHandle.Alloc(
                _tmpPoints, System.Runtime.InteropServices.GCHandleType.Pinned);
        }

        ~PlaneApi()
        {
            _tmpPointsHandle.Free();
        }

        public Pose GetCenterPose(IntPtr planeHandle)
        {
            var poseHandle = _nativeSession.PoseApi.Create();
            ExternApi.ArPlane_getCenterPose(_nativeSession.SessionHandle, planeHandle, poseHandle);
            Pose resultPose = _nativeSession.PoseApi.ExtractPoseValue(poseHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public float GetExtentX(IntPtr planeHandle)
        {
            float extentX = 0.0f;
            ExternApi.ArPlane_getExtentX(_nativeSession.SessionHandle, planeHandle, ref extentX);
            return extentX;
        }

        public float GetExtentZ(IntPtr planeHandle)
        {
            float extentZ = 0.0f;
            ExternApi.ArPlane_getExtentZ(_nativeSession.SessionHandle, planeHandle, ref extentZ);
            return extentZ;
        }

        public void GetPolygon(IntPtr planeHandle, List<Vector3> points)
        {
            points.Clear();
            int pointCount = 0;
            ExternApi.ArPlane_getPolygonSize(
                _nativeSession.SessionHandle, planeHandle, ref pointCount);
            if (pointCount < 1)
            {
                return;
            }
            else if (pointCount > _maxPolygonSize)
            {
                Debug.LogError("GetPolygon::Plane polygon size exceeds buffer capacity.");
                pointCount = _maxPolygonSize;
            }

            ExternApi.ArPlane_getPolygon(
                _nativeSession.SessionHandle, planeHandle, _tmpPointsHandle.AddrOfPinnedObject());

            var planeCenter = GetCenterPose(planeHandle);
            var unityWorldTPlane =
                Matrix4x4.TRS(planeCenter.position, planeCenter.rotation, Vector3.one);
            for (int i = pointCount - 2; i >= 0; i -= 2)
            {
                var point = unityWorldTPlane.MultiplyPoint3x4(
                    new Vector3(_tmpPoints[i], 0, -_tmpPoints[i + 1]));
                points.Add(point);
            }
        }

        public DetectedPlane GetSubsumedBy(IntPtr planeHandle)
        {
            IntPtr subsumerHandle = IntPtr.Zero;
            ExternApi.ArPlane_acquireSubsumedBy(
                _nativeSession.SessionHandle, planeHandle, ref subsumerHandle);
            return _nativeSession.TrackableFactory(subsumerHandle) as DetectedPlane;
        }

        public DetectedPlaneType GetPlaneType(IntPtr planeHandle)
        {
            ApiPlaneType planeType = ApiPlaneType.HorizontalDownwardFacing;
            ExternApi.ArPlane_getType(_nativeSession.SessionHandle, planeHandle, ref planeType);
            return planeType.ToDetectedPlaneType();
        }

        public bool IsPoseInExtents(IntPtr planeHandle, Pose pose)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent
            // a boolean.
            int isPoseInExtents = 0;
            var poseHandle = _nativeSession.PoseApi.Create(pose);
            ExternApi.ArPlane_isPoseInExtents(
                _nativeSession.SessionHandle, planeHandle, poseHandle, ref isPoseInExtents);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return isPoseInExtents != 0;
        }

        public bool IsPoseInExtents(IntPtr planeHandle, IntPtr poseHandle)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent
            // a boolean.
            int isPoseInExtents = 0;
            ExternApi.ArPlane_isPoseInExtents(
                _nativeSession.SessionHandle, planeHandle, poseHandle, ref isPoseInExtents);
            return isPoseInExtents != 0;
        }

        public bool IsPoseInPolygon(IntPtr planeHandle, Pose pose)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent
            // a boolean.
            int isPoseInPolygon = 0;
            var poseHandle = _nativeSession.PoseApi.Create(pose);
            ExternApi.ArPlane_isPoseInPolygon(
                _nativeSession.SessionHandle, planeHandle, poseHandle, ref isPoseInPolygon);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return isPoseInPolygon != 0;
        }

        public bool IsPoseInPolygon(IntPtr planeHandle, IntPtr poseHandle)
        {
            // The int is used as a boolean value as the C API expects a int32_t value to represent
            // a boolean.
            int isPoseInPolygon = 0;
            ExternApi.ArPlane_isPoseInPolygon(
                _nativeSession.SessionHandle, planeHandle, poseHandle, ref isPoseInPolygon);
            return isPoseInPolygon != 0;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getCenterPose(
                IntPtr sessionHandle, IntPtr planeHandle, IntPtr poseHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_acquireSubsumedBy(
                IntPtr sessionHandle, IntPtr planeHandle, ref IntPtr subsumerHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getExtentX(
                IntPtr sessionHandle, IntPtr planeHandle, ref float extentX);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getExtentZ(
                IntPtr sessionHandle, IntPtr planeHandle, ref float extentZ);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getType(
                IntPtr sessionHandle, IntPtr planeHandle, ref ApiPlaneType planeType);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getPolygonSize(
                IntPtr sessionHandle, IntPtr planeHandle, ref int polygonSize);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_getPolygon(
                IntPtr sessionHandle, IntPtr planeHandle, IntPtr polygonXZ);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_isPoseInExtents(
                IntPtr sessionHandle, IntPtr planeHandle, IntPtr poseHandle,
                ref int isPoseInExtents);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPlane_isPoseInPolygon(
                IntPtr sessionHandle, IntPtr planeHandle, IntPtr poseHandle,
                ref int isPoseInPolygon);
#pragma warning restore 626
        }
    }
}
