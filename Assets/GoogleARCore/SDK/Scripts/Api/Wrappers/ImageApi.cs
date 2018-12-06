//-----------------------------------------------------------------------
// <copyright file="ImageApi.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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
        private NativeSession m_NativeSession;

        public ImageApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public void GetImageBuffer(IntPtr imageHandle, out int width, out int height, out IntPtr yPlane,
            out IntPtr uPlane, out IntPtr vPlane, out int yRowStride, out int uvPixelStride, out int uvRowStride)
        {
            IntPtr ndkImageHandle = IntPtr.Zero;
            ExternApi.ArImage_getNdkImage(imageHandle, ref ndkImageHandle);

            width = 0;
            ExternApi.AImage_getWidth(ndkImageHandle, ref width);

            height = 0;
            ExternApi.AImage_getHeight(ndkImageHandle, ref height);

            const int Y_PLANE = 0;
            const int U_PLANE = 1;
            const int V_PLANE = 2;
            int bufferLength = 0;

            yPlane = IntPtr.Zero;
            ExternApi.AImage_getPlaneData(ndkImageHandle, Y_PLANE, ref yPlane, ref bufferLength);

            uPlane = IntPtr.Zero;
            ExternApi.AImage_getPlaneData(ndkImageHandle, U_PLANE, ref uPlane, ref bufferLength);

            vPlane = IntPtr.Zero;
            ExternApi.AImage_getPlaneData(ndkImageHandle, V_PLANE, ref vPlane, ref bufferLength);

            yRowStride = 0;
            ExternApi.AImage_getPlaneRowStride(ndkImageHandle, Y_PLANE, ref yRowStride);

            uvPixelStride = 0;
            ExternApi.AImage_getPlanePixelStride(ndkImageHandle, U_PLANE, ref uvPixelStride);

            uvRowStride = 0;
            ExternApi.AImage_getPlaneRowStride(ndkImageHandle, U_PLANE, ref uvRowStride);
        }

        public void Release(IntPtr imageHandle)
        {
            m_NativeSession.MarkHandleReleased(imageHandle);
            ExternApi.ArImage_release(imageHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getNdkImage(IntPtr imageHandle, ref IntPtr ndkImage);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_release(IntPtr imageHandle);

            [AndroidImport(ApiConstants.MediaNdk)]
            public static extern int AImage_getWidth(IntPtr ndkImageHandle, ref int width);

            [AndroidImport(ApiConstants.MediaNdk)]
            public static extern int AImage_getHeight(IntPtr ndkImageHandle, ref int height);

            [AndroidImport(ApiConstants.MediaNdk)]
            public static extern int AImage_getPlaneData(IntPtr imageHandle, int planeIdx, ref IntPtr data,
                ref int dataLength);

            [AndroidImport(ApiConstants.MediaNdk)]
            public static extern int AImage_getPlanePixelStride(IntPtr imageHandle, int planeIdx, ref int pixelStride);

            [AndroidImport(ApiConstants.MediaNdk)]
            public static extern int AImage_getPlaneRowStride(IntPtr imageHandle, int planeIdx, ref int rowStride);
#pragma warning restore 626
        }
    }
}
