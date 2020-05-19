//-----------------------------------------------------------------------
// <copyright file="CloudAnchorMode.cs" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    /// Defines possible modes for ARCore Cloud Anchors.
    /// </summary>
    public enum CloudAnchorMode
    {
        /// <summary>
        /// Cloud Anchors are disabled. This is the default value.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Cloud Anchors are enabled, HostCloudAnchor() and ResolveCloudAnchor() functions are
        /// available. The app is expected to have the INTERNET permission (Android only).
        /// </summary>
        Enabled = 1,

    }
}
