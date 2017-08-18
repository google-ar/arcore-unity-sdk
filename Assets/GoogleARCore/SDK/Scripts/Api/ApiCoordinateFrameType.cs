//-----------------------------------------------------------------------
// <copyright file="ApiCoordinateFrameType.cs" company="Google">
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
    /// <summary>
    /// Tango coordinate frame enumerations.
    /// </summary>
    public enum ApiCoordinateFrameType
    {
        /// <summary>
        /// Coordinate system for the entire Earth.
        /// See WGS84: [http://en.wikipedia.org/wiki/World_Geodetic_System].
        /// </summary>
        GlobalWGS84 = 0,

        /// <summary>
        /// Origin within a saved area description.
        /// </summary>
        AreaDescription = 1,

        /// <summary>
        /// Origin when the device started tracking.
        /// </summary>
        StartOfService = 2,

        /// <summary>
        /// Immediately previous device pose.
        /// </summary>
        PreviousDevicePose = 3,

        /// <summary>
        /// Device coordinate frame.
        /// </summary>
        Device = 4,

        /// <summary>
        /// Inertial Measurement Unit.
        /// </summary>
        IMU = 5,

        /// <summary>
        /// Display coordinate frame.
        /// </summary>
        Display = 6,

        /// <summary>
        /// Color camera.
        /// </summary>
        CameraColor = 7,

        /// <summary>
        /// Depth camera.
        /// </summary>
        CameraDepth = 8,

        /// <summary>
        /// Fisheye camera.
        /// </summary>
        CameraFisheye = 9,

        // Should UUID Frame be here?

        /// <summary>
        /// An invalid frame.
        /// </summary>
        Invalid = 10,

        /// <summary>
        /// Maximum allowed.
        /// </summary>
        MaxCoordinateFrame = 11,
    }
}
