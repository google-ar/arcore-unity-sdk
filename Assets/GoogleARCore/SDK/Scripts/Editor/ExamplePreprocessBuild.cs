//-----------------------------------------------------------------------
// <copyright file="ExamplePreprocessBuild.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    internal class ExamplePreprocessBuild : ExampleBuildHelper
    {
        public ExamplePreprocessBuild()
        {
            _AddExampleScene(new ExampleScene()
                {
                    ProductName = "HelloAR U3D",
                    PackageName = "com.google.ar.core.examples.unity.helloar",
                    SceneGuid = "e6a6fa04348cb45c9b0221eb19c946da",
                    IconGuid = "36b7440e71f344bef8fca770c2d365f8"
                });
            _AddExampleScene(new ExampleScene()
                {
                    ProductName = "CV U3D",
                    PackageName = "com.google.ar.core.examples.unity.computervision",
                    SceneGuid = "5ef0f7f7f2c7b4285b707265348bbffd",
                    IconGuid = "7c556c651080f499d9eaeea95d392d80"
                });
            _AddExampleScene(new ExampleScene()
                {
                    ProductName = "AugmentedImage U3D",
                    PackageName = "com.google.ar.core.examples.unity.augmentedimage",
                    SceneGuid = "be567d47d3ab94b3badc5b211f535a24",
                    IconGuid = "0bf81216732894b46b8b5437b1acc57a"
                });
            _AddExampleScene(new ExampleScene()
                {
                    ProductName = "CloudAnchors U3D",
                    PackageName = "com.google.ar.core.examples.unity.cloudanchors",
                    SceneGuid = "83fb41cc294e74bdea57537befa00ffc",
                    IconGuid = "dcfb8b44c93d547e2bdf8a638c1415af"
                });
            _AddExampleScene(new ExampleScene()
                {
                    ProductName = "AugmentedFaces U3D",
                    PackageName = "com.google.ar.core.examples.unity.augmentedfaces",
                    SceneGuid = "7d2be221c0e8f4e259a08279fab0da42",
                    IconGuid = "c63c0025880214284b97a9d1b5de07dc"
                });
            _AddExampleScene(new ExampleScene()
            {
                ProductName = "ObjectManipulation U3D",
                PackageName = "com.google.ar.core.examples.unity.objectmanipulation",
                SceneGuid = "9dba7e74f4d2b410f90b920f80528d44",
                IconGuid = "db4401439efdb4e83a8095ae5370d9a5"
            });
        }

        public override void OnPreprocessBuild(BuildTarget target, string path)
        {
            _DoPreprocessBuild(target, path);
        }
    }
}
