//-----------------------------------------------------------------------
// <copyright file="TextureReaderApi.cs" company="Google">
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
namespace GoogleARCore.Examples.ComputerVision
{
    using System;
    using UnityEngine;

#if UNITY_IOS
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    /// <summary>
    /// API that provides CPU access to GPU texture.
    /// </summary>
    public class TextureReaderApi
    {
        /// <summary>
        /// Image format type.
        /// </summary>
        public enum ImageFormatType
        {
            /// <summary>
            /// Color image pixel format. Four bytes per pixel, in the order of R, G, B, and A.
            /// </summary>
            ImageFormatColor = 0,

            /// <summary>
            /// Grayscale image pixel format. One byte per pixel.
            /// </summary>
            ImageFormatGrayscale = 1
        }

        /// <summary>
        /// Creates the texture reader instance.
        /// </summary>
        /// <param name="format">Format of the output image pixel. Can be either eImageFormat_RGBA or eImageFormat_I8.</param>
        /// <param name="width">Width of the output image, in pixels.</param>
        /// <param name="height">Height of the output image, in pixels.</param>
        /// <param name="keepAspectRatio">Indicate whether or not to keep aspect ratio. If true, the output image may be cropped
        /// if the image aspect ratio is different from the texture aspect ratio. If false, the output image covers the entire
        /// texture scope and no cropping is applied.</param>
        public void Create(ImageFormatType format, int width, int height, bool keepAspectRatio)
        {
            ExternApi.TextureReader_create((int)format, width, height, keepAspectRatio);
        }

        /// <summary>
        /// Destroys the texture reader instance and release internal resources.
        /// </summary>
        public void Destroy()
        {
            ExternApi.TextureReader_destroy();
        }

        /// <summary>
        /// Submits a texture reading request to GPU driver. The result of this request will be available in the next
        /// frame through AcquireFrame().
        /// </summary>
        /// <param name="textureId">The GLES texture id of the input camera texture. It has to be created as OES texture.</param>
        /// <param name="textureWidth">Width of the texture, in pixels.</param>
        /// <param name="textureHeight">Height of the texture, in pixels.</param>
        /// <returns>The frame buffer index, which can be used to retrieve the frame later through AcquireFrame(). -1 if the submission fails.</returns>
        public int SubmitFrame(int textureId, int textureWidth, int textureHeight)
        {
            int bufferIndex = ExternApi.TextureReader_submitFrame(textureId, textureWidth, textureHeight);
            GL.InvalidateState();
            return bufferIndex;
        }

        /// <summary>
        /// Acquires the output image pixels from a previous reading request.
        /// </summary>
        /// <param name="bufferIndex">The buffer index required by previous call to SubmitFrame().</param>
        /// <param name="bufferSize">The size of the output image pixel buffer, in bytes.</param>
        /// <returns>The pointer to the raw buffer of the output image. null if fails.</returns>
        public IntPtr AcquireFrame(int bufferIndex, ref int bufferSize)
        {
            IntPtr pixelBuffer = ExternApi.TextureReader_acquireFrame(bufferIndex, ref bufferSize);
            return pixelBuffer;
        }

        /// <summary>
        /// Releases a previously used frame buffer.
        /// </summary>
        /// <param name="bufferIndex">The buffer index required by previous call to SubmitFrame().</param>
        public void ReleaseFrame(int bufferIndex)
        {
            ExternApi.TextureReader_releaseFrame(bufferIndex);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            public const string ARCoreCameraUtilityAPI = "arcore_camera_utility";

            [AndroidImport(ARCoreCameraUtilityAPI)]
            public static extern void TextureReader_create(int format, int width, int height, bool keepAspectRatio);

            [AndroidImport(ARCoreCameraUtilityAPI)]
            public static extern void TextureReader_destroy();

            [AndroidImport(ARCoreCameraUtilityAPI)]
            public static extern int TextureReader_submitFrame(int textureId, int textureWidth, int textureHeight);

            [AndroidImport(ARCoreCameraUtilityAPI)]
            public static extern IntPtr TextureReader_acquireFrame(int bufferIndex, ref int bufferSize);

            [AndroidImport(ARCoreCameraUtilityAPI)]
            public static extern void TextureReader_releaseFrame(int bufferIndex);
#pragma warning restore 626
        }
    }
}