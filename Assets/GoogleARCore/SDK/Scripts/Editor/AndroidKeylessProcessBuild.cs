//-----------------------------------------------------------------------
// <copyright file="AndroidKeylessProcessBuild.cs" company="Google LLC">
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
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    /// <summary>
    /// This handles the addition and removal of dependencies into the App's build.
    /// For BatchMode builds, perform clean after a build is complete.
    /// </summary>
    internal class AndroidKeylessProcessBuild : PreprocessBuildBase
    {
        private const string _androidKeylessPluginGuid = "aafa8cb6617464d6290c8fdfb9607794";
        private const string _androidKeylessDependenciesGuid = "1fc346056f53a42949a3dcadaae39d67";

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.Android && IsKeylessAuthenticationEnabled())
            {
                PostprocessAndroidBuild();
            }
        }

        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                if (IsKeylessAuthenticationEnabled())
                {
                    Debug.Log("ARCore: Enabling Cloud Anchors with Keyless Authentication " +
                        "in this build.");
                }

                PreprocessAndroidBuild(IsKeylessAuthenticationEnabled());
            }
        }

        private static void PreprocessAndroidBuild(bool enabledKeyless)
        {
            AndroidDependenciesHelper.SetAndroidPluginEnabled(
                enabledKeyless, _androidKeylessPluginGuid);
            AndroidDependenciesHelper.UpdateAndroidDependencies(
                enabledKeyless, _androidKeylessDependenciesGuid);

           if (enabledKeyless)
            {
                Debug.Log("ARCore: Including Keyless dependencies in this build.");
                AndroidDependenciesHelper.DoPlayServicesResolve();
            }
        }

        private static void PostprocessAndroidBuild()
        {
            Debug.Log("ARCore: Cleaning up Keyless dependencies.");

            // Run the pre-process step with <c>Keyless</c> disabled so project files get
            // reset.  Then run the PlayServicesResolver dependency resolution which will remove
            // the Keyless dependencies.
            PreprocessAndroidBuild(false);
            AndroidDependenciesHelper.DoPlayServicesResolve();
        }

        private static bool IsKeylessAuthenticationEnabled()
        {
            return ARCoreProjectSettings.Instance.AndroidAuthenticationStrategySetting ==
                AndroidAuthenticationStrategy.Keyless;
        }
    }
}
