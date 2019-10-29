//-----------------------------------------------------------------------
// <copyright file="ARCoreAnalyticsPreferences.cs" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
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

// PreferencesItem has been deprecated and is included here to support
// versions of Unity earlier that 2018.3. See also ARCoreAnalyticsProvider.
#if !UNITY_2018_3_OR_NEWER

namespace GoogleARCoreInternal
{
    using UnityEditor;

    internal class ARCoreAnalyticsPreferences : EditorWindow
    {
        [PreferenceItem("Google ARCore")]
        public static void PreferencesGUI()
        {
            ARCoreAnalyticsGUI.OnGUI();
        }
    }
}

#endif
