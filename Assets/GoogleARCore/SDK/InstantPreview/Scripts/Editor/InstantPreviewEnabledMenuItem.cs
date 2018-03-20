//-----------------------------------------------------------------------
// <copyright file="InstantPreviewEnabledMenuItem.cs" company="Google">
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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public static class InstantPreviewEnabledMenuItem
    {
        private const string k_MenuName = "Edit/Project Settings/ARCore/Instant Preview Enabled";
        private const int k_MenuPriority = 902;

        [MenuItem(k_MenuName, false, k_MenuPriority)]
        private static void ToggleARCoreRequiredMenuItem()
        {
            ARCoreProjectSettings.Instance.IsInstantPreviewEnabled =
                !ARCoreProjectSettings.Instance.IsInstantPreviewEnabled;
            ARCoreProjectSettings.Instance.Save();
        }

        [MenuItem(k_MenuName, true, k_MenuPriority)]
        private static bool ValidateARCoreRequiredMenuItem()
        {
            Menu.SetChecked(k_MenuName, ARCoreProjectSettings.Instance.IsInstantPreviewEnabled);
            return true;
        }
    }
}
