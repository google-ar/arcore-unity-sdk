//-----------------------------------------------------------------------
// <copyright file="AnchorApi.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using GoogleARCore.CrossPlatform;
    using GoogleARCoreInternal.CrossPlatform;
    using UnityEngine;

    internal class AnchorApi
    {
        private NativeSession m_NativeSession;

        public AnchorApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public static void Release(IntPtr anchorHandle)
        {
            ExternApi.ArAnchor_release(anchorHandle);
        }

        public Pose GetPose(IntPtr anchorHandle)
        {
            var poseHandle = m_NativeSession.PoseApi.Create();
            ExternApi.ArAnchor_getPose(m_NativeSession.SessionHandle, anchorHandle, poseHandle);
            Pose resultPose = m_NativeSession.PoseApi.ExtractPoseValue(poseHandle);
            m_NativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public TrackingState GetTrackingState(IntPtr anchorHandle)
        {
            ApiTrackingState trackingState = ApiTrackingState.Stopped;
            ExternApi.ArAnchor_getTrackingState(m_NativeSession.SessionHandle, anchorHandle,
                ref trackingState);
            return trackingState.ToTrackingState();
        }

        public ApiCloudAnchorState GetCloudAnchorState(IntPtr anchorHandle)
        {
            ApiCloudAnchorState cloudState = ApiCloudAnchorState.None;
            ExternApi.ArAnchor_getCloudAnchorState(
                m_NativeSession.SessionHandle, anchorHandle, ref cloudState);
            return cloudState;
        }

        public string GetCloudAnchorId(IntPtr anchorHandle)
        {
            IntPtr cloudIdHandle = IntPtr.Zero;
            ExternApi.ArAnchor_acquireCloudAnchorId(
                m_NativeSession.SessionHandle, anchorHandle, ref cloudIdHandle);

            var result = Marshal.PtrToStringAnsi(cloudIdHandle);
            ExternApi.ArString_release(cloudIdHandle);
            return result;
        }

        public void Detach(IntPtr anchorHandle)
        {
            if (LifecycleManager.Instance.NativeSession == m_NativeSession)
            {
                ExternApi.ArAnchor_detach(m_NativeSession.SessionHandle, anchorHandle);
            }
        }

        public IntPtr CreateList()
        {
            IntPtr listHandle = IntPtr.Zero;
            ExternApi.ArAnchorList_create(m_NativeSession.SessionHandle, ref listHandle);
            return listHandle;
        }

        public int GetListSize(IntPtr anchorListHandle)
        {
            int size = 0;
            ExternApi.ArAnchorList_getSize(
                m_NativeSession.SessionHandle, anchorListHandle, ref size);
            return size;
        }

        public IntPtr AcquireListItem(IntPtr anchorListHandle, int index)
        {
            IntPtr anchorHandle = IntPtr.Zero;
            ExternApi.ArAnchorList_acquireItem(
                m_NativeSession.SessionHandle, anchorListHandle, index, ref anchorHandle);
            return anchorHandle;
        }

        public void DestroyList(IntPtr anchorListHandle)
        {
            ExternApi.ArAnchorList_destroy(anchorListHandle);
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getPose(
                IntPtr sessionHandle, IntPtr anchorHandle, IntPtr poseHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getTrackingState(
                IntPtr sessionHandle, IntPtr anchorHandle, ref ApiTrackingState trackingState);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getCloudAnchorState(
                IntPtr sessionHandle, IntPtr anchorHandle, ref ApiCloudAnchorState state);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_acquireCloudAnchorId(
                IntPtr sessionHandle, IntPtr anchorHandle, ref IntPtr hostingIdHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_release(IntPtr anchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_detach(IntPtr sessionHandle, IntPtr anchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArString_release(IntPtr stringHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_create(
                IntPtr sessionHandle, ref IntPtr outputAnchorListHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_destroy(IntPtr anchorListHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_getSize(
                IntPtr sessionHandle, IntPtr anchorListHandle, ref int outputSize);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_acquireItem(
                IntPtr sessionHandle, IntPtr anchorListHandle, int index,
                ref IntPtr outputAnchorHandle);
        }
    }
}
