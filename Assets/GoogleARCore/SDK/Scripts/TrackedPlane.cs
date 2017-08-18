//-----------------------------------------------------------------------
// <copyright file="TrackedPlane.cs" company="Google">
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
    using System.Runtime.InteropServices;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A real-world plane being tracked by ARCore.
    /// </summary>
    public class TrackedPlane
    {
        /// <summary>
        /// Center position of the plane.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Rotation of the plane.
        /// </summary>
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// Bounding box size aligned with plane's x and z axis.
        /// </summary>
        public Vector2 Bounds
        {
            get
            {
                return new Vector2((float)m_apiPlaneData.width, (float)m_apiPlaneData.height);
            }
        }

        /// <summary>
        /// Gets a value indicating if the plane data has been updated in the current ARCore Frame.
        /// </summary>
        public bool IsUpdated
        {
            get
            {
                return Time.frameCount == m_lastUpdatedFrame;
            }
        }

        /// <summary>
        /// Gets a value indicating the plane is valid. If this is false, the plane should not be rendered.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return m_apiPlaneData.isValid;
            }
        }

        /// <summary>
        /// Gets a reference to the plane subsuming this plane, if any.  If not null, only the subsuming plane should be
        /// considered valid for rendering.
        /// </summary>
        public TrackedPlane SubsumedBy { get; private set; }

        private ApiPlaneData m_apiPlaneData;

        private Matrix4x4 m_unityWorldTPlane;

        private List<Vector3> m_boundaryPolygonPoints = new List<Vector3>();

        private int m_lastUpdatedFrame;

        private  bool m_initialized;

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Construct PlaneData from APIPlaneData.
        /// Note that this will convert plane's pose from Tango space to Unity world space.
        /// </summary>
        /// <param name="apiPlaneData">ApiPlaneData source.</param>
        /// <param name="updatePlane">A callback to update the API data of the plane.</param>
        public TrackedPlane(ApiPlaneData apiPlaneData, out Action<ApiPlaneData, TrackedPlane, bool> updatePlane)
        {
            _UpdatePlaneIfNeeded(apiPlaneData, null, true);
            updatePlane = _UpdatePlaneIfNeeded;
        }
        /// @endcond

        /// <summary>
        /// Gets a list of points (in clockwise order) in Unity world space representing a boundary polygon for
        /// the plane.
        /// </summary>
        /// <param name="boundaryPolygonPoints">A list of <b>Vector3</b> to be filled by the method call.</param>
        public void GetBoundaryPolygon(ref List<Vector3> boundaryPolygonPoints)
        {
            if (boundaryPolygonPoints == null)
            {
                boundaryPolygonPoints = new List<Vector3>();
            }

            boundaryPolygonPoints.Clear();
            boundaryPolygonPoints.AddRange(m_boundaryPolygonPoints);
        }

        /// <summary>
        /// Update the plane's data with APIPlaneData
        /// Note that this will convert plane's pose from Tango space to Unity world space.
        /// </summary>
        /// <param name="apiPlaneData">ApiPlaneData source.</param>
        /// <param name="subsumedBy">The plane subsuming this plane or null.</param>
        /// <param name="forceUpdate">Force to update.</param>
        private void _UpdatePlaneIfNeeded(ApiPlaneData apiPlaneData, TrackedPlane subsumedBy, bool forceUpdate)
        {
            if (m_initialized && apiPlaneData.id != m_apiPlaneData.id)
            {
                ARDebug.LogError("Cannot update plane with mismatched id.");
                return;
            }
            else if (m_initialized && ! forceUpdate && apiPlaneData.timestamp == m_apiPlaneData.timestamp)
            {
                return;
            }

            if (subsumedBy != null)
            {
                SubsumedBy = subsumedBy;
            }

            m_apiPlaneData = apiPlaneData;
            m_initialized = true;
            m_lastUpdatedFrame = Time.frameCount;

            Matrix4x4 startServiceTplane = Matrix4x4.TRS(apiPlaneData.pose.translation.ToVector3(),
                apiPlaneData.pose.orientation.ToQuaternion(), Vector3.one);

            // Because startServiceTplane is a Pose (position, orientation), the multiplication of the first two terms
            // rotates plane orientation.  This must be undone with the last term (inverse) of the equation.
            m_unityWorldTPlane = Constants.UNITY_WORLD_T_START_SERVICE * startServiceTplane *
                Constants.UNITY_WORLD_T_START_SERVICE.inverse;

            Position = m_unityWorldTPlane.GetColumn(3);
            Position += new Vector3((float)apiPlaneData.centerX, 0.0f, (float)apiPlaneData.centerY);

            Quaternion yaw = Quaternion.Euler(0.0f, -Mathf.Rad2Deg * (float)apiPlaneData.yaw, 0.0f);
            Rotation = yaw * Quaternion.LookRotation(m_unityWorldTPlane.GetColumn(2), m_unityWorldTPlane.GetColumn(1));

            m_boundaryPolygonPoints.Clear();
            int boudaryLength = m_apiPlaneData.boundaryPointNum;
            if (boudaryLength != 0)
            {
                double[] apiBoundaryPolygon = new double[boudaryLength * 2];
                Marshal.Copy(m_apiPlaneData.boundaryPolygon, apiBoundaryPolygon, 0, boudaryLength * 2);

                m_boundaryPolygonPoints.Clear();
                for(int i = 0; i < boudaryLength; ++i)
                {
                    Vector3 localPoint = new Vector3((float)apiBoundaryPolygon[2 * i],
                        0.0f, (float)apiBoundaryPolygon[2*i+1]);
                    m_boundaryPolygonPoints.Add(m_unityWorldTPlane.MultiplyPoint3x4(localPoint));
                }
            }

            // Reverse the m_boundaryPolygonPoints because the raw data is in counter-clockwise.
            // As Unity is left handed system, this should be clockwise.
            m_boundaryPolygonPoints.Reverse();
        }
    }
}