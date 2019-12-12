//-----------------------------------------------------------------------
// <copyright file="LostTrackingReason.cs" company="Google">
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
    /// Describes the reason for a loss in motion tracking.
    /// </summary>
    public enum LostTrackingReason
    {
        /// <summary>
        /// Motion tracking is working properly.
        /// </summary>
        None = 0,

        /// <summary>
        /// An internal error is causing motion tracking to fail.
        /// </summary>
        BadState = 1,

        /// <summary>
        /// The camera feed being too dark is causing motion tracking to fail.
        /// </summary>
        InsufficientLight = 2,

        /// <summary>
        /// Excessive movement of the device camera is causing motion tracking
        /// to fail.
        /// </summary>
        ExcessiveMotion = 3,

        /// <summary>
        /// A lack of visually distinct environmental features in the camera feed
        /// is causing motion tracking to fail.
        /// </summary>
        InsufficientFeatures = 4,

        /// <summary>
        /// Motion tracking paused because the camera is in use by another application.
        /// Tracking will resume once this app regains priority, or once all apps with
        /// higher priority have stopped using the camera. Prior to ARCore SDK 1.13,
        /// <see cref="LostTrackingReason"/>.<c>None</c> is returned in this case instead.
        /// </summary>
        CameraUnavailable = 5,
    }
}
