//-----------------------------------------------------------------------
// <copyright file="Trackable.cs" company="Google LLC">
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
        internal IntPtr _trackableNativeHandle = IntPtr.Zero;

        /// <summary>
        /// The native api for ARCore.
        /// </summary>
        internal NativeSession _nativeSession;

        internal Trackable()
        {
        }

        internal Trackable(IntPtr trackableNativeHandle, NativeSession nativeSession)
        {
            _trackableNativeHandle = trackableNativeHandle;
            _nativeSession = nativeSession;
        }

        ~Trackable()
        {
            _nativeSession.TrackableApi.Release(_trackableNativeHandle);
        }

        /// <summary>
        /// Gets the tracking state of for the Trackable in the current frame.
        /// </summary>
        /// <returns>The tracking state of for the Trackable in the current frame.</returns>
        public virtual TrackingState TrackingState
        {
            [SuppressMemoryAllocationError(
                IsWarning = true, Reason = "Requires further investigation.")]
            get
            {
                if (IsSessionDestroyed())
                {
                    // Trackables from another session are considered stopped.
                    return TrackingState.Stopped;
                }

                return _nativeSession.TrackableApi.GetTrackingState(_trackableNativeHandle);
            }
        }

        /// <summary>
        /// Creates an Anchor at the given <c>Pose</c> that is attached to the Trackable where
        /// semantics of the attachment relationship are defined by the subcass of Trackable (e.g.,
        /// DetectedPlane).   Note that the relative offset between the Pose of multiple Anchors
        /// attached to the same Trackable may change over time as ARCore refines its understanding
        /// of the world.
        /// </summary>
        /// <param name="pose">The Pose of the location to create the anchor.</param>
        /// <returns>An Anchor attached to the Trackable at <c>Pose</c>.</returns>
        [SuppressMemoryAllocationError(Reason = "Could allocate a new Anchor object")]
        public virtual Anchor CreateAnchor(Pose pose)
        {
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "CreateAnchor:: Trying to access a session that has already been destroyed.");
                return null;
            }

            IntPtr anchorHandle;
            if (!_nativeSession.TrackableApi.AcquireNewAnchor(
                _trackableNativeHandle, pose, out anchorHandle))
            {
                Debug.Log("Failed to create anchor on trackable.");
                return null;
            }

            return Anchor.Factory(_nativeSession, anchorHandle);
        }

        /// <summary>
        /// Gets all anchors attached to the Trackable.
        /// </summary>
        /// <param name="anchors">A list of anchors to be filled by the method.</param>
        [SuppressMemoryAllocationError(Reason = "List could be resized.")]
        public virtual void GetAllAnchors(List<Anchor> anchors)
        {
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetAllAnchors:: Trying to access a session that has already been destroyed.");
                anchors.Clear();
                return;
            }

            _nativeSession.TrackableApi.GetAnchors(_trackableNativeHandle, anchors);
        }

        /// <summary>
        /// Tells if the session was destroyed.
        /// </summary>
        /// <returns><c>true</c> if the session this Trackable belongs to was destroyed,
        /// <c>false</c> otherwise.</returns>
        protected bool IsSessionDestroyed()
        {
            return _nativeSession.IsDestroyed;
        }
    }
}
