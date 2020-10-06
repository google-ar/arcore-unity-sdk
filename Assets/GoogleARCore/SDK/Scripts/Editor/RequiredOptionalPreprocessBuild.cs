//-----------------------------------------------------------------------
// <copyright file="RequiredOptionalPreprocessBuild.cs" company="Google LLC">
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

namespace GoogleARCoreInternal
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    internal class RequiredOptionalPreprocessBuild : PreprocessBuildBase
    {
        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            var isARCoreRequired = ARCoreProjectSettings.Instance.IsARCoreRequired;

            Debug.LogFormat(
                "Building \"{0}\" app. Use 'Edit > Project Settings > ARCore' to adjust " +
                "ARCore SDK for Unity settings.\n" +
                "See {1} for more information.",
                isARCoreRequired ? "AR Required" : "AR Optional",
                "https://developers.google.com/ar/develop/unity/enable-arcore");

            AssetHelper.GetPluginImporterByName("google_ar_required.aar")
                .SetCompatibleWithPlatform(BuildTarget.Android, isARCoreRequired);
            AssetHelper.GetPluginImporterByName("google_ar_optional.aar")
                .SetCompatibleWithPlatform(BuildTarget.Android, !isARCoreRequired);
        }
    }
}
