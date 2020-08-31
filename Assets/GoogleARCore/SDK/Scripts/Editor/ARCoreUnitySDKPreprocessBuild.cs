//-----------------------------------------------------------------------
// <copyright file="ARCoreUnitySDKPreprocessBuild.cs" company="Unity">
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

#if UNITY_2020_1_OR_NEWER
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine.Rendering;

namespace GoogleARCoreInternal
{
    internal class ARCoreUnitySDKPreprocessBuild : IPreprocessBuildWithReport
    {
        private static int _minSdkVersion = 14;

        public int callbackOrder { get { return 0; } }

        private ListRequest _request;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            _EnsureUnityARCoreIsNotPresent();
            _EnsureMinSdkVersion();
            _EnsureCompatibleOpenGLES();
        }

        private void _EnsureMinSdkVersion()
        {
            if ((int)PlayerSettings.Android.minSdkVersion < _minSdkVersion)
            {
                throw new BuildFailedException(string.Format("ARCore apps require a minimum " +
                    "SDK version of {0}. Currently set to {1}",
                    _minSdkVersion, PlayerSettings.Android.minSdkVersion));
            }
        }

        private void _EnsureUnityARCoreIsNotPresent()
        {
            _request = Client.List();    // List packages installed for the Project
            EditorApplication.update += _PackageListProgress;
        }

        private void _PackageListProgress()
        {
            if (_request.IsCompleted)
            {
                if (_request.Status == StatusCode.Success)
                {
                    foreach (var package in _request.Result)
                    {
                        if (package.name == "com.unity.xr.arcore")
                        {
                            throw new BuildFailedException("ARCore XR Plugin detected. Google's" +
                                " \"ARCore SDK for Unity\" and Unity's \"ARCore XR Plugin\" " +
                                "package cannot be used together.");
                        }
                    }
                }
                else if (_request.Status >= StatusCode.Failure)
                {
                    throw new BuildFailedException("Failure iterating packages when checking for" +
                    " ARCore XR Plugin.");
                }

                EditorApplication.update -= _PackageListProgress;
           }
        }

        private void _EnsureCompatibleOpenGLES()
        {
            var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            foreach (var graphicsApi in graphicsApis)
            {
                if (graphicsApi != GraphicsDeviceType.OpenGLES2 &&
                    graphicsApi != GraphicsDeviceType.OpenGLES3)
                {
                    throw new BuildFailedException(
                        string.Format("You have enabled the {0} graphics API, which is not " +
                        "supported by ARCore.", graphicsApi));
                }
            }
        }
    }
}

#endif //UNITY_2020_1_OR_NEWER
