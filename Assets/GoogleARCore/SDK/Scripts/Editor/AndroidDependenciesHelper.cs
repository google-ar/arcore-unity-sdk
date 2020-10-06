//-----------------------------------------------------------------------
// <copyright file="AndroidDependenciesHelper.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using System.IO;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This handles the addition and removal android dependencies, and run PlayServicesResolver
    /// plugin.
    /// </summary>
    internal static class AndroidDependenciesHelper
    {
        private static readonly string _templateFileExtension = ".template";
        private static readonly string _playServiceDependencyFileExtension = ".xml";

        /// <summary>
        /// Gets the JDK path used by this project.
        /// </summary>
        /// <returns>If found, returns the JDK path used by this project. Otherwise, returns null.
        /// </returns>
        public static string GetJdkPath()
        {
            string jdkPath = null;

            // Unity started offering the embedded JDK in 2018.3
#if UNITY_2018_3_OR_NEWER
            if (EditorPrefs.GetBool("JdkUseEmbedded"))
            {
                // Use OpenJDK that is bundled with Unity. JAVA_HOME will be set when
                // 'Preferences > External Tools > Android > JDK installed with Unity' is checked.
                jdkPath = Environment.GetEnvironmentVariable("JAVA_HOME");
                if (string.IsNullOrEmpty(jdkPath))
                {
                    Debug.LogError(
                        "'Preferences > External Tools > Android > JDK installed with Unity' is " +
                        "checked, but JAVA_HOME is unset or empty. Try unchecking this setting " +
                        "and configuring a valid JDK path under " +
                        "'Preferences > External Tools > Android > JDK'.");
                }
            }
            else
#endif // UNITY_2018_3_OR_NEWER
            {
                // Use JDK path specified by 'Preferences > External Tools > Android > JDK'.
                jdkPath = EditorPrefs.GetString("JdkPath");
                if (string.IsNullOrEmpty(jdkPath))
                {
                    // Use JAVA_HOME from the O/S environment.
                    jdkPath = Environment.GetEnvironmentVariable("JAVA_HOME");
                    if (string.IsNullOrEmpty(jdkPath))
                    {
                        Debug.LogError(
                            "'Preferences > External Tools > Android > JDK installed with Unity' " +
                            "is unchecked, but 'Preferences > External Tools > Android > JDK' " +
                            "path is empty and JAVA_HOME environment variable is unset or empty.");
                    }
                }
            }

            if (!string.IsNullOrEmpty(jdkPath) &&
                (File.GetAttributes(jdkPath) & FileAttributes.Directory) == 0)
            {
                Debug.LogError(string.Format("Invalid JDK path '{0}'", jdkPath));
                jdkPath = null;
            }

            return jdkPath;
        }

        /// <summary>
        /// Handle the updating of the AndroidManifest tags by enabling/disabling the dependencies
        /// manifest AAR as necessary.
        /// </summary>
        /// <param name="enabledDependencies">If set to <c>true</c> enabled dependencies.</param>
        /// <param name="dependenciesManifestGuid">Dependencies manifest GUID.</param>
        public static void SetAndroidPluginEnabled(bool enabledDependencies,
            string dependenciesManifestGuid)
        {
            string manifestAssetPath = AssetDatabase.GUIDToAssetPath(dependenciesManifestGuid);
            if (manifestAssetPath == null)
            {
                Debug.LogErrorFormat("ARCore: Could not locate dependencies manifest plugin.");
                return;
            }

            PluginImporter pluginImporter =
                AssetImporter.GetAtPath(manifestAssetPath) as PluginImporter;
            if (pluginImporter == null)
            {
                Debug.LogErrorFormat("ARCore: Could not locate dependencies manifest plugin {0}.",
                    Path.GetFileName(manifestAssetPath));
                return;
            }

            pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, enabledDependencies);
        }

        /// <summary>
        /// Handle the addition or removal Android dependencies using the PlayServicesResolver.
        /// Adding the dependencies is done by renaming the dependencies .template file to a .xml
        /// file so that it will be picked up by the PlayServicesResolver plugin.
        /// </summary>
        /// <param name="enabledDependencies">If set to <c>true</c> enabled dependencies.</param>
        /// <param name="dependenciesTemplateGuid">Dependencies template GUID.</param>
        public static void UpdateAndroidDependencies(bool enabledDependencies,
            string dependenciesTemplateGuid)
        {
            string dependenciesTemplatePath =
                AssetDatabase.GUIDToAssetPath(dependenciesTemplateGuid);
            if (dependenciesTemplatePath == null)
            {
                Debug.LogError(
                    "Failed to enable ARCore SDK for Unity Android dependencies xml. " +
                    "Template file is missing.");
                return;
            }

            string dependenciesXMLPath = dependenciesTemplatePath.Replace(
                _templateFileExtension, _playServiceDependencyFileExtension);

            if (enabledDependencies && !File.Exists(dependenciesXMLPath))
            {
                Debug.LogFormat(
                    "Adding {0}.", Path.GetFileNameWithoutExtension(dependenciesTemplatePath));

                File.Copy(dependenciesTemplatePath, dependenciesXMLPath);
                AssetDatabase.Refresh();
            }
            else if (!enabledDependencies && File.Exists(dependenciesXMLPath))
            {
                Debug.LogFormat(
                    "Removing {0}.", Path.GetFileNameWithoutExtension(dependenciesTemplatePath));

                File.Delete(dependenciesXMLPath);
                File.Delete(dependenciesXMLPath + ".meta");
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Uses reflection to find the GooglePlayServices.PlayServicesResolver class and invoke
        /// the public static method, MenuResolve() in order to resolve dependencies change.
        /// </summary>
        public static void DoPlayServicesResolve()
        {
            const string namespaceName = "GooglePlayServices";
            const string className = "PlayServicesResolver";
            const string methodName = "MenuResolve";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    if (assembly.GetTypes() == null)
                    {
                        continue;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Could not get the Assembly types; skip it.
                    continue;
                }

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.Namespace != namespaceName)
                    {
                        continue;
                    }

                    if (type.Name == className)
                    {
                        // We found the class we're looking for. Attempt to call the method and
                        // then return.
                        var menuResolveMethod = type.GetMethod(methodName,
                            BindingFlags.Public | BindingFlags.Static);
                        if (menuResolveMethod == null)
                        {
                            Debug.LogErrorFormat("ARCore SDK for Unity: Error finding public " +
                                                 "static method {0} on {1}.{2}.",
                                                 methodName, namespaceName, className);
                            return;
                        }

                        Debug.LogFormat("ARCore SDK for Unity: Invoking {0}.{1}.{2}()",
                            namespaceName, className, methodName);
                        menuResolveMethod.Invoke(null, null);
                        return;
                    }
                }
            }

            Debug.LogFormat("ARCore SDK for Unity: " +
                            "Could not find class {0}.{1} for dependency resolution.",
                namespaceName, className);
        }
    }
}
