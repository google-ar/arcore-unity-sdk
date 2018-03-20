//-----------------------------------------------------------------------
// <copyright file="ARCoreBackgroundRenderer.cs" company="Google">
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
    using System.Collections;
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;
    using UnityEngine.XR;

    /// <summary>
    /// Renders the device's camera as a background to the attached Unity camera component.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ARCoreBackgroundRenderer : MonoBehaviour
    {
        /// <summary>
        /// A material used to render the AR background image.
        /// </summary>
        [Tooltip("A material used to render the AR background image.")]
        public Material BackgroundMaterial;

        private Camera m_Camera;

        private ARBackgroundRenderer m_BackgroundRenderer;

        private void OnEnable()
        {
            if (m_BackgroundRenderer == null)
            {
                m_BackgroundRenderer = new ARBackgroundRenderer();
            }

            if (BackgroundMaterial == null)
            {
                Debug.LogError("ArCameraBackground:: No material assigned.");
                return;
            }

            m_Camera = GetComponent<Camera>();
            m_BackgroundRenderer.backgroundMaterial = BackgroundMaterial;
            m_BackgroundRenderer.camera = m_Camera;
            m_BackgroundRenderer.mode = ARRenderMode.MaterialAsBackground;
        }

        private void OnDisable()
        {
            Disable();
        }

        private void Update()
        {
            if (BackgroundMaterial == null)
            {
                return;
            }

            Texture backgroundTexture = Frame.CameraImage.Texture;
            if (backgroundTexture == null)
            {
                return;
            }

            const string mainTexVar = "_MainTex";
            const string topLeftRightVar = "_UvTopLeftRight";
            const string bottomLeftRightVar = "_UvBottomLeftRight";

            BackgroundMaterial.SetTexture(mainTexVar, backgroundTexture);

            var uvQuad = Frame.CameraImage.DisplayUvCoords;
            BackgroundMaterial.SetVector(topLeftRightVar,
                new Vector4(uvQuad.TopLeft.x, uvQuad.TopLeft.y, uvQuad.TopRight.x, uvQuad.TopRight.y));
            BackgroundMaterial.SetVector(bottomLeftRightVar,
                new Vector4(uvQuad.BottomLeft.x, uvQuad.BottomLeft.y, uvQuad.BottomRight.x, uvQuad.BottomRight.y));

            m_Camera.projectionMatrix = Frame.CameraImage.GetCameraProjectionMatrix(
                m_Camera.nearClipPlane, m_Camera.farClipPlane);
        }

        private void Disable()
        {
            if (m_BackgroundRenderer != null)
            {
                m_BackgroundRenderer.camera = null;
                m_BackgroundRenderer.mode = ARRenderMode.StandardBackground;
            }
        }
    }
}
