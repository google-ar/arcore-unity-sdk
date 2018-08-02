//-----------------------------------------------------------------------
// <copyright file="CameraConfigListApi.cs" company="Google">
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
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class CameraConfigListApi
    {
        private NativeSession m_NativeSession;

        public CameraConfigListApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public IntPtr Create()
        {
            IntPtr cameraConfigListHandle = IntPtr.Zero;
            ExternApi.ArCameraConfigList_create(m_NativeSession.SessionHandle, ref cameraConfigListHandle);
            return cameraConfigListHandle;
        }

        public void Destroy(IntPtr cameraConfigListHandle)
        {
            ExternApi.ArCameraConfigList_destroy(cameraConfigListHandle);
        }

        public int GetSize(IntPtr cameraConfigListHandle)
        {
            int size = 0;
            ExternApi.ArCameraConfigList_getSize(m_NativeSession.SessionHandle, cameraConfigListHandle, ref size);
            return size;
        }

        public void GetItemAt(IntPtr cameraConfigListHandle, int index, IntPtr cameraConfigHandle)
        {
            ExternApi.ArCameraConfigList_getItem(m_NativeSession.SessionHandle, cameraConfigListHandle,
                                                 index, cameraConfigHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigList_create(IntPtr sessionHandle,
                ref IntPtr cameraConfigListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigList_destroy(IntPtr cameraConfigListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigList_getSize(IntPtr sessionHandle,
                IntPtr cameraConfigListHandle, ref int size);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArCameraConfigList_getItem(IntPtr sessionHandle,
                IntPtr cameraConfigListHandle, int index, IntPtr itemHandle);
#pragma warning restore 626
        }
    }
}
