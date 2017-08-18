//-----------------------------------------------------------------------
// <copyright file="CoordinateFrameTypeExtensions.cs" company="Google">
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
    using System.Collections.Generic;
    using UnityTango = GoogleAR.UnityNative;

    /// <summary>
    /// Extension methods for the tango coordinate frame enumerations.
    /// </summary>
    public static class CoordinateFrameTypeExtensions
    {
        /// <summary>
        /// Conversion from unity CoordinateFrameType to api CoordinateFrameType.
        /// </summary>
        private static readonly Dictionary<UnityTango.CoordinateFrame, ApiCoordinateFrameType> UNITY_TO_API
            = new Dictionary<UnityTango.CoordinateFrame, ApiCoordinateFrameType>()
            {
                { UnityTango.CoordinateFrame.GlobalWGS84, ApiCoordinateFrameType.GlobalWGS84 },
                { UnityTango.CoordinateFrame.AreaDescription, ApiCoordinateFrameType.AreaDescription },
                { UnityTango.CoordinateFrame.StartOfService, ApiCoordinateFrameType.StartOfService },
                { UnityTango.CoordinateFrame.PreviousDevicePose, ApiCoordinateFrameType.PreviousDevicePose },
                { UnityTango.CoordinateFrame.Device, ApiCoordinateFrameType.Device },
                { UnityTango.CoordinateFrame.IMU, ApiCoordinateFrameType.IMU },
                { UnityTango.CoordinateFrame.Display, ApiCoordinateFrameType.Display },
                { UnityTango.CoordinateFrame.CameraColor, ApiCoordinateFrameType.CameraColor },
                { UnityTango.CoordinateFrame.CameraDepth, ApiCoordinateFrameType.CameraDepth },
                { UnityTango.CoordinateFrame.CameraFisheye, ApiCoordinateFrameType.CameraFisheye },
                { UnityTango.CoordinateFrame.Invalid, ApiCoordinateFrameType.Invalid },
                { UnityTango.CoordinateFrame.MaxCoordinateFrameType, ApiCoordinateFrameType.MaxCoordinateFrame },
            };

        /// <summary>
        /// Conversion from api CoordinateFrameType to unity CoordinateFrameType.
        /// </summary>
        private static readonly Dictionary<ApiCoordinateFrameType, UnityTango.CoordinateFrame> API_TO_UNITY
            = new Dictionary<ApiCoordinateFrameType, UnityTango.CoordinateFrame>()
            {
                { ApiCoordinateFrameType.GlobalWGS84, UnityTango.CoordinateFrame.GlobalWGS84 },
                { ApiCoordinateFrameType.AreaDescription, UnityTango.CoordinateFrame.AreaDescription },
                { ApiCoordinateFrameType.StartOfService, UnityTango.CoordinateFrame.StartOfService },
                { ApiCoordinateFrameType.PreviousDevicePose, UnityTango.CoordinateFrame.PreviousDevicePose },
                { ApiCoordinateFrameType.Device, UnityTango.CoordinateFrame.Device },
                { ApiCoordinateFrameType.IMU, UnityTango.CoordinateFrame.IMU },
                { ApiCoordinateFrameType.Display, UnityTango.CoordinateFrame.Display },
                { ApiCoordinateFrameType.CameraColor, UnityTango.CoordinateFrame.CameraColor },
                { ApiCoordinateFrameType.CameraDepth, UnityTango.CoordinateFrame.CameraDepth },
                { ApiCoordinateFrameType.CameraFisheye, UnityTango.CoordinateFrame.CameraFisheye },
                { ApiCoordinateFrameType.Invalid, UnityTango.CoordinateFrame.Invalid },
                { ApiCoordinateFrameType.MaxCoordinateFrame, UnityTango.CoordinateFrame.MaxCoordinateFrameType },
            };

        /// <summary>
        /// Converts an ApiCoordinateFrameType to it's Unity equivalent.
        /// </summary>
        public static UnityTango.CoordinateFrame ToUnityType(this ApiCoordinateFrameType apiFrame)
        {
            return API_TO_UNITY[apiFrame];
        }

        /// <summary>
        /// Converts a unity CoordinateFrameType to it's ApiCoordinateFrameType equivalent.
        /// </summary>
        public static ApiCoordinateFrameType ToApiType(this UnityTango.CoordinateFrame unityFrame)
        {
            return UNITY_TO_API[unityFrame];
        }
    }
}