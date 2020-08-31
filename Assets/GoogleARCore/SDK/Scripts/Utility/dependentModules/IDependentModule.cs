//-----------------------------------------------------------------------
// <copyright file="IDependentModule.cs" company="Google LLC">
//
// Copyright 2020 Google LLC. All Rights Reserved.
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
    using System;
    using System.Xml;
    using System.Xml.Linq;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// The interface defines one feature module.
    /// </summary>
    public interface IDependentModule
    {
        /// <summary>
        /// Checking whether it needs to be included in the customized AndroidManifest.
        /// The default values for new fields in ARCoreProjectSettings should cause the
        /// associated module to return false.
        /// </summary>
        /// <param name="settings">ARCore Project Settings.</param>
        /// <returns>The boolean shows whether the module is enabled.</returns>
        bool IsEnabled(ARCoreProjectSettings settings);

        /// <summary>
        /// Return the XML snippet needs to be included if this module is enabled.
        /// The string output would be added as a child node of in the ‘manifest’ node
        /// of the customized AndroidManifest.xml. The android namespace would be provided
        /// and feature developers could use it directly.
        /// </summary>
        /// <param name="settings">ARCore Project Settings.</param>
        /// <returns>The XML string snippet to add as a child of node 'manifest'.</returns>
        string GetAndroidManifestSnippet(ARCoreProjectSettings settings);

        /// <summary>
        /// Checking whether this module is compatible with sessionConfig. If it returns false,
        /// the preprocessbuild will throw a general Build Failure Error. A feature developer
        /// should use this function to log detailed error messages that also include a
        /// recommendation of how to resolve the issue.
        /// </summary>
        /// <param name="settings">ARCore Project Settings.</param>
        /// <param name="sessionConfig">ARCore SessionConfig.</param>
        /// <returns>The boolean shows whether the ARCoreProjectSettings is compatible
        /// with ARCoreSessionConfig.</returns>
        bool IsCompatibleWithSessionConfig(
            ARCoreProjectSettings settings, ARCoreSessionConfig sessionConfig);
    }
}
