//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettings.cs" company="Google">
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
        public bool IsIOSSupportEnabled;
        public string CloudServicesApiKey;
        public string IosCloudServicesApiKey;
        private const string k_ProjectSettingsPath = "ProjectSettings/ARCoreProjectSettings.json";
        private static ARCoreProjectSettings s_Instance = null;

        public static ARCoreProjectSettings Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    if (Application.isEditor)
                    {
                        s_Instance = new ARCoreProjectSettings();
                        s_Instance.Load();
                    }
                    else
                    {
                        Debug.LogError("Cannot access ARCoreProjectSettings outside of " +
                            "Unity Editor.");
                    }
                }

                return s_Instance;
            }
        }

        public void Load()
        {
            Version = GoogleARCore.VersionInfo.Version;
            IsARCoreRequired = true;
            IsInstantPreviewEnabled = true;
            CloudServicesApiKey = string.Empty;
            IosCloudServicesApiKey = string.Empty;

            if (File.Exists(k_ProjectSettingsPath))
            {
                ARCoreProjectSettings settings = JsonUtility.FromJson<ARCoreProjectSettings>(
                    File.ReadAllText(k_ProjectSettingsPath));
                Version = settings.Version;
                IsARCoreRequired = settings.IsARCoreRequired;
                IsInstantPreviewEnabled = settings.IsInstantPreviewEnabled;
                CloudServicesApiKey = settings.CloudServicesApiKey;
                IosCloudServicesApiKey = settings.IosCloudServicesApiKey;
                IsIOSSupportEnabled = settings.IsIOSSupportEnabled;
            }

            // Upgrades settings from V1.0.0 to V1.1.0.
            if (Version.Equals("V1.0.0"))
            {
                IsInstantPreviewEnabled = true;
                Version = "V1.1.0";
            }

            // Upgrades setting from V1.1.0 and V1.2.0 to V1.3.0.
            // Note: V1.2.0 went out with k_VersionString = V1.1.0
            if (Version.Equals("V1.1.0"))
            {
                IosCloudServicesApiKey = CloudServicesApiKey;
                Version = "V1.3.0";
            }

            if (!Version.Equals(GoogleARCore.VersionInfo.Version))
            {
                Version = GoogleARCore.VersionInfo.Version;
            }
        }

        public void Save()
        {
            File.WriteAllText(k_ProjectSettingsPath, JsonUtility.ToJson(this));
        }
    }
}
