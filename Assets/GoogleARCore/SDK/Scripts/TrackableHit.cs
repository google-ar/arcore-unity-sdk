//-----------------------------------------------------------------------
// <copyright file="TrackableHit.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using UnityEngine;

    /// <summary>
    /// Contains information about a raycast hit against a physical object tracked by ARCore.
    /// </summary>
    public struct TrackableHit
    {
        /// <summary>
        /// Constructs a TrackableHit.
        /// </summary>
        /// <param name="pose">Hit's pose.</param>
        /// <param name="distance">Hit's distance from the origin of the ray to the hit.</param>
        /// <param name="flags">Type of the hit.</param>
        /// <param name="trackable">Trackable object of the hit.</param>
        internal TrackableHit(
            Pose pose, float distance, TrackableHitFlags flags, Trackable trackable) : this()
        {
            Pose = pose;
            Distance = distance;
            Flags = flags;
            Trackable = trackable;
        }

        /// <summary>
        /// Gets the pose where the raycast hit the object in Unity world coordinates.
        /// </summary>
        public Pose Pose { get; private set; }

        /// <summary>
        /// Gets the distance from the origin of the ray to the hit.
        /// </summary>
        public float Distance { get; private set; }

        /// <summary>
        /// Gets a bitmask where set TrackableHitFlag flags correspond to categories of objects
        /// the hit belongs to.
        /// </summary>
        public TrackableHitFlags Flags { get; private set; }

        /// <summary>
        /// Gets the hit's trackable object.
        /// </summary>
        public Trackable Trackable { get; private set; }
    }
}
