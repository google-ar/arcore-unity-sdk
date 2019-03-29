//-----------------------------------------------------------------------
// <copyright file="ApiTrackingFailureReasonExtensions.cs" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
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
    using GoogleARCore;

    internal static class ApiTrackingFailureReasonExtensions
    {
        public static LostTrackingReason ToLostTrackingReason(
            this ApiTrackingFailureReason apiTrackingFailureReason)
        {
            switch (apiTrackingFailureReason)
            {
                case ApiTrackingFailureReason.None:
                    return LostTrackingReason.None;
                case ApiTrackingFailureReason.BadState:
                    return LostTrackingReason.BadState;
                case ApiTrackingFailureReason.InsufficientLight:
                    return LostTrackingReason.InsufficientLight;
                case ApiTrackingFailureReason.ExcessiveMotion:
                    return LostTrackingReason.ExcessiveMotion;
                case ApiTrackingFailureReason.InsufficientFeatures:
                    return LostTrackingReason.InsufficientFeatures;
                default:
                    return LostTrackingReason.None;
            }
        }
    }
}
