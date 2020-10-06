//-----------------------------------------------------------------------
// <copyright file="AugmentedImageSrc.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
            this._format = image.format;
            this._pixels = image.GetPixels();
            this._height = image.height;
            this._width = image.width;
        }

        internal TextureFormat _format { get; private set; }

        internal Color[] _pixels { get; private set; }

        internal int _height { get; private set; }

        internal int _width { get; private set; }
    }
}
