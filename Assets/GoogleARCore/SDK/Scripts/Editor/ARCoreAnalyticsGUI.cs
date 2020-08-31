//-----------------------------------------------------------------------
// <copyright file="ARCoreAnalyticsGUI.cs" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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

    internal class ARCoreAnalyticsGUI
    {
        // Use public static properties here, so that labels are automatically extracted by
        // GetSearchKeywordsFromGUIContentProperties() in ARCoreAnalyticsProvider.
        public static readonly GUIContent SDKAnalytics =
                    new GUIContent("Enable Google ARCore SDK Analytics");

        private static float _groupLabelWidth = 260;

        // Render ARCore Analytics Settings for ARCoreAnalyticsProvider and
        // ARCoreAnalyticsPreferences.
        internal static void OnGUI()
        {
            EditorGUIUtility.labelWidth = _groupLabelWidth;
            ARCoreAnalytics.Instance.EnableAnalytics =
                EditorGUILayout.Toggle(SDKAnalytics, ARCoreAnalytics.Instance.EnableAnalytics);

            if (GUI.changed)
            {
                ARCoreAnalytics.Instance.Save();
            }
        }
    }
}
