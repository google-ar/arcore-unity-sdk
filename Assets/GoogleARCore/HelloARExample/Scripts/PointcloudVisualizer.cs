// <copyright file="PointcloudVisualizer.cs" company="Google">
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

namespace GoogleARCore.HelloAR
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Profiling;
    using GoogleARCore;

    /// <summary>
    /// Visualize the point cloud.
    /// </summary>
    public class PointcloudVisualizer : MonoBehaviour
    {
        private const int MAX_POINT_COUNT = 61440;

        private Mesh m_mesh;

        private Vector3[] m_points = new Vector3[MAX_POINT_COUNT];

        private double m_lastPointCloudTimestamp;

        /// <summary>
        /// Unity start.
        /// </summary>
        public void Start()
        {
            m_mesh = GetComponent<MeshFilter>().mesh;
            m_mesh.Clear();
        }

        /// <summary>
        /// Unity update.
        /// </summary>
        public void Update()
        {
            // Do not update if ARCore is not tracking.
            if (Frame.TrackingState != FrameTrackingState.Tracking)
            {
                return;
            }

            // Fill in the data to draw the point cloud.
            PointCloud pointcloud = Frame.PointCloud;
            if (pointcloud.PointCount > 0 && pointcloud.Timestamp > m_lastPointCloudTimestamp)
            {
                // Copy the point cloud points for mesh verticies.
                for (int i = 0; i < pointcloud.PointCount; i++)
                {
                    m_points[i] = pointcloud.GetPoint(i);
                }

                // Update the mesh indicies array.
                int[] indices = new int[pointcloud.PointCount];
                for (int i = 0; i < pointcloud.PointCount; i++)
                {
                    indices[i] = i;
                }

                m_mesh.Clear();
                m_mesh.vertices = m_points;
                m_mesh.SetIndices(indices, MeshTopology.Points, 0);
                m_lastPointCloudTimestamp = pointcloud.Timestamp;
            }
        }
    }
}