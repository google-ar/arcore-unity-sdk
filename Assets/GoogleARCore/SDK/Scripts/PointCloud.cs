//-----------------------------------------------------------------------
// <copyright file="PointCloud.cs" company="Google">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCoreInternal;
    using UnityEngine;
    /// @cond EXCLUDE_FROM_DOXYGEN
    using UnityTango = GoogleAR.UnityNative;
    /// @endcond

    /// <summary>
    /// A point cloud is a set of observed 3D points and associated confidence
    /// values that correspond to the feature points tracked by ARCore.
    /// </summary>
    public struct PointCloud
    {
        /// <summary>
        /// The point cloud in Depth Coordinates being wrapped.
        /// </summary>
        private UnityTango.PointCloudData m_rawPointCloud;

        /// <summary>
        /// The matrix converting between unity world and depth camera coordinate frames.
        /// </summary>
        private Matrix4x4 m_unityWorldTDepthCamera;

        private const string TANGO_CLIENT_API_DLL = "tango_client_api2";

        /// <summary>
        /// Constructor for a new PointCloud wrapping <c>rawPointCloud</c>.
        /// </summary>
        /// <param name="rawPointCloud">The raw point cloud to wrap.  If <c>rawPointCloud</c> is null, the
        /// resulting PointCloud will have <c>IsValid</c> set to false.</param>
        public PointCloud(UnityTango.PointCloudData? rawPointCloud)
        {
            if (rawPointCloud == null)
            {
                IsValid = false;
                m_rawPointCloud = new UnityTango.PointCloudData();
                Pose = new Pose();
                m_unityWorldTDepthCamera = Matrix4x4.identity;
                return;
            }

            IsValid = true;
            m_rawPointCloud = rawPointCloud.Value;
            Pose = new Pose();
            m_unityWorldTDepthCamera = Matrix4x4.identity;

            ApiPoseData apiPoseData = new ApiPoseData();
            if (ExternApi.TangoService_getPoseAtTime(m_rawPointCloud.timestamp,
                new ApiCoordinateFramePair(Constants.START_SERVICE_T_DEPTH_FRAME_PAIR),
                ref apiPoseData) == ApiServiceErrorStatus.Success)
            {
                Matrix4x4 ss_T_depth = Matrix4x4.TRS(apiPoseData.translation.ToVector3(),
                    apiPoseData.orientation.ToQuaternion(), Vector3.one);
                m_unityWorldTDepthCamera = Constants.UNITY_WORLD_T_START_SERVICE * ss_T_depth;
                Vector3 translation = m_unityWorldTDepthCamera.GetColumn(3);
                Quaternion rotation = Quaternion.LookRotation(m_unityWorldTDepthCamera.GetColumn(2),
                    m_unityWorldTDepthCamera.GetColumn(1));
                Pose = new Pose(translation, rotation);
            }
            else
            {
                ARDebug.LogError("Could not retrieve a pose for the point cloud.");
                Pose = new Pose();
                m_unityWorldTDepthCamera = Matrix4x4.identity;
            }
        }

        /// <summary>
        /// Returns whether this point cloud stores valid data. May be false
        /// when ARCore is not in a tracking state.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Gets the number of points in the point cloud.
        /// </summary>
        public int PointCount
        {
            get
            {
                if (m_rawPointCloud.points == null)
                {
                    return 0;
                }
                return m_rawPointCloud.points.Count;
            }
        }

        /// <summary>
        /// Gets the pose associated with the point cloud in world space. The
        /// points in this cloud are defined relative to this pose.
        /// {@link #GetPoint} can be used to access each point in the cloud in Unity
        /// Coordinates, automatically transforming them properly.
        /// </summary>
        public Pose Pose { get; private set; }

        /// <summary>
        /// Gets the timestamp of when the point cloud was observed.
        /// </summary>
        public double Timestamp
        {
            get
            {
                return m_rawPointCloud.timestamp;
            }
        }

        /// <summary>
        /// Gets a point from the point cloud in Unity Coordinates.
        /// </summary>
        /// <param name="index">The index of the point to access.</param>
        /// <returns>The point cloud point at <c>index</c>.</returns>
        public Vector3 GetPoint(int index)
        {
            return m_unityWorldTDepthCamera.MultiplyPoint3x4(m_rawPointCloud.points[index]);
        }

        /// <summary>
        /// Interface to tango client API. This interface used as a temporary workaround for the screen rotation bug in
        /// TangoInputTracking.TryGetPoseAtTime.
        /// </summary>=
        private struct ExternApi
        {
            [DllImport(TANGO_CLIENT_API_DLL)]
            public static extern ApiServiceErrorStatus TangoService_getPoseAtTime(double timestamp, ApiCoordinateFramePair framePair,
                ref ApiPoseData poseData);
        }

    }
}
