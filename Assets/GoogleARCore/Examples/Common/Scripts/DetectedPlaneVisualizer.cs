//-----------------------------------------------------------------------
// <copyright file="DetectedPlaneVisualizer.cs" company="Google">
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

namespace GoogleARCore.Examples.Common
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Visualizes a single DetectedPlane in the Unity scene.
    /// </summary>
    public class DetectedPlaneVisualizer : MonoBehaviour
    {
        private static int s_PlaneCount = 0;

        private readonly Color[] k_PlaneColors = new Color[]
        {
            new Color(1.0f, 1.0f, 1.0f),
            new Color(0.956f, 0.262f, 0.211f),
            new Color(0.913f, 0.117f, 0.388f),
            new Color(0.611f, 0.152f, 0.654f),
            new Color(0.403f, 0.227f, 0.717f),
            new Color(0.247f, 0.317f, 0.709f),
            new Color(0.129f, 0.588f, 0.952f),
            new Color(0.011f, 0.662f, 0.956f),
            new Color(0f, 0.737f, 0.831f),
            new Color(0f, 0.588f, 0.533f),
            new Color(0.298f, 0.686f, 0.313f),
            new Color(0.545f, 0.764f, 0.290f),
            new Color(0.803f, 0.862f, 0.223f),
            new Color(1.0f, 0.921f, 0.231f),
            new Color(1.0f, 0.756f, 0.027f)
        };

        private DetectedPlane m_DetectedPlane;

        // Keep previous frame's mesh polygon to avoid mesh update every frame.
        private List<Vector3> m_PreviousFrameMeshVertices = new List<Vector3>();
        private List<Vector3> m_MeshVertices = new List<Vector3>();
        private Vector3 m_PlaneCenter = new Vector3();

        private List<Color> m_MeshColors = new List<Color>();

        private List<int> m_MeshIndices = new List<int>();

        private Mesh m_Mesh;

        private MeshRenderer m_MeshRenderer;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_MeshRenderer = GetComponent<UnityEngine.MeshRenderer>();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (m_DetectedPlane == null)
            {
                return;
            }
            else if (m_DetectedPlane.SubsumedBy != null)
            {
                Destroy(gameObject);
                return;
            }
            else if (m_DetectedPlane.TrackingState != TrackingState.Tracking)
            {
                 m_MeshRenderer.enabled = false;
                 return;
            }

            m_MeshRenderer.enabled = true;

            _UpdateMeshIfNeeded();
        }

        /// <summary>
        /// Initializes the DetectedPlaneVisualizer with a DetectedPlane.
        /// </summary>
        /// <param name="plane">The plane to vizualize.</param>
        public void Initialize(DetectedPlane plane)
        {
            m_DetectedPlane = plane;
            m_MeshRenderer.material.SetColor("_GridColor", k_PlaneColors[s_PlaneCount++ % k_PlaneColors.Length]);
            m_MeshRenderer.material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));

            Update();
        }

        /// <summary>
        /// Update mesh with a list of Vector3 and plane's center position.
        /// </summary>
        private void _UpdateMeshIfNeeded()
        {
            m_DetectedPlane.GetBoundaryPolygon(m_MeshVertices);

            if (_AreVerticesListsEqual(m_PreviousFrameMeshVertices, m_MeshVertices))
            {
                return;
            }

            m_PreviousFrameMeshVertices.Clear();
            m_PreviousFrameMeshVertices.AddRange(m_MeshVertices);

            m_PlaneCenter = m_DetectedPlane.CenterPose.position;

            Vector3 planeNormal = m_DetectedPlane.CenterPose.rotation * Vector3.up;

            m_MeshRenderer.material.SetVector("_PlaneNormal", planeNormal);

            int planePolygonCount = m_MeshVertices.Count;

            // The following code converts a polygon to a mesh with two polygons, inner
            // polygon renders with 100% opacity and fade out to outter polygon with opacity 0%, as shown below.
            // The indices shown in the diagram are used in comments below.
            // _______________     0_______________1
            // |             |      |4___________5|
            // |             |      | |         | |
            // |             | =>   | |         | |
            // |             |      | |         | |
            // |             |      |7-----------6|
            // ---------------     3---------------2
            m_MeshColors.Clear();

            // Fill transparent color to vertices 0 to 3.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                m_MeshColors.Add(Color.clear);
            }

            // Feather distance 0.2 meters.
            const float featherLength = 0.2f;

            // Feather scale over the distance between plane center and vertices.
            const float featherScale = 0.2f;

            // Add vertex 4 to 7.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                Vector3 v = m_MeshVertices[i];

                // Vector from plane center to current point
                Vector3 d = v - m_PlaneCenter;

                float scale = 1.0f - Mathf.Min(featherLength / d.magnitude, featherScale);
                m_MeshVertices.Add((scale * d) + m_PlaneCenter);

                m_MeshColors.Add(Color.white);
            }

            m_MeshIndices.Clear();
            int firstOuterVertex = 0;
            int firstInnerVertex = planePolygonCount;

            // Generate triangle (4, 5, 6) and (4, 6, 7).
            for (int i = 0; i < planePolygonCount - 2; ++i)
            {
                m_MeshIndices.Add(firstInnerVertex);
                m_MeshIndices.Add(firstInnerVertex + i + 1);
                m_MeshIndices.Add(firstInnerVertex + i + 2);
            }

            // Generate triangle (0, 1, 4), (4, 1, 5), (5, 1, 2), (5, 2, 6), (6, 2, 3), (6, 3, 7)
            // (7, 3, 0), (7, 0, 4)
            for (int i = 0; i < planePolygonCount; ++i)
            {
                int outerVertex1 = firstOuterVertex + i;
                int outerVertex2 = firstOuterVertex + ((i + 1) % planePolygonCount);
                int innerVertex1 = firstInnerVertex + i;
                int innerVertex2 = firstInnerVertex + ((i + 1) % planePolygonCount);

                m_MeshIndices.Add(outerVertex1);
                m_MeshIndices.Add(outerVertex2);
                m_MeshIndices.Add(innerVertex1);

                m_MeshIndices.Add(innerVertex1);
                m_MeshIndices.Add(outerVertex2);
                m_MeshIndices.Add(innerVertex2);
            }

            m_Mesh.Clear();
            m_Mesh.SetVertices(m_MeshVertices);
            m_Mesh.SetIndices(m_MeshIndices.ToArray(), MeshTopology.Triangles, 0);
            m_Mesh.SetColors(m_MeshColors);
        }

        private bool _AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
        {
            if (firstList.Count != secondList.Count)
            {
                return false;
            }

            for (int i = 0; i < firstList.Count; i++)
            {
                if (firstList[i] != secondList[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
