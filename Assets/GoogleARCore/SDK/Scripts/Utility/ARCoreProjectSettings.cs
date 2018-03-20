//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettings.cs" company="Google">
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
    using UnityEngine;

    [Serializable]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class ARCoreProjectSettings
    {
        public string Version;
        public bool IsARCoreRequired;
        public bool IsInstantPreviewEnabled;
        private const string k_VersionString = "V1.1.0";
        private const string k_ProjectSettingsPath = "ProjectSettings/ARCoreProjectSettings.json";

        static ARCoreProjectSettings()
        {
            if (Application.isEditor)
            {
                Instance = new ARCoreProjectSettings();
                Instance.Load();
            }
            else
            {
                Instance = null;
                Debug.LogError("Cannot access ARCoreProjectSettings outside of Unity Editor.");
            }
        }

        public static ARCoreProjectSettings Instance { get; private set; }

        public void Load()
        {
            Version = k_VersionString;
            IsARCoreRequired = true;
            IsInstantPreviewEnabled = true;

            if (File.Exists(k_ProjectSettingsPath))
            {
                ARCoreProjectSettings settings = JsonUtility.FromJson<ARCoreProjectSettings>(
                    File.ReadAllText(k_ProjectSettingsPath));
                Version = settings.Version;
                IsARCoreRequired = settings.IsARCoreRequired;
                IsInstantPreviewEnabled = settings.IsInstantPreviewEnabled;
            }

            // Upgrades settings from v1.0.0 to v1.1.0
            if (Version.Equals("V1.0.0"))
            {
                IsInstantPreviewEnabled = true;
                Version = k_VersionString;
            }
        }

        public void Save()
        {
            File.WriteAllText(k_ProjectSettingsPath, JsonUtility.ToJson(this));
        }
    }
}
