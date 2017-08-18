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
    /// Extension methods for the tango PoseStatus enumerations.
    /// </summary>
    public static class ApiPoseStatusTypeExtensions
    {
        /// <summary>
        /// Conversion from unity PoseStatus to ApiPoseStatusType.
        /// </summary>
        private static readonly Dictionary<UnityTango.PoseStatus, ApiPoseStatusType> UNITY_TO_API
            = new Dictionary<UnityTango.PoseStatus, ApiPoseStatusType>()
            {
                { UnityTango.PoseStatus.Initializing, ApiPoseStatusType.Initializing },
                { UnityTango.PoseStatus.Valid, ApiPoseStatusType.Valid },
                { UnityTango.PoseStatus.Invalid, ApiPoseStatusType.Invalid },
                { UnityTango.PoseStatus.Unknown, ApiPoseStatusType.Unknown },
            };

        /// <summary>
        /// Conversion from ApiPoseStatusType to unity PoseStatus.
        /// </summary>
        private static readonly Dictionary<ApiPoseStatusType, UnityTango.PoseStatus> API_TO_UNITY
            = new Dictionary<ApiPoseStatusType, UnityTango.PoseStatus>()
            {
                { ApiPoseStatusType.Initializing, UnityTango.PoseStatus.Initializing},
                { ApiPoseStatusType.Valid, UnityTango.PoseStatus.Valid },
                { ApiPoseStatusType.Invalid, UnityTango.PoseStatus.Invalid },
                { ApiPoseStatusType.Unknown, UnityTango.PoseStatus.Unknown },
            };

        /// <summary>
        /// Converts an ApiPoseStatusType to it's unity PoseStatus equivalent.
        /// </summary>
        public static UnityTango.PoseStatus ToUnityType(this ApiPoseStatusType apiFrame)
        {
            return API_TO_UNITY[apiFrame];
        }

        /// <summary>
        /// Converts a unity PoseStatus to it's ApiPoseStatusType equivalent.
        /// </summary>
        public static ApiPoseStatusType ToApiType(this UnityTango.PoseStatus unityFrame)
        {
            return UNITY_TO_API[unityFrame];
        }
    }
}