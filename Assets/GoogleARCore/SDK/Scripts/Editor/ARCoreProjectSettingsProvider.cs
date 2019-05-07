//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettingsProvider.cs" company="Google">
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

// Only add ARCore Project Settings to the unified project settings window in Unity 2018.3 and
// later. In Unity versions prior to 2018.3, settings are instead made available by
// ARCoreProjectSettingsWindow.
#if UNITY_2018_3_OR_NEWER
    internal class ARCoreProjectSettingsProvider : SettingsProvider
    {
        public ARCoreProjectSettingsProvider(string path, SettingsScope scope): base(path, scope)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateARCoreProjectSettingsProvider()
        {
            var provider =
                new ARCoreProjectSettingsProvider("Project/Google ARCore", SettingsScope.Project);

            // Automatically extract all keywords from public static GUI content.
            provider.keywords =
                GetSearchKeywordsFromGUIContentProperties<ARCoreProjectSettingsGUI>();

            return provider;
        }

        public override void OnGUI(string searchContext)
        {
            ARCoreProjectSettingsGUI.OnGUI(false);

            if (GUI.changed)
            {
                ARCoreProjectSettings.Instance.Save();
            }
        }
    }
#endif
}
