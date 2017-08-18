//-----------------------------------------------------------------------
// <copyright file="CurrentFrame.cs" company="Google">
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
    using UnityEngine;
    using GoogleARCoreInternal;

    /// <summary>
    /// Provides a snapshot of the state of ARCore at a specific timestamp
    /// associated with the current frame.  Frame holds information
    /// about ARCore's state including tracking status, the pose of the camera
    /// relative to the world, estimated lighting parameters, and information
    /// on updates to objects (like Planes or Point Clouds) that ARCore is
    /// tracking.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// Gets the tracking state of the frame.
        /// </summary>
        public static FrameTrackingState TrackingState
        {
            get
            {
                if (SessionManager.ConnectionState != SessionConnectionState.Connected)
                {
                    return FrameTrackingState.TrackingNotInitialized;
                }

                Pose junkPose;
                double junkTimestamp;
                bool isTracking = SessionManager.Instance.MotionTrackingManager.TryGetLatestPose(out junkPose,
                    out junkTimestamp);

                if (isTracking)
                {
                    return FrameTrackingState.Tracking;
                }
                else
                {
                    return FrameTrackingState.LostTracking;
                }
            }
        }

        /// <summary>
        /// Gets the pose of the device's camera in the world coordinate frame
        /// at the time of capture of the current frame.
        /// </summary>
        public static Pose Pose
        {
            get
            {
                Pose pose;
                double timestamp;
                SessionManager.Instance.MotionTrackingManager.TryGetLatestPose(out pose, out timestamp);
                return pose;
            }
        }

        /// <summary>
        /// Gets the current light estimate for this frame.
        /// </summary>
        public static LightEstimate LightEstimate
        {
            get
            {
                return SessionManager.Instance.LightEstimateManager.GetLatestLightEstimate();
            }
        }

        /// <summary>
        /// Get the ARCore device's point cloud for the current ARCore frame.
        /// </summary>
        public static PointCloud PointCloud
        {
            get
            {
                return SessionManager.Instance.PointCloudManager.GetLatestPointCloud();
            }
        }

        /// <summary>
        /// Gets the hardware timestamp of the current ARCore frame.
        /// </summary>
        public static double Timestamp
        {
            get
            {
                Pose pose;
                double timestamp;
                SessionManager.Instance.MotionTrackingManager.TryGetLatestPose(out pose, out timestamp);
                return timestamp;
            }
        }

        /// <summary>
        /// Gets planes newly detected in the current ARCore frame.
        /// </summary>
        /// <param name="newPlanes">A list reference that to be filled with planes detected in the current frame.
        /// </param>
        public static void GetNewPlanes(ref List<TrackedPlane> newPlanes)
        {
            SessionManager.Instance.TrackedPlaneManager.GetNewPlanes(ref newPlanes);
        }

        /// <summary>
        /// Gets all TrackedPlane objects that have been detected in the session.
        /// </summary>
        /// <param name="planes">A list of TrackedPlane to be filled by the method call.</param>
        public static void GetAllPlanes(ref List<TrackedPlane> planes)
        {
            SessionManager.Instance.TrackedPlaneManager.GetAllPlanes(ref planes);
        }
    }
}
