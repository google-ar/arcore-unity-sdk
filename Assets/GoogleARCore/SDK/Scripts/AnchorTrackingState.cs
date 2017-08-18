//-----------------------------------------------------------------------
// <copyright file="AnchorTrackingState.cs" company="Google">
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
    /// The tracking state for an anchor.
    /// </summary>
    public enum AnchorTrackingState
    {
        /// <summary>
        /// ARCore has stopped tracking this Anchor and will never resume tracking it. This typically happens
        /// because the anchor was created when the device's tracking state was diminished and then became further
        /// diminished or lost.
        /// </summary>
        StoppedTracking,

        /// <summary>
        /// The anchor is not currently being tracked but tracking may resume in the future. This can happen
        /// if device tracking is lost or if the user enters a new space. When in this state the anchor GameObject
        /// is disabled since the transform could be very inaccurate.
        /// </summary>
        LostTracking,

        /// <summary>
        /// The Anchor is being tracked and its transform is current.
        /// </summary>
        Tracking,
    }
}