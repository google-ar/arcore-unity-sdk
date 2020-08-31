//-----------------------------------------------------------------------
// <copyright file="DetectedPlaneVisualizer.cs" company="Google LLC">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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
        private DetectedPlane _detectedPlane;

        // Keep previous frame's mesh polygon to avoid mesh update every frame.
        private List<Vector3> _previousFrameMeshVertices = new List<Vector3>();
        private List<Vector3> _meshVertices = new List<Vector3>();
        private Vector3 _planeCenter = new Vector3();

        private List<Color> _meshColors = new List<Color>();

        private List<int> _meshIndices = new List<int>();

        private Mesh _mesh;

        private MeshRenderer _meshRenderer;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _meshRenderer = GetComponent<UnityEngine.MeshRenderer>();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (_detectedPlane == null)
            {
                return;
            }
            else if (_detectedPlane.SubsumedBy != null)
            {
                Destroy(gameObject);
                return;
            }
            else if (_detectedPlane.TrackingState != TrackingState.Tracking)
            {
                 _meshRenderer.enabled = false;
                 return;
            }

            _meshRenderer.enabled = true;

            UpdateMeshIfNeeded();
        }

        /// <summary>
        /// Initializes the DetectedPlaneVisualizer with a DetectedPlane.
        /// </summary>
        /// <param name="plane">The plane to vizualize.</param>
        public void Initialize(DetectedPlane plane)
        {
            _detectedPlane = plane;
            _meshRenderer.material.SetColor("_GridColor", Color.white);
            _meshRenderer.material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));

            Update();
        }

        /// <summary>
        /// Update mesh with a list of Vector3 and plane's center position.
        /// </summary>
        private void UpdateMeshIfNeeded()
        {
            _detectedPlane.GetBoundaryPolygon(_meshVertices);

            if (AreVerticesListsEqual(_previousFrameMeshVertices, _meshVertices))
            {
                return;
            }

            _previousFrameMeshVertices.Clear();
            _previousFrameMeshVertices.AddRange(_meshVertices);

            _planeCenter = _detectedPlane.CenterPose.position;

            Vector3 planeNormal = _detectedPlane.CenterPose.rotation * Vector3.up;

            _meshRenderer.material.SetVector("_PlaneNormal", planeNormal);

            int planePolygonCount = _meshVertices.Count;

            // The following code converts a polygon to a mesh with two polygons, inner polygon
            // renders with 100% opacity and fade out to outter polygon with opacity 0%, as shown
            // below.  The indices shown in the diagram are used in comments below.
            // _______________     0_______________1
            // |             |      |4___________5|
            // |             |      | |         | |
            // |             | =>   | |         | |
            // |             |      | |         | |
            // |             |      |7-----------6|
            // ---------------     3---------------2
            _meshColors.Clear();

            // Fill transparent color to vertices 0 to 3.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                _meshColors.Add(Color.clear);
            }

            // Feather distance 0.2 meters.
            const float featherLength = 0.2f;

            // Feather scale over the distance between plane center and vertices.
            const float featherScale = 0.2f;

            // Add vertex 4 to 7.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                Vector3 v = _meshVertices[i];

                // Vector from plane center to current point
                Vector3 d = v - _planeCenter;

                float scale = 1.0f - Mathf.Min(featherLength / d.magnitude, featherScale);
                _meshVertices.Add((scale * d) + _planeCenter);

                _meshColors.Add(Color.white);
            }

            _meshIndices.Clear();
            int firstOuterVertex = 0;
            int firstInnerVertex = planePolygonCount;

            // Generate triangle (4, 5, 6) and (4, 6, 7).
            for (int i = 0; i < planePolygonCount - 2; ++i)
            {
                _meshIndices.Add(firstInnerVertex);
                _meshIndices.Add(firstInnerVertex + i + 1);
                _meshIndices.Add(firstInnerVertex + i + 2);
            }

            // Generate triangle (0, 1, 4), (4, 1, 5), (5, 1, 2), (5, 2, 6), (6, 2, 3), (6, 3, 7)
            // (7, 3, 0), (7, 0, 4)
            for (int i = 0; i < planePolygonCount; ++i)
            {
                int outerVertex1 = firstOuterVertex + i;
                int outerVertex2 = firstOuterVertex + ((i + 1) % planePolygonCount);
                int innerVertex1 = firstInnerVertex + i;
                int innerVertex2 = firstInnerVertex + ((i + 1) % planePolygonCount);

                _meshIndices.Add(outerVertex1);
                _meshIndices.Add(outerVertex2);
                _meshIndices.Add(innerVertex1);

                _meshIndices.Add(innerVertex1);
                _meshIndices.Add(outerVertex2);
                _meshIndices.Add(innerVertex2);
            }

            _mesh.Clear();
            _mesh.SetVertices(_meshVertices);
            _mesh.SetTriangles(_meshIndices, 0);
            _mesh.SetColors(_meshColors);
        }

        private bool AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
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
