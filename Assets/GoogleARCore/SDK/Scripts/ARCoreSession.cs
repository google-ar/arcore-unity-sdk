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
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A component that manages the ARCore Session in a Unity scene.
    /// </summary>
    [HelpURL("https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreSession")]
    public class ARCoreSession : MonoBehaviour
    {
        /// <summary>
        /// A scriptable object specifying the ARCore session configuration.
        /// </summary>
        [Tooltip("A scriptable object specifying the ARCore session configuration.")]
        public ARCoreSessionConfig SessionConfig;

        private OnChooseCameraConfigurationDelegate m_OnChooseCameraConfiguration;

        /// <summary>
        /// Selects a camera configuration for the ARCore session being resumed.
        /// </summary>
        /// <param name="supportedConfigurations">
        /// A list of supported camera configurations. Currently it contains 3 camera configs.
        /// The GPU texture resolutions are the same in all three configs.
        /// Currently, most devices provide GPU texture resolution of 1920 x 1080,
        /// but devices might provide higher or lower resolution textures, depending
        /// on device capabilities. The CPU image resolutions returned are VGA, 720p,
        /// and a resolution matching the GPU texture.</param>
        /// <returns>The index of the camera configuration in <c>supportedConfigurations</c> to be used for the
        /// ARCore session.  If the return value is not a valid index (e.g. the value -1), then no camera
        /// configuration will be set and the ARCore session will use the previously selected camera
        /// configuration or a default configuration if no previous selection exists.</returns>
        public delegate int OnChooseCameraConfigurationDelegate(List<CameraConfig> supportedConfigurations);

        /// <summary>
        /// Unity Awake.
        /// </summary>
        public void Awake()
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

        /// <summary>
        /// Registers a callback that allows a camera configuration to be selected from a list of valid configurations.
        /// The callback will be invoked each time the ARCore session is resumed which can happen when the ARCoreSession
        /// component becomes enabled or the Android application moves from 'paused' to 'resumed' state.
        /// </summary>
        /// <param name="onChooseCameraConfiguration">The callback to register for selecting a camera configuration.</param>
        public void RegisterChooseCameraConfigurationCallback(OnChooseCameraConfigurationDelegate onChooseCameraConfiguration)
        {
            m_OnChooseCameraConfiguration = onChooseCameraConfiguration;
        }

        internal OnChooseCameraConfigurationDelegate GetChooseCameraConfigurationCallback()
        {
            return m_OnChooseCameraConfiguration;
        }
    }
}
