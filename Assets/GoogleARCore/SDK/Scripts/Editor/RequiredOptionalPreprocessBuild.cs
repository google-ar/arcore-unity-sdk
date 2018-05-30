﻿//-----------------------------------------------------------------------
// <copyright file="RequiredOptionalPreprocessBuild.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    internal class RequiredOptionalPreprocessBuild : IPreprocessBuild
    {
        [SuppressMessage("UnityRules.UnityStyleRules", "US1000:FieldsMustBeUpperCamelCase",
         Justification = "Overriden property.")]
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

#if UNITY_2018
        public void OnPreprocessBuild(BuildReport report)
        {
            var isARCoreRequired = ARCoreProjectSettings.Instance.IsARCoreRequired;

            Debug.LogFormat("Building application with {0} ARCore support.",
                isARCoreRequired ? "REQUIRED" : "OPTIONAL");

            AssetHelper.GetPluginImporterByName("google_ar_required.aar")
                .SetCompatibleWithPlatform(BuildTarget.Android, isARCoreRequired);
            AssetHelper.GetPluginImporterByName("google_ar_optional.aar")
                .SetCompatibleWithPlatform(BuildTarget.Android, !isARCoreRequired);
        }
#endif

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            var isARCoreRequired = ARCoreProjectSettings.Instance.IsARCoreRequired;

            Debug.LogFormat("Building application with {0} ARCore support.",
                isARCoreRequired ? "REQUIRED" : "OPTIONAL");

            AssetHelper.GetPluginImporterByName("google_ar_required.aar")
                .SetCompatibleWithPlatform(BuildTarget.Android, isARCoreRequired);
            AssetHelper.GetPluginImporterByName("google_ar_optional.aar")
                .SetCompatibleWithPlatform(BuildTarget.Android, !isARCoreRequired);
        }

    }
}
