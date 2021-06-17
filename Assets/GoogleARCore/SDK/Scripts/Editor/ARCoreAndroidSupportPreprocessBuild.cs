//-----------------------------------------------------------------------
// <copyright file="ARCoreAndroidSupportPreprocessBuild.cs" company="Unity">
//
// Copyright 2020 Unity Technologies All Rights Reserved.
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
    using System.IO;
    using System.Text;
    using System.Xml;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Callbacks;
    using UnityEngine;

    internal class ARCoreAndroidSupportPreprocessBuild : PreprocessBuildBase
    {
        private const string _pluginsFolderGuid = "93be2b9777c348648a2d9151b7e233fc";
        private const string _clientAarName = "arcore_client";
        private const string _gradleTempatePath = "Plugins/Android/mainTemplate.gradle";
        private const string _launcherTemplatePath = "Plugins/Android/launcherTemplate.gradle";

        private const string _backupExtension = ".backup";
        private static bool _hasBackup = false;

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.Android)
            {
                RestoreClientAar();
            }
        }

        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                Check64BitArch();
#if UNITY_2018_4 || UNITY_2019
#if UNITY_2018_4
                CheckBuildSystem();
#endif
                CheckGradleVersion();
                CheckMainGradle();
#if UNITY_2019_3_OR_NEWER
                CheckLauncherGradle();
#endif // UNITY_2019_3_OR_NEWER
#elif !UNITY_2018_4_OR_NEWER
                StripAndBackupClientAar();
