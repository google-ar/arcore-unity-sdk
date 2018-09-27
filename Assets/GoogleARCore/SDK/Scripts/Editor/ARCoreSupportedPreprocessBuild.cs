//-----------------------------------------------------------------------
// <copyright file="ARCoreSupportedPreprocessBuild.cs" company="Google">
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
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
        Justification = "Internal")]
    internal class ARCoreSupportedPreprocessBuild : PreprocessBuildBase
#if UNITY_2017_4_OR_NEWER
        , IActiveBuildTargetChanged
#endif
    {
        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                CheckARCoreSupported();
            }
        }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            if (newTarget == BuildTarget.Android)
            {
                CheckARCoreSupported();
            }
        }

        private void CheckARCoreSupported()
        {
            // `PlayerSettings.Android.ARCoreEnabled` is reliably available in 2018.2.1 and later.
#if UNITY_2018_2_OR_NEWER && !UNITY_2018_2_0
            if (!PlayerSettings.Android.ARCoreEnabled)
            {
                Debug.LogWarning("ARCore support is disabled. To use ARCore on Android, 'XR Settings > ARCore Supported' must be enabled.");
            }
#endif  //UNITY_2018_2_OR_NEWER && !UNITY_2018_2_0
        }
    }
}
