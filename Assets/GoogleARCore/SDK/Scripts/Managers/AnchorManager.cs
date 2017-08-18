//-----------------------------------------------------------------------
// <copyright file="AnchorManager.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityTango = UnityEngine.XR.Tango;

    public class AnchorManager
    {
        public enum AnchorManagerTrackingState
        {
            Tracking,
            TrackingNotLocalized,
            TrackingLost,
            TrackingStopped,
        }

        private List<AnchorRecord> m_anchors = new List<AnchorRecord>();

        private List<AnchorRecord> m_nonLocalizedAnchors = new List<AnchorRecord>();

        public AnchorManagerTrackingState m_trackingState = AnchorManagerTrackingState.TrackingStopped;

        /// <summary>
        /// ARCore drift correction may shift the virtual coordinate frame as motion tracking estimates are refined;
        /// this method creates an anchor that attempts to represent <c>position</c> and <c>rotation</c> relative to the
        /// "real world" coordinate compensating for drift correction updates.
        /// </summary>
        /// <param name="position">The position to anchor.</param>
        /// <param name="rotation">The rotation to anchor.</param>
        /// <returns>A newly created anchor tracking <c>position</c> and <c>rotation</c> if successful, otherwise
        /// <c>null</c>.</returns>
        public Anchor CreateAnchor(Vector3 position, Quaternion rotation)
        {
            Action<double> updateTracking;
            Action<AnchorTrackingState> updateTrackingState;
            var newAnchor = Anchor.InstantiateAnchor(position, rotation, out updateTracking, out updateTrackingState);
            var newAnchorRecord = new AnchorRecord(newAnchor, updateTracking, updateTrackingState);
            if (SessionManager.Instance.MotionTrackingManager.IsLocalized)
            {
                m_anchors.Add(newAnchorRecord);
            }
            else
            {
                m_nonLocalizedAnchors.Add(newAnchorRecord);
            }

            return newAnchor;
        }

        /// <summary>
        /// Updates the tracking of all anchors.
        /// </summary>
        public void EarlyUpdate()
        {
            _SetNewTrackingState(Frame.TrackingState, SessionManager.Instance.MotionTrackingManager.IsLocalized);

            const string POSE_HISTORY_EVENT_KEY = "EXPERIMENTAL_PoseHistoryChanged";
            double earliestTimestamp = double.MaxValue;
            for (int i = 0; i < SessionManager.Instance.TangoEvents.Count; i++)
            {
                var tangoEvent = SessionManager.Instance.TangoEvents[i];
                if (tangoEvent.key == POSE_HISTORY_EVENT_KEY && double.Parse(tangoEvent.value) < earliestTimestamp)
                {
                    earliestTimestamp = double.Parse(tangoEvent.value);
                }
            }

            // Update the pose of anchors.
            if (earliestTimestamp < double.MaxValue)
            {
                for (int i = 0; i < m_anchors.Count; i++)
                {
                    m_anchors[i].m_updateTracking(earliestTimestamp);
                }

                for (int i = 0;  i < m_nonLocalizedAnchors.Count; i++)
                {
                    m_nonLocalizedAnchors[i].m_updateTracking(earliestTimestamp);
                }
            }
        }

        public void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                _UpdateAnchorTrackingState(AnchorTrackingState.StoppedTracking, m_anchors);
                _UpdateAnchorTrackingState(AnchorTrackingState.StoppedTracking, m_nonLocalizedAnchors);
                m_anchors.Clear();
                m_nonLocalizedAnchors.Clear();
            }
        }

        private void _SetNewTrackingState(FrameTrackingState frameTrackingState, bool localized)
        {
            AnchorManagerTrackingState oldTrackingState = m_trackingState;

            if (frameTrackingState == FrameTrackingState.LostTracking)
            {
                m_trackingState = AnchorManagerTrackingState.TrackingLost;
            }
            else if (frameTrackingState == FrameTrackingState.TrackingNotInitialized)
            {
                m_trackingState = AnchorManagerTrackingState.TrackingStopped;
            }
            else if (!localized)
            {
                m_trackingState = AnchorManagerTrackingState.TrackingNotLocalized;
            }
            else
            {
                m_trackingState = AnchorManagerTrackingState.Tracking;
            }

            _HandleTrackingStateChange(oldTrackingState, m_trackingState);
        }

        private void _HandleTrackingStateChange(AnchorManagerTrackingState oldState,
            AnchorManagerTrackingState newState)
        {
            if (oldState == newState)
            {
                // There is no state change to process.
                return;
            }
            else if (newState == AnchorManagerTrackingState.Tracking)
            {
                // Tracking has been (re-)established, any non-localized anchors that were not lost become localized and
                // all localized anchors become tracked.
                m_anchors.AddRange(m_nonLocalizedAnchors);
                m_nonLocalizedAnchors.Clear();
                _UpdateAnchorTrackingState(AnchorTrackingState.Tracking, m_anchors);
            }
            else if (newState == AnchorManagerTrackingState.TrackingNotLocalized)
            {
                // The device is tracking but not localized; meaning the device is on a separate map from localized
                // anchors. Thus, localized anchors are lost.
                _UpdateAnchorTrackingState(AnchorTrackingState.LostTracking, m_anchors);
            }
            else if (newState == AnchorManagerTrackingState.TrackingLost)
            {
                // Tracking is now lost and localized anchors should move to not tracking.  Any non-localized anchors
                // are now lost forever (stopped).
                _UpdateAnchorTrackingState(AnchorTrackingState.LostTracking, m_anchors);
                _UpdateAnchorTrackingState(AnchorTrackingState.StoppedTracking, m_nonLocalizedAnchors);
                m_nonLocalizedAnchors.Clear();
            }
        }

        private void _UpdateAnchorTrackingState(AnchorTrackingState trackingState, List<AnchorRecord> anchorRecords)
        {
            for (int i = 0; i < anchorRecords.Count; i++)
            {
                anchorRecords[i].m_updateTrackingState(trackingState);
            }
        }

        private struct AnchorRecord
        {
            public Anchor m_anchor;

            public Action<double> m_updateTracking;

            public Action<AnchorTrackingState> m_updateTrackingState;

            public AnchorRecord(Anchor anchor, Action<double> updateTracking,
                Action<AnchorTrackingState> updateTrackingState)
            {
                m_anchor = anchor;
                m_updateTracking = updateTracking;
                m_updateTrackingState = updateTrackingState;
            }
        }
    }
}
