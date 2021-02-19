//-----------------------------------------------------------------------
// <copyright file="LightEstimate.cs" company="Google LLC">
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
    /// An estimate of lighting conditions in the environment corresponding to
    /// an AR frame.
    /// </summary>
    public struct LightEstimate
    {
        private float _pixelIntensity;
        private Color _colorCorrection;

        private Quaternion _directionalLightRotation;
        private Color _directionalLightColor;
        private SphericalHarmonicsL2 _ambientProbe;
        private Cubemap _cachedCubemap;

        /// <summary>
        /// Constructor for a LightEstimate.
        /// </summary>
        /// <param name="state">State of the estimate.</param>
        /// <param name="pixelIntensity">Average pixel intensity. Values range from 0.0
        /// to 1.0, where 0.0 represents black and 1.0 represents white.</param>
        /// <param name="colorCorrection">Color correction RGB scaling factors to be
        /// applied to the final color computed by the fragment shader to match the
        /// ambient color. The color correction method uses the green channel as
        /// reference baseline and scales the red and blue channels accordingly. In this way
        /// the overall intensity will not be significantly changed.
        /// </param>
        /// @deprecated Please use constructor LightEstimate(LightEstimateState, float, Color,
        /// Quaternion, Color, float[,], long) instead.
        [System.Obsolete("LightEstimate(LightEstimateState, float, Color) has been deprecated. " +
             "Please use new constructor instead.")]
        public LightEstimate(
            LightEstimateState state, float pixelIntensity, Color colorCorrection) :
            this(state, pixelIntensity, colorCorrection, Quaternion.identity, Color.black, null, -1)
        {
        }

        /// <summary>
        /// Constructor for a LightEstimate.
        /// </summary>
        /// <param name="state">State of the estimate.</param>
        /// <param name="pixelIntensity">Average pixel intensity. Values range from 0.0
        /// to 1.0, where 0.0 represents black and 1.0 represents white.</param>
        /// <param name="colorCorrection">Color correction RGB scaling factors to be
        /// applied to the final color computed by the fragment shader to match the
        /// ambient color. The color correction method uses the green channel as
        /// reference baseline and scales the red and blue channels accordingly. In this way
        /// the overall intensity will not be significantly changed.
        /// </param>
        /// <param name="directionalLightRotation">The rotation of the main directional light
        /// estimated by ARCore.</param>
        /// <param name="directionalLightColor">The color of the main directional light estimated
        /// by ARCore.</param>
        /// <param name="ambientSHCoefficients">A 2D float[3, 9] array that store the spherical
        /// harmonics coefficients estimated by ARCore. </param>
        /// <param name="timestamp">The timestamp of the LightEstimate.</param>
        public LightEstimate(
            LightEstimateState state, float pixelIntensity, Color colorCorrection,
            Quaternion directionalLightRotation, Color directionalLightColor,
            float[,] ambientSHCoefficients, long timestamp) : this()
        {
            InitializeLightEstimateMode();
            State = state;
            Timestamp = timestamp;
            _pixelIntensity = pixelIntensity;
            _colorCorrection = colorCorrection;

            _directionalLightRotation = directionalLightRotation;

            // Apply the energy conservation term to the light color directly since Unity doesn't
            // apply the term in their standard shader.
            _directionalLightColor = directionalLightColor;

            // Apply gamma correction to the light color. Unity light color is in gamma space.
            _directionalLightColor = _directionalLightColor.gamma;

            // Unity spherical harmonics is in linear space.
            var ambientProbe = new SphericalHarmonicsL2();
            if (ambientSHCoefficients != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        ambientProbe[i, j] = ambientSHCoefficients[j, i];
                    }
                }
            }

            _ambientProbe = ambientProbe;
            _cachedCubemap = null;
        }

        /// <summary>
        /// Gets the light estimation mode for the current session.
        /// </summary>
        /// <value>The light estimation mode.</value>
        public LightEstimationMode Mode { get; private set; }

        /// <summary>
        /// Gets the state of the current estimate.
        /// </summary>
        public LightEstimateState State { get; private set; }

        /// <summary>
        /// Gets an average pixel intensity. Values range from 0.0 to 1.0, where 0.0
        /// represents black and 1.0 represents white.
        /// </summary>
        public float PixelIntensity
        {
            get
            {
                if (Mode != LightEstimationMode.AmbientIntensity)
                {
                    Debug.LogWarning("PixelIntensity value is not meaningful when " +
                        "LightEstimationMode is not AmbientIntensity.");
                }

                return _pixelIntensity;
            }

            private set
            {
                _pixelIntensity = value;
            }
        }

        /// <summary>
        /// Gets the color correction RGB scaling factors to be applied to the final color
        /// computed by the fragment shader to match the ambient color.
        /// The color correction method uses the green channel as reference baseline and
        /// scales the red and blue channels accordingly. In this way the overall intensity
        /// will not be significantly changed.
        /// </summary>
        public Color ColorCorrection
        {
            get
            {
                if (Mode != LightEstimationMode.AmbientIntensity)
                {
                    Debug.LogWarning("ColorCorrection value is not meaningful when " +
                        "LightEstimationMode is not AmbientIntensity.");
                }

                return _colorCorrection;
            }

            private set
            {
                _colorCorrection = value;
            }
        }

        /// <summary>
        /// Gets the quaternion rotation of the main directional light estimated by
        /// ARCore. It will return Quaternion.identity when the LightEstimateState is invalid
        /// or LightEstimationMode is not one of the Environmental HDR modes.
        /// </summary>
        public Quaternion DirectionalLightRotation
        {
            get
            {
                if (Mode != LightEstimationMode.EnvironmentalHDRWithoutReflections &&
                    Mode != LightEstimationMode.EnvironmentalHDRWithReflections)
                {
                    Debug.LogWarning("DirectionalLightRotation value is not meaningful when " +
                        "LightEstimationMode is not one of the Environmental HDR modes.");
                }

                return _directionalLightRotation;
            }

            private set
            {
                _directionalLightRotation = value;
            }
        }

        /// <summary>
        /// Gets the estimated color of the main directional light in the world.
        /// It will return black when the LightEstimateState is invalid or LightEstimationMode is
        /// not one of the Environmental HDR modes.
        /// </summary>
        public Color DirectionalLightColor
        {
            get
            {
                if (Mode != LightEstimationMode.EnvironmentalHDRWithoutReflections &&
                    Mode != LightEstimationMode.EnvironmentalHDRWithReflections)
                {
                    Debug.LogWarning("DirectionalLightColor value is not meaningful when " +
                        "LightEstimationMode is not one of the Environmental HDR modes.");
                }

                return _directionalLightColor;
            }

            private set
            {
                _directionalLightColor = value;
            }
        }

        /// <summary>
        /// Gets ambient spherical harmonics estimated by ARCore. It will return a spherical
        /// harmonic probe with all 27 coefficients set to zero when the LightEstimateState is
        /// invalid or LightEstimationMode is not one of the Environmental HDR modes.
        /// </summary>
        public SphericalHarmonicsL2 AmbientProbe
        {
            get
            {
                if (Mode != LightEstimationMode.EnvironmentalHDRWithoutReflections &&
                    Mode != LightEstimationMode.EnvironmentalHDRWithReflections)
                {
                    Debug.LogWarning("AmbientProbe value is not meaningful when " +
                        "LightEstimationMode is not one of the Environmental HDR modes.");
                }

                return _ambientProbe;
            }

            private set
            {
                _ambientProbe = value;
            }
        }

        /// <summary>
        /// Gets the reflection cubemap from the light estimation.
        /// For performance reasons, the cubemap will only be created when this function
        /// is called on the LightEstimate object. It will return null
        /// when the LightEstimateState is invalid or LightEstimationMode is not
        /// EnvironmentalHDRWithReflections.
        /// </summary>
        public Cubemap ReflectionProbe
        {
            get
            {
                if (Mode != LightEstimationMode.EnvironmentalHDRWithReflections)
                {
                    Debug.LogWarning("ReflectionProbe value is not meaningful when " +
                        "LightEstimationMode is not EnvironmentalHDRWithReflections.");

                    return null;
                }

                if (_cachedCubemap == null)
                {
                    var nativeSession = LifecycleManager.Instance.NativeSession;
                    if (nativeSession == null)
                    {
                        return null;
                    }

                    _cachedCubemap = nativeSession.FrameApi.GetReflectionCubemap();
                }

                return _cachedCubemap;
            }
        }

        /// <summary>
        /// Gets the timestamp of the LightEstimate in nanoseconds.
        /// </summary>
        /// <remarks>
        /// ARCore returns a different timestamp when the underlying light estimate has changed.
        /// Conversely, the same timestamp is returned if the light estimate has not changed.
        ///
        /// This timestamp uses the same time base as frame timestamp.
        ///
        /// This timestamp is used to improve performance
        /// of the EnvironmentalHDRWithReflection light estimation mode, by ensuring that the
        /// Unity environmental reflections are only updated when ReflectionCubemap has changed.
        /// </remarks>
        /// <value>The timestamp of the LightEstimate.</value>
        public long Timestamp { get; private set; }

        private void InitializeLightEstimateMode()
        {
            Mode = LightEstimationMode.Disabled;
            if (LifecycleManager.Instance.SessionComponent != null)
            {
                Mode =
                    LifecycleManager.Instance.SessionComponent.SessionConfig.LightEstimationMode;
            }
        }
    }
}
