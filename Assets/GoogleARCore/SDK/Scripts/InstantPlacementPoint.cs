//-----------------------------------------------------------------------
// <copyright file="InstantPlacementPoint.cs" company="Google LLC">
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
    using System;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Trackable Instant Placement point returned by <see
    /// cref="Frame.RaycastInstantPlacement(float, float, float, TrackableHit)"/>.
    ///
    /// If ARCore has an accurate 3D pose for the <c><see cref="InstantPlacementPoint"/></c>
    /// returned by
    /// <c><see cref="Frame.RaycastInstantPlacement(float, float, float, TrackableHit)"/></c>
    /// it will start with tracking method
    /// <c><see cref="InstantPlacementPointTrackingMethod"/></c>.<c>FullTracking</c>.
    /// Otherwise, it will start with tracking method
    /// <c>InstantPlacementPointTrackingMethod.ScreenspaceWithApproximateDistance</c>, and will
    /// transition to <c><see cref="InstantPlacementPointTrackingMethod"/></c>.<c>FullTracking</c>
    /// once ARCore has an accurate 3D pose. Once the tracking method is
    /// <c><see cref="InstantPlacementPointTrackingMethod"/></c>.<c>FullTracking</c> it will not
    /// revert to <c>InstantPlacementPointTrackingMethod.ScreenspaceWithApproximateDistance</c>.
    ///
    /// When the tracking method changes from
    /// <c>InstantPlacementPointTrackingMethod.ScreenspaceWithApproximateDistance</c> in one frame
    /// to <c><see cref="InstantPlacementPointTrackingMethod"/></c>.<c>FullTracking</c> in the next
    /// frame, the pose will jump from its initial location based on the provided approximate
    /// distance to a new location at an accurate distance.
    ///
    /// This instantaneous change in pose will change the apparent scale of any objects that are
    /// anchored to the <c><see cref="InstantPlacementPoint"/></c>. That is, an object will suddenly
    /// appear larger or smaller than it was in the previous frame.
    ///
    /// To avoid the visual jump due to the sudden change in apparent object scale, use the
    /// following procedure:
    /// 1. Keep track of the pose and tracking method of the
    ///    <c><see cref="InstantPlacementPoint"/></c> in each frame.
    /// 2. Wait for the tracking method to change to
    ///    <c><see cref="InstantPlacementPointTrackingMethod"/></c>.<c>FullTracking</c>.
    /// 3. Use the pose from the previous frame and the pose in the current frame to determine the
    ///    object's distance to the device in both frames.
    /// 4. Calculate the apparent change in scale due to the change in distance from the camera.
    /// 5. Adjust the scale of the object to counteract the perceived change in scale, so that
    ///    visually the object does not appear to change in size.
    /// 6. Optionally, smoothly adjust the scale of the object back to its original value over
    ///    several frames.
    /// </summary>
    public class InstantPlacementPoint : Trackable
    {
        /// <summary>
        /// Construct an InstantPlacementPoint from a native handle.
        /// </summary>
        /// <param name="nativeHandle">A handle to the native ARCore API Trackable.</param>
        /// <param name="nativeApi">The ARCore native api.</param>
        internal InstantPlacementPoint(IntPtr nativeHandle, NativeSession nativeApi) :
            base(nativeHandle, nativeApi)
        {
        }

        /// <summary>
        /// Gets the pose of the InstantPlacementPoint.
        /// </summary>
        public Pose Pose
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "Pose:: Trying to access a session that has already been destroyed.");
                    return new Pose();
                }

                return _nativeSession.PointApi.GetInstantPlacementPointPose(_trackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the tracking method of the InstantPlacementPoint.
        /// </summary>
        public InstantPlacementPointTrackingMethod TrackingMethod
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "InstantPlacementPointTrackingMethod:: " +
                        "Trying to access a session that has already been destroyed.");
                    return InstantPlacementPointTrackingMethod.NotTracking;
                }

                return _nativeSession.PointApi.GetInstantPlacementPointTrackingMethod(
                    _trackableNativeHandle);
            }
        }
    }
}
