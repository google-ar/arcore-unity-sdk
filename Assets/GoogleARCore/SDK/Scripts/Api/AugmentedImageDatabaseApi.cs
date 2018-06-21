//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabaseApi.cs" company="Google">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using GoogleARCoreInternal;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class AugmentedImageDatabaseApi
    {
        private NativeSession m_NativeSession;

        public AugmentedImageDatabaseApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public IntPtr CreateFromBytes(byte[] databaseBytes)
        {
            IntPtr result = IntPtr.Zero;
            var bytesHandle = GCHandle.Alloc(databaseBytes, GCHandleType.Pinned);
            var status = ExternApi.ArAugmentedImageDatabase_deserialize(m_NativeSession.SessionHandle,
                bytesHandle.AddrOfPinnedObject(), databaseBytes.Length, ref result);

            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("ArAugmentedImageDatabase_deserialize returned {0} when passed a " +
                    "database with size: {1}", status, databaseBytes.Length);
            }

            bytesHandle.Free();
            return result;
        }

        public void Destroy(IntPtr trackedImageDatabaseHandle)
        {
            ExternApi.ArAugmentedImageDatabase_destroy(trackedImageDatabaseHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImageDatabase_destroy(IntPtr augmentedImageDatabaseHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArAugmentedImageDatabase_deserialize(IntPtr sessionHandle,
                 IntPtr rawBytes, Int64 rawBytesSize, ref IntPtr outAugmentedImageDatabaseHandle);
#pragma warning restore 626
        }
    }
}
