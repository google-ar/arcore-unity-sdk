//-----------------------------------------------------------------------
// <copyright file="DepthEffect.cs" company="Google LLC">
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
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// A component that controls the full-screen occlusion effect.
    /// Exposes parameters to control the occlusion effect, which get applied every time update
    /// gets called.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DepthEffect : MonoBehaviour
    {
        /// <summary>
        /// The global shader property name for the camera texture.
        /// </summary>
        public const string BackgroundTexturePropertyName = "_BackgroundTexture";

        /// <summary>
        /// The image effect shader to blit every frame with.
        /// </summary>
        public Shader OcclusionShader;

        /// <summary>
        /// The blur kernel size applied to the camera feed. In pixels.
        /// </summary>
        [Space]
        public float BlurSize = 20f;

        /// <summary>
        /// The number of times occlusion map is downsampled before blurring. Useful for
        /// performance optimization. The value of 1 means no downsampling, each next one
        /// downsamples by 2.
        /// </summary>
        public int BlurDownsample = 2;

        /// <summary>
        /// Maximum occlusion transparency. The value of 1.0 means completely invisible when
        /// occluded.
        /// </summary>
        [Range(0, 1)]
        public float OcclusionTransparency = 1.0f;

        /// <summary>
        /// The bias added to the estimated depth. Useful to avoid occlusion of objects anchored
        /// to planes. In meters.
        /// </summary>
        [Space]
        public float OcclusionOffset = 0.08f;

        /// <summary>
        /// Velocity occlusions effect fades in/out when being enabled/disabled.
        /// </summary>
        public float OcclusionFadeVelocity = 4.0f;

        /// <summary>
        /// Instead of a hard z-buffer test, allows the asset to fade into the background
        /// gradually. The parameter is unitless, it is a fraction of the distance between the
        /// camera and the virtual object where blending is applied.
        /// </summary>
        public float TransitionSize = 0.1f;

        private static readonly string k_CurrentDepthTexturePropertyName = "_CurrentDepthTexture";
        private static readonly string k_TopLeftRightPropertyName = "_UvTopLeftRight";
        private static readonly string k_BottomLeftRightPropertyName = "_UvBottomLeftRight";

        private Camera m_Camera;
        private Material m_DepthMaterial;
        private Texture2D m_DepthTexture;
        private float m_CurrentOcclusionTransparency = 1.0f;
        private ARCoreBackgroundRenderer m_BackgroundRenderer;
        private CommandBuffer m_DepthBuffer;
        private CommandBuffer m_BackgroundBuffer;
        private int m_BackgroundTextureID = -1;

        /// <summary>
        /// Unity's Awake() method.
        /// </summary>
        public void Awake()
        {
            m_CurrentOcclusionTransparency = OcclusionTransparency;

            Debug.Assert(OcclusionShader != null, "Occlusion Shader parameter must be set.");
            m_DepthMaterial = new Material(OcclusionShader);
            m_DepthMaterial.SetFloat("_OcclusionTransparency", m_CurrentOcclusionTransparency);
            m_DepthMaterial.SetFloat("_OcclusionOffsetMeters", OcclusionOffset);
            m_DepthMaterial.SetFloat("_TransitionSize", TransitionSize);

            // Default texture, will be updated each frame.
            m_DepthTexture = new Texture2D(2, 2);
            m_DepthTexture.filterMode = FilterMode.Bilinear;
            m_DepthMaterial.SetTexture(k_CurrentDepthTexturePropertyName, m_DepthTexture);

            m_Camera = Camera.main;
            m_Camera.depthTextureMode |= DepthTextureMode.Depth;

            m_DepthBuffer = new CommandBuffer();
            m_DepthBuffer.name = "Auxilary occlusion textures";

            // Creates the occlusion map.
            int occlusionMapTextureID = Shader.PropertyToID("_OcclusionMap");
            m_DepthBuffer.GetTemporaryRT(occlusionMapTextureID, -1, -1, 0, FilterMode.Bilinear);

            // Pass #0 renders an auxilary buffer - occlusion map that indicates the
            // regions of virtual objects that are behind real geometry.
            m_DepthBuffer.Blit(
                BuiltinRenderTextureType.CameraTarget,
                occlusionMapTextureID, m_DepthMaterial, /*pass=*/ 0);

            // Blurs the occlusion map.
            m_DepthBuffer.SetGlobalTexture("_OcclusionMapBlurred", occlusionMapTextureID);

            m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthBuffer);
            m_Camera.AddCommandBuffer(CameraEvent.AfterGBuffer, m_DepthBuffer);

            m_BackgroundRenderer = FindObjectOfType<ARCoreBackgroundRenderer>();
            if (m_BackgroundRenderer == null)
            {
                Debug.LogError("BackgroundTextureProvider requires ARCoreBackgroundRenderer " +
                    "anywhere in the scene.");
                return;
            }

            m_BackgroundBuffer = new CommandBuffer();
            m_BackgroundBuffer.name = "Camera texture";
            m_BackgroundTextureID = Shader.PropertyToID(BackgroundTexturePropertyName);
            m_BackgroundBuffer.GetTemporaryRT(m_BackgroundTextureID,
                /*width=*/
                -1, /*height=*/ -1,
                /*depthBuffer=*/
                0, FilterMode.Bilinear);

            var material = m_BackgroundRenderer.BackgroundMaterial;
            if (material != null)
            {
                m_BackgroundBuffer.Blit(material.mainTexture, m_BackgroundTextureID, material);
            }

            m_BackgroundBuffer.SetGlobalTexture(
                BackgroundTexturePropertyName, m_BackgroundTextureID);
            m_Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, m_BackgroundBuffer);
            m_Camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, m_BackgroundBuffer);
        }

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            m_CurrentOcclusionTransparency +=
                (OcclusionTransparency - m_CurrentOcclusionTransparency) *
                Time.deltaTime * OcclusionFadeVelocity;

            m_CurrentOcclusionTransparency =
                Mathf.Clamp(m_CurrentOcclusionTransparency, 0.0f, OcclusionTransparency);
            m_DepthMaterial.SetFloat("_OcclusionTransparency", m_CurrentOcclusionTransparency);
            m_DepthMaterial.SetFloat("_TransitionSize", TransitionSize);
            Shader.SetGlobalFloat("_BlurSize", BlurSize / BlurDownsample);

            // Gets the latest depth map from ARCore.
            Frame.CameraImage.UpdateDepthTexture(ref m_DepthTexture);

            // Updates the screen orientation for each material.
            _UpdateScreenOrientationOnMaterial();
        }

        /// <summary>
        /// Unity's OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            if (m_DepthBuffer != null)
            {
                m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthBuffer);
                m_Camera.AddCommandBuffer(CameraEvent.AfterGBuffer, m_DepthBuffer);
            }

            if (m_BackgroundBuffer != null)
            {
                m_Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, m_BackgroundBuffer);
                m_Camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, m_BackgroundBuffer);
            }
        }

        /// <summary>
        /// Unity's OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            if (m_DepthBuffer != null)
            {
                m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthBuffer);
                m_Camera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, m_DepthBuffer);
            }

            if (m_BackgroundBuffer != null)
            {
                m_Camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, m_BackgroundBuffer);
                m_Camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, m_BackgroundBuffer);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // Only render the image when tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            // Pass #1 combines virtual and real cameras based on the occlusion map.
            Graphics.Blit(source, destination, m_DepthMaterial, /*pass=*/ 1);
        }

        /// <summary>
        /// Updates the screen orientation of the depth map.
        /// </summary>
        private void _UpdateScreenOrientationOnMaterial()
        {
            var uvQuad = Frame.CameraImage.TextureDisplayUvs;
            m_DepthMaterial.SetVector(
                k_TopLeftRightPropertyName,
                new Vector4(
                    uvQuad.TopLeft.x, uvQuad.TopLeft.y, uvQuad.TopRight.x, uvQuad.TopRight.y));
            m_DepthMaterial.SetVector(
                k_BottomLeftRightPropertyName,
                new Vector4(uvQuad.BottomLeft.x, uvQuad.BottomLeft.y, uvQuad.BottomRight.x,
                    uvQuad.BottomRight.y));
        }
    }
}
