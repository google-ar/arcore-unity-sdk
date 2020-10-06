//-----------------------------------------------------------------------
// <copyright file="EnvironmentalLight.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using GoogleARCoreInternal;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// A component that automatically adjusts lighting settings for the scene
    /// to be inline with those estimated by ARCore.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL(
        "https://developers.google.com/ar/reference/unity/class/GoogleARCore/EnvironmentalLight")]
    public class EnvironmentalLight : MonoBehaviour
    {
        /// <summary>
        /// The directional light used by
        /// <see cref="LightEstimationMode"/>.<c>EnvironmentalHDRWithReflections</c> and
        /// <see cref="LightEstimationMode"/>.<c>EnvironmentalHDRWithoutReflections</c>.
        /// The rotation and color will be updated automatically by this component.
        /// </summary>
        public Light DirectionalLight;

        private long _lightEstimateTimestamp = -1;

        /// <summary>
        /// Unity update method that sets global light estimation shader constant and
        /// <a href="https://docs.unity3d.com/ScriptReference/RenderSettings.html">
        /// RenderSettings</a> to match ARCore's calculated values.
        /// </summary>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Requires further investigation.")]
        public void Update()
        {
            if (Application.isEditor && (!Application.isPlaying ||
                 !ARCoreProjectSettings.Instance.IsInstantPreviewEnabled))
            {
                // Set _GlobalColorCorrection to white in editor, if the value is not set, all
                // materials using light estimation shaders will be black.
                Shader.SetGlobalColor("_GlobalColorCorrection", Color.white);

                // Set _GlobalLightEstimation for backward compatibility.
                Shader.SetGlobalFloat("_GlobalLightEstimation", 1f);
                return;
            }

            LightEstimate estimate = Frame.LightEstimate;
            if (estimate.State != LightEstimateState.Valid ||
                estimate.Mode == LightEstimationMode.Disabled)
            {
                return;
            }

            if (estimate.Mode == LightEstimationMode.AmbientIntensity)
            {
                // Normalize pixel intensity by middle gray in gamma space.
                const float middleGray = 0.466f;
                float normalizedIntensity = estimate.PixelIntensity / middleGray;

                // Apply color correction along with normalized pixel intensity in gamma space.
                Shader.SetGlobalColor(
                    "_GlobalColorCorrection",
                    estimate.ColorCorrection * normalizedIntensity);

                // Set _GlobalLightEstimation for backward compatibility.
                Shader.SetGlobalFloat("_GlobalLightEstimation", normalizedIntensity);
            }
            else if (_lightEstimateTimestamp != estimate.Timestamp)
            {
                _lightEstimateTimestamp = estimate.Timestamp;
                if (DirectionalLight != null)
                {
                    if (!DirectionalLight.gameObject.activeSelf || !DirectionalLight.enabled)
                    {
                        DirectionalLight.gameObject.SetActive(true);
                        DirectionalLight.enabled = true;
                    }

                    DirectionalLight.transform.rotation = estimate.DirectionalLightRotation;
                    DirectionalLight.color = estimate.DirectionalLightColor;
                }

                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientProbe = estimate.AmbientProbe;

                if (estimate.Mode == LightEstimationMode.EnvironmentalHDRWithReflections)
                {
                    RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                    RenderSettings.customReflection = estimate.ReflectionProbe;
                }
            }
        }
    }
}
