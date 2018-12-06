//-----------------------------------------------------------------------
// <copyright file="ApiTrackingStateXPExtensions.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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

namespace GoogleARCoreInternal.CrossPlatform
{
    using GoogleARCore;
    using GoogleARCore.CrossPlatform;

    internal static class ApiTrackingStateXPExtensions
    {
        public static XPTrackingState ToXPTrackingState(this TrackingState apiTrackingState)
        {
            switch (apiTrackingState)
            {
                case TrackingState.Tracking:
                    return XPTrackingState.Tracking;
                case TrackingState.Paused:
                    return XPTrackingState.Paused;
                case TrackingState.Stopped:
                    return XPTrackingState.Stopped;
                default:
                    return XPTrackingState.Stopped;
            }
        }
    }
}
