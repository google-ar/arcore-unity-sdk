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
                IntPtr y, u, v;
                y = u = v = IntPtr.Zero;
                int bufferLengthIgnore = 0;
                const int Y_PLANE = 0;
                const int U_PLANE = 1;
                const int V_PLANE = 2;

                IsAvailable = true;

                Width = LifecycleManager.Instance.NativeSession.ImageApi.GetWidth(imageHandle);
                Height = LifecycleManager.Instance.NativeSession.ImageApi.GetHeight(imageHandle);

                LifecycleManager.Instance.NativeSession.ImageApi.GetPlaneData(imageHandle, Y_PLANE,
                    ref y, ref bufferLengthIgnore);
                LifecycleManager.Instance.NativeSession.ImageApi.GetPlaneData(imageHandle, U_PLANE,
                    ref u, ref bufferLengthIgnore);
                LifecycleManager.Instance.NativeSession.ImageApi.GetPlaneData(imageHandle, V_PLANE,
                    ref v, ref bufferLengthIgnore);

                YRowStride = LifecycleManager.Instance.NativeSession.ImageApi.GetPlaneRowStride(
                    imageHandle, Y_PLANE);
                UVPixelStride = LifecycleManager.Instance.NativeSession.ImageApi
                    .GetPlanePixelStride(imageHandle, U_PLANE);
                UVRowStride = LifecycleManager.Instance.NativeSession.ImageApi.GetPlaneRowStride(
                    imageHandle, U_PLANE);

                Y = y;
                U = u;
                V = v;
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
        [SuppressMemoryAllocationError(IsWarning = true,
            Reason = "Requires further investigation.")]
        public void Dispose()
        {
            Release();
        }
    }
}
