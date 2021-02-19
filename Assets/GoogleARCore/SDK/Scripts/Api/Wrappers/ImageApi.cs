//-----------------------------------------------------------------------
// <copyright file="ImageApi.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    using GoogleARCore;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class ImageApi
    {
        private NativeSession _nativeSession;

        public ImageApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public int GetPlanePixelStride(IntPtr imageHandle, int planeIndex)
        {
            int stride = 0;
            ExternApi.ArImage_getPlanePixelStride(_nativeSession.SessionHandle, imageHandle,
                planeIndex, ref stride);
            return stride;
        }

        public int GetPlaneRowStride(IntPtr imageHandle, int planeIndex)
        {
            int stride = 0;
            ExternApi.ArImage_getPlaneRowStride(_nativeSession.SessionHandle, imageHandle,
                planeIndex, ref stride);
            return stride;
        }

        public void GetPlaneData(IntPtr imageHandle, int planeIndex, ref IntPtr surfaceData,
                                 ref int dataLength)
        {
            ExternApi.ArImage_getPlaneData(_nativeSession.SessionHandle, imageHandle, planeIndex,
                ref surfaceData, ref dataLength);
        }

        public int GetWidth(IntPtr imageHandle)
        {
            int width = 0;
            ExternApi.ArImage_getWidth(_nativeSession.SessionHandle, imageHandle, out width);
            return width;
        }

        public int GetHeight(IntPtr imageHandle)
        {
            int height = 0;
            ExternApi.ArImage_getHeight(_nativeSession.SessionHandle, imageHandle, out height);
            return height;
        }

        public void Release(IntPtr imageHandle)
        {
            ExternApi.ArImage_release(imageHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_release(IntPtr imageHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getWidth(IntPtr sessionHandle, IntPtr imageHandle,
                out int width);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getHeight(IntPtr sessionHandle, IntPtr imageHandle,
                out int height);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getPlaneData(IntPtr sessionHandle, IntPtr imageHandle,
                int planeIndex, ref IntPtr surfaceData, ref int dataLength);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getPlanePixelStride(IntPtr sessionHandle,
                IntPtr imageHandle, int planeIdx, ref int pixelStride);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getPlaneRowStride(IntPtr sessionHandle,
                IntPtr imageHandle, int planeIdx, ref int rowStride);
#pragma warning restore 626
        }
    }
}
