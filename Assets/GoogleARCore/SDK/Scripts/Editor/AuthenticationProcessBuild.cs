//-----------------------------------------------------------------------
// <copyright file="AuthenticationProcessBuild.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Callbacks;
    using UnityEngine;

    /// <summary>
    /// This handles the addition and removal of dependencies into the App's build.
    /// For BatchMode builds, perform clean after a build is complete.
    /// </summary>
    internal class AuthenticationProcessBuild : PreprocessBuildBase
    {
        private const string _androidKeylessPluginGuid = "aafa8cb6617464d6290c8fdfb9607794";
        private const string _androidKeylessDependenciesGuid = "1fc346056f53a42949a3dcadaae39d67";

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.Android &&
                ARCoreProjectSettings.Instance.AndroidAuthenticationStrategySetting ==
                AndroidAuthenticationStrategy.Keyless)
            {
                PostprocessAndroidBuild();
            }
        }

        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                VerifyAndroidAuthentication();
                bool includeKeyless =
                    ARCoreProjectSettings.Instance.AndroidAuthenticationStrategySetting ==
                    AndroidAuthenticationStrategy.Keyless;
                if (includeKeyless)
                {
                    Debug.Log("ARCore: Enabling Cloud Anchors with Keyless Authentication " +
                        "in this build.");
                }

                PreprocessAndroidBuild(includeKeyless);
            }
            else if (target == BuildTarget.iOS)
            {
                VerifyIosAuthentication();
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

            // Run the pre-process step with <c>Keyless</c> disabled so project files get reset.
            // Then run the ExternalDependencyManager dependency resolution which will remove
            // the Keyless dependencies.
            PreprocessAndroidBuild(false);
            AndroidDependenciesHelper.DoPlayServicesResolve();
        }

        private static void VerifyAndroidAuthentication()
        {
            CloudAnchorMode mode = GetActiveCloudAnchorMode();
            string projectSettingPath =
                "Project Settings > Google ARCore > Android Authentication Strategy";
            switch (mode)
            {
                case CloudAnchorMode.Disabled:
                    if (ARCoreProjectSettings.Instance.AndroidAuthenticationStrategySetting !=
                        AndroidAuthenticationStrategy.DoNotUse)
                    {
                        Debug.LogWarningFormat(
                            "{0} authentication is selected in ARCore Project Settings but " +
                            "Cloud Anchor is not used in any Scenes in Build. To turn off {0}, " +
                            "select Do Not Use in {1}.",
                            ARCoreProjectSettings.Instance.AndroidAuthenticationStrategySetting,
                            projectSettingPath);
                    }

                    break;
                case CloudAnchorMode.Enabled:
                    if (ARCoreProjectSettings.Instance.AndroidAuthenticationStrategySetting ==
                        AndroidAuthenticationStrategy.DoNotUse)
                    {
                        throw new BuildFailedException(string.Format(
                            "Cloud Anchor authentication is required by CloudAnchorMode {0}. " +
                            "An Android Authentication Strategy must be set in {1} " +
                            "when CloudAnchorMode is {0}", mode, projectSettingPath));
                    }

                    break;
            }
        }

        private static void VerifyIosAuthentication()
        {
#if ARCORE_IOS_SUPPORT
            CloudAnchorMode mode = GetActiveCloudAnchorMode();
            string projectSettingPath =
                "Project Settings > Google ARCore > iOS Authentication Strategy";
            switch (mode)
            {
                case CloudAnchorMode.Disabled:
                    if (ARCoreProjectSettings.Instance.IOSAuthenticationStrategySetting ==
                        IOSAuthenticationStrategy.DoNotUse)
                    {
                        Debug.LogWarning(
                            "Cloud Anchor APIs require one of the iOS authentication strategies. " +
                            "If it’s not in use, you can uncheck iOS Support Enabled in " +
                            "Project Settings > Google ARCore so ARCore SDK for Unity " +
                            "won’t import Cloud Anchor iOS cocoapod into your project.");
                    }
                    else
                    {
                        Debug.LogWarningFormat(
                            "{0} authentication is selected in ARCore Project Settings but " +
                            "Cloud Anchor is not used in any Scenes in Build. To turn off {0}, " +
                            "select Do Not Use in {1}.",
                            ARCoreProjectSettings.Instance.IOSAuthenticationStrategySetting,
                            projectSettingPath);
                    }

                    break;
                case CloudAnchorMode.Enabled:
                    if (ARCoreProjectSettings.Instance.IOSAuthenticationStrategySetting ==
                        IOSAuthenticationStrategy.DoNotUse)
                    {
                        throw new BuildFailedException(string.Format(
                            "Cloud Anchor authentication is required by CloudAnchorMode {0}. " +
                            "An iOS Authentication Strategy must be set in {1} " +
                            "when CloudAnchorMode is {0}", mode, projectSettingPath));
                    }

                    break;
            }

            if (mode != CloudAnchorMode.Disabled &&
                ARCoreProjectSettings.Instance.IOSAuthenticationStrategySetting ==
                IOSAuthenticationStrategy.AuthenticationToken)
            {
                Debug.Log(
                    "Authentication Token is selected as the Cloud Anchor Authentication. " +
                    "To authenticate with the Google Cloud Anchor Service, use " +
                    "XPSession.SetAuthToken(string) in runtime.");
            }
#endif
        }

        private static CloudAnchorMode GetActiveCloudAnchorMode()
        {
            CloudAnchorMode mode = CloudAnchorMode.Disabled;
            foreach (ARCoreSessionConfig config in
                AndroidDependenciesHelper.GetAllSessionConfigs().Keys)
            {
                if (config.CloudAnchorMode > mode)
                {
                    mode = config.CloudAnchorMode;
                }
            }

            return mode;
        }
    }
}
