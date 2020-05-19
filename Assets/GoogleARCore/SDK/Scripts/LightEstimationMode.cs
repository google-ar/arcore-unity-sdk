//-----------------------------------------------------------------------
// <copyright file="LightEstimationMode.cs" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    /// <summary>
    /// Defines possible modes for ARCore real-world light estimation.
    /// </summary>
    public enum LightEstimationMode
    {
        /// <summary>
        /// A mode where light estimation is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Lighting estimation is enabled, generating ambient intensity
        /// and color-correction estimation.
        /// </summary>
        AmbientIntensity = 1,

        /// <summary>
        /// Lighting estimation is enabled. ARCore will estimate lighting
        /// to provide directional light and ambient spherical harmonics.
        /// This LightEstimationMode is incompatible with the front-facing (selfie) camera.
        /// </summary>
        EnvironmentalHDRWithoutReflections = 2,

        /// <summary>
        /// Lighting estimation is enabled. ARCore will estimate lighting
        /// to provide directional light, ambient spherical harmonics, and reflection
        /// cubemap estimation.
        /// This LightEstimationMode is incompatible with front-facing (selfie) camera.
        /// </summary>
        EnvironmentalHDRWithReflections = 3,
    }
}
