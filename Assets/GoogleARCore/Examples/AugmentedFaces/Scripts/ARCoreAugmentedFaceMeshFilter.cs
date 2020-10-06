//-----------------------------------------------------------------------
// <copyright file="ARCoreAugmentedFaceMeshFilter.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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

        private AugmentedFace _augmentedFace = null;
        private List<AugmentedFace> _augmentedFaceList = null;

        // Keep previous frame's mesh polygon to avoid mesh update every frame.
        private List<Vector3> _meshVertices = new List<Vector3>();
        private List<Vector3> _meshNormals = new List<Vector3>();
        private List<Vector2> _meshUVs = new List<Vector2>();
        private List<int> _meshIndices = new List<int>();
        private Mesh _mesh = null;
        private bool _meshInitialized = false;

        /// <summary>
        /// Gets or sets the ARCore AugmentedFace object that will be used to update the face mesh data.
        /// </summary>
        public AugmentedFace AumgnetedFace
        {
            get
            {
                return _augmentedFace;
            }

            set
            {
                _augmentedFace = value;
                Update();
            }
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
            _augmentedFaceList = new List<AugmentedFace>();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (AutoBind)
            {
                _augmentedFaceList.Clear();
                Session.GetTrackables<AugmentedFace>(_augmentedFaceList, TrackableQueryFilter.All);
                if (_augmentedFaceList.Count != 0)
                {
                    _augmentedFace = _augmentedFaceList[0];
                }
            }

            if (_augmentedFace == null)
            {
                return;
            }

            // Update game object position;
            transform.position = _augmentedFace.CenterPose.position;
            transform.rotation = _augmentedFace.CenterPose.rotation;

            UpdateMesh();
        }

        /// <summary>
        /// Update mesh with a face mesh vertices, texture coordinates and indices.
        /// </summary>
        private void UpdateMesh()
        {
            _augmentedFace.GetVertices(_meshVertices);
            _augmentedFace.GetNormals(_meshNormals);

            if (!_meshInitialized)
            {
                _augmentedFace.GetTextureCoordinates(_meshUVs);
                _augmentedFace.GetTriangleIndices(_meshIndices);

                // Only update mesh indices and uvs once as they don't change every frame.
                _meshInitialized = true;
            }

            _mesh.Clear();
            _mesh.SetVertices(_meshVertices);
            _mesh.SetNormals(_meshNormals);
            _mesh.SetTriangles(_meshIndices, 0);
            _mesh.SetUVs(0, _meshUVs);

            _mesh.RecalculateBounds();
        }
    }
}
