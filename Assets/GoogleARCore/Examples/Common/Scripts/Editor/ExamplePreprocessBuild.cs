//-----------------------------------------------------------------------
// <copyright file="ExamplePreprocessBuild.cs" company="Google">
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

namespace GoogleARCore.Examples.Common
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    internal class ExamplePreprocessBuild : IPreprocessBuild
    {
        private readonly List<ExampleScene> k_ExampleScenes = new List<ExampleScene>()
        {
            new ExampleScene()
            {
                ProductName = "HelloAR U3D",
                PackageName = "com.google.ar.core.examples.unity.helloar",
                SceneGuid = "e6a6fa04348cb45c9b0221eb19c946da",
                IconGuid = "36b7440e71f344bef8fca770c2d365f8"
            },
            new ExampleScene()
            {
                ProductName = "CV U3D",
                PackageName = "com.google.ar.core.examples.unity.computervision",
                SceneGuid = "5ef0f7f7f2c7b4285b707265348bbffd",
                IconGuid = "7c556c651080f499d9eaeea95d392d80"
            },
            new ExampleScene()
            {
                ProductName = "AugmentedImage U3D",
                PackageName = "com.google.ar.core.examples.unity.augmentedimage",
                SceneGuid = "be567d47d3ab94b3badc5b211f535a24",
                IconGuid = "0bf81216732894b46b8b5437b1acc57a"
            },
            new ExampleScene()
            {
                ProductName = "CloudAnchors U3D",
                PackageName = "com.google.ar.core.examples.unity.cloudanchors",
                SceneGuid = "83fb41cc294e74bdea57537befa00ffc",
                IconGuid = "dcfb8b44c93d547e2bdf8a638c1415af"
            },
        };

        [SuppressMessage("UnityRules.UnityStyleRules", "US1000:FieldsMustBeUpperCamelCase",
         Justification = "Overriden property.")]
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            BuildTargetGroup buildTargetGroup;
            if (target == BuildTarget.Android)
            {
                buildTargetGroup = BuildTargetGroup.Android;
            }
            else if (target == BuildTarget.iOS)
            {
                buildTargetGroup = BuildTargetGroup.iOS;
            }
            else
            {
                return;
            }

            EditorBuildSettingsScene enabledBuildScene = null;
            int enabledSceneCount = 0;
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                {
                    continue;
                }

                enabledBuildScene = buildScene;
                enabledSceneCount++;
            }

            if (enabledSceneCount != 1)
            {
                return;
            }

            foreach (var exampleScene in k_ExampleScenes)
            {
                if (enabledBuildScene.guid.ToString() == exampleScene.SceneGuid)
                {
                    PlayerSettings.SetApplicationIdentifier(buildTargetGroup, exampleScene.PackageName);
                    PlayerSettings.productName = exampleScene.ProductName;
                    var applicationIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        AssetDatabase.GUIDToAssetPath(exampleScene.IconGuid));

                    var icons = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup, IconKind.Application);
                    for (int i = 0; i < icons.Length; i++)
                    {
                        icons[i] = applicationIcon;
                    }

                    PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, icons, IconKind.Application);
                    break;
                }
            }
        }

        private struct ExampleScene
        {
            public string ProductName;
            public string PackageName;
            public string SceneGuid;
            public string IconGuid;
        }
    }
}
