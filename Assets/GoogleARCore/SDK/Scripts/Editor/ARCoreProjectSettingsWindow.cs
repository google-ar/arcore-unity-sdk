//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettingsWindow.cs" company="Google LLC">
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

namespace GoogleARCoreInternal
{
    using UnityEditor;
    using UnityEngine;

// Only add 'Edit > Project Settings > Google ARCore' menu item in Unity versions prior to 2018.3.
// In 2018.3 and later, settings are instead made available by ARCoreProjectSettingsProvider.
#if !UNITY_2018_3_OR_NEWER
    internal class ARCoreProjectSettingsWindow : EditorWindow
    {
        [MenuItem("Edit/Project Settings/Google ARCore")]
        private static void ShowARCoreProjectSettingsWindow()
        {
            ARCoreProjectSettings.Instance.Load();
            Rect rect = new Rect(500, 300, 400, 200);
            ARCoreProjectSettingsWindow window =
                GetWindowWithRect<ARCoreProjectSettingsWindow>(rect);
            window.titleContent = new GUIContent("ARCore Project Settings");
            window.Show();
        }

        private void OnGUI()
        {
            OnGUIHeader();
            ARCoreProjectSettingsGUI.OnGUI(true);
            OnGUIFooter();

            if (GUI.changed)
            {
                ARCoreProjectSettings.Instance.Save();
            }
        }

        private void OnGUIHeader()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.stretchWidth = true;
            titleStyle.fontSize = 14;
            titleStyle.fixedHeight = 20;

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("ARCore Project Settings", titleStyle);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
        }

        private void OnGUIFooter()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(50), GUILayout.Height(20)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
