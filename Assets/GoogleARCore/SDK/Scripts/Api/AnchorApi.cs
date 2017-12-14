//-----------------------------------------------------------------------
// <copyright file="AnchorApi.cs" company="Google">
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
    public class AnchorApi
    {
        private NativeApi m_NativeApi;

        public AnchorApi(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public Pose GetPose(IntPtr anchorHandle)
        {
            var poseHandle = m_NativeApi.Pose.Create();
            ExternApi.ArAnchor_getPose(m_NativeApi.SessionHandle, anchorHandle, poseHandle);
            Pose resultPose = m_NativeApi.Pose.ExtractPoseValue(poseHandle);
            m_NativeApi.Pose.Destroy(poseHandle);
            return resultPose;
        }

        public TrackingState GetTrackingState(IntPtr anchorHandle)
        {
            ApiTrackingState trackingState = ApiTrackingState.Stopped;
            ExternApi.ArAnchor_getTrackingState(m_NativeApi.SessionHandle, anchorHandle,
                ref trackingState);
            return trackingState.ToTrackingState();
        }

        public void Detach(IntPtr anchorHandle)
        {
            ExternApi.ArAnchor_detach(m_NativeApi.SessionHandle, anchorHandle);
        }

        public void Release(IntPtr anchorHandle)
        {
            ExternApi.ArAnchor_release(anchorHandle);
        }

        public IntPtr CreateList()
        {
            IntPtr listHandle = IntPtr.Zero;
            ExternApi.ArAnchorList_create(m_NativeApi.SessionHandle, ref listHandle);
            return listHandle;
        }

        public int GetListSize(IntPtr anchorListHandle)
        {
            int size = 0;
            ExternApi.ArAnchorList_getSize(m_NativeApi.SessionHandle, anchorListHandle, ref size);
            return size;
        }

        public IntPtr AcquireListItem(IntPtr anchorListHandle, int index)
        {
            IntPtr anchorHandle = IntPtr.Zero;
            ExternApi.ArAnchorList_acquireItem(m_NativeApi.SessionHandle, anchorListHandle, index, 
                ref anchorHandle);
            return anchorHandle;
        }

        public void DestroyList(IntPtr anchorListHandle)
        {
            ExternApi.ArAnchorList_destroy(anchorListHandle);
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getPose(IntPtr sessionHandle, IntPtr anchorHandle, IntPtr poseHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_getTrackingState(IntPtr sessionHandle, IntPtr anchorHandle,
                ref ApiTrackingState trackingState);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_detach(IntPtr sessionHandle, IntPtr anchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchor_release(IntPtr anchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_create(IntPtr sessionHandle, ref IntPtr outputAnchorListHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_destroy(IntPtr anchorListHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_getSize(IntPtr sessionHandle, IntPtr anchorListHandle, ref int outputSize);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAnchorList_acquireItem(IntPtr sessionHandle, IntPtr anchorListHandle,  int index,
                ref IntPtr outputAnchorHandle);
        }
    }
}
