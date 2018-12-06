//-----------------------------------------------------------------------
// <copyright file="CameraConfig.cs" company="Google">
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
// limitations under the License.v
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
    /// A configuration for ARCore accessing the device's camera sensor.
    /// </summary>
    public struct CameraConfig
    {
        internal CameraConfig(Vector2 imageSize, Vector2 textureSize) : this()
        {
            ImageSize = imageSize;
            TextureSize = textureSize;
        }

        /// <summary>
        /// Gets the dimensions of the CPU-accessible image bytes for the camera configuration.
        /// </summary>
        public Vector2 ImageSize { get; private set; }

        /// <summary>
        /// Gets the dimensions of the GPU-accessible external texture for the camera configuration.
        /// </summary>
        public Vector2 TextureSize { get; private set; }
    }
}
