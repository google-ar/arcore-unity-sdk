//-----------------------------------------------------------------------
// <copyright file="LightEstimate.cs" company="Google">
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

    /// <summary>
    /// An estimate of lighting conditions in the environment corresponding to
    /// an AR frame.
    /// </summary>
    public struct LightEstimate
    {
        /// <summary>
        /// Constructor for a LightEstimate.
        /// </summary>
        /// <param name="state">State of the estimate.</param>
        /// <param name="pixelIntensity">Average pixel intensity. Values range from 0.0
        /// to 1.0, where 0.0 represents black and 1.0 represents white.</param>
        public LightEstimate(LightEstimateState state, float pixelIntensity)
        {
            State = state;
            PixelIntensity = pixelIntensity;
        }

        /// <summary>
        /// Gets the state of the current estimate.
        /// </summary>
        public LightEstimateState State { get; private set; }

        /// <summary>
        /// Gets an average pixel intensity. Values range from 0.0 to 1.0, where 0.0
        /// represents black and 1.0 represents white.
        /// </summary>
        public float PixelIntensity { get; private set; }
    }
}
