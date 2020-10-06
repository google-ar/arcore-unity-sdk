//-----------------------------------------------------------------------
// <copyright file="AugmentedFaceRegion.cs" company="Google LLC">
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
    /// Defines face regions for which the pose can be queried. Left and right of an Augmented Face
    /// are defined relative to the person that the mesh belongs to.
    /// </summary>
    public enum AugmentedFaceRegion
    {
        /// <summary>
        /// A region around the nose of the AugmentedFace.
        /// </summary>
        NoseTip = 0,

        /// <summary>
        /// A region around the left forehead of the AugmentedFace.
        /// </summary>
        ForeheadLeft = 1,

        /// <summary>
        /// A region around the right forehead of the AugmentedFace.
        /// </summary>
        ForeheadRight = 2,
    }
}
