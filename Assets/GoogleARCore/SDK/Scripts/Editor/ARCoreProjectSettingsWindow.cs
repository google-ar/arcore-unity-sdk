//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettingsWindow.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using UnityEditor;
    using UnityEngine;

    internal class ARCoreProjectSettingsWindow : EditorWindow
    {
        [MenuItem("Edit/Project Settings/ARCore")]
        private static void ShowARCoreProjectSettingsWindow()
        {
            ARCoreProjectSettings.Instance.Load();
            Rect rect = new Rect(500, 300, 400, 200);
            ARCoreProjectSettingsWindow window = GetWindowWithRect<ARCoreProjectSettingsWindow>(rect);
            window.titleContent = new GUIContent("ARCore");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.stretchWidth = true;
            titleStyle.fontSize = 14;
            titleStyle.fixedHeight = 20;

            EditorGUILayout.LabelField("ARCore Project Settings", titleStyle);
            GUILayout.Space(15);

            ARCoreProjectSettings.Instance.IsARCoreRequired =
                EditorGUILayout.Toggle("ARCore Required", ARCoreProjectSettings.Instance.IsARCoreRequired);
            GUILayout.Space(5);

            ARCoreProjectSettings.Instance.IsInstantPreviewEnabled =
                EditorGUILayout.Toggle("Instant Preview enabled",
                                       ARCoreProjectSettings.Instance.IsInstantPreviewEnabled);
            GUILayout.Space(5);
            bool newARCoreIOSEnabled =
                EditorGUILayout.Toggle("IOS Support Enabled",
                                       ARCoreProjectSettings.Instance.IsIOSSupportEnabled);
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(3);

            EditorGUILayout.LabelField("Android Cloud Services API Key", GUILayout.Width(180));
            ARCoreProjectSettings.Instance.CloudServicesApiKey =
                EditorGUILayout.TextField(ARCoreProjectSettings.Instance.CloudServicesApiKey);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(3);

            EditorGUILayout.LabelField("iOS Cloud Services API Key", GUILayout.Width(180));
            ARCoreProjectSettings.Instance.IosCloudServicesApiKey =
                EditorGUILayout.TextField(ARCoreProjectSettings.Instance.IosCloudServicesApiKey);

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (GUI.changed)
            {
                if (newARCoreIOSEnabled != ARCoreProjectSettings.Instance.IsIOSSupportEnabled)
                {
                    ARCoreProjectSettings.Instance.IsIOSSupportEnabled = newARCoreIOSEnabled;

                    ARCoreIOSSupportHelper.SetARCoreIOSSupportEnabled(newARCoreIOSEnabled);
                }

                ARCoreProjectSettings.Instance.Save();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(50), GUILayout.Height(20)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
