//-----------------------------------------------------------------------
// <copyright file="TrackedPlaneComponent.cs" company="Google">
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
    using GoogleARCoreInternal;

    /// <summary>
    /// Visualizes a TrackedPlane in the Unity scene.
    /// </summary>
    public class TrackedPlaneVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The ARCore tracked plane to represent.
        /// </summary>
        private TrackedPlane m_trackedPlane;

        private List<Vector3> m_meshVertices = new List<Vector3>();

        private List<Color> m_meshColors = new List<Color>();

        private List<int> m_meshIndices = new List<int>();

        private Mesh m_mesh;

        private MeshRenderer m_meshRenderer;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        private void Awake()
        {
            m_mesh = GetComponent<MeshFilter>().mesh;
            m_meshRenderer = GetComponent<UnityEngine.MeshRenderer>();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        private void Update()
        {
            if (m_trackedPlane == null)
            {
                return;
            }
            else if (m_trackedPlane.SubsumedBy != null)
            {
                Destroy(gameObject);
                return;
            }
            else if (!m_trackedPlane.IsValid || Frame.TrackingState != FrameTrackingState.Tracking)
            {
                 m_meshRenderer.enabled = false;
                 return;
            }

            m_meshRenderer.enabled = true;
            if (m_trackedPlane.IsUpdated)
            {
                _UpdateMeshWithCurrentTrackedPlane();
            }
        }

        /// <summary>
        /// Update the TrackedPlane reference.
        /// </summary>
        /// <param name="plane">The TrackedPlane reference..</param>
        public void SetTrackedPlane(TrackedPlane plane)
        {
            m_trackedPlane = plane;
            _UpdateMeshWithCurrentTrackedPlane();
        }

        private void _UpdateMeshWithCurrentTrackedPlane()
        {
            // Note that GetBoundaryPolygon returns points in clockwise order.
            m_trackedPlane.GetBoundaryPolygon(ref m_meshVertices);

            Vector3 planeCenter = m_trackedPlane.Position;
            int planePolygonCount = m_meshVertices.Count;

            // The following code convert a polygon to a mesh with two polygons, inner
            // polygon renders with 100% opacity and fade out to outter polygon with opacity 0%, as shown below.
            // The indices shown in the diagram are used in comments below.
            // _______________     0_______________1
            // |             |      |4___________5|
            // |             |      | |         | |
            // |             | =>   | |         | |
            // |             |      | |         | |
            // |             |      |7-----------6|
            // ---------------     3---------------2
            m_meshColors.Clear();

            // Fill transparent color to vertices 1 to 3.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                m_meshColors.Add(new Color(0.0f, 0.0f, 0.0f, 0.0f));
            }

            // Feather distance 0.2 meters.
            const float FEATHER_LENGTH = 0.2f;

            // Feather scale over the distance between plane center and vertices.
            const float FEATHER_SCALE = 0.2f;

            // Add vertex 4 to 5.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                Vector3 v = m_meshVertices[i];

                // Vector from plane center to current point
                Vector3 d = v - planeCenter;

                float scale = 1.0f - Mathf.Min((FEATHER_LENGTH / d.magnitude), FEATHER_SCALE);
                m_meshVertices.Add(scale * d + planeCenter);

                m_meshColors.Add(new Color(0.0f, 0.0f, 0.0f, 1.0f));
            }

            m_meshIndices.Clear();
            int verticeLength = m_meshVertices.Count;
            int verticeLengthHalf = verticeLength / 2;
            // Generate triangle (4, 5, 6) and (4, 6, 7).
            for (int i = verticeLengthHalf + 1; i < verticeLength - 1; ++i)
            {
                m_meshIndices.Add(verticeLengthHalf);
                m_meshIndices.Add(i);
                m_meshIndices.Add(i + 1);
            }

            // Generate triangle (0, 1, 4), (4, 1, 5), (5, 1, 2), (5, 2, 6), (6, 2, 3), (6, 3, 7)
            // (7, 3, 0), (7, 0, 4)
            for (int i = 0; i < verticeLengthHalf; ++i)
            {
                m_meshIndices.Add(i);
                m_meshIndices.Add((i + 1) % verticeLengthHalf);
                m_meshIndices.Add(i + verticeLengthHalf);

                m_meshIndices.Add(i + verticeLengthHalf);
                m_meshIndices.Add((i + 1) % verticeLengthHalf);
                m_meshIndices.Add((i + verticeLengthHalf + 1) % verticeLengthHalf + verticeLengthHalf);
            }

            m_mesh.Clear();
            m_mesh.SetVertices(m_meshVertices);
            m_mesh.SetIndices(m_meshIndices.ToArray(), MeshTopology.Triangles, 0);
            m_mesh.SetColors(m_meshColors);
        }
    }
}