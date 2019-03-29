//-----------------------------------------------------------------------
// <copyright file="SessionStatusExtensions.cs" company="Google">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Extension methods for the SessionStatus enumeration.
    /// </summary>
    public static class SessionStatusExtensions
    {
        private const int k_NotInitializedGroupStart = 0;
        private const int k_ValidSessionGroupStart = 100;
        private const int k_ErrorGroupStart = 200;

        /// <summary>
        /// Gets whether a SessionStatus is not yet initialized.
        /// </summary>
        /// <param name="status">The SessionStatus to check.</param>
        /// <returns><c>true</c> if the SessionStatus is not initialized, otherwise
        /// <c>false</c>.</returns>
        public static bool IsNotInitialized(this SessionStatus status)
        {
            int normalizedValue = (int)status - k_NotInitializedGroupStart;
            return normalizedValue >= 0 && normalizedValue < 100;
        }

        /// <summary>
        /// Gets whether a SessionStatus is initialized and valid.
        /// </summary>
        /// <param name="status">The SessionStatus to check.</param>
        /// <returns><c>true</c> if the SessionStatus is initialized and valid,
        /// otherwise <c>false</c>.</returns>
        public static bool IsValid(this SessionStatus status)
        {
            int normalizedValue = (int)status - k_ValidSessionGroupStart;
            return normalizedValue >= 0 && normalizedValue < 100;
        }

        /// <summary>
        /// Gets whether a SessionStatus is an error.
        /// </summary>
        /// <param name="status">The SessionStatus to check.</param>
        /// <returns><c>true</c> if the SessionStatus is an error,
        /// otherwise <c>false</c>.</returns>
        public static bool IsError(this SessionStatus status)
        {
            int normalizedValue = (int)status - k_ErrorGroupStart;
            return normalizedValue >= 0 && normalizedValue < 100;
        }
    }
}
