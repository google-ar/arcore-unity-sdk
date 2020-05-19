//-----------------------------------------------------------------------
// <copyright file="CameraIntrinsics.cs" company="Google LLC">
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

namespace GoogleARCore
{
    using UnityEngine;

    /// <summary>
    /// A struct to provide camera intrinsics in ARCore.
    /// </summary>
    public struct CameraIntrinsics
    {
        /// <summary>
        /// The focal length in pixels.
        /// Focal length is conventionally represented in pixels. For a detailed
        /// explanation, please see
        /// http://ksimek.github.io/2013/08/13/intrinsic.
        /// Pixels-to-meters conversion can use SENSOR_INFO_PHYSICAL_SIZE and
        /// SENSOR_INFO_PIXEL_ARRAY_SIZE in the Android CameraCharacteristics API.
        /// </summary>
        public Vector2 FocalLength;

        /// <summary>
        /// The principal point in pixels.
        /// </summary>
        public Vector2 PrincipalPoint;

        /// <summary>
        /// The intrinsic's width and height in pixels.
        /// </summary>
        public Vector2Int ImageDimensions;

        internal CameraIntrinsics(
            Vector2 focalLength, Vector2 principalPoint, Vector2Int imageDimensions)
        {
            FocalLength = focalLength;
            PrincipalPoint = principalPoint;
            ImageDimensions = imageDimensions;
        }
    }
}
