//-----------------------------------------------------------------------
// <copyright file="CloudAnchorResult.cs" company="Google LLC">
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

namespace GoogleARCore.CrossPlatform
{
    using System;
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// The result of a cloud service operation that returns a cloud anchor.
    /// </summary>
    public struct CloudAnchorResult
    {
        /// <summary>
        /// The response from the cloud service.
        /// </summary>
        public CloudServiceResponse Response;

        /// <summary>
        /// The anchor return from the operation. This will be a valid cloud anchor if <c>Response</c> is
        /// <c>CloudServiceResponse.Success</c> and null otherwise.
        /// </summary>
        public XPAnchor Anchor;
    }
}
