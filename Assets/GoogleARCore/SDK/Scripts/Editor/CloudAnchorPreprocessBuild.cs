//-----------------------------------------------------------------------
// <copyright file="CloudAnchorPreprocessBuild.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Xml;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Callbacks;
    using UnityEngine;

    internal class CloudAnchorPreprocessBuild : PreprocessBuildBase
    {
        private const string _manifestTemplateGuid = "5e182918f0b8c4929a3d4b0af0ed6f56";
        private const string _pluginsFolderGuid = "93be2b9777c348648a2d9151b7e233fc";
        private const string _runtimeSettingsPath = "GoogleARCore/Resources/RuntimeSettings";
        private const string _aarFileName = "cloud_anchor_manifest.aar";

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.Android)
            {
                CleanupGeneratedAAR();
            }
        }

        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                PreprocessAndroidBuild();
            }
            else if (target == BuildTarget.iOS)
            {
                PreprocessIosBuild();
            }
        }

        private static void CleanupGeneratedAAR()
        {
            Debug.Log("Cleaning generated aar for Cloud Anchors.");
            string cachedCurrentDirectory = Directory.GetCurrentDirectory();
            string pluginsFolderPath = Path.Combine(cachedCurrentDirectory,
                AssetDatabase.GUIDToAssetPath(_pluginsFolderGuid));
            string aarPath =
                Path.Combine(pluginsFolderPath, _aarFileName);
            File.Delete(aarPath);
        }

        private void PreprocessAndroidBuild()
        {
            SetApiKeyOnAndroid();
        }

        private void PreprocessIosBuild()
        {
            SetApiKeyOnIos();
        }

        private void SetApiKeyOnAndroid()
        {
            string cachedCurrentDirectory = Directory.GetCurrentDirectory();
            string pluginsFolderPath = Path.Combine(cachedCurrentDirectory,
                AssetDatabase.GUIDToAssetPath(_pluginsFolderGuid));
            string cloudAnchorsManifestAarPath = Path.Combine(pluginsFolderPath, _aarFileName);

            bool cloudAnchorsEnabled =
                !string.IsNullOrEmpty(ARCoreProjectSettings.Instance.CloudServicesApiKey);
            if (cloudAnchorsEnabled)
            {
                string jarPath = AndroidDependenciesHelper.GetJdkPath();
                if (string.IsNullOrEmpty(jarPath))
                {
                    throw new BuildFailedException("Cannot find a valid JDK path in this build.");
                }

                jarPath = Path.Combine(jarPath, "bin/jar");

                // Replace the project's cloud anchor AAR with the newly generated AAR.
                Debug.Log("Enabling Cloud Anchors with API Key Authentication in this build.");

                var tempDirectoryPath =
                    Path.Combine(cachedCurrentDirectory, FileUtil.GetUniqueTempPathInProject());

                try
                {
                    // Move to a temp directory.
                    Directory.CreateDirectory(tempDirectoryPath);
                    Directory.SetCurrentDirectory(tempDirectoryPath);

                    var manifestTemplatePath = Path.Combine(
                        cachedCurrentDirectory,
                        AssetDatabase.GUIDToAssetPath(_manifestTemplateGuid));

                    // Extract the "template AAR" and remove it.
                    string output;
                    string errors;
                    ShellHelper.RunCommand(
                        jarPath, string.Format("xf \"{0}\"", manifestTemplatePath), out output,
                        out errors);

                    // Replace Api key template parameter in manifest file.
                    var manifestPath = Path.Combine(tempDirectoryPath, "AndroidManifest.xml");
                    var manifestText = File.ReadAllText(manifestPath);
                    manifestText = manifestText.Replace(
                        "{{CLOUD_ANCHOR_API_KEY}}",
                        ARCoreProjectSettings.Instance.CloudServicesApiKey);
                    File.WriteAllText(manifestPath, manifestText);

                    // Compress the new AAR.
                    var fileListBuilder = new StringBuilder();
                    foreach (var filePath in Directory.GetFiles(tempDirectoryPath))
                    {
                        fileListBuilder.AppendFormat(" {0}", Path.GetFileName(filePath));
                    }

                    string command = string.Format(
                        "cf {0} {1}", _aarFileName, fileListBuilder.ToString());

                    ShellHelper.RunCommand(
                        jarPath,
                        command,
                        out output,
                        out errors);

                    if (!string.IsNullOrEmpty(errors))
                    {
                        throw new BuildFailedException(
                            string.Format(
                                "Error creating jar for cloud anchor manifest: {0}", errors));
                    }

                    File.Copy(Path.Combine(tempDirectoryPath, _aarFileName),
                        cloudAnchorsManifestAarPath, true);
                }
                finally
                {
                    // Cleanup.
                    Directory.SetCurrentDirectory(cachedCurrentDirectory);
                    Directory.Delete(tempDirectoryPath, true);

                    AssetDatabase.Refresh();
                }

                AssetHelper.GetPluginImporterByName(_aarFileName)
                    .SetCompatibleWithPlatform(BuildTarget.Android, true);
            }
            else
            {
                Debug.Log(
                    "Cloud Anchor API key has not been set in this build.");
                File.Delete(cloudAnchorsManifestAarPath);
                AssetDatabase.Refresh();
            }
        }

        private void SetApiKeyOnIos()
        {
            var runtimeSettingsPath = Path.Combine(Application.dataPath, _runtimeSettingsPath);
            Directory.CreateDirectory(runtimeSettingsPath);
            string cloudServicesApiKey = ARCoreProjectSettings.Instance.IosCloudServicesApiKey;
            File.WriteAllText(
                Path.Combine(runtimeSettingsPath, "CloudServicesApiKey.txt"), cloudServicesApiKey);
            AssetDatabase.Refresh();
        }
    }
}
