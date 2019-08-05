//-----------------------------------------------------------------------
// <copyright file="ARCoreIOSSupportHelper.cs" company="Google">
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

    internal class ARCoreIOSSupportHelper
    {
        private const string k_ARCoreEditorFolderGuid = "3efa82e8eae0d4459a41fa9c799ea3f8";
        private const string k_ARCoreIOSDependencyFileName = "ARCoreiOSDependencies";

        public static void SetARCoreIOSSupportEnabled(bool arcoreIOSEnabled)
        {
            if (arcoreIOSEnabled)
            {
                Debug.Log(
                    "Enabling Google ARCore SDK for Unity iOS Support. " +
                    "Note that you will need to add ARKit Unity SDK " +
                    "to your project to make ARCore work on iOS.");
            }
            else
            {
                Debug.Log("Disabling ARCore iOS support.");
            }

            _UpdateIOSScriptingDefineSymbols(arcoreIOSEnabled);
            _UpdateIOSPodDependencies(arcoreIOSEnabled);
            _UpdateARCoreARKitIntegrationPlugin(arcoreIOSEnabled);
        }

        private static void _UpdateIOSScriptingDefineSymbols(bool arcoreIOSEnabled)
        {
            string iOSScriptingDefineSymbols =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            bool iOSSupportDefined = iOSScriptingDefineSymbols.Contains("ARCORE_IOS_SUPPORT");

            if (arcoreIOSEnabled && !iOSSupportDefined)
            {
                Debug.Log("Adding ARCORE_IOS_SUPPORT define symbol.");
                iOSScriptingDefineSymbols += ";ARCORE_IOS_SUPPORT";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, iOSScriptingDefineSymbols);
            }
            else if (!arcoreIOSEnabled && iOSSupportDefined)
            {
                Debug.Log("Removing ARCORE_IOS_SUPPORT define symbol.");
                iOSScriptingDefineSymbols =
                    iOSScriptingDefineSymbols.Replace("ARCORE_IOS_SUPPORT", string.Empty);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.iOS, iOSScriptingDefineSymbols);
            }
        }

        private static void _UpdateIOSPodDependencies(bool arcoreIOSEnabled)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string arcoreEditorPath = Path.Combine(currentDirectory,
              AssetDatabase.GUIDToAssetPath(k_ARCoreEditorFolderGuid));

            string iOSPodDependencyTemplatePath =
                Path.Combine(arcoreEditorPath, k_ARCoreIOSDependencyFileName + ".template");
            string iOSPodDependencyXMLPath =
                Path.Combine(arcoreEditorPath, k_ARCoreIOSDependencyFileName + ".xml");

            if (arcoreIOSEnabled && !File.Exists(iOSPodDependencyXMLPath))
            {
                Debug.Log("Adding ARCoreiOSDependencies.");

                if (!File.Exists(iOSPodDependencyTemplatePath))
                {
                    Debug.LogError(
                        "Failed to enable ARCore iOS dependency xml. Template file is missing.");
                    return;
                }

                File.Copy(iOSPodDependencyTemplatePath, iOSPodDependencyXMLPath);

                AssetDatabase.Refresh();
            }
            else if (!arcoreIOSEnabled && File.Exists(iOSPodDependencyXMLPath))
            {
                Debug.Log("Removing ARCoreiOSDependencies.");

                File.Delete(iOSPodDependencyXMLPath);
                File.Delete(iOSPodDependencyXMLPath + ".meta");

                AssetDatabase.Refresh();
            }
        }

        private static void _UpdateARCoreARKitIntegrationPlugin(bool arcoreIOSEnabled)
        {
            string enableString = arcoreIOSEnabled ? "Enabling" : "Disabling";
            Debug.LogFormat("{0} ARCoreARKitIntegrationPlugin.", enableString);

            AssetHelper.GetPluginImporterByName("arcore_arkit_integration.mm")
               .SetCompatibleWithPlatform(BuildTarget.iOS, arcoreIOSEnabled);
        }
    }
}
