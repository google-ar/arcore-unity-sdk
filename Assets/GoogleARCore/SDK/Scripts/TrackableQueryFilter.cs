//-----------------------------------------------------------------------
// <copyright file="TrackableQueryFilter.cs" company="Google">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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
    using System;
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A filter for trackable queries.
    /// </summary>
    public enum TrackableQueryFilter
    {
        /// <summary>
        /// Indicates available trackables.
        /// </summary>
        All,

        /// <summary>
        /// Indicates new trackables detected in the current ARCore Frame.
        /// </summary>
        New,

        /// <summary>
        /// Indicates trackables that were updated in the current ARCore Frame.
        /// </summary>
        Updated,
    }
}
