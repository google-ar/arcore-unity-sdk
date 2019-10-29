//-----------------------------------------------------------------------
// <copyright file="FeaturePointOrientationMode.cs" company="Google">
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
    /// <summary>
    /// The orientation mode of the feature point.
    /// </summary>
    public enum FeaturePointOrientationMode
    {
        /// <summary>
        /// The orientation of the feature point is initialized to identity but may
        /// adjust slightly over time.
        /// </summary>
        Identity = 0,

        /// <summary>
        /// The orientation of the feature point will follow the behavior that X+ is perpendicular
        /// to the cast ray and parallel to the physical surface centered around the hit test,
        /// Y+ points along the estimated surface normal, and Z+ points roughly toward
        /// the user's device.
        /// </summary>
        SurfaceNormal = 1,
    }
}
