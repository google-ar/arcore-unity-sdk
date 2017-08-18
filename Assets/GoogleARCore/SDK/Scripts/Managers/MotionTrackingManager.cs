//-----------------------------------------------------------------------
// <copyright file="MotionTrackingManager.cs" company="Google">
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
    using UnityEngine;
    using UnityTango = GoogleAR.UnityNative;
    using System.Collections.Generic;

    /// <summary>
    /// A manager for motion tracking.
    /// </summary>
    public class MotionTrackingManager
    {
        private Pose m_latestPose = new Pose();

        private bool m_latestPoseValid;

        private int m_latestPoseFrame = 0;

        private double m_latestTimestamp = 0.0f;

        private Queue<ApiPoseData> m_poseQueue = new Queue<ApiPoseData>();

        private object m_poseQueueLock = new object();

        public bool IsLocalized { get; private set; }

        public MotionTrackingManager()
        {
            ApiCoordinateFramePair[] framePairs = new ApiCoordinateFramePair[]
            {
                new ApiCoordinateFramePair()
                {
                    baseFrame = ApiCoordinateFrameType.AreaDescription,
                    targetFrame = ApiCoordinateFrameType.StartOfService,
                }
            };

            TangoClientApi.ConnectOnPoseAvailable(framePairs, m_poseQueue, m_poseQueueLock);
        }

        public void EarlyUpdate()
        {
            lock (m_poseQueueLock)
            {
                while (m_poseQueue.Count > 0)
                {
                    ApiPoseData pose = m_poseQueue.Dequeue();
                    if (pose.framePair.baseFrame == ApiCoordinateFrameType.AreaDescription &&
                        pose.framePair.targetFrame == ApiCoordinateFrameType.StartOfService)
                    {
                        IsLocalized = pose.statusCode == ApiPoseStatusType.Valid;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to get the most recent valid pose produced by ARCore motion tracking.  This will be frame
        /// consistent with Unity.
        /// </summary>
        /// <param name="pose">The latest pose.</param>
        /// <param name="timestamp">The timestamp of the latest pose.</param>
        /// <returns><c>true</c> if the ARCore could produce a latest pose, otherwise <c>false</c>.</returns>
        public bool TryGetLatestPose(out Pose pose, out double timestamp)
        {
            // Maintain frame consistency.
            if (Time.frameCount == m_latestPoseFrame)
            {
                pose = m_latestPose;
                timestamp = m_latestTimestamp;
                return m_latestPoseValid;
            }

            m_latestPoseFrame = Time.frameCount;

            UnityTango.PoseData poseData;
            bool getPoseSuccess = UnityTango.InputTracking.TryGetPoseAtTime(
                out poseData, UnityTango.CoordinateFrame.StartOfService, UnityTango.CoordinateFrame.CameraColor, 0.0f);
            if (!getPoseSuccess || (poseData.statusCode != UnityTango.PoseStatus.Valid))
            {
                m_latestPose = pose = new Pose();
                timestamp = 0.0d;
                m_latestPoseValid = false;
                return false;
            }

            // Update latest pose with new valid pose.
            m_latestPose = pose = new Pose(poseData.position, poseData.rotation);
            m_latestTimestamp = timestamp = poseData.timestamp;
            m_latestPoseValid = true;
            return true;
        }
    }
}
