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
        /// The direction of the device camera used by the session.
        /// </summary>
        /// <remarks>
        /// Note that changing this value will trigger a re-initialization of session. ARCore
        /// tracking data (e.g. Trackables) are not shared between cameras.
        /// </remarks>
        [Tooltip("The direction of the device camera used by the session.")]
        public DeviceCameraDirection DeviceCameraDirection = DeviceCameraDirection.BackFacing;

        /// <summary>
        /// A scriptable object specifying the ARCore session configuration.
        /// </summary>
        [Tooltip("A scriptable object specifying the ARCore session configuration.")]
        public ARCoreSessionConfig SessionConfig;

        /// <summary>
        /// The camera configuration filter object that defines the set of
        /// properties desired or required by the app to run.
        /// </summary>
        [Tooltip("Configuration options to select the camera mode and features.")]
        public ARCoreCameraConfigFilter CameraConfigFilter;

        private OnChooseCameraConfigurationDelegate m_OnChooseCameraConfiguration;

        /// <summary>
        /// Selects a camera configuration for the ARCore session being resumed.
        /// </summary>
        /// <param name="supportedConfigurations">
        /// A list of supported camera configurations. The size is dependent on
        /// <see cref="ARCoreSession.CameraConfigFilter"/> settings.
        /// The GPU texture resolutions are the same in all configs.
        /// Currently, most devices provide GPU texture resolution of 1920 x 1080,
        /// but devices might provide higher or lower resolution textures, depending
        /// on device capabilities.
        /// The CPU image resolutions returned are VGA, 720p, and a resolution matching the GPU
        /// texture, typically the native resolution of the device.</param>
        /// <returns>The index of the camera configuration in <c>supportedConfigurations</c> to be
        /// used for the ARCore session.  If the return value is not a valid index (e.g. the value
        /// -1), then no camera configuration will be set and the ARCore session will use the
        /// previously selected camera configuration or a default configuration if no previous
        /// selection exists.</returns>
        public delegate int OnChooseCameraConfigurationDelegate(
            List<CameraConfig> supportedConfigurations);

        /// <summary>
        /// Unity Awake.
        /// </summary>
        [SuppressMemoryAllocationError(Reason = "Could create new LifecycleManager")]
        public virtual void Awake()
        {
            if (SessionConfig != null &&
                SessionConfig.LightEstimationMode != LightEstimationMode.Disabled &&
                Object.FindObjectsOfType<EnvironmentalLight>().Length == 0)
            {
                Debug.Log("Light Estimation may not work properly when EnvironmentalLight is not" +
                    " attached to the scene.");
            }

            LifecycleManager.Instance.CreateSession(this);
        }

        /// <summary>
        /// Unity OnDestroy.
        /// </summary>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Requires further investigation.")]
        public virtual void OnDestroy()
        {
            LifecycleManager.Instance.ResetSession();
        }

        /// <summary>
        /// Unity OnEnable.
        /// </summary>
        [SuppressMemoryAllocationError(
            Reason = "Enabling session creates a new ARSessionConfiguration")]
        public void OnEnable()
        {
            LifecycleManager.Instance.EnableSession();
        }

        /// <summary>
        /// Unity OnDisable.
        /// </summary>
        [SuppressMemoryAllocationError(
            IsWarning = true, Reason = "Requires further investigation.")]
        public void OnDisable()
        {
            LifecycleManager.Instance.DisableSession();
        }

        /// <summary>
        /// Unity OnValidate.
        /// </summary>
        public void OnValidate()
        {
            if (DeviceCameraDirection == DeviceCameraDirection.FrontFacing && SessionConfig != null)
            {
                if (SessionConfig.PlaneFindingMode != DetectedPlaneFindingMode.Disabled)
                {
                    Debug.LogErrorFormat("Plane Finding requires back-facing camera.");
                }

                if (SessionConfig.LightEstimationMode ==
                        LightEstimationMode.EnvironmentalHDRWithoutReflections ||
                    SessionConfig.LightEstimationMode ==
                        LightEstimationMode.EnvironmentalHDRWithReflections)
                {
                    Debug.LogErrorFormat("LightEstimationMode.{0} is incompatible with" +
                        "front-facing (selfie) camera.", SessionConfig.LightEstimationMode);
                }

                if (SessionConfig.EnableCloudAnchor)
                {
                    Debug.LogErrorFormat("Cloud Anchors require back-facing camera.");
                }

                if (SessionConfig.AugmentedImageDatabase != null)
                {
                    Debug.LogErrorFormat("Augmented Images require back-facing camera.");
                }
            }

            if (DeviceCameraDirection == DeviceCameraDirection.BackFacing &&
                SessionConfig != null && SessionConfig.AugmentedFaceMode !=
                    AugmentedFaceMode.Disabled)
            {
                Debug.LogErrorFormat("AugmentedFaceMode.{0} requires front-facing (selfie) camera.",
                    SessionConfig.AugmentedFaceMode);
            }

            if (SessionConfig == null)
            {
                Debug.LogError("SessionConfig is required by ARCoreSession.");
            }

            if (CameraConfigFilter == null)
            {
                Debug.LogError("CameraConfigFilter is required by ARCoreSession. " +
                    "To get all available configurations, set CameraConfigFilter to " +
                    "a filter with all options selected.");
            }
        }

        /// <summary>
        /// Registers a callback that allows a camera configuration to be selected from a list of
        /// valid configurations.
        /// The callback should be registered before the ARCore session is enabled
        /// to ensure it is triggered on the first frame update.
        /// The callback will then be invoked each time the ARCore session is resumed,
        /// which can happen when the <see cref="ARCoreSession"/> component is enabled or the
        /// Android app moves from a state of 'paused' to 'resumed' state.
        ///
        /// Note: Starting in ARCore 1.12, changing the active camera config will make existing
        /// anchors and trackables fail to regain tracking.
        /// </summary>
        /// <param name="onChooseCameraConfiguration">The callback to register for selecting a
        /// camera configuration.</param>
        public void RegisterChooseCameraConfigurationCallback(
            OnChooseCameraConfigurationDelegate onChooseCameraConfiguration)
        {
            m_OnChooseCameraConfiguration = onChooseCameraConfiguration;
        }

        internal OnChooseCameraConfigurationDelegate GetChooseCameraConfigurationCallback()
        {
            return m_OnChooseCameraConfiguration;
        }
    }
}
