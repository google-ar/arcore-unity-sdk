//-----------------------------------------------------------------------
// <copyright file="DepthMode.cs" company="Google LLC">
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

namespace GoogleARCore
{
    /// <summary>
    /// Desired depth mode.
    /// </summary>
    public enum DepthMode
    {
        /// <summary>
        /// Depth is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// On supported devices the best possible depth is estimated based on hardware
        /// and software sources. Available sources of automatic depth are:
        ///  - Depth from motion, using the main RGB camera
        ///  - Hardware depth sensor, such as a time-of-flight sensor (or ToF sensor)
        /// Provides depth estimation for every pixel in the image, and works best
        /// for static scenes. For a list of supported devices, see:
        ///  https://developers.google.com/ar/devices
        /// Adds significant computational load.
        /// </summary>
        Automatic = 1,

        /// <summary>
        /// On <a href="https://developers.google.com/ar/devices">ARCore
        /// supported devices</a> that also support the Depth API, provides a "raw", mostly
        /// unfiltered, depth image (<c>Frame.UpdateRawDepthTexture</c>) and depth confidence image
        /// (<c>Frame.UpdateRawDepthConfidenceTexture</c>).
        ///
        /// The raw depth image is sparse and does not provide valid depth for all pixel. Pixels
        /// without a valid depth estimate have a pixel value of 0.
        ///
        /// Raw depth data is also available when <c><see cref="Automatic"/></c> is selected,
        /// however <c>RawDepthOnly</c> has a lower runtime performance cost.
        ///
        /// Raw depth is intended to be used in cases that involve understanding of the geometry in
        /// the environment.
        /// </summary>
        RawDepthOnly = 3,
  }
}

