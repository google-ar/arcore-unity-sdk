//-----------------------------------------------------------------------
// <copyright file="SessionConfig.cs" company="Google">
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
    using UnityEngine;

    /// <summary>
    /// Holds settings that are used to configure the session.
    /// </summary>
    [CreateAssetMenu(fileName = "ARCoreSessionConfig", menuName = "GoogleARCore/SessionConfig", order = 1)]
    public class SessionConfig : ScriptableObject
    {
        /// <summary>
        /// Toggles whether the color camera passthrough is enabled.
        /// </summary>
        [Space(10), Header("Color Camera"), Space(5)]
        [Tooltip("Toggles whether the color camera is rendered as an AR background.")]
        public bool m_enableARBackground = true;

        /// <summary>
        /// Toggles whether plane finding is enabled.
        /// </summary>
        [Space(10), Header("Services"), Space(5)]
        [Tooltip("Toggles whether plane finding is enabled.")]
        public bool m_enablePlaneFinding = true;

        /// <summary>
        /// Toggles whether point cloud is enabled.
        /// </summary>
        [Tooltip("Toggles whether point cloud is enabled.")]
        public bool m_enablePointcloud = true;
    }
}
