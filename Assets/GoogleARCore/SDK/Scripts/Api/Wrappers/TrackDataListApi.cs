//-----------------------------------------------------------------------
// <copyright file="TrackDataListApi.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class TrackDataListApi
    {
        private NativeSession _nativeSession;

        public TrackDataListApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public IntPtr Create()
        {
            IntPtr handle = IntPtr.Zero;
            ExternApi.ArTrackDataList_create(_nativeSession.SessionHandle, ref handle);
            return handle;
        }

        public void Destroy(IntPtr listHandle)
        {
            ExternApi.ArTrackDataList_destroy(listHandle);
        }

        public int GetCount(IntPtr listHandle)
        {
            int count = 0;
            ExternApi.ArTrackDataList_getSize(_nativeSession.SessionHandle, listHandle, ref count);
            return count;
        }

        public IntPtr AcquireItem(IntPtr listHandle, int index)
        {
            IntPtr trackDataHandle = IntPtr.Zero;
            ExternApi.ArTrackDataList_acquireItem(
                _nativeSession.SessionHandle, listHandle, index, ref trackDataHandle);
            return trackDataHandle;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_create(
                IntPtr sessionHandle, ref IntPtr trackDataListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_destroy(IntPtr trackDataListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_getSize(
                IntPtr sessionHandle, IntPtr trackDataListHandle, ref int outSize);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackDataList_acquireItem(
                IntPtr sessionHandle, IntPtr trackDataListHandle, int index,
                ref IntPtr outTrackData);
#pragma warning restore 626
        }
    }
}
