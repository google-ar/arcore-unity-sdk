//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using UnityEngine;
    using UnityTango = GoogleAR.UnityNative;

    /// <summary>
    /// Constants used across the ARCore SDK.
    /// </summary>
    public struct Constants
    {
        /// <summary>
        /// The default marker size.
        /// </summary>
        public const double DEFAULT_MARKER_SIZE = 0.1397;

        /// <summary>
        /// Matrix that transforms from Start of Service to the Unity World.
        /// </summary>
        public static readonly Matrix4x4 UNITY_WORLD_T_START_SERVICE = new Matrix4x4
        {
            m00 = 1.0f, m01 = 0.0f, m02 = 0.0f, m03 = 0.0f,
            m10 = 0.0f, m11 = 0.0f, m12 = 1.0f, m13 = 0.0f,
            m20 = 0.0f, m21 = 1.0f, m22 = 0.0f, m23 = 0.0f,
            m30 = 0.0f, m31 = 0.0f, m32 = 0.0f, m33 = 1.0f
        };

        /// <summary>
        /// A coordinate frame pair from the start of service frame to the color frame.
        /// </summary>
        public static readonly UnityTango.CoordinateFramePair START_SERVICE_T_COLOR_FRAME_PAIR =
            new UnityTango.CoordinateFramePair
            {
                baseFrame = UnityTango.CoordinateFrame.StartOfService,
                targetFrame = UnityTango.CoordinateFrame.CameraColor,
            };

        public static readonly UnityTango.CoordinateFramePair START_SERVICE_T_DEPTH_FRAME_PAIR =
            new UnityTango.CoordinateFramePair
            {
                baseFrame = UnityTango.CoordinateFrame.StartOfService,
                targetFrame = UnityTango.CoordinateFrame.CameraDepth,
            };
    }
}
