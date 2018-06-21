//-----------------------------------------------------------------------
// <copyright file="Trackable.cs" company="Google">
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
    /// An object ARCore is tracking in the real world.
    /// </summary>
    public abstract class Trackable
    {
        /// <summary>
        /// A native handle for the ARCore trackable.
        /// </summary>
        internal IntPtr m_TrackableNativeHandle = IntPtr.Zero;

        /// <summary>
        /// The native api for ARCore.
        /// </summary>
        internal NativeSession m_NativeSession;

        private bool m_IsSessionDestroyed = false;

        internal Trackable()
        {
        }

        internal Trackable(IntPtr trackableNativeHandle, NativeSession nativeSession)
        {
            m_TrackableNativeHandle = trackableNativeHandle;
            m_NativeSession = nativeSession;
        }

        ~Trackable()
        {
            m_NativeSession.TrackableApi.Release(m_TrackableNativeHandle);
        }

        /// <summary>
        /// Gets the tracking state of for the Trackable in the current frame.
        /// </summary>
        /// <returns>The tracking state of for the Trackable in the current frame.</returns>
        public virtual TrackingState TrackingState
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    // Trackables from another session are considered stopped.
                    return TrackingState.Stopped;
                }

                return m_NativeSession.TrackableApi.GetTrackingState(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Creates an Anchor at the given <c>Pose</c> that is attached to the Trackable where semantics of the
        /// attachment relationship are defined by the subcass of Trackable (e.g. DetectedPlane).   Note that the
        /// relative offset between the Pose of multiple Anchors attached to the same Trackable may change
        /// over time as ARCore refines its understanding of the world.
        /// </summary>
        /// <param name="pose">The Pose of the location to create the anchor.</param>
        /// <returns>An Anchor attached to the Trackable at <c>Pose</c>.</returns>
        public virtual Anchor CreateAnchor(Pose pose)
        {
            if (_IsSessionDestroyed())
            {
                Debug.LogError("CreateAnchor:: Trying to access a session that has already been destroyed.");
                return null;
            }

            IntPtr anchorHandle;
            if (!m_NativeSession.TrackableApi.AcquireNewAnchor(m_TrackableNativeHandle, pose, out anchorHandle))
            {
                Debug.Log("Failed to create anchor on trackable.");
                return null;
            }

            return Anchor.Factory(m_NativeSession, anchorHandle);
        }

        /// <summary>
        /// Gets all anchors attached to the Trackable.
        /// </summary>
        /// <param name="anchors">A list of anchors to be filled by the method.</param>
        public virtual void GetAllAnchors(List<Anchor> anchors)
        {
            if (_IsSessionDestroyed())
            {
                Debug.LogError("GetAllAnchors:: Trying to access a session that has already been destroyed.");
                anchors.Clear();
                return;
            }

            m_NativeSession.TrackableApi.GetAnchors(m_TrackableNativeHandle, anchors);
        }

        /// <summary>
        /// Tells if the session was destroyed.
        /// </summary>
        /// <returns><c>true</c> if the session this Trackable belong to was destroyed,
        /// <c>false</c> otherwise.</returns>
        protected bool _IsSessionDestroyed()
        {
            if (!m_IsSessionDestroyed)
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession != m_NativeSession)
                {
                    m_IsSessionDestroyed = true;
                }
            }

            return m_IsSessionDestroyed;
        }
    }
}
