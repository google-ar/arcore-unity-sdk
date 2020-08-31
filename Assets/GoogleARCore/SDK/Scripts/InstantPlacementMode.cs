//-----------------------------------------------------------------------
// <copyright file="InstantPlacementMode.cs" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    /// Indicates whether Instant Placement is enabled or disabled.
    /// The default value is <see cref="InstantPlacementMode"/>.<c>Disabled</c>.
    /// </summary>
    public enum InstantPlacementMode
    {
        /// <summary>
        /// Instant Placement mode is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Enable Instant Placement. If the hit test is successful,
        /// it will return a single <see cref="InstantPlacementPoint"/> with the +Y pointing upward,
        /// against gravity. Otherwise, returns an empty result set.
        ///
        /// This mode is currently intended to be used with hit tests against
        /// horizontal surfaces.
        ///
        /// Hit tests may also be performed against surfaces with any orientation, however:
        /// <list>
        ///   <item> The resulting Instant Placement point will always have a pose
        ///    with +Y pointing upward, against gravity.</item>
        ///   <item> No guarantees are made with respect to orientation of +X and +Z.
        ///    Specifically, a hit test against a vertical surface, such as a wall,
        ///    will not result in a pose that's in any way aligned to the plane of the
        ///    wall, other than +Y being up, against gravity.</item>
        ///   <item> The <see cref="InstantPlacementPoint"/>'s tracking method may never become
        ///    <see cref="InstantPlacementPointTrackingMethod"/>.<c>FullTracking</c> or may take
        ///    a long time to reach this state.The tracking method remains
        ///    <see cref="InstantPlacementPointTrackingMethod"/>.
        ///    <c>ScreenspaceWithApproximateDistance</c>
        ///    until a (tiny) horizontal plane is fitted at the point of the hit test.</item>
        /// </list>
        /// </summary>
        LocalYUp = 2,
    }
}
