//-----------------------------------------------------------------------
// <copyright file="ARCoreIOSSupportPreprocessBuild.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    internal class ARCoreIOSSupportPreprocessBuild : PreprocessBuildBase
    {
        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.iOS)
            {
                bool arcoreiOSEnabled = ARCoreProjectSettings.Instance.IsIOSSupportEnabled;
                Debug.LogFormat("Building application with ARCore SDK for Unity iOS support {0}",
                    arcoreiOSEnabled ? "ENABLED" : "DISABLED");

                ARCoreIOSSupportHelper.SetARCoreIOSSupportEnabled(arcoreiOSEnabled);
            }
        }
    }
}
