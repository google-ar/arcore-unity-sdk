//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettingsGUI.cs" company="Google LLC">
//
// Copyright 2019 Google, LLC. All Rights Reserved.
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

    // Common GUI for ARCore Project Settings, used by ARCoreProjectSettingsWindow and
    // ARCoreProjectSettingsProvider.
    internal class ARCoreProjectSettingsGUI
    {
        // Use public static properties here, so that labels are automatically extracted by
        // GetSearchKeywordsFromGUIContentProperties() in ARCoreProjectSettingsProvider.
        public static readonly GUIContent ARCoreRequired = new GUIContent("ARCore Required");
        public static readonly GUIContent InstantPreviewEnabled =
            new GUIContent("Instant Preview Enabled");

        public static readonly GUIContent IOSSupportEnabled =
            new GUIContent("iOS Support Enabled");

        public static readonly GUIContent CloudAnchorAPIKeys =
            new GUIContent("Cloud Anchor API Keys");

        public static readonly GUIContent Android = new GUIContent("Android");
        public static readonly GUIContent IOS = new GUIContent("iOS");

        private static readonly float k_GroupLabelIndent = 15;
        private static readonly float k_GroupFieldIndent =
            EditorGUIUtility.labelWidth - k_GroupLabelIndent;

        private static bool s_FoldoutCloudAnchorAPIKeys = true;

        // Render ARCore Project Settings for ARCoreProjectSettingsWindow and
        // ARCoreProjectSettingsProvider.
        internal static void OnGUI(bool renderForStandaloneWindow)
        {
            ARCoreProjectSettings.Instance.IsARCoreRequired =
                EditorGUILayout.Toggle(ARCoreRequired,
                    ARCoreProjectSettings.Instance.IsARCoreRequired);
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            ARCoreProjectSettings.Instance.IsInstantPreviewEnabled =
                EditorGUILayout.Toggle(InstantPreviewEnabled,
                    ARCoreProjectSettings.Instance.IsInstantPreviewEnabled);
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            bool newARCoreIOSEnabled =
                EditorGUILayout.Toggle(IOSSupportEnabled,
                    ARCoreProjectSettings.Instance.IsIOSSupportEnabled);
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            s_FoldoutCloudAnchorAPIKeys =
                EditorGUILayout.Foldout(s_FoldoutCloudAnchorAPIKeys, CloudAnchorAPIKeys);
            if (s_FoldoutCloudAnchorAPIKeys)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(k_GroupLabelIndent);
                EditorGUILayout.LabelField(Android, GUILayout.Width(k_GroupFieldIndent));
                ARCoreProjectSettings.Instance.CloudServicesApiKey =
                    EditorGUILayout.TextField(ARCoreProjectSettings.Instance.CloudServicesApiKey);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(k_GroupLabelIndent);
                EditorGUILayout.LabelField(IOS, GUILayout.Width(k_GroupFieldIndent));
                ARCoreProjectSettings.Instance.IosCloudServicesApiKey =
                    EditorGUILayout.TextField(
                        ARCoreProjectSettings.Instance.IosCloudServicesApiKey);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }

            if (GUI.changed)
            {
                if (newARCoreIOSEnabled != ARCoreProjectSettings.Instance.IsIOSSupportEnabled)
                {
                    ARCoreProjectSettings.Instance.IsIOSSupportEnabled = newARCoreIOSEnabled;
                    ARCoreIOSSupportHelper.SetARCoreIOSSupportEnabled(newARCoreIOSEnabled);
                }
            }
        }
    }
}
