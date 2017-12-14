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
    using System;
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
        //// @cond EXCLUDE_FROM_DOXYGEN

        /// <summary>
        /// Gets the manager for the sttatic session.
        /// </summary>
        public static SessionManager SessionManager { get; private set; }

        //// @endcond

        /// <summary>
        /// Gets current connection state.
        ///
        /// Members of the Session class (apart from ConnectionState itself) are only considered valid for access when
        /// Session.ConnectionState == SessionConnectionState.Connected.  Access to members of Session outside of this
        /// state is considered undefined and may use stale data or throw an exception.
        /// </summary>
        public static SessionConnectionState ConnectionState
        {
            get
            {
                if (SessionManager == null)
                {
                    return SessionConnectionState.Uninitialized;
                }

                return SessionManager.ConnectionState;
            }
        }

        /// <summary>
        /// Creates a new anchor at a world pose. As ARCore updates its understading of the space, it will update the
        /// virtual pose of the of the anchor to attempt to keep the anchor in the same real world location.
        /// </summary>
        /// <param name="pose">The Unity world pose where the anchor is to be creates.</param>
        /// <returns>The newly created anchor or null.</returns>
        public static Anchor CreateWorldAnchor(Pose pose)
        {
            if (SessionManager == null)
            {
                return null;
            }

            return SessionManager.CreateWorldAnchor(pose);
        }

        /// <summary>
        /// Performs a raycast against physical objects being tracked by ARCore.
        /// Output the closest hit from the camera.
        /// Note that the Unity's screen coordinate (0, 0)
        /// starts from bottom left.
        /// </summary>
        /// <param name="x">Horizontal touch position in Unity's screen coordiante.</param>
        /// <param name="y">Vertical touch position in Unity's screen coordiante.</param>
        /// <param name="filter">A filter bitmask where each {@link TrackableHitFlag} which is set represents a category
        /// of raycast hits the method call should consider valid.</param>
        /// <param name="hitResult">A {@link TrackableHit} that will be set if the raycast is successful.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        public static bool Raycast(float x, float y, TrackableHitFlags filter,
            out TrackableHit hitResult)
        {
            if (SessionManager == null)
            {
                hitResult = new TrackableHit();
                return false;
            }

            return SessionManager.FrameManager.Raycast(x, y, filter, out hitResult);
        }

        /// <summary>
        /// Performs a raycast against physical objects being tracked by ARCore.
        /// Output all hits from the camera.
        /// Note that the Unity's screen coordinate (0, 0)
        /// starts from bottom left.
        /// </summary>
        /// <param name="x">Horizontal touch position in Unity's screen coordiante.</param>
        /// <param name="y">Vertical touch position in Unity's screen coordiante.</param>
        /// <param name="filter">A filter bitmask where each {@link TrackableHitFlag} which is set represents a category
        /// of raycast hits the method call should consider valid.</param>
        /// <param name="hitResults">A list of {@link TrackableHit} that will be set if the raycast is successful.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        public static bool RaycastAll(float x, float y, TrackableHitFlags filter, List<TrackableHit> hitResults)
        {
            if (SessionManager == null)
            {
                hitResults.Clear();
                return false;
            }

            return SessionManager.FrameManager.RaycastAll(x, Screen.height - y, filter, hitResults);
        }

        //// @cond EXCLUDE_FROM_DOXYGEN

        /// <summary>
        /// Initialized the static session.
        /// </summary>
        /// <param name="sessionManager">The manaager for the static session.</param>
        public static void Initialize(SessionManager sessionManager)
        {
            if (Session.SessionManager != null)
            {
                Debug.LogWarning("Cleaning up old session that was not destroyed.");
                Session.SessionManager.Destroy();
            }

            Session.SessionManager = sessionManager;
        }

        /// <summary>
        /// Destroys the context of the static session class.
        /// </summary>
        public static void Destroy()
        {
            if (SessionManager != null)
            {
                SessionManager.Destroy();
                SessionManager = null;
            }
        }

        //// @endcond
    }
}
