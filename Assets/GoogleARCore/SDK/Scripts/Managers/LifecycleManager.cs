//-----------------------------------------------------------------------
// <copyright file="LifecycleManager.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using GoogleARCore;
    using UnityEngine;

    internal class LifecycleManager
    {
        private static ILifecycleManager _instance;

        public static ILifecycleManager Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_ANDROID || UNITY_EDITOR
                    _instance = ARCoreAndroidLifecycleManager.Instance;
#elif UNITY_IOS
                    _instance = ARCoreIOSLifecycleManager.Instance;
#endif
                }

                return _instance;
            }
        }

        /// <summary>
        /// Force reset the singleton instance to null. Should only be used in Unit Test.
        /// </summary>
        internal static void ResetInstance()
        {
            if (_instance != null)
            {
                if (_instance is ARCoreAndroidLifecycleManager)
                {
                    ARCoreAndroidLifecycleManager.ResetInstance();
                }
#if UNITY_IOS
                else if(_instance is ARCoreIOSLifecycleManager)
                {
                    ARCoreIOSLifecycleManager.ResetInstance();
                }
#endif
                _instance = null;
            }
        }
    }
}
