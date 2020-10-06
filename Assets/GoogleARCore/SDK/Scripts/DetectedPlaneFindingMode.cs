//-----------------------------------------------------------------------
// <copyright file="DetectedPlaneFindingMode.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    /// Selects the behavior of the plane detection subsystem.
    /// </summary>
    public enum DetectedPlaneFindingMode
    {
        /// <summary>
        /// Plane detection is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Detection of both horizontal and vertical planes is enabled.
        /// </summary>
        HorizontalAndVertical = 1,

        /// <summary>
        /// Detection of only horizontal planes is enabled.
        /// </summary>
        Horizontal = 2,

        /// <summary>
        /// Detection of only vertical planes is enabled.
        /// </summary>
        Vertical = 3,

    }
}
