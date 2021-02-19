//-----------------------------------------------------------------------
// <copyright file="DepthStatus.cs" company="Google LLC">
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
    /// Status for an attempt to retrieve the current depth texture.
    /// </summary>
    public enum DepthStatus
    {
        /// <summary>
        /// The depth image was retrieved successfully.
        /// </summary>
        Success,

        /// <summary>
        /// An internal error occurred, so the depth image was not updated. This could
        /// occur if the session is not ready, or in the event of other internal
        /// errors.
        /// </summary>
        InternalError,

        /// <summary>
        /// The depth information is not available for this frame, the number of
        /// observed camera frames is not yet sufficient for depth estimation, or
        /// depth estimation was not possible due to poor lighting, camera
        /// occlusion, or no motion observed.
        /// </summary>
        NotYetAvailable,

        /// <summary>
        /// <c><see cref="Session.Status"/></c> is not <c><see cref="SessionStatus.Tracking"/></c>,
        /// which is required to acquire depth images.
        /// </summary>
        NotTracking,

        /// <summary>
        /// A supported depth mode was not enabled in Session configuration.
        /// </summary>
        IllegalState,
    }
}
