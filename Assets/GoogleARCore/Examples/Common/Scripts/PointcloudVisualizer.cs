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
        [SerializeField] private int m_MaxPointCount = 1000;

        /// <summary>
        /// The default size of the points.
        /// </summary>
        [Tooltip("The default size of the points.")]
        [SerializeField] private int m_DefaultSize = 10;

        /// <summary>
        /// The maximum size that the points will have when they pop.
        /// </summary>
        [Tooltip("The maximum size that the points will have when they pop.")]
        [SerializeField] private int m_PopSize = 50;

        /// <summary>
        /// The mesh.
        /// </summary>
        private Mesh m_Mesh;

        /// <summary>
        /// The mesh renderer.
        /// </summary>
        private MeshRenderer m_MeshRenderer;

        /// <summary>
        /// The unique identifier for the shader _ScreenWidth property.
        /// </summary>
        private int m_ScreenWidthId;

        /// <summary>
        /// The unique identifier for the shader _ScreenHeight property.
        /// </summary>
        private int m_ScreenHeightId;

        /// <summary>
        /// The unique identifier for the shader _Color property.
        /// </summary>
        private int m_ColorId;

        /// <summary>
        /// The property block.
        /// </summary>
        private MaterialPropertyBlock m_PropertyBlock;

        /// <summary>
        /// The cached resolution of the screen.
        /// </summary>
        private Resolution m_CachedResolution;

        /// <summary>
        /// The cached color of the points.
        /// </summary>
        private Color m_CachedColor;

        /// <summary>
        /// The cached feature points.
        /// </summary>
        private LinkedList<PointInfo> m_CachedPoints;

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_Mesh = GetComponent<MeshFilter>().mesh;
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
            }

            m_Mesh.Clear();

            m_CachedColor = PointColor;

            m_ScreenWidthId = Shader.PropertyToID("_ScreenWidth");
            m_ScreenHeightId = Shader.PropertyToID("_ScreenHeight");
            m_ColorId = Shader.PropertyToID("_Color");

            m_PropertyBlock = new MaterialPropertyBlock();
            m_MeshRenderer.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetColor(m_ColorId, m_CachedColor);
            m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);

            m_CachedPoints = new LinkedList<PointInfo>();
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            _ClearCachedPoints();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // If ARCore is not tracking, clear the caches and don't update.
            if (Session.Status != SessionStatus.Tracking)
            {
                _ClearCachedPoints();
                return;
            }

            if (Screen.currentResolution.height != m_CachedResolution.height
                || Screen.currentResolution.width != m_CachedResolution.width)
            {
                _UpdateResolution();
            }

            if (m_CachedColor != PointColor)
            {
                _UpdateColor();
            }

            if (EnablePopAnimation)
            {
                _AddPointsIncrementallyToCache();
                _UpdatePointSize();
            }
            else
            {
                _AddAllPointsToCache();
            }

            _UpdateMesh();
        }

        /// <summary>
        /// Clears all cached feature points.
        /// </summary>
        private void _ClearCachedPoints()
        {
            m_CachedPoints.Clear();
            m_Mesh.Clear();
        }

        /// <summary>
        /// Updates the screen resolution.
        /// </summary>
        private void _UpdateResolution()
        {
            m_CachedResolution = Screen.currentResolution;
            if (m_MeshRenderer != null)
            {
                m_MeshRenderer.GetPropertyBlock(m_PropertyBlock);
                m_PropertyBlock.SetFloat(m_ScreenWidthId, m_CachedResolution.width);
                m_PropertyBlock.SetFloat(m_ScreenHeightId, m_CachedResolution.height);
                m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
            }
        }

        /// <summary>
        /// Updates the color of the feature points.
        /// </summary>
        private void _UpdateColor()
        {
            m_CachedColor = PointColor;
            m_MeshRenderer.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetColor("_Color", m_CachedColor);
            m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
        }

        /// <summary>
        /// Adds points incrementally to the cache, by selecting points at random each frame.
        /// </summary>
        private void _AddPointsIncrementallyToCache()
        {
            if (Frame.PointCloud.PointCount > 0 && Frame.PointCloud.IsUpdatedThisFrame)
            {
                int iterations = Mathf.Min(MaxPointsToAddPerFrame, Frame.PointCloud.PointCount);
                for (int i = 0; i < iterations; i++)
                {
                    Vector3 point = Frame.PointCloud.GetPointAsStruct(
                        Random.Range(0, Frame.PointCloud.PointCount - 1));

                    _AddPointToCache(point);
                }
            }
        }

        /// <summary>
        /// Adds all points from this frame's pointcloud to the cache.
        /// </summary>
        private void _AddAllPointsToCache()
        {
            if (Frame.PointCloud.IsUpdatedThisFrame)
            {
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    _AddPointToCache(Frame.PointCloud.GetPointAsStruct(i));
                }
            }
        }

        /// <summary>
        /// Adds the specified point to cache.
        /// </summary>
        /// <param name="point">A feature point to be added.</param>
        private void _AddPointToCache(Vector3 point)
        {
            if (m_CachedPoints.Count >= m_MaxPointCount)
            {
                m_CachedPoints.RemoveFirst();
            }

            m_CachedPoints.AddLast(new PointInfo(point, new Vector2(m_DefaultSize, m_DefaultSize),
                                                 Time.time));
        }

        /// <summary>
        /// Updates the size of the feature points, producing a pop animation where the size
        /// increases to a maximum size and then goes back to the original size.
        /// </summary>
        private void _UpdatePointSize()
        {
            if (m_CachedPoints.Count <= 0 || !EnablePopAnimation)
            {
                return;
            }

            LinkedListNode<PointInfo> pointNode;

            for (pointNode = m_CachedPoints.First; pointNode != null; pointNode = pointNode.Next)
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
                    size = Mathf.Lerp(m_DefaultSize, m_PopSize, value * 2f);
                }
                else
                {
                    size = Mathf.Lerp(m_PopSize, m_DefaultSize, (value - 0.5f) * 2f);
                }

                pointNode.Value = new PointInfo(pointNode.Value.Position, new Vector2(size, size),
                                                pointNode.Value.CreationTime);
            }
        }

        /// <summary>
        /// Updates the mesh, adding the feature points.
        /// </summary>
        private void _UpdateMesh()
        {
            m_Mesh.Clear();
            m_Mesh.vertices = m_CachedPoints.Select(p => p.Position).ToArray();
            m_Mesh.uv = m_CachedPoints.Select(p => p.Size).ToArray();
            m_Mesh.SetIndices(Enumerable.Range(0, m_CachedPoints.Count).ToArray(),
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
