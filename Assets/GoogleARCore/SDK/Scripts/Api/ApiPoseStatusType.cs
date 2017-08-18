//-----------------------------------------------------------------------
// <copyright file="ApiPoseStatusType.cs" company="Google">
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
    /// <summary>
    /// Tango pose status lifecycle enumerations.
    ///
    /// Every pose has a state denoted by this enum, which provides information about the internal status of the
    /// position estimate. The application may use the status to decide what actions or rendering should be taken.
    /// A change in the status between poses and subsequent timestamps can denote lifecycle state changes. The
    /// status affects the rotation and position estimates. Other fields are considered valid (i.e. version or
    /// timestamp).
    /// </summary>
    public enum ApiPoseStatusType
    {
        /// <summary>
        /// Motion estimation is being initialized.
        /// </summary>
        Initializing,

        /// <summary>
        /// The pose of this estimate is valid.
        /// </summary>
        Valid,

        /// <summary>
        /// The pose of this estimate is not valid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Could not estimate pose at this time.
        /// </summary>
        Unknown,

        /// <summary>
        /// Not Available, not a real <c>TangoPoseStatusType</c>.
        /// </summary>
        NA
    }
}
