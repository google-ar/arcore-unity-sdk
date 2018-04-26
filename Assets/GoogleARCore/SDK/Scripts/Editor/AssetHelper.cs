//-----------------------------------------------------------------------
// <copyright file="AssetHelper.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    internal class AssetHelper
    {
        /// <summary>
        /// Get a PluginImporter object for a specific plugin file, anywhere in the project.
        ///
        /// If the plugin file is not found or multiple exist, this will throw a BuildFailedException.
        /// </summary>
        /// <param name="name">File name of the plugin including its extension.</param>
        /// <returns>The PluginImporter.</returns>
        public static PluginImporter GetPluginImporterByName(string name)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(name));

            PluginImporter pluginImporter = null;
            int foundCount = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path) == name)
                {
                    pluginImporter = AssetImporter.GetAtPath(path) as PluginImporter;
                    ++foundCount;
                }
            }

            if (foundCount == 0)
            {
                throw new BuildFailedException(
                    string.Format("ARCore could not find plugin {0}. Was it removed from the ARCore SDK?", name));
            }
            else if (foundCount != 1)
            {
                throw new BuildFailedException(
                    string.Format("ARCore found multiple plugins named {0}. This project should only contain one such " +
                        "plugin and it should be inside the ARCore SDK", name));
            }
            else if (pluginImporter == null)
            {
                throw new BuildFailedException(
                    string.Format("Found {0} file, but it is not a plugin.", name));
            }

            return pluginImporter;
        }
    }
}
