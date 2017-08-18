//-----------------------------------------------------------------------
// <copyright file="DepthCameraManager.cs" company="Google">
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
    using GoogleARCore;
    using UnityEngine;
    using UnityTango = GoogleAR.UnityNative;

    /// <summary>
    /// A manager for the depth camera.
    /// </summary>
    public class PointCloudManager
    {
        /// <summary>
        /// The latest raw point cloud data.
        /// </summary>
        private PointCloud m_latestRawPointCloud = new PointCloud(null);

        /// <summary>
        /// The unity frame count where <c>m_latestPose</c> was last updated.
        /// </summary>
        private int m_latestPointCloudFrame = 0;

        /// <summary>
        /// Gets the latest point cloud from the depth camera.
        /// </summary>
        /// <returns>The latest point cloud from the depth camera.</returns>
        public PointCloud GetLatestPointCloud()
        {
            // Maintain frame consistency.
            if (Time.frameCount == m_latestPointCloudFrame)
            {
                return m_latestRawPointCloud;
            }

            // Attempt to query latest point cloud.
            UnityTango.PointCloudData rawPointCloud = new UnityTango.PointCloudData();
            if (!UnityTango.Device.TryGetLatestPointCloud(ref rawPointCloud))
            {
                return m_latestRawPointCloud;
            }

            // Update the latest point cloud.
            m_latestRawPointCloud = new PointCloud(rawPointCloud);
            m_latestPointCloudFrame = Time.frameCount;

            return m_latestRawPointCloud;
        }
    }
}
