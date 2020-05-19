//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabaseApi.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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

        public static void Release(IntPtr augmentedImageDatabaseHandle)
        {
            ExternApi.ArAugmentedImageDatabase_destroy(augmentedImageDatabaseHandle);
        }

        public IntPtr Create(byte[] rawData)
        {
            IntPtr outDatabaseHandle = IntPtr.Zero;
            if (rawData != null)
            {
                GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);

                ApiArStatus status = ExternApi.ArAugmentedImageDatabase_deserialize(
                    m_NativeSession.SessionHandle, handle.AddrOfPinnedObject(), rawData.Length,
                    ref outDatabaseHandle);
                if (status != ApiArStatus.Success)
                {
                    Debug.LogWarningFormat(
                        "Failed to deserialize AugmentedImageDatabase raw data with status: {0}",
                        status);
                    outDatabaseHandle = IntPtr.Zero;
                }

                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            else
            {
                ExternApi.ArAugmentedImageDatabase_create(
                    m_NativeSession.SessionHandle, ref outDatabaseHandle);
            }

            return outDatabaseHandle;
        }

        public int AddAugmentedImageAtRuntime(IntPtr augmentedImageDatabaseHandle, string name,
            AugmentedImageSrc imageSrc, float width)
        {
            int outIndex = -1;
            if (InstantPreviewManager.IsProvidingPlatform)
            {
                InstantPreviewManager.LogLimitedSupportMessage(
                    "add images to Augmented Image database");
                return outIndex;
            }

            GCHandle grayscaleBytesHandle = _ConvertTextureToGrayscaleBytes(imageSrc);
            if (grayscaleBytesHandle.AddrOfPinnedObject() == IntPtr.Zero)
            {
                return -1;
            }

            ApiArStatus status;
            if (width > 0)
            {
                status = ExternApi.ArAugmentedImageDatabase_addImageWithPhysicalSize(
                    m_NativeSession.SessionHandle, augmentedImageDatabaseHandle, name,
                    grayscaleBytesHandle.AddrOfPinnedObject(), imageSrc.Width,
                    imageSrc.Height, imageSrc.Width, width, ref outIndex);
            }
            else
            {
                status = ExternApi.ArAugmentedImageDatabase_addImage(
                    m_NativeSession.SessionHandle, augmentedImageDatabaseHandle, name,
                    grayscaleBytesHandle.AddrOfPinnedObject(), imageSrc.Width,
                    imageSrc.Height, imageSrc.Width, ref outIndex);
            }

            if (grayscaleBytesHandle.IsAllocated)
            {
                grayscaleBytesHandle.Free();
            }

            if (status != ApiArStatus.Success)
            {
                Debug.LogWarningFormat(
                    "Failed to add aumented image at runtime with status {0}", status);
                return -1;
            }

            return outIndex;
        }

        private GCHandle _ConvertTextureToGrayscaleBytes(AugmentedImageSrc imageSrc)
        {
            byte[] grayscaleBytes = null;

            if (imageSrc.Format == TextureFormat.RGB24 ||
                imageSrc.Format == TextureFormat.RGBA32)
            {
                Color[] pixels = imageSrc.Pixels;
                grayscaleBytes = new byte[pixels.Length];
                for (int i = 0; i < imageSrc.Height; i++)
                {
                    int widthDelta = i * imageSrc.Width;
                    for (int j = 0; j < imageSrc.Width; j++)
                    {
                        int pixelIndex = ((imageSrc.Height - 1 - i) * imageSrc.Width) + j;
                        grayscaleBytes[widthDelta + j] =
                            (byte)((
                            (0.213 * pixels[pixelIndex].r) +
                            (0.715 * pixels[pixelIndex].g) +
                            (0.072 * pixels[pixelIndex].b)) * 255);
                    }
                }
            }
            else
            {
                Debug.LogError("Unsupported texture format " + imageSrc.Format);
            }

            return GCHandle.Alloc(grayscaleBytes, GCHandleType.Pinned);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImageDatabase_create(
                IntPtr session,
                ref IntPtr out_augmented_image_database);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedImageDatabase_destroy(
                IntPtr augmented_image_database);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArAugmentedImageDatabase_deserialize(
                IntPtr session,
                IntPtr database_raw_bytes,
                long database_raw_bytes_size,
                ref IntPtr out_augmented_image_database);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArAugmentedImageDatabase_addImageWithPhysicalSize(
                IntPtr session,
                IntPtr augmented_image_database,
                string image_name,
                IntPtr image_grayscale_pixels,
                int image_width_in_pixels,
                int image_height_in_pixels,
                int image_stride_in_pixels,
                float image_width_in_meters,
                ref int out_index);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArAugmentedImageDatabase_addImage(
                IntPtr session,
                IntPtr augmented_image_database,
                string image_name,
                IntPtr image_grayscale_pixels,
                int image_width_in_pixels,
                int image_height_in_pixels,
                int image_stride_in_pixels,
                ref int out_index);
#pragma warning restore 626
        }
    }
}
