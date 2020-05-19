//-----------------------------------------------------------------------
// <copyright file="ARCoreAnalyticsProvider.cs" company="Google LLC">
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

// Unity 2018.3 and later uses a SettingsProvider to add preference
// options to the common dialog. See also ARCoreAnalyticsPreferences.
#if UNITY_2018_3_OR_NEWER

namespace GoogleARCoreInternal
{
    using UnityEditor;

    internal class ARCoreAnalyticsProvider : SettingsProvider
    {
        public ARCoreAnalyticsProvider(string path, SettingsScope scope)
            : base(path, scope)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateARCoreAnalyticsProvider()
        {
            var provider =
                new ARCoreAnalyticsProvider("Preferences/Google ARCore", SettingsScope.User);

            // Automatically extract all keywords from public static GUI content.
            provider.keywords =
                GetSearchKeywordsFromGUIContentProperties<ARCoreAnalyticsGUI>();

            return provider;
        }

        public override void OnGUI(string searchContext)
        {
            ARCoreAnalyticsGUI.OnGUI();
        }
    }
}

#endif
