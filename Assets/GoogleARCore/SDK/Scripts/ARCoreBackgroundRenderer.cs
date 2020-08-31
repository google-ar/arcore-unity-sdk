//-----------------------------------------------------------------------
// <copyright file="ARCoreBackgroundRenderer.cs" company="Google LLC">
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

namespace GoogleARCore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCoreInternal;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// Renders the device's camera as a background to the attached Unity camera component.
    /// When using the front-facing (selfie) camera, this temporarily inverts culling when
    /// rendering.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [HelpURL("https://developers.google.com/ar/reference/unity/class/GoogleARCore/" +
             "ARCoreBackgroundRenderer")]
    public class ARCoreBackgroundRenderer : MonoBehaviour
    {
        /// <summary>
        /// A material used to render the AR background image.
        /// </summary>
        [Tooltip("A material used to render the AR background image.")]
        public Material BackgroundMaterial;

        private static readonly float _blackScreenDuration = 0.5f;

        private static readonly float _fadingInDuration = 0.5f;

        private Camera _camera;

        private Texture _transitionImageTexture;

        private BackgroundTransitionState _transitionState = BackgroundTransitionState.BlackScreen;

        private float _currentStateElapsed = 0.0f;

        private bool _sessionEnabled = false;

        private bool _userInvertCullingValue = false;

        private CameraClearFlags _cameraClearFlags = CameraClearFlags.Skybox;

        private CommandBuffer _commandBuffer = null;

        private enum BackgroundTransitionState
        {
            BlackScreen = 0,
            FadingIn = 1,
            CameraImage = 2,
        }

        private void OnEnable()
        {
            if (BackgroundMaterial == null)
            {
                Debug.LogError("ArCameraBackground:: No material assigned.");
                return;
            }

            LifecycleManager.Instance.OnSessionSetEnabled += OnSessionSetEnabled;

            _camera = GetComponent<Camera>();

            _transitionImageTexture = Resources.Load<Texture2D>("ViewInARIcon");
            BackgroundMaterial.SetTexture("_TransitionIconTex", _transitionImageTexture);

            EnableARBackgroundRendering();
        }

        private void OnDisable()
        {
            LifecycleManager.Instance.OnSessionSetEnabled -= OnSessionSetEnabled;
            _transitionState = BackgroundTransitionState.BlackScreen;
            _currentStateElapsed = 0.0f;

            _camera.ResetProjectionMatrix();

            DisableARBackgroundRendering();
        }

        private void OnPreRender()
        {
            _userInvertCullingValue = GL.invertCulling;
            var sessionComponent = LifecycleManager.Instance.SessionComponent;
            if (sessionComponent != null &&
                sessionComponent.DeviceCameraDirection == DeviceCameraDirection.FrontFacing)
            {
                GL.invertCulling = true;
            }
        }

        private void OnPostRender()
        {
            GL.invertCulling = _userInvertCullingValue;
        }

        private void Update()
        {
            _currentStateElapsed += Time.deltaTime;
            UpdateState();
            UpdateShaderVariables();
        }

        private void UpdateState()
        {
            if (!_sessionEnabled && _transitionState != BackgroundTransitionState.BlackScreen)
            {
                _transitionState = BackgroundTransitionState.BlackScreen;
                _currentStateElapsed = 0.0f;
            }
            else if (_sessionEnabled &&
                     _transitionState == BackgroundTransitionState.BlackScreen &&
                     _currentStateElapsed > _blackScreenDuration)
            {
                _transitionState = BackgroundTransitionState.FadingIn;
                _currentStateElapsed = 0.0f;
            }
            else if (_sessionEnabled &&
                     _transitionState == BackgroundTransitionState.FadingIn &&
                     _currentStateElapsed > _fadingInDuration)
            {
                _transitionState = BackgroundTransitionState.CameraImage;
                _currentStateElapsed = 0.0f;
            }
        }

        private void UpdateShaderVariables()
        {
            const string brightnessVar = "_Brightness";
            if (_transitionState == BackgroundTransitionState.BlackScreen)
            {
                BackgroundMaterial.SetFloat(brightnessVar, 0.0f);
            }
            else if (_transitionState == BackgroundTransitionState.FadingIn)
            {
                BackgroundMaterial.SetFloat(
                    brightnessVar,
                    CosineLerp(_currentStateElapsed, _fadingInDuration));
            }
            else
            {
                BackgroundMaterial.SetFloat(brightnessVar, 1.0f);
            }

            // Set transform of the transition image texture, it may be visible or invisible based
            // on lerp value.
            const string transformVar = "_TransitionIconTexTransform";
            BackgroundMaterial.SetVector(transformVar, TextureTransform());

            // Background texture should not be rendered when the session is disabled or
            // there is no camera image texture available.
            if (_transitionState == BackgroundTransitionState.BlackScreen ||
                Frame.CameraImage.Texture == null)
            {
                return;
            }

            const string mainTexVar = "_MainTex";
            const string topLeftRightVar = "_UvTopLeftRight";
            const string bottomLeftRightVar = "_UvBottomLeftRight";

            BackgroundMaterial.SetTexture(mainTexVar, Frame.CameraImage.Texture);

            var uvQuad = Frame.CameraImage.TextureDisplayUvs;
            BackgroundMaterial.SetVector(
                topLeftRightVar,
                new Vector4(
                    uvQuad.TopLeft.x, uvQuad.TopLeft.y, uvQuad.TopRight.x, uvQuad.TopRight.y));
            BackgroundMaterial.SetVector(
                bottomLeftRightVar,
                new Vector4(uvQuad.BottomLeft.x, uvQuad.BottomLeft.y, uvQuad.BottomRight.x,
                    uvQuad.BottomRight.y));

            _camera.projectionMatrix = Frame.CameraImage.GetCameraProjectionMatrix(
                _camera.nearClipPlane, _camera.farClipPlane);
        }

        private void OnSessionSetEnabled(bool sessionEnabled)
        {
            _sessionEnabled = sessionEnabled;
            if (!_sessionEnabled)
            {
                UpdateState();
                UpdateShaderVariables();
            }
        }

        private float CosineLerp(float elapsed, float duration)
        {
            float clampedElapsed = Mathf.Clamp(elapsed, 0.0f, duration);
            return Mathf.Cos(((clampedElapsed / duration) - 1) * (Mathf.PI / 2));
        }

        /// <summary>
        /// Textures transform used in background shader to get texture uv coordinates based on
        /// screen uv.
        /// The transformation follows these equations:
        /// textureUv.x = transform[0] * screenUv.x + transform[1],
        /// textureUv.y = transform[2] * screenUv.y + transform[3].
        /// </summary>
        /// <returns>The transform.</returns>
        private Vector4 TextureTransform()
        {
            float transitionWidthTransform = (_transitionImageTexture.width - Screen.width) /
                (2.0f * _transitionImageTexture.width);
            float transitionHeightTransform = (_transitionImageTexture.height - Screen.height) /
                (2.0f * _transitionImageTexture.height);
            return new Vector4(
                (float)Screen.width / _transitionImageTexture.width,
                transitionWidthTransform,
                (float)Screen.height / _transitionImageTexture.height,
                transitionHeightTransform);
        }

        private void EnableARBackgroundRendering()
        {
            if (BackgroundMaterial == null || _camera == null)
            {
                return;
            }

            _cameraClearFlags = _camera.clearFlags;
            _camera.clearFlags = CameraClearFlags.Depth;

            _commandBuffer = new CommandBuffer();

#if UNITY_ANDROID
            if (SystemInfo.graphicsMultiThreaded && !InstantPreviewManager.IsProvidingPlatform)
            {
                _commandBuffer.IssuePluginEvent(ExternApi.ARCoreRenderingUtils_GetRenderEventFunc(),
                                                (int)ApiRenderEvent.WaitOnPostUpdateFence);
#if UNITY_2018_2_OR_NEWER
                // There is a bug in Unity that IssuePluginEvent will reset the opengl state but it
                // doesn't respect the value set to GL.invertCulling. Hence we need to reapply
                // the invert culling in the command buffer for front camera session.
                // Note that the CommandBuffer.SetInvertCulling is only available for 2018.2+.
                var sessionComponent = LifecycleManager.Instance.SessionComponent;
                if (sessionComponent != null &&
                    sessionComponent.DeviceCameraDirection == DeviceCameraDirection.FrontFacing)
                {
                    _commandBuffer.SetInvertCulling(true);
                }
#endif
            }

#endif
            _commandBuffer.Blit(null,
                BuiltinRenderTextureType.CameraTarget, BackgroundMaterial);

            _camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
            _camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, _commandBuffer);
        }

        private void DisableARBackgroundRendering()
        {
            if (_commandBuffer == null || _camera == null)
            {
                return;
            }

            _camera.clearFlags = _cameraClearFlags;

            _camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
            _camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, _commandBuffer);
        }

#if UNITY_ANDROID
        private struct ExternApi {
            [DllImport(ApiConstants.ARRenderingUtilsApi)]
            public static extern IntPtr ARCoreRenderingUtils_GetRenderEventFunc();
        }
#endif
    }
}
