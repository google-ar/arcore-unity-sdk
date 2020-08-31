//-----------------------------------------------------------------------
// <copyright file="DepthTexture.cs" company="Google LLC">
//
// Copyright 2020 Google LLC. All Rights Reserved.
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
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Attaches and updates the depth texture each frame.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class DepthTexture : MonoBehaviour
    {
        private static readonly string _currentDepthTexturePropertyName = "_CurrentDepthTexture";
        private static readonly string _topLeftRightPropertyName = "_UvTopLeftRight";
        private static readonly string _bottomLeftRightPropertyName = "_UvBottomLeftRight";
        private Texture2D _depthTexture;
        private Material _material;

        /// <summary>
        /// Unity's Start() method.
        /// </summary>
        public void Start()
        {
            // Default texture, will be updated each frame.
            _depthTexture = new Texture2D(2, 2);
            _depthTexture.filterMode = FilterMode.Bilinear;

            // Assign the texture to the material.
            _material = GetComponent<Renderer>().sharedMaterial;
            _material.SetTexture(_currentDepthTexturePropertyName, _depthTexture);
            UpdateScreenOrientationOnMaterial();
        }

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            if (Frame.CameraImage.UpdateDepthTexture(ref _depthTexture) !=
                DepthStatus.Success)
            {
                // Rendering will use the most recently acquired depth image.
                // Typically a new image will be produced in the next 1-3 frames,
                // but an app may choose to track this state and potentially pause
                // or suggest behavior changes to the user.
            }

            UpdateScreenOrientationOnMaterial();
        }

        /// <summary>
        /// Updates the screen orientation of the depth map.
        /// </summary>
        private void UpdateScreenOrientationOnMaterial()
        {
            var uvQuad = Frame.CameraImage.TextureDisplayUvs;
            _material.SetVector(
                _topLeftRightPropertyName,
                new Vector4(
                    uvQuad.TopLeft.x, uvQuad.TopLeft.y, uvQuad.TopRight.x, uvQuad.TopRight.y));
            _material.SetVector(
                _bottomLeftRightPropertyName,
                new Vector4(uvQuad.BottomLeft.x, uvQuad.BottomLeft.y, uvQuad.BottomRight.x,
                    uvQuad.BottomRight.y));
        }
    }
}
