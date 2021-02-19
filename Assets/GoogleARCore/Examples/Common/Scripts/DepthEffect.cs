//-----------------------------------------------------------------------
// <copyright file="DepthEffect.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
        /// The Depth Setting Menu.
        /// </summary>
        public DepthMenu DepthMenu;

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

        private static readonly string _currentDepthTexturePropertyName = "_CurrentDepthTexture";
        private static readonly string _topLeftRightPropertyName = "_UvTopLeftRight";
        private static readonly string _bottomLeftRightPropertyName = "_UvBottomLeftRight";

        private Camera _camera;
        private Material _depthMaterial;
        private Texture2D _depthTexture;
        private float _currentOcclusionTransparency = 1.0f;
        private ARCoreBackgroundRenderer _backgroundRenderer;
        private CommandBuffer _depthBuffer;
        private CommandBuffer _backgroundBuffer;
        private int _backgroundTextureID = -1;

        /// <summary>
        /// Unity's Awake() method.
        /// </summary>
        public void Awake()
        {
            _currentOcclusionTransparency = OcclusionTransparency;

            Debug.Assert(OcclusionShader != null, "Occlusion Shader parameter must be set.");
            _depthMaterial = new Material(OcclusionShader);
            _depthMaterial.SetFloat("_OcclusionTransparency", _currentOcclusionTransparency);
            _depthMaterial.SetFloat("_OcclusionOffsetMeters", OcclusionOffset);
            _depthMaterial.SetFloat("_TransitionSize", TransitionSize);

            // Default texture, will be updated each frame.
            _depthTexture = new Texture2D(2, 2);
            _depthTexture.filterMode = FilterMode.Bilinear;
            _depthMaterial.SetTexture(_currentDepthTexturePropertyName, _depthTexture);

            _camera = Camera.main;
            _camera.depthTextureMode |= DepthTextureMode.Depth;

            _depthBuffer = new CommandBuffer();
            _depthBuffer.name = "Auxilary occlusion textures";

            // Creates the occlusion map.
            int occlusionMapTextureID = Shader.PropertyToID("_OcclusionMap");
            _depthBuffer.GetTemporaryRT(occlusionMapTextureID, -1, -1, 0, FilterMode.Bilinear);

            // Pass #0 renders an auxilary buffer - occlusion map that indicates the
            // regions of virtual objects that are behind real geometry.
            _depthBuffer.Blit(
                BuiltinRenderTextureType.CameraTarget,
                occlusionMapTextureID, _depthMaterial, /*pass=*/ 0);
            _depthBuffer.SetGlobalTexture("_OcclusionMap", occlusionMapTextureID);

            _camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _depthBuffer);
            _camera.AddCommandBuffer(CameraEvent.AfterGBuffer, _depthBuffer);

            _backgroundRenderer = FindObjectOfType<ARCoreBackgroundRenderer>();
            if (_backgroundRenderer == null)
            {
                Debug.LogError("BackgroundTextureProvider requires ARCoreBackgroundRenderer " +
                    "anywhere in the scene.");
                return;
            }

            _backgroundBuffer = new CommandBuffer();
            _backgroundBuffer.name = "Camera texture";
            _backgroundTextureID = Shader.PropertyToID(BackgroundTexturePropertyName);
            _backgroundBuffer.GetTemporaryRT(_backgroundTextureID,
                                             /*width=*/ -1, /*height=*/ -1, /*depthBuffer=*/0,
                                             FilterMode.Bilinear);

            var material = _backgroundRenderer.BackgroundMaterial;
            if (material != null)
            {
                _backgroundBuffer.Blit(material.mainTexture, _backgroundTextureID, material);
            }

            _backgroundBuffer.SetGlobalTexture(
                BackgroundTexturePropertyName, _backgroundTextureID);
            _camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _backgroundBuffer);
            _camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, _backgroundBuffer);
        }

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            _currentOcclusionTransparency +=
                (OcclusionTransparency - _currentOcclusionTransparency) *
                Time.deltaTime * OcclusionFadeVelocity;

            _currentOcclusionTransparency =
                Mathf.Clamp(_currentOcclusionTransparency, 0.0f, OcclusionTransparency);
            _depthMaterial.SetFloat("_OcclusionTransparency", _currentOcclusionTransparency);
            _depthMaterial.SetFloat("_TransitionSize", TransitionSize);

            if (Session.Status == SessionStatus.Tracking && DepthMenu != null &&
                DepthMenu.IsDepthEnabled())
            {
                // Gets the latest depth map from ARCore.
                Frame.CameraImage.UpdateDepthTexture(ref _depthTexture);
            }

            // Updates the screen orientation for each material.
            UpdateScreenOrientationOnMaterial();
        }

        /// <summary>
        /// Unity's OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            if (_depthBuffer != null)
            {
                _camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _depthBuffer);
                _camera.AddCommandBuffer(CameraEvent.AfterGBuffer, _depthBuffer);
            }

            if (_backgroundBuffer != null)
            {
                _camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _backgroundBuffer);
                _camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, _backgroundBuffer);
            }
        }

        /// <summary>
        /// Unity's OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            if (_depthBuffer != null)
            {
                _camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, _depthBuffer);
                _camera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, _depthBuffer);
            }

            if (_backgroundBuffer != null)
            {
                _camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _backgroundBuffer);
                _camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, _backgroundBuffer);
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
            Graphics.Blit(source, destination, _depthMaterial, /*pass=*/ 1);
        }

        /// <summary>
        /// Updates the screen orientation of the depth map.
        /// </summary>
        private void UpdateScreenOrientationOnMaterial()
        {
            var uvQuad = Frame.CameraImage.TextureDisplayUvs;
            _depthMaterial.SetVector(
                _topLeftRightPropertyName,
                new Vector4(
                    uvQuad.TopLeft.x, uvQuad.TopLeft.y, uvQuad.TopRight.x, uvQuad.TopRight.y));
            _depthMaterial.SetVector(
                _bottomLeftRightPropertyName,
                new Vector4(uvQuad.BottomLeft.x, uvQuad.BottomLeft.y, uvQuad.BottomRight.x,
                    uvQuad.BottomRight.y));
        }
    }
}
