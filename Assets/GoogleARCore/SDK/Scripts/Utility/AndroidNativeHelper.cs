//-----------------------------------------------------------------------
// <copyright file="AndroidNativeHelper.cs" company="Google">
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
    using System.Diagnostics.CodeAnalysis;
    using UnityEngine;


    /// <summary>
    /// Static stateless Utility methods for performing native Android calls.
    /// </summary>
    public class AndroidNativeHelper
    {
        /// <summary>
        /// Mirrors Android <c>Surface</c> constants for display rotation.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1602:EnumerationItemsMustBeDocumented",
         Justification = "Internal.")]
        public enum AndroidSurfaceRotation
        {
            Rotation0 = 0,
            Rotation90 = 1,
            Rotation180 = 2,
            Rotation270 = 3,
        }

        /// <summary>
        /// Gets the current Android display rotation.
        /// </summary>
        /// <returns>The current Android display rotation.</returns>
        public static AndroidSurfaceRotation GetDisplayRotation()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return AndroidSurfaceRotation.Rotation0;
            }

            AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
            AndroidJavaClass unityPlayerClass =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject windowManager =
                unityActivity.Call<AndroidJavaObject>(
                    "getSystemService", contextClass.GetStatic<string>("WINDOW_SERVICE"));

            return
                (AndroidSurfaceRotation)windowManager.Call<AndroidJavaObject>("getDefaultDisplay")
                    .Call<int>("getRotation");
        }
    }
}
