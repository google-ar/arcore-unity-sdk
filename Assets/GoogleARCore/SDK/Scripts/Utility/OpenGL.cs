//-----------------------------------------------------------------------
// <copyright file="OpenGL.cs" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

#if UNITY_ANDROID
    using Import = System.Runtime.InteropServices.DllImportAttribute;
#else
    using Import = GoogleARCoreInternal.DllImportNoop;
#endif // !UNITY_ANDROID

    /// <summary>
    /// Direct interface to the OpenGL ES API.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1600:ElementsMustBeDocumented",
                     Justification = "OpenGL API")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1602:EnumerationItemsMustBeDocumented",
                     Justification = "OpenGL API")]
    [SuppressMessage("UnityRules.UnityStyleRules", "US1113:MethodsMustBeUpperCamelCase",
     Justification = "OpenGL API.")]
    public static class OpenGL
    {
        public enum Target
        {
            GL_TEXTURE_EXTERNAL_OES = 0x8D65
        }

#if UNITY_EDITOR
        public static int glGetError() {
            return 0;
        }

        public static void glGenTextures(int n, int[] textures) {
            Debug.Assert(n <= textures.Length);
            for (int i = 0; i < n; ++i) {
                textures[i] = i + 1;
            }
        }

        public static void glBindTexture(Target target, int texture) {
            // do nothing
        }
#else
#pragma warning disable 626
        [Import(ApiConstants.GLESApi)]
        public static extern int glGetError();

        [Import(ApiConstants.GLESApi)]
        public static extern void glGenTextures(int n, int[] textures);

        [Import(ApiConstants.GLESApi)]
        public static extern void glBindTexture(Target target, int texture);
#pragma warning restore 626
#endif // !UNITY_EDITOR
    }
}
