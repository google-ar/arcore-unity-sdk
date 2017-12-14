//-----------------------------------------------------------------------
// <copyright file="PointCloudPoint.cs" company="Google">
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

namespace GoogleARCore
{
    using UnityEngine;

    /// <summary>
    /// A point in a point cloud.
    /// </summary>
    public struct PointCloudPoint
    {
        /// <summary>
        /// The x-position of the point.
        /// </summary>
        public float X;

        /// <summary>
        /// The y-position of the point.
        /// </summary>
        public float Y;

        /// <summary>
        /// The z-position of the point.
        /// </summary>
        public float Z;

        /// <summary>
        /// A normalized confidence value for the point.
        /// </summary>
        public float Confidence;

        /// <summary>
        /// Constructs a new PointCloudPoint.
        /// </summary>
        /// <param name="x">The x-position of the point.</param>
        /// <param name="y">The y-position of the point.</param>
        /// <param name="z">The z-position of the point.</param>
        /// <param name="confidence">The confidence of the point.</param>
        public PointCloudPoint(float x, float y, float z, float confidence)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Confidence = confidence;
        }

        /// <summary>
        /// Implicitly converts a PointCloudPoint to a Vector4.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        public static implicit operator Vector4(PointCloudPoint point)
        {
            return new Vector4(point.X, point.Y, point.Z, point.Confidence);
        }

        /// <summary>
        /// Implicitly converts a Vector4 to a PointCloudPoint.
        /// </summary>
        /// <param name="vectorPoint">The Vector3 to convert.</param>
        public static implicit operator PointCloudPoint(Vector4 vectorPoint)
        {
            return new PointCloudPoint(vectorPoint.x, vectorPoint.y, vectorPoint.z, vectorPoint.w);
        }
    }
}
