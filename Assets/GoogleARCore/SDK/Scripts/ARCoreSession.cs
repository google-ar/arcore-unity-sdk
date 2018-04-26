//-----------------------------------------------------------------------
// <copyright file="ARCoreSession.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
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

namespace GoogleARCore
{
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A component that manages the ARCore Session in a Unity scene.
    /// </summary>
    public class ARCoreSession : MonoBehaviour
    {
        /// <summary>
        /// A scriptable object specifying the ARCore session configuration.
        /// </summary>
        [Tooltip("A scriptable object specifying the ARCore session configuration.")]
        public ARCoreSessionConfig SessionConfig;

        /// <summary>
        /// Unity Start.
        /// </summary>
        public void Start()
        {
            LifecycleManager.Instance.CreateSession(this);
        }

        /// <summary>
        /// Unity OnDestroy.
        /// </summary>
        public void OnDestroy()
        {
            LifecycleManager.Instance.ResetSession();
        }

        /// <summary>
        /// Unity OnEnable.
        /// </summary>
        public void OnEnable()
        {
            LifecycleManager.Instance.EnableSession();
        }

        /// <summary>
        /// Unity OnDisable.
        /// </summary>
        public void OnDisable()
        {
            LifecycleManager.Instance.DisableSession();
        }
    }
}