#endif
            }
        }

        private static void RestoreClientAar()
        {
            if (_hasBackup)
            {
                string pluginsFolderPath = Path.Combine(Directory.GetCurrentDirectory(),
                    AssetDatabase.GUIDToAssetPath(_pluginsFolderGuid));
                string[] plugins = Directory.GetFiles(pluginsFolderPath);
                string backupFilePath = string.Empty;
                foreach (var pluginPath in plugins)
                {
                    if (pluginPath.Contains(_backupExtension) && !pluginPath.Contains(".meta"))
                    {
                        backupFilePath = pluginPath;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(backupFilePath))
                {
                    Debug.LogWarning("Failed to find the arcore client backup file.");
                    _hasBackup = false;
                    return;
                }

                string clientAarFilename = Path.GetFileNameWithoutExtension(backupFilePath);
                Debug.LogFormat("Restoring {0} from backup.", clientAarFilename);
                File.Copy(backupFilePath, Path.Combine(pluginsFolderPath, clientAarFilename), true);
                File.Delete(backupFilePath);
                File.Delete(backupFilePath + ".meta");
                _hasBackup = false;

                AssetDatabase.Refresh();
            }
        }

        private void CheckBuildSystem()
        {
            if (EditorUserBuildSettings.androidBuildSystem != AndroidBuildSystem.Gradle)
            {
                Debug.LogWarning(
                    "Android Build System is not Gradle. " +
                    "Go to 'Build Setting > Android > Build System', set to Gradle.");
            }
        }

        private void CheckGradleVersion()
        {
            // Need to set gradle version >= 5.6.4 by
            // 'Preferences > External Tools > Android > Gradle'
            // so it can compile android manifest with <queries> tag.
            var gradlePath = EditorPrefs.GetString("GradlePath");
            if (string.IsNullOrEmpty(gradlePath))
            {
                throw new BuildFailedException(
                    "'Preferences > External Tools > Android > Gradle' is empty. " +
                    "ARCore SDK for Unity requires a customized Gradle with version >= 5.6.4.");
            }
        }

        private void CheckMainGradle()
        {
            // Need to use gradle plugin version >= 3.6.0 in main gradle by editing
            // 'Assets/Plugins/Android/mainTemplate.gradle'.
            // so it can compile android manifest with <queries> tag.
            if (!File.Exists(Path.Combine(Application.dataPath, _gradleTempatePath)))
            {
                throw new BuildFailedException(
                    "Main Gradle template is not used in this build. ARCore SDK for Unity " +
                    "requires gradle plugin version >= 3.6.0. Navigate to " +
                    "'Project Settings > Player > Android Tab > Publish Settings > Build', " +
                    "check 'Custom Gradle Template'. Then edit the generated file " +
                    "'Assets/Plugins/Android/mainTemplate.gradle' by adding dependency " +
                    "'com.android.tools.build:gradle:3.6.0.'.");
            }
        }

        private void CheckLauncherGradle()
        {
            // Need to use gradle plugin version >= 3.6.0 in launcher gradle by editing
            // 'Assets/Plugins/Android/launcherTemplate.gradle'.
            // so it can compile android manifest with <queries> tag.
            if (!File.Exists(Path.Combine(Application.dataPath, _launcherTemplatePath)))
            {
                throw new BuildFailedException(
                    "Launcher Gradle Template is not used in this build. ARCore SDK for Unity " +
                    "requires gradle plugin version >= 3.6.0. Navigate to " +
                    "'Project Settings > Player > Android Tab > Publish Settings > Build', " +
                    "check 'Custom Launcher Gradle Template'. Then edit the generated file " +
                    "'Assets/Plugins/Android/launcherTemplate.gradle' by adding dependency " +
                    "'com.android.tools.build:gradle:3.6.0.'.");
            }
        }

        private void Check64BitArch()
        {
            bool includes64Bit =
                    (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0;
            if (!includes64Bit)
            {
                Debug.LogWarning("ARCore: Missing ARM64 architecture which is required for " +
                "Android 64-bit devices. See https://developers.google.com/ar/64bit.\nSelect " +
                "IL2CPP  in 'Project Settings > Player > Other Settings > Scripting Backend' and " +
                "select ARM64 in 'Project Settings > Player > Other Settings > Target " +
                "Architectures.'");
            }
        }

        private void StripAndBackupClientAar()
        {
            // Strip the <queries> tag from the arcore_client.aar when it's incompatible with
            // Unity's built-in gradle version.
            string cachedCurrentDirectory = Directory.GetCurrentDirectory();
            string pluginsFolderPath = Path.Combine(cachedCurrentDirectory,
                AssetDatabase.GUIDToAssetPath(_pluginsFolderGuid));

            string[] plugins = Directory.GetFiles(pluginsFolderPath);
            string clientAarPath = string.Empty;
            foreach (var pluginPath in plugins)
            {
                if (pluginPath.Contains(_clientAarName) &&
                    !pluginPath.Contains(".meta") &&
                    AssetHelper.GetPluginImporterByName(Path.GetFileName(pluginPath))
                        .GetCompatibleWithPlatform(BuildTarget.Android))
                {
                    clientAarPath = pluginPath;
                    break;
                }
            }

            if (string.IsNullOrEmpty(clientAarPath))
            {
                throw new BuildFailedException(
                    string.Format("Cannot find a valid arcore client plugin under '{0}'",
                    pluginsFolderPath));
            }

            string clientAarFileName = Path.GetFileName(clientAarPath);
            string jarPath = AndroidDependenciesHelper.GetJdkPath();
            if (string.IsNullOrEmpty(jarPath))
            {
                throw new BuildFailedException("Cannot find a valid JDK path in this build.");
            }

            jarPath = Path.Combine(jarPath, "bin/jar");
            var tempDirectoryPath =
                    Path.Combine(cachedCurrentDirectory, FileUtil.GetUniqueTempPathInProject());

            // Back up existing client AAR.
            string backupFilename = clientAarFileName + _backupExtension;
            Debug.LogFormat("Backing up {0} in {1}.", clientAarFileName, backupFilename);
            File.Copy(clientAarPath, Path.Combine(pluginsFolderPath, backupFilename), true);
            _hasBackup = true;

            Debug.LogFormat("Stripping the <queries> tag from {0} in this build.",
                clientAarFileName);
            try
            {
                // Move to a temp directory.
                Directory.CreateDirectory(tempDirectoryPath);
                Directory.SetCurrentDirectory(tempDirectoryPath);

                // Extract the existing AAR in the temp directory.
                string output;
                string errors;
                ShellHelper.RunCommand(
                    jarPath, string.Format("xf \"{0}\"", clientAarPath), out output,
                    out errors);

                // Strip the <queries> tag from AndroidManifest.xml.
                var manifestPath = Path.Combine(tempDirectoryPath, "AndroidManifest.xml");
                var manifestText = File.ReadAllText(manifestPath);
                manifestText = System.Text.RegularExpressions.Regex.Replace(
                    manifestText, "(/s)?<queries>(.*)</queries>(\n|\r|\r\n)", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.Singleline);
                File.WriteAllText(manifestPath, manifestText);

                // Compress the modified AAR.
                string command = string.Format("cf {0} .", clientAarFileName);
                ShellHelper.RunCommand(
                    jarPath,
                    command,
                    out output,
                    out errors);

                if (!string.IsNullOrEmpty(errors))
                {
                    throw new BuildFailedException(
                        string.Format(
                            "Error creating jar for stripped arcore client manifest: {0}", errors));
                }

                // Override the existing client AAR with the modified one.
                File.Copy(Path.Combine(tempDirectoryPath, clientAarFileName),
                    clientAarPath, true);
            }
            finally
            {
                // Cleanup.
                Directory.SetCurrentDirectory(cachedCurrentDirectory);
                Directory.Delete(tempDirectoryPath, true);

                AssetDatabase.Refresh();
            }

            AssetHelper.GetPluginImporterByName(clientAarFileName)
                .SetCompatibleWithPlatform(BuildTarget.Android, true);
        }
    }
}
