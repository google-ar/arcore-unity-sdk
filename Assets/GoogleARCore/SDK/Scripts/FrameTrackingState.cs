//-----------------------------------------------------------------------
// <copyright file="FrameTrackingState.cs" company="Google">
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
    /// <summary>
    /// The tracking state for an ARCore frame.
    /// </summary>
    public enum FrameTrackingState
    {
        /// <summary>
        /// The motion tracking system is not initialized.  The frame is invalid.
        /// </summary>
        TrackingNotInitialized = 0,

        /// <summary>
        /// The motion tracking system has lost tracking.  This can happen for various reasons including poor
        /// lighting conditions or a lack of visually distinct features in the camera frame.  ARCore will attempt
        /// to re-establish tracking, but the frame is invalid.
        /// </summary>
        LostTracking,

        /// <summary>
        /// The motion tracking system is tracking and the frame is valid.
        /// </summary>
        Tracking,
    }
}