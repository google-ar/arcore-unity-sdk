//-----------------------------------------------------------------------
// <copyright file="CameraMetadataApi.cs" company="Google">
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
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    using Marshal = System.Runtime.InteropServices.Marshal;

    internal class CameraMetadataApi
    {
        private NativeSession m_NativeSession;

        public CameraMetadataApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public void Release(IntPtr arCameraMetadataHandle)
        {
            ExternApi.ArImageMetadata_release(arCameraMetadataHandle);
        }

        public bool TryGetValues(IntPtr cameraMetadataHandle,
            CameraMetadataTag tag, List<CameraMetadataValue> resultList)
        {
            IntPtr ndkMetadataHandle = IntPtr.Zero;
            ExternApi.ArImageMetadata_getNdkCameraMetadata(m_NativeSession.SessionHandle,
                cameraMetadataHandle, ref ndkMetadataHandle);

            resultList.Clear();
            NdkCameraMetadata entry = new NdkCameraMetadata();
            NdkCameraStatus status = ExternApi.ACameraMetadata_getConstEntry(ndkMetadataHandle, tag, ref entry);
            if (status != NdkCameraStatus.Ok)
            {
                ARDebug.LogErrorFormat("ACameraMetadata_getConstEntry error with native camera error code: {0}",
                    status);
                return false;
            }

            for (int i = 0; i < entry.Count; i++)
            {
                switch (entry.Type)
                {
                    case NdkCameraMetadataType.Byte:
                        sbyte byteValue = (sbyte)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<sbyte>(entry.Value, i),
                            typeof(sbyte));
                        resultList.Add(new CameraMetadataValue(byteValue));
                        break;
                    case NdkCameraMetadataType.Int32:
                        int intValue = (int)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<int>(entry.Value, i),
                            typeof(int));
                        resultList.Add(new CameraMetadataValue(intValue));
                        break;
                    case NdkCameraMetadataType.Float:
                        float floatValue = (float)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<float>(entry.Value, i),
                            typeof(float));
                        resultList.Add(new CameraMetadataValue(floatValue));
                        break;
                    case NdkCameraMetadataType.Int64:
                        long longValue = (long)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<long>(entry.Value, i),
                            typeof(long));
                        resultList.Add(new CameraMetadataValue(longValue));
                        break;
                    case NdkCameraMetadataType.Double:
                        double doubleValue = (double)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<double>(entry.Value, i),
                            typeof(double));
                        resultList.Add(new CameraMetadataValue(doubleValue));
                        break;
                    case NdkCameraMetadataType.Rational:
                        CameraMetadataRational rationalValue = (CameraMetadataRational)Marshal.PtrToStructure(
                            MarshalingHelper.GetPtrToUnmanagedArrayElement<CameraMetadataRational>(entry.Value, i),
                            typeof(CameraMetadataRational));
                        resultList.Add(new CameraMetadataValue(rationalValue));
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        public bool GetAllCameraMetadataTags(IntPtr cameraMetadataHandle, List<CameraMetadataTag> resultList)
        {
            IntPtr ndkMetadataHandle = IntPtr.Zero;
            ExternApi.ArImageMetadata_getNdkCameraMetadata(m_NativeSession.SessionHandle,
                cameraMetadataHandle, ref ndkMetadataHandle);

            IntPtr tagsHandle = IntPtr.Zero;
            int tagsCount = 0;
            NdkCameraStatus status = ExternApi.ACameraMetadata_getAllTags(ndkMetadataHandle, ref tagsCount, ref tagsHandle);
            if (status != NdkCameraStatus.Ok)
            {
                ARDebug.LogErrorFormat("ACameraMetadata_getAllTags error with native camera error code: {0}",
                    status);
                return false;
            }

            for (int i = 0; i < tagsCount; i++)
            {
                resultList.Add((CameraMetadataTag)Marshal.PtrToStructure(
                    MarshalingHelper.GetPtrToUnmanagedArrayElement<int>(tagsHandle, i),
                    typeof(int)));
            }

            return true;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImageMetadata_getNdkCameraMetadata(IntPtr session, IntPtr image_metadata,
                ref IntPtr out_ndk_metadata);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImageMetadata_release(IntPtr metadata);

            [AndroidImport(ApiConstants.NdkCameraApi)]
            public static extern NdkCameraStatus ACameraMetadata_getConstEntry(IntPtr ndkCameraMetadata,
                CameraMetadataTag tag, ref NdkCameraMetadata entry);

            [AndroidImport(ApiConstants.NdkCameraApi)]
            public static extern NdkCameraStatus ACameraMetadata_getAllTags(IntPtr ndkCameraMetadata,
                ref int numEntries, ref IntPtr tags);
#pragma warning restore 626
        }
    }
}
