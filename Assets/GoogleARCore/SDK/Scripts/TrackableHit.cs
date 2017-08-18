//-----------------------------------------------------------------------
// <copyright file="TrackableHit.cs" company="Google">
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
    using UnityEngine;

    /// <summary>
    /// Contains information about a raycast hit against a physical object tracked by ARCore.
    /// </summary>
    public struct TrackableHit
    {
        /// <summary>
        /// The location where the raycast hit the object in Unity world coordinates.
        /// </summary>
        public Vector3 Point { get; private set; }

        /// <summary>
        /// The normal of the hit.
        /// </summary>
        public Vector3 Normal { get; private set; }

        /// <summary>
        /// The distance from the origin of the ray to the hit.
        /// </summary>
        public float Distance { get; private set; }

        /// <summary>
        /// A bitmask where set TrackableHitFlag flags correspond to categories of objects the hit belongs to.
        /// </summary>
        public TrackableHitFlag Flags { get; private set; }

        /// <summary>
        /// Gets the TrackedPlane intersected by the Raycast if one exists, otherwise gets <c>null</c>.
        /// </summary>
        public TrackedPlane Plane { get; private set; }

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Constructs a TrackableHit.
        /// </summary>
        public TrackableHit(Vector3 point, Vector3 normal, float distance, TrackableHitFlag flags, TrackedPlane plane)
        {
            Point = point;
            Normal = normal;
            Distance = distance;
            Flags = flags;
            Plane = plane;
        }
        /// @endcond
    }
}
