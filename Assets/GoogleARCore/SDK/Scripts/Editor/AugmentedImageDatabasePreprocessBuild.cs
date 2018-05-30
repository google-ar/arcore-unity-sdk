//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabasePreprocessBuild.cs" company="Google">
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
    using GoogleARCore;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class AugmentedImageDatabasePreprocessBuild : IPreprocessBuildWithReport
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
            var augmentedImageDatabaseGuids = AssetDatabase.FindAssets("t:AugmentedImageDatabase");
            foreach (var databaseGuid in augmentedImageDatabaseGuids)
            {
                var database = AssetDatabase.LoadAssetAtPath<AugmentedImageDatabase>(
                    AssetDatabase.GUIDToAssetPath(databaseGuid));

                string error;
                database.BuildIfNeeded(out error);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new BuildFailedException(error);
                }
            }
        }
#endif

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            var augmentedImageDatabaseGuids = AssetDatabase.FindAssets("t:AugmentedImageDatabase");
            foreach (var databaseGuid in augmentedImageDatabaseGuids)
            {
                var database = AssetDatabase.LoadAssetAtPath<AugmentedImageDatabase>(
                    AssetDatabase.GUIDToAssetPath(databaseGuid));

                string error;
                database.BuildIfNeeded(out error);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new BuildFailedException(error);
                }
            }
        }

    }
}
