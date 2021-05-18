//-----------------------------------------------------------------------
// <copyright file="TrackApi.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class TrackApi
    {
        private NativeSession _nativeSession;

        public TrackApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public IntPtr Create(Track track)
        {
            IntPtr trackHandle = IntPtr.Zero;
            ExternApi.ArTrack_create(_nativeSession.SessionHandle, ref trackHandle);

            // Track ID
            GCHandle trackIdHandle = GCHandle.Alloc(track.Id.ToByteArray(), GCHandleType.Pinned);

            ExternApi.ArTrack_setId(_nativeSession.SessionHandle, trackHandle,
                                    trackIdHandle.AddrOfPinnedObject());

            if (trackIdHandle.IsAllocated)
            {
                trackIdHandle.Free();
            }

            // Metadata
            GCHandle metadataHandle = GCHandle.Alloc(track.Metadata, GCHandleType.Pinned);

            ExternApi.ArTrack_setMetadata(_nativeSession.SessionHandle, trackHandle,
                metadataHandle.AddrOfPinnedObject(), track.Metadata.Length);

            if (metadataHandle.IsAllocated)
            {
                metadataHandle.Free();
            }

            ExternApi.ArTrack_setMimeType(_nativeSession.SessionHandle, trackHandle,
                                          track.MimeType);

            return trackHandle;
        }

        public void Destroy(IntPtr trackHandle)
        {
            ExternApi.ArTrack_destroy(trackHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_create(IntPtr session, ref IntPtr trackHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_destroy(IntPtr trackHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_setId(
                IntPtr session, IntPtr trackHandle, IntPtr trackIdBytes);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_setMetadata(
                IntPtr session, IntPtr trackHandle, IntPtr metadataBytes,
                int metadataBufferSize);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArTrack_setMimeType(
                IntPtr session, IntPtr trackHandle, string mimeType);
#pragma warning restore 626
        }
    }
}
