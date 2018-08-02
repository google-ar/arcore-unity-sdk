//-----------------------------------------------------------------------
// <copyright file="EnvironmentalLight.cs" company="Google">
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
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// A component that automatically adjust lighting settings for the scene
    /// to be inline with those estimated by ARCore.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://developers.google.com/ar/reference/unity/class/GoogleARCore/EnvironmentalLight")]
    public class EnvironmentalLight : MonoBehaviour
    {
        /// <summary>
        /// Unity update method that sets global light estimation shader constant to match
        /// ARCore's calculated values.
        /// </summary>
        public void Update()
        {
            if (Application.isEditor && (!Application.isPlaying ||
                 !GoogleARCoreInternal.ARCoreProjectSettings.Instance.IsInstantPreviewEnabled))
            {
                // Set _GlobalColorCorrection to white in editor, if the value is not set, all materials
                // using light estimation shaders will be black.
                Shader.SetGlobalColor("_GlobalColorCorrection", Color.white);

                // Set _GlobalLightEstimation for backward compatibility.
                Shader.SetGlobalFloat("_GlobalLightEstimation", 1f);
                return;
            }

            if (Frame.LightEstimate.State != LightEstimateState.Valid)
            {
                return;
            }

            // Normalize pixel intensity by middle gray in gamma space.
            const float middleGray = 0.466f;
            float normalizedIntensity = Frame.LightEstimate.PixelIntensity / middleGray;

            // Apply color correction along with normalized pixel intensity in gamma space.
            Shader.SetGlobalColor("_GlobalColorCorrection", Frame.LightEstimate.ColorCorrection * normalizedIntensity);

            // Set _GlobalLightEstimation for backward compatibility.
            Shader.SetGlobalFloat("_GlobalLightEstimation", normalizedIntensity);
        }
    }
}
