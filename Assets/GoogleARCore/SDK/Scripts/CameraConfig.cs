//-----------------------------------------------------------------------
// <copyright file="CameraConfig.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Type of hardware depth sensor, such as a time-of-flight sensor (or ToF sensor), usage for a
    /// camera config.
    /// </summary>
    [Flags]
    [SuppressMessage("UnityRules.UnityStyleRules", "US1200:FlagsEnumsMustBePlural",
                     Justification = "Usage is plural.")]
    public enum CameraConfigDepthSensorUsage
    {
        /// <summary>
        /// Indicates that a hardware depth sensor, such as a time-of-flight sensor (or ToF sensor),
        /// must be present on the device, and the hardware depth sensor will be used by ARCore.
        /// Not supported on all devices.
        /// </summary>
        RequireAndUse = 0x0001,

        /// <summary>
        /// Indicates that ARCore will not attempt to use a hardware depth sensor, such as a
        /// time-of-flight sensor (or ToF sensor), even if it is present.
        /// Most commonly used to filter camera configurations when the app requires
        /// exclusive access to the hardware depth sensor outside of ARCore, for example to
        /// support 3D mesh reconstruction. Available on all ARCore supported devices.
        /// </summary>
        DoNotUse = 0x0002,
    }

    /// <summary>
    /// Type of stereo camera usage for a camera config.
    /// </summary>
    [Flags]
    [SuppressMessage("UnityRules.UnityStyleRules", "US1200:FlagsEnumsMustBePlural",
                     Justification = "Usage is plural.")]
    public enum CameraConfigStereoCameraUsage
    {
        /// <summary>
        /// Indicates that a stereo camera must be present on the device and the stereo camera will
        /// be used by ARCore.
        /// Not supported on all devices.
        /// </summary>
        RequireAndUse = 0x0001,

        /// <summary>
        /// Indicates that ARCore will not attempt to use a stereo camera, even if one is present.
        /// Can be used to limit power consumption.
        /// Available on all ARCore supported devices.
        /// </summary>
        DoNotUse = 0x0002,
    }

    /// <summary>
    /// A configuration for ARCore accessing the device's camera sensor.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines",
                     Justification = "Bypass source check.")]
    public struct CameraConfig
    {
        internal CameraConfig(
            DeviceCameraDirection facingDirection,
            Vector2 imageSize,
            Vector2 textureSize,
            int minFPS,
            int maxFPS,
            CameraConfigStereoCameraUsage stereoCamera,
            CameraConfigDepthSensorUsage depthSensor) : this()
        {
            FacingDirection = facingDirection;
            ImageSize = imageSize;
            TextureSize = textureSize;
            MinFPS = minFPS;
            MaxFPS = maxFPS;
            DepthSensorUsage = depthSensor;
            StereoCameraUsage = stereoCamera;
        }

        /// <summary>
        /// Gets the camera facing direction for this camera config.
        /// </summary>
        public DeviceCameraDirection FacingDirection { get; private set; }

        /// <summary>
        /// Gets the dimensions of the CPU-accessible image bytes for this camera config.
        /// </summary>
        public Vector2 ImageSize { get; private set; }

        /// <summary>
        /// Gets the dimensions of the GPU-accessible external texture for this camera config.
        /// </summary>
        public Vector2 TextureSize { get; private set; }

        /// <summary>
        /// Gets minimum target camera capture frame rate for this camera config.
        /// </summary>
        public int MinFPS { get; private set; }

        /// <summary>
        /// Gets maximum target camera capture frame rate for this camera config.
        /// </summary>
        public int MaxFPS { get; private set; }

        /// <summary>
        /// Gets the stereo camera usage for this camera config.
        /// </summary>
        public CameraConfigStereoCameraUsage StereoCameraUsage { get; private set; }

        /// <summary>
        /// Gets the hardware depth sensor, such as a time-of-flight sensor (or ToF sensor), usage
        /// for this camera config.
        /// </summary>
        public CameraConfigDepthSensorUsage DepthSensorUsage { get; private set; }
    }
}
