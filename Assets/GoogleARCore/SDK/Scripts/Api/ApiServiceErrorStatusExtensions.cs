//-----------------------------------------------------------------------
// <copyright file="ApiServiceErrorStatusExtensions.cs" company="Google">
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
    using GoogleARCore;

    /// <summary>
    /// A class containing utility extension methods.
    /// </summary>
    public static class ApiServiceErrorStatusExtensions
    {
        /// <summary>
        /// Checks if an integer is equal to TANGO_SUCCESS.
        /// </summary>
        /// <param name="status">The status integer.</param>
        /// <returns><c>true</c> if the calling integer is TANGO_SUCCESS, otherwise <c>false</c>.</returns>
        public static bool IsTangoSuccess(this ApiServiceErrorStatus status)
        {
            return status == ApiServiceErrorStatus.Success;
        }

        /// <summary>
        /// Checks if an integer is NOT equal to TANGO_SUCCESS.
        /// </summary>
        /// <param name="status">The status integer.</param>
        /// <returns><c>true</c> if the calling integer is NOT TANGO_SUCCESS, otherwise <c>false</c>.</returns>
        public static bool IsTangoFailure(this ApiServiceErrorStatus status)
        {
            return status != ApiServiceErrorStatus.Success;
        }
    }
}
