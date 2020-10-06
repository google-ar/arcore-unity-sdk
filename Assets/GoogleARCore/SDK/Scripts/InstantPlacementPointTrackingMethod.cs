//-----------------------------------------------------------------------
// <copyright file="InstantPlacementPointTrackingMethod.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    /// Tracking methods for <see cref="InstantPlacementPoint"/>.
    /// </summary>
    public enum InstantPlacementPointTrackingMethod
    {
        /// <summary>
        /// The <see cref="InstantPlacementPoint"/> is not currently being tracked. The <see
        /// cref="TrackingState"/> is <see cref="TrackingState"/>.<c>Paused</c> or <see
        /// cref="TrackingState"/>.<c>Stopped</c>.
        /// </summary>
        NotTracking = 0x00,

        /// <summary>
        /// The <see cref="InstantPlacementPoint"/> is currently being tracked in screen space and
        /// the pose returned by <see cref="InstantPlacementPoint"/>.<c>Pose</c> is being estimated
        /// using the approximate distance provided to <see
        /// cref="Frame.RaycastInstantPlacement(float, float, float, TrackableHit)"/>.
        ///
        /// ARCore concurrently tracks at most 20 <see cref="InstantPlacementPoint"/>s that are
        /// <c>ScreenspaceWithApproximateDistance</c>.
        /// As additional <see cref="InstantPlacementPoint"/>s with
        /// <c>ScreenspaceWithApproximateDistance</c> are created, the oldest points will
        /// become permanently cref="TrackingState"/>.<c>Stopped</c> in order to maintain the
        /// the maximum number of concurrently tracked points.
        /// </summary>
        ScreenspaceWithApproximateDistance = 0x01,

        /// <summary>
        /// The <see cref="InstantPlacementPoint"/> is being tracked normally and
        /// <see cref="InstantPlacementPoint"/>.Pose is fully determined by ARCore.
        ///
        /// ARCore does not limit the number of <see cref="InstantPlacementPoint"/>s with
        /// <c>FullTracking</c> that are being tracked concurrently.
        /// </summary>
        FullTracking = 0x02,
    }
}
