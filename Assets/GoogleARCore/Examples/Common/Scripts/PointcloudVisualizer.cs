// <copyright file="PointcloudVisualizer.cs" company="Google LLC">
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
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Visualizes the feature points for spatial mapping, showing a pop animation when they appear.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PointcloudVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The color of the feature points.
        /// </summary>
        [Tooltip("The color of the feature points.")]
        public Color PointColor;

        /// <summary>
        /// Whether to enable the pop animation for the feature points.
        /// </summary>
        [Tooltip("Whether to enable the pop animation for the feature points.")]
        public bool EnablePopAnimation = true;

        /// <summary>
        /// The maximum number of points to add per frame.
        /// </summary>
        [Tooltip("The maximum number of points to add per frame.")]
        public int MaxPointsToAddPerFrame = 1;

        /// <summary>
        /// The time interval that the pop animation lasts in seconds.
        /// </summary>
        [Tooltip("The time interval that the animation lasts in seconds.")]
        public float AnimationDuration = 0.3f;

        /// <summary>
        /// The maximum number of points to show on the screen.
        /// </summary>
        [Tooltip("The maximum number of points to show on the screen.")]
        [FormerlySerializedAs("m_MaxPointCount")]
        [SerializeField]
        private int _maxPointCount = 1000;

        /// <summary>
        /// The default size of the points.
        /// </summary>
        [Tooltip("The default size of the points.")]
        [FormerlySerializedAs("m_DefaultSize")]
        [SerializeField]
        private int _defaultSize = 10;

        /// <summary>
        /// The maximum size that the points will have when they pop.
        /// </summary>
        [Tooltip("The maximum size that the points will have when they pop.")]
        [FormerlySerializedAs("m_PopSize")]
        [SerializeField]
        private int _popSize = 50;

        /// <summary>
        /// The mesh.
        /// </summary>
        private Mesh _mesh;

        /// <summary>
        /// The mesh renderer.
        /// </summary>
        private MeshRenderer _meshRenderer;

        /// <summary>
        /// The unique identifier for the shader _ScreenWidth property.
        /// </summary>
        private int _screenWidthId;

        /// <summary>
        /// The unique identifier for the shader _ScreenHeight property.
        /// </summary>
        private int _screenHeightId;

        /// <summary>
        /// The unique identifier for the shader _Color property.
        /// </summary>
        private int _colorId;

        /// <summary>
        /// The property block.
        /// </summary>
        private MaterialPropertyBlock _propertyBlock;

        /// <summary>
        /// The cached resolution of the screen.
        /// </summary>
        private Resolution _cachedResolution;

        /// <summary>
        /// The cached color of the points.
        /// </summary>
        private Color _cachedColor;

        /// <summary>
        /// The cached feature points.
        /// </summary>
        private LinkedList<PointInfo> _cachedPoints;

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _mesh = GetComponent<MeshFilter>().mesh;
            if (_mesh == null)
            {
                _mesh = new Mesh();
            }

            _mesh.Clear();

            _cachedColor = PointColor;

            _screenWidthId = Shader.PropertyToID("_ScreenWidth");
            _screenHeightId = Shader.PropertyToID("_ScreenHeight");
            _colorId = Shader.PropertyToID("_Color");

            _propertyBlock = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(_colorId, _cachedColor);
            _meshRenderer.SetPropertyBlock(_propertyBlock);

            _cachedPoints = new LinkedList<PointInfo>();
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            ClearCachedPoints();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // If ARCore is not tracking, clear the caches and don't update.
            if (Session.Status != SessionStatus.Tracking)
            {
                ClearCachedPoints();
                return;
            }

            if (Screen.currentResolution.height != _cachedResolution.height
                || Screen.currentResolution.width != _cachedResolution.width)
            {
                UpdateResolution();
            }

            if (_cachedColor != PointColor)
            {
                UpdateColor();
            }

            if (EnablePopAnimation)
            {
                AddPointsIncrementallyToCache();
                UpdatePointSize();
            }
            else
            {
                AddAllPointsToCache();
            }

            UpdateMesh();
        }

        /// <summary>
        /// Clears all cached feature points.
        /// </summary>
        private void ClearCachedPoints()
        {
            _cachedPoints.Clear();
            _mesh.Clear();
        }

        /// <summary>
        /// Updates the screen resolution.
        /// </summary>
        private void UpdateResolution()
        {
            _cachedResolution = Screen.currentResolution;
            if (_meshRenderer != null)
            {
                _meshRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat(_screenWidthId, _cachedResolution.width);
                _propertyBlock.SetFloat(_screenHeightId, _cachedResolution.height);
                _meshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>
        /// Updates the color of the feature points.
        /// </summary>
        private void UpdateColor()
        {
            _cachedColor = PointColor;
            _meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_Color", _cachedColor);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>
        /// Adds points incrementally to the cache, by selecting points at random each frame.
        /// </summary>
        private void AddPointsIncrementallyToCache()
        {
            if (Frame.PointCloud.PointCount > 0 && Frame.PointCloud.IsUpdatedThisFrame)
            {
                int iterations = Mathf.Min(MaxPointsToAddPerFrame, Frame.PointCloud.PointCount);
                for (int i = 0; i < iterations; i++)
                {
                    Vector3 point = Frame.PointCloud.GetPointAsStruct(
                        Random.Range(0, Frame.PointCloud.PointCount - 1));

                    AddPointToCache(point);
                }
            }
        }

        /// <summary>
        /// Adds all points from this frame's pointcloud to the cache.
        /// </summary>
        private void AddAllPointsToCache()
        {
            if (Frame.PointCloud.IsUpdatedThisFrame)
            {
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    AddPointToCache(Frame.PointCloud.GetPointAsStruct(i));
                }
            }
        }

        /// <summary>
        /// Adds the specified point to cache.
        /// </summary>
        /// <param name="point">A feature point to be added.</param>
        private void AddPointToCache(Vector3 point)
        {
            if (_cachedPoints.Count >= _maxPointCount)
            {
                _cachedPoints.RemoveFirst();
            }

            _cachedPoints.AddLast(new PointInfo(point, new Vector2(_defaultSize, _defaultSize),
                                                 Time.time));
        }

        /// <summary>
        /// Updates the size of the feature points, producing a pop animation where the size
        /// increases to a maximum size and then goes back to the original size.
        /// </summary>
        private void UpdatePointSize()
        {
            if (_cachedPoints.Count <= 0 || !EnablePopAnimation)
            {
                return;
            }

            LinkedListNode<PointInfo> pointNode;

            for (pointNode = _cachedPoints.First; pointNode != null; pointNode = pointNode.Next)
            {
                float timeSinceAdded = Time.time - pointNode.Value.CreationTime;
                if (timeSinceAdded >= AnimationDuration)
                {
                    continue;
                }

                float value = timeSinceAdded / AnimationDuration;
                float size = 0f;

                if (value < 0.5f)
                {
                    size = Mathf.Lerp(_defaultSize, _popSize, value * 2f);
                }
                else
                {
                    size = Mathf.Lerp(_popSize, _defaultSize, (value - 0.5f) * 2f);
                }

                pointNode.Value = new PointInfo(pointNode.Value.Position, new Vector2(size, size),
                                                pointNode.Value.CreationTime);
            }
        }

        /// <summary>
        /// Updates the mesh, adding the feature points.
        /// </summary>
        private void UpdateMesh()
        {
            _mesh.Clear();
            _mesh.vertices = _cachedPoints.Select(p => p.Position).ToArray();
            _mesh.uv = _cachedPoints.Select(p => p.Size).ToArray();
            _mesh.SetIndices(Enumerable.Range(0, _cachedPoints.Count).ToArray(),
                              MeshTopology.Points, 0);
        }

        /// <summary>
        /// Contains the information of a feature point.
        /// </summary>
        private struct PointInfo
        {
            /// <summary>
            /// The position of the point.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The size of the point.
            /// </summary>
            public Vector2 Size;

            /// <summary>
            /// The creation time of the point.
            /// </summary>
            public float CreationTime;

            public PointInfo(Vector3 position, Vector2 size, float creationTime)
            {
                Position = position;
                Size = size;
                CreationTime = creationTime;
            }
        }
    }
}
