//-----------------------------------------------------------------------
// <copyright file="TrackableApi.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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

    internal class TrackableApi
    {
        private NativeSession _nativeSession;

        public TrackableApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public ApiTrackableType GetType(IntPtr trackableHandle)
        {
            ApiTrackableType type = ApiTrackableType.Plane;
            ExternApi.ArTrackable_getType(_nativeSession.SessionHandle, trackableHandle, ref type);
            return type;
        }

        public TrackingState GetTrackingState(IntPtr trackableHandle)
        {
            ApiTrackingState apiTrackingState = ApiTrackingState.Stopped;
            ExternApi.ArTrackable_getTrackingState(_nativeSession.SessionHandle, trackableHandle,
                ref apiTrackingState);
            return apiTrackingState.ToTrackingState();
        }

        public bool AcquireNewAnchor(IntPtr trackableHandle, Pose pose, out IntPtr anchorHandle)
        {
            IntPtr poseHandle = _nativeSession.PoseApi.Create(pose);
            anchorHandle = IntPtr.Zero;
            int status = ExternApi.ArTrackable_acquireNewAnchor(
                _nativeSession.SessionHandle, trackableHandle, poseHandle, ref anchorHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return status == 0;
        }

        public void Release(IntPtr trackableHandle)
        {
             ExternApi.ArTrackable_release(trackableHandle);
        }

        public void GetAnchors(IntPtr trackableHandle, List<Anchor> anchors)
        {
            IntPtr anchorListHandle = _nativeSession.AnchorApi.CreateList();
            ExternApi.ArTrackable_getAnchors(
                _nativeSession.SessionHandle, trackableHandle, anchorListHandle);

            anchors.Clear();
            int anchorCount = _nativeSession.AnchorApi.GetListSize(anchorListHandle);
            for (int i = 0; i < anchorCount; i++)
            {
                IntPtr anchorHandle =
                    _nativeSession.AnchorApi.AcquireListItem(anchorListHandle, i);
                Anchor anchor = Anchor.Factory(_nativeSession, anchorHandle, false);
                if (anchor == null)
                {
                    Debug.LogFormat("Unable to find Anchor component for handle {0}", anchorHandle);
                }
                else
                {
                    anchors.Add(anchor);
                }
            }

            _nativeSession.AnchorApi.DestroyList(anchorListHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_getType(
                IntPtr sessionHandle, IntPtr trackableHandle, ref ApiTrackableType trackableType);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_getTrackingState(
                IntPtr sessionHandle, IntPtr trackableHandle, ref ApiTrackingState trackingState);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArTrackable_acquireNewAnchor(
                IntPtr sessionHandle, IntPtr trackableHandle, IntPtr poseHandle,
                ref IntPtr anchorHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_release(IntPtr trackableHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackable_getAnchors(
                IntPtr sessionHandle, IntPtr trackableHandle, IntPtr outputListHandle);
#pragma warning restore 626
        }
    }
}
