//-----------------------------------------------------------------------
// <copyright file="TrackDataApi.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class TrackDataApi
    {
        private NativeSession _nativeSession;

        public TrackDataApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public long GetFrameTimestamp(IntPtr trackDataHandle)
        {
            long timestamp = 0L;
            ExternApi.ArTrackData_getFrameTimestamp(_nativeSession.SessionHandle,
                                                    trackDataHandle,
                                                    ref timestamp);
            return timestamp;
        }

        public byte[] GetData(IntPtr trackDataHandle)
        {
            IntPtr dataPtr = IntPtr.Zero;
            int size = 0;

            ExternApi.ArTrackData_getData(_nativeSession.SessionHandle, trackDataHandle,
                                          ref dataPtr, ref size);
            byte[] data = new byte[size];
            if (size > 0)
            {
                Marshal.Copy(dataPtr, data, 0, size);
            }

            return data;
        }

        public void Release(IntPtr trackDataHandle)
        {
            ExternApi.ArTrackData_release(trackDataHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackData_getFrameTimestamp(
                IntPtr sessionHandle, IntPtr trackDataHandle, ref long timestamp);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackData_getData(
                IntPtr sessionHandle, IntPtr trackDataHandle, ref IntPtr dataBytesHandle,
                ref int size);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrackData_release(IntPtr trackDataHandle);
#pragma warning restore 626
        }
    }
}
