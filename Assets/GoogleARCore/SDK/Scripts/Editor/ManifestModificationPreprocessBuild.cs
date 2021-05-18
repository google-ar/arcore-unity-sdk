//-----------------------------------------------------------------------
// <copyright file="ManifestModificationPreprocessBuild.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using GoogleARCore;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Callbacks;
    using UnityEngine;

    internal class ManifestModificationPreprocessBuild : PreprocessBuildBase
    {
        private const string _pluginsFolderGuid = "93be2b9777c348648a2d9151b7e233fc";

        /// <summary>
        /// Generate the AndroidManifest XDocument based on the ARCoreProjectSettings.
        /// This function would be used in Tests.
        /// </summary>
        /// <param name="settings">ARCore Project Settings.</param>
        /// <returns>The XDocument of the final AndroidManifest.</returns>
        public static XDocument GenerateCustomizedAndroidManifest(
            ARCoreProjectSettings settings)
        {
            XElement mergedRoot = GetDefaultAndroidManifest().Root;
            List<IDependentModule> featureModules = DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                if (module.IsEnabled(settings))
                {
                    XDocument xDocument =
                        AndroidManifestMerger.TransferToXDocument(
                            module.GetAndroidManifestSnippet(settings));
                    mergedRoot = AndroidManifestMerger.MergeXElement(
                        mergedRoot, xDocument.Root);
                }
            }

            return new XDocument(mergedRoot);
        }

        /// <summary>
        /// Callback after the build is done.
        /// </summary>
        /// <param name="target">Build target platform.</param>
        /// <param name="pathToBuiltProject">Path to build project.</param>
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(
            UnityEditor.BuildTarget target, string pathToBuiltProject)
        {
            if (target == UnityEditor.BuildTarget.Android)
            {
                Debug.Log("Cleaning generated aar for customized manifest.");
                string cachedCurrentDirectory = Directory.GetCurrentDirectory();
                string pluginsFolderPath = Path.Combine(cachedCurrentDirectory,
                    AssetDatabase.GUIDToAssetPath(_pluginsFolderGuid));
                string customizedManifestAarPath =
                    Path.Combine(pluginsFolderPath, "customized_manifest.aar");
                File.Delete(customizedManifestAarPath);
            }
        }

        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                PreprocessAndroidBuild();
            }
        }

        private static void CheckCompatibilityWithAllSesssionConfigs(
            ARCoreProjectSettings settings,
            Dictionary<ARCoreSessionConfig, string> sessionToSceneMap)
        {
            List<IDependentModule> featureModules = DependentModulesManager.GetModules();
            foreach (IDependentModule module in featureModules)
            {
                foreach (var entry in sessionToSceneMap)
                {
                    if (!module.IsCompatibleWithSessionConfig(
                            settings, entry.Key))
                    {
                        throw new BuildFailedException(
                            string.Format(
                                "Module {0} isn't compatible with the setting in {1}",
                                module.GetType().Name, entry.Value));
                    }
                }
            }
        }

        private static XDocument GetDefaultAndroidManifest()
        {
            string str =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
                    <manifest xmlns:android=""http://schemas.android.com/apk/res/android""
                        package=""com.google.ar.core.unity.manifestsettings""
                        android:versionCode=""1""
                        android:versionName=""1.0"" >
                        <uses-sdk android:minSdkVersion=""14""/>
                    </manifest>";
            return XDocument.Parse(str);
        }

        private static void CreateClassesJar(string jarPath, string tempDirectoryPath)
        {
            var javaPath = Path.Combine(
                tempDirectoryPath, "GoogleArCoreCustomizedManifestEmptyClass.java");

            using (FileStream fs = File.Create(javaPath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(
                    "public class GoogleArCoreCustomizedManifestEmptyClass {}");
                fs.Write(info, 0, info.Length);
            }

            var fileListBuilder = new StringBuilder();
            foreach (var filePath in Directory.GetFiles(tempDirectoryPath))
            {
                fileListBuilder.AppendFormat(" {0}", Path.GetFileName(filePath));
            }

            string command = string.Format(
                "cf classes.jar {0}", fileListBuilder.ToString());
            string output;
            string errors;
            ShellHelper.RunCommand(jarPath, command, out output, out errors);

            if (!string.IsNullOrEmpty(errors))
            {
                throw new BuildFailedException(
                    string.Format(
                        "Error creating classes.jar for manifest: {0}", errors));
            }

            File.Delete(javaPath);
        }

        private void PreprocessAndroidBuild()
        {
            // This function causes build error in 2020.2 if the current directory is changed.
            // So do the check first here.
            CheckCompatibilityWithAllSesssionConfigs(
                ARCoreProjectSettings.Instance, AndroidDependenciesHelper.GetAllSessionConfigs());

            string cachedCurrentDirectory = Directory.GetCurrentDirectory();
            string pluginsFolderPath = Path.Combine(cachedCurrentDirectory,
                AssetDatabase.GUIDToAssetPath(_pluginsFolderGuid));
            string customizedManifestAarPath =
                Path.Combine(pluginsFolderPath, "customized_manifest.aar");

            string jarPath = Path.Combine(AndroidDependenciesHelper.GetJdkPath(), "bin/jar");

            var tempDirectoryPath =
                Path.Combine(cachedCurrentDirectory, FileUtil.GetUniqueTempPathInProject());

            try
            {
                // Move to a temp directory.
                Directory.CreateDirectory(tempDirectoryPath);
                Directory.SetCurrentDirectory(tempDirectoryPath);

                CreateClassesJar(jarPath, tempDirectoryPath);

                XDocument customizedManifest =
                    GenerateCustomizedAndroidManifest(ARCoreProjectSettings.Instance);
                var manifestPath = Path.Combine(tempDirectoryPath, "AndroidManifest.xml");
                customizedManifest.Save(manifestPath);

                // Compress the new AAR.
                var fileListBuilder = new StringBuilder();
                foreach (var filePath in Directory.GetFiles(tempDirectoryPath))
                {
                    fileListBuilder.AppendFormat(" {0}", Path.GetFileName(filePath));
                }

                string command = string.Format(
                    "cf customized_manifest.aar {0}", fileListBuilder.ToString());

                string output;
                string errors;
                ShellHelper.RunCommand(jarPath, command, out output, out errors);

                if (!string.IsNullOrEmpty(errors))
                {
                    throw new BuildFailedException(
                        string.Format(
                            "Error creating aar for manifest: {0}", errors));
                }

                File.Copy(Path.Combine(tempDirectoryPath, "customized_manifest.aar"),
                    customizedManifestAarPath, true);
            }
            finally
            {
                // Cleanup.
                Directory.SetCurrentDirectory(cachedCurrentDirectory);
                Directory.Delete(tempDirectoryPath, true);

                AssetDatabase.Refresh();
            }

            AssetHelper.GetPluginImporterByName("customized_manifest.aar")
                .SetCompatibleWithPlatform(BuildTarget.Android, true);
        }
    }
}
