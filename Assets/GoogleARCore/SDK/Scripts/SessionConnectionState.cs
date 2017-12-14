//-----------------------------------------------------------------------
// <copyright file="SessionConnectionState.cs" company="Google">
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
    /// Possible states for the ARCore session connection.
    /// </summary>
    public enum SessionConnectionState
    {
        /// <summary>
        /// The ARCore session has not been initialized.
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        /// The ARCore session is connected.
        /// </summary>
        Connected,

        /// <summary>
        /// The ARCore session failed to connect because no configuration was supplied.
        /// </summary>
        MissingConfiguration,

        /// <summary>
        /// The ARCore session failed to connect because an invalid configuration was supplied.
        /// </summary>
        InvalidConfiguration,

        /// <summary>
        /// The ARCore session failed to connect because the user rejected at least one needed permission.
        /// </summary>
        UserRejectedNeededPermission,

        /// <summary>
        /// The ARCore session failed to connect for unknown reason.
        /// </summary>
        ConnectToServiceFailed,
    }
}