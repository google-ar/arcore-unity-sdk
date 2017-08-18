//-----------------------------------------------------------------------
// <copyright file="Session.cs" company="Google">
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
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Represents an ARCore session, which is an attachment point from the app
    /// to the ARCore service. Holds information about the global state for
    /// ARCore, manages tracking of Anchors and Planes, and performs hit tests
    /// against objects ARCore is tracking in the world.
    /// </summary>
    public static class Session
    {
        public static SessionConnectionState ConnectionState
        {
            get
            {
                return SessionManager.ConnectionState;
            }
        }

        /// <summary>
        /// Creates an anchor in the current ARCore session.
        ///
        /// Anchors a gameobject to a position/rotation in the Unity world relative to ARCore's understanding of the
        /// physical world.  ARCore may periodically perform operations that affect the mapping of Unity world coordinates
        /// to the physical world; an example of such being drift correction.  Anchors allow GameObjects to retain their
        /// physical world location when these operations occur.
        /// </summary>
        /// <param name="position">The position to anchor.</param>
        /// <param name="rotation">The rotation to anchor.</param>
        /// <returns>A newly created anchor tracking <c>position</c> and <c>rotation</c> if successful, otherwise
        /// <c>null</c>.</returns>
        public static Anchor CreateAnchor(Vector3 position, Quaternion rotation)
        {
            return  SessionManager.Instance.AnchorManager.CreateAnchor(position, rotation);
        }

        /// <summary>
        /// Performs a raycast against physical objects being tracked by ARCore.
        /// </summary>
        /// <param name="ray">The starting point and direction of the ray.</param>
        /// <param name="filter">A filter bitmask where each <c>TrackableHitFlag</c> which is set represents a category
        /// of raycast hits the method call should consider valid.</param>
        /// <param name="hitResult">A <c>TrackableHit</c> that will be set if the raycast is successful.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        public static bool Raycast(Ray ray, TrackableHitFlag filter, out TrackableHit hitResult)
        {
            return  SessionManager.Instance.RaycastManager.Raycast(ray, filter, out hitResult);
        }
    }
}
