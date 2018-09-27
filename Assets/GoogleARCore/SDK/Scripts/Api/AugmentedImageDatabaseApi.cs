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
        public AugmentedImageDatabaseApi(NativeSession nativeSession)
        {
        }

        public IntPtr CreateArPrestoAugmentedImageDatabase(byte[] rawData)
        {
            IntPtr outDatabaseHandle = IntPtr.Zero;
            GCHandle handle = new GCHandle();
            IntPtr rawDataHandle = IntPtr.Zero;
            Int32 length = 0;

            if (rawData != null)
            {
                handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
                rawDataHandle = handle.AddrOfPinnedObject();
                length = rawData.Length;
            }

            ExternApi.ArPrestoAugmentedImageDatabase_create(rawDataHandle, length, ref outDatabaseHandle);

            if (handle.IsAllocated)
            {
                handle.Free();
            }

            return outDatabaseHandle;
        }

        public Int32 AddImageAtRuntime(IntPtr databaseHandle, string name, Texture2D image, float width)
        {
            Int32 outIndex = -1;
            GCHandle grayscaleBytesHandle = _ConvertTextureToGrayscaleBytes(image);
            if (grayscaleBytesHandle.AddrOfPinnedObject() == IntPtr.Zero)
            {
                return -1;
            }

            ApiArStatus status =
                ExternApi.ArPrestoAugmentedImageDatabase_addImageAtRuntime(databaseHandle, name,
                    grayscaleBytesHandle.AddrOfPinnedObject(), image.width, image.height, image.width,
                    width, ref outIndex);

            if (grayscaleBytesHandle.IsAllocated)
            {
                grayscaleBytesHandle.Free();
            }

            if (status != ApiArStatus.Success)
            {
                Debug.LogWarningFormat("Failed to add aumented image at runtime with status {0}", status);
                return -1;
            }

            return outIndex;
        }

        private GCHandle _ConvertTextureToGrayscaleBytes(Texture2D image)
        {
            byte[] grayscaleBytes = null;

            if (image.format == TextureFormat.RGB24 || image.format == TextureFormat.RGBA32)
            {
                Color[] pixels = image.GetPixels();
                grayscaleBytes = new byte[pixels.Length];
                for (int i = 0; i < image.height; i++)
                {
                    for (int j = 0; j < image.width; j++)
                    {
                        grayscaleBytes[(i * image.width) + j] = 
                            (byte)(((0.213 * pixels[((image.height - 1 - i) * image.width) + j].r)
                            + (0.715 * pixels[((image.height - 1 - i) * image.width) + j].g)
                            + (0.072 * pixels[((image.height - 1 - i) * image.width) + j].b)) * 255);
                    }
                }
            }
            else
            {
                Debug.LogError("Unsupported texture format " + image.format);
            }

            return GCHandle.Alloc(grayscaleBytes, GCHandleType.Pinned);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPrestoAugmentedImageDatabase_create(IntPtr rawBytes,
                Int64 rawBytesSize, ref IntPtr outAugmentedImageDatabaseHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPrestoAugmentedImageDatabase_destroy(IntPtr augmentedImageDatabaseHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern ApiArStatus ArPrestoAugmentedImageDatabase_addImageAtRuntime(
                IntPtr augmentedImageDatabaseHandle, string imageName, IntPtr imageBytes, Int32 imageWidth,
                Int32 imageHeight, Int32 imageStride, float imageWidthInMeters, ref Int32 outIndex);
#pragma warning restore 626
        }
    }
}
