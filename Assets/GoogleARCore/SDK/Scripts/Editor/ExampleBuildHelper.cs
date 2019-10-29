//-----------------------------------------------------------------------
// <copyright file="ExampleBuildHelper.cs" company="Google">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    internal class ExampleBuildHelper : PreprocessBuildBase
    {
        private List<ExampleScene> m_ExampleScenes = new List<ExampleScene>();

        internal List<ExampleScene> AllExampleScenes
        {
            get
            {
                return m_ExampleScenes;
            }
        }

        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
        }

        protected void _AddExampleScene(ExampleScene scene)
        {
            m_ExampleScenes.Add(scene);
        }

        protected void _DoPreprocessBuild(BuildTarget target, string path)
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

            List<Texture2D> exampleSceneIcons = new List<Texture2D>();
            List<string> exampleProductNames = new List<string>();
            foreach (var exampleScene in m_ExampleScenes)
            {
                exampleSceneIcons.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(
                    AssetDatabase.GUIDToAssetPath(exampleScene.IconGuid)));
                exampleProductNames.Add(exampleScene.ProductName);
            }

            string[] projectFolders = Application.dataPath.Split('/');
            string defaultProductName = projectFolders[projectFolders.Length - 2];
            if (PlayerSettings.productName != defaultProductName
                  && !exampleProductNames.Contains(PlayerSettings.productName))
            {
                return;
            }

            Texture2D[] applicationIcons =
                PlayerSettings.GetIconsForTargetGroup(buildTargetGroup, IconKind.Application);

            for (int i = 0; i < applicationIcons.Length; i++)
            {
                if (applicationIcons[i] != null && !exampleSceneIcons.Contains(applicationIcons[i]))
                {
                    return;
                }
            }

            foreach (var exampleScene in m_ExampleScenes)
            {
                if (enabledBuildScene.guid.ToString() == exampleScene.SceneGuid)
                {
                    PlayerSettings.productName = exampleScene.ProductName;

                    Texture2D exampleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        AssetDatabase.GUIDToAssetPath(exampleScene.IconGuid));

                    for (int i = 0; i < applicationIcons.Length; i++)
                    {
                        applicationIcons[i] = exampleIcon;
                    }

                    PlayerSettings.SetIconsForTargetGroup(
                        buildTargetGroup, applicationIcons, IconKind.Application);
                    break;
                }
            }
        }

        internal struct ExampleScene
        {
            public string ProductName;
            public string PackageName;
            public string SceneGuid;
            public string IconGuid;
        }
    }
}
