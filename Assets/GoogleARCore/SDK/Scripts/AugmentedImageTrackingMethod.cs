//-----------------------------------------------------------------------
// <copyright file="AugmentedImageTrackingMethod.cs" company="Google">
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
namespace GoogleARCore
{
    /// <summary>
    /// Indicates whether an image is being tracked using the camera image,
    /// or is being tracked based on its last known pose.
    /// </summary>
    public enum AugmentedImageTrackingMethod
    {
        /// <summary>
        /// The Augmented Image is not currently being tracked.
        /// This state indicates that the image's <see cref="TrackingState"/> is
        /// <see cref="TrackingState"/>.<c>Paused</c> or
        /// <see cref="TrackingState"/>.<c>Stopped</c>.
        /// </summary>
        NotTracking = 0,

        /// <summary>
        /// The Augmented Image is currently being tracked using the camera image.
        /// This state can only occur when the image's <see cref="TrackingState"/> is
        /// <see cref="TrackingState"/>.<c>Tracking</c>.
        /// </summary>
        FullTracking = 1,

        /// <summary>
        /// The Augmented Image is currently being tracked based on its last known pose,
        /// because it can no longer be tracked using the camera image.
        /// This state can only occur when the image's <see cref="TrackingState"/> is
        /// <see cref="TrackingState"/>.<c>Tracking</c>.
        /// </summary>
        LastKnownPose = 2,
    }
}
