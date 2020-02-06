//-----------------------------------------------------------------------
// <copyright file="AugmentedImageSrc.cs" company="Google">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// This class caches information and content about a Texture2D for use
    /// by ARCore on threads other than the main thread.
    /// </summary>
    public class AugmentedImageSrc
    {
        /// <summary>
        /// Copies information about a source Texture2D into the instance
        /// for use later by ARCore.
        /// </summary>
        /// <param name="image">Source Texture2D image.</param>
        public AugmentedImageSrc(Texture2D image)
        {
            this.Format = image.format;
            this.Pixels = image.GetPixels();
            this.Height = image.height;
            this.Width = image.width;
        }

        internal TextureFormat Format { get; private set; }

        internal Color[] Pixels { get; private set; }

        internal int Height { get; private set; }

        internal int Width { get; private set; }
    }
}
