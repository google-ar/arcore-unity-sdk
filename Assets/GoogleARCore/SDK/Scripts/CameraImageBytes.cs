//-----------------------------------------------------------------------
// <copyright file="CameraImageBytes.cs" company="Google LLC">
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

namespace GoogleARCore
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// An ARCore camera image with its data accessible from the CPU in YUV-420-888 format.
    /// </summary>
    public struct CameraImageBytes : IDisposable
    {
        private IntPtr _imageHandle;

        internal CameraImageBytes(IntPtr imageHandle) : this()
        {
            _imageHandle = imageHandle;
            if (_imageHandle != IntPtr.Zero)
            {
                int width, height;
                IntPtr y, u, v;
                int yRowStride, uvPixelStride, uvRowStride;
                LifecycleManager.Instance.NativeSession.ImageApi.GetImageBuffer(
                    _imageHandle, out width, out height, out y, out u, out v, out yRowStride,
                    out uvPixelStride, out uvRowStride);

                IsAvailable = true;
                Width = width;
                Height = height;
                Y = y;
                U = u;
                V = v;
                YRowStride = yRowStride;
                UVPixelStride = uvPixelStride;
                UVRowStride = uvRowStride;
            }
            else
            {
                IsAvailable = false;
                Width = Height = 0;
                Y = U = V = IntPtr.Zero;
                YRowStride = UVPixelStride = UVRowStride = 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the image bytes are available. The struct should not be
        /// accessed if this value is <c>false</c>.
        /// </summary>
        public bool IsAvailable { get; private set; }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets a pointer to the Y buffer with a pixel stride of 1 and a row stride of
        /// <c>YRowStride</c>.
        /// </summary>
        public IntPtr Y { get; private set; }

        /// <summary>
        /// Gets a pointer to the U buffer with a pixel stride of <c>UVPixelStride</c> and a row
        /// stride of <c>UVRowStride</c>.
        /// </summary>
        public IntPtr U { get; private set; }

        /// <summary>
        /// Gets a pointer to the V buffer with a pixel stride of <c>UVPixelStride</c> and a row
        /// stride of <c>UVRowStride</c>.
        /// </summary>
        public IntPtr V { get; private set; }

        /// <summary>
        /// Gets the row stride of the Y plane.
        /// </summary>
        public int YRowStride { get; private set; }

        /// <summary>
        /// Gets the pixel stride of the U and V planes.
        /// </summary>
        public int UVPixelStride { get; private set; }

        /// <summary>
        /// Gets the row stride of the U and V planes.
        /// </summary>
        public int UVRowStride { get; private set; }

        /// <summary>
        /// Releases the camera image and associated resources, and signifies the developer will no
        /// longer access those resources.
        /// </summary>
        public void Release()
        {
            if (_imageHandle != IntPtr.Zero)
            {
                LifecycleManager.Instance.NativeSession.ImageApi.Release(_imageHandle);
                _imageHandle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Calls release as part of IDisposable pattern supporting 'using' statements.
        /// </summary>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Requires further investigation.")]
        public void Dispose()
        {
            Release();
        }
    }
}
