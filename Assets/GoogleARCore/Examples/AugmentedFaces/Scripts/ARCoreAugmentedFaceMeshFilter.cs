//-----------------------------------------------------------------------
// <copyright file="ARCoreAugmentedFaceMeshFilter.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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

namespace GoogleARCore.Examples.AugmentedFaces
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Helper component to update face mesh data.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class ARCoreAugmentedFaceMeshFilter : MonoBehaviour
    {
        /// <summary>
        /// If true, this component will update itself using the first AugmentedFace detected by ARCore.
        /// </summary>
        public bool AutoBind = false;

        private AugmentedFace m_AugmentedFace = null;
        private List<AugmentedFace> m_AugmentedFaceList = null;

        // Keep previous frame's mesh polygon to avoid mesh update every frame.
        private List<Vector3> m_MeshVertices = new List<Vector3>();
        private List<Vector3> m_MeshNormals = new List<Vector3>();
        private List<Vector2> m_MeshUVs = new List<Vector2>();
        private List<int> m_MeshIndices = new List<int>();
        private Mesh m_Mesh = null;
        private bool m_MeshInitialized = false;

        /// <summary>
        /// Gets or sets the ARCore AugmentedFace object that will be used to update the face mesh data.
        /// </summary>
        public AugmentedFace AumgnetedFace
        {
            get
            {
                return m_AugmentedFace;
            }

            set
            {
                m_AugmentedFace = value;
                Update();
            }
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            m_Mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = m_Mesh;
            m_AugmentedFaceList = new List<AugmentedFace>();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (AutoBind)
            {
                m_AugmentedFaceList.Clear();
                Session.GetTrackables<AugmentedFace>(m_AugmentedFaceList, TrackableQueryFilter.All);
                if (m_AugmentedFaceList.Count != 0)
                {
                    m_AugmentedFace = m_AugmentedFaceList[0];
                }
            }

            if (m_AugmentedFace == null)
            {
                return;
            }

            // Update game object position;
            transform.position = m_AugmentedFace.CenterPose.position;
            transform.rotation = m_AugmentedFace.CenterPose.rotation;

            _UpdateMesh();
        }

        /// <summary>
        /// Update mesh with a face mesh vertices, texture coordinates and indices.
        /// </summary>
        private void _UpdateMesh()
        {
            m_AugmentedFace.GetVertices(m_MeshVertices);
            m_AugmentedFace.GetNormals(m_MeshNormals);

            if (!m_MeshInitialized)
            {
                m_AugmentedFace.GetTextureCoordinates(m_MeshUVs);
                m_AugmentedFace.GetTriangleIndices(m_MeshIndices);

                // Only update mesh indices and uvs once as they don't change every frame.
                m_MeshInitialized = true;
            }

            m_Mesh.Clear();
            m_Mesh.SetVertices(m_MeshVertices);
            m_Mesh.SetNormals(m_MeshNormals);
            m_Mesh.SetTriangles(m_MeshIndices, 0);
            m_Mesh.SetUVs(0, m_MeshUVs);

            m_Mesh.RecalculateBounds();
        }
    }
}
