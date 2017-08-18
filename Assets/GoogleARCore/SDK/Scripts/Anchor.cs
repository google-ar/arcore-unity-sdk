//-----------------------------------------------------------------------
// <copyright file="Anchor.cs" company="Google">
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
    using UnityEngine;
    /// @cond EXCLUDE_FROM_DOXYGEN
    using UnityTango = GoogleAR.UnityNative;
    /// @endcond

    /// <summary>
    /// Anchors a gameobject to a position/rotation in the Unity world relative to ARCore's understanding of the
    /// physical world; Created using <c>Session.CreateAnchor(Vector3, Quaternion)</c>.
    /// ARCore may periodically perform operations that affect the mapping of Unity world coordinates to the
    /// physical world; an example of such being drift correction. Anchors allow GameObjects to retain their
    /// physical world location when these operations occur. If ARCore is unable to track an anchor for any reason,
    /// the attached GameObject will be set inactive until tracking resumes.
    /// </summary>
    public class Anchor : MonoBehaviour
    {
        private Matrix4x4 m_poseTAnchor;

        private double m_creationTimestamp;

        private ScreenOrientation m_creationScreenOrientation;

        /// <summary>
        /// Gets a unique identifier for the anchor.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the tracking state of the anchor.
        /// </summary>
        public AnchorTrackingState TrackingState { get; private set; }

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Instantiates a new GameObject with an Anchor component attached.
        /// </summary>
        /// <param name="position">The unity world position to instantiate the anchor GameObject.</param>
        /// <param name="rotation">The unity world rotation to instantiate the anchor GameObject.</param>
        /// <param name="updateTracking">A callback to update the anchor's position based on latest
        /// estimation of the physical world pose from ARCore.</param>
        /// <param name="updateTrackingState">A callback to update the private tracking state of the anchor.</param>
        /// <returns>A GameObject with an Anchor component attached.</returns>
        public static Anchor InstantiateAnchor(Vector3 position, Quaternion rotation,
            out Action<double> updateTracking, out Action<AnchorTrackingState> updateTrackingState)
        {
            Anchor anchor = (new GameObject()).AddComponent<Anchor>();
            anchor.gameObject.name = "Anchor";
            anchor.Id = Guid.NewGuid().ToString();
            anchor.TrackingState = AnchorTrackingState.Tracking;
            anchor.transform.position = position;
            anchor.transform.rotation = rotation;
            var cameraPose = Frame.Pose;
            anchor.m_poseTAnchor = Matrix4x4.TRS(cameraPose.position, cameraPose.rotation, Vector3.one).inverse *
                Matrix4x4.TRS(position, rotation, Vector3.one);
            anchor.m_creationTimestamp = Frame.Timestamp;
            anchor.m_creationScreenOrientation = Screen.orientation;
            updateTracking = anchor._UpdateTracking;
            updateTrackingState = anchor._UpdateTrackingState;
            return anchor;
        }
        /// @endcond

        private void _UpdateTracking(double earliestTimestamp)
        {
            if (m_creationTimestamp < earliestTimestamp)
            {
                return;
            }

            UnityTango.PoseData poseData;
            bool getPoseSuccess = UnityTango.InputTracking.TryGetPoseAtTime(
                out poseData, UnityTango.CoordinateFrame.StartOfService, UnityTango.CoordinateFrame.CameraColor,
                m_creationTimestamp, m_creationScreenOrientation);
            if (!getPoseSuccess || poseData.statusCode != UnityTango.PoseStatus.Valid)
            {
                return;
            }

            var unityWorldAnchor = Matrix4x4.TRS(poseData.position, poseData.rotation, Vector3.one) * m_poseTAnchor;
            transform.position = unityWorldAnchor.GetColumn(3);
            transform.rotation = Quaternion.LookRotation(unityWorldAnchor.GetColumn(2), unityWorldAnchor.GetColumn(1));
        }

        private void _UpdateTrackingState(AnchorTrackingState trackingState)
        {
            TrackingState = trackingState;
            gameObject.SetActive(TrackingState == AnchorTrackingState.Tracking);
        }
    }
}
