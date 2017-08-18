//-----------------------------------------------------------------------
// <copyright file="LightEstimateManager.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityTango = GoogleAR.UnityNative;

    /// <summary>
    /// A manager for light estimate state.
    /// </summary>
    public class LightEstimateManager
    {
        /// <summary>
        /// The latest light estimate for this frame.
        /// </summary>
        private LightEstimate m_latestLightEstimate;

        /// <summary>
        /// The unity frame count where <c>m_latestLightEstimate</c> was last updated.
        /// </summary>
        private int m_latestLightEstimateFrame = 0;

        /// <summary>
        /// Get the most recent light estimate.
        /// </summary>
        /// <returns></returns>
        public LightEstimate GetLatestLightEstimate()
        { 
            // Maintain frame consistency.
            if (Time.frameCount == m_latestLightEstimateFrame)
            {
                return m_latestLightEstimate;
            }

            m_latestLightEstimateFrame = Time.frameCount;

            UnityTango.NativeImage nativeImage = new UnityTango.NativeImage();
            if (!UnityTango.Device.TryAcquireLatestImageBuffer(ref nativeImage))
            {
                Debug.LogError("Unable to acquire image buffer.");
                return m_latestLightEstimate;
            }

            // The Y plane is always the first one.
            var yPlaneInfo = nativeImage.planeInfos[0];
            IntPtr yPlaneStart = new IntPtr(nativeImage.planeData.ToInt64() + yPlaneInfo.offset);
            float intensity;
            ApiServiceErrorStatus status = TangoClientApi.TangoService_getPixelIntensity(
                yPlaneStart,
                (int)nativeImage.width,
                (int)nativeImage.height,
                nativeImage.planeInfos[0].rowStride,
                out intensity);
            if (status != ApiServiceErrorStatus.Success)
            {
                Debug.LogErrorFormat("Call to getPixelIntensity failed: {0}.", status);
                return m_latestLightEstimate;
            }

            m_latestLightEstimate = new LightEstimate(intensity);

            UnityTango.Device.ReleaseImageBuffer(nativeImage);

            return m_latestLightEstimate;
        }
    }
}
