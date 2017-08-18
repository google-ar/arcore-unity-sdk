//-----------------------------------------------------------------------
// <copyright file="ApiServiceErrorStatus.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
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
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Error status codes returned by Tango API functions.
    /// </summary>
    public enum ApiServiceErrorStatus
    {
        /// <summary>
        /// No error, success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// General error state.
        /// </summary>
        Error = -1,

        /// <summary>
        /// General invalid state.
        /// </summary>
        Invalid = -2,

        /// <summary>
        /// Motion tracking not allowed.
        /// </summary>
        NoMotionTrackingPermission = -3,

        /// <summary>
        /// ADF access not allowed.
        /// </summary>
        NoAdfPermission = -4,

        /// <summary>
        /// Camera access not allowed.
        /// </summary>
        NoCameraPermission = -5,
    }
}
