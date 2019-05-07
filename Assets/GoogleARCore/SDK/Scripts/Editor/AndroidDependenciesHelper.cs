//-----------------------------------------------------------------------
// <copyright file="AndroidDependenciesHelper.cs" company="Google">
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
        private static readonly string k_TemplateFileExtension = ".template";
        private static readonly string k_PlayServiceDependencyFileExtension = ".xml";

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
                    "Failed to enable ARCore Android dependencies xml. Template file is missing.");
                return;
            }

            string dependenciesXMLPath = dependenciesTemplatePath.Replace(
                k_TemplateFileExtension, k_PlayServiceDependencyFileExtension);

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
                            Debug.LogErrorFormat("ARCore: Error finding public static method " +
                                "{0} on {1}.{2}.", methodName, namespaceName, className);
                            return;
                        }

                        Debug.LogFormat("ARCore: Invoking {0}.{1}.{2}()",
                            namespaceName, className, methodName);
                        menuResolveMethod.Invoke(null, null);
                        return;
                    }
                }
            }

            Debug.LogFormat("ARCore: Could not find class {0}.{1} for dependency resolution.",
                namespaceName, className);
        }
    }
}
