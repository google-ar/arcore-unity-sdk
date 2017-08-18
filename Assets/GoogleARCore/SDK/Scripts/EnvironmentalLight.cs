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
    public class EnvironmentalLight : MonoBehaviour
    {
        /// <summary>
        /// Unity update method that sets global light estimation shader constant to match
        /// ARCore's calculated values.
        /// </summary>
        public void Update()
        {
#if UNITY_EDITOR
            // Set _GlobalLightEstimation to 1 in editor, if the value is not set, all materials
            // using light estimation shaders will be black.
            Shader.SetGlobalFloat("_GlobalLightEstimation", 1.0f);
#else
            if (Frame.TrackingState != FrameTrackingState.Tracking)
            {
                return;
            }

            // Use the following function to compute color scale:
            // * linear growth from (0.0, 0.0) to (1.0, LinearRampThreshold)
            // * slow growth from (1.0, LinearRampThreshold)
            const float LinearRampThreshold = 0.8f;
            const float MiddleGray = 0.18f;
            const float Inclination = 0.4f;

            float normalizedIntensity = Frame.LightEstimate.PixelIntensity / MiddleGray;
            float colorScale = 1.0f;

            if (normalizedIntensity < 1.0f)
            {
                colorScale = normalizedIntensity * LinearRampThreshold;
            }
            else
            {
                float b = LinearRampThreshold / Inclination - 1.0f;
                float a = (b + 1.0f) / b * LinearRampThreshold;
                colorScale = a * (1.0f - (1.0f / (b * normalizedIntensity + 1.0f)));
            }

            Shader.SetGlobalFloat("_GlobalLightEstimation", colorScale);
#endif
        }
    }
}

