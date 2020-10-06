//-----------------------------------------------------------------------
// <copyright file="ARCoreCameraConfigFilter.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using System;
    using UnityEngine;

    /// <summary>
    /// The <see cref="ARCoreCameraConfigFilter"/> class derives a list of camera configurations
    /// available on the device at runtime.
    /// </summary>
    /// <remarks>
    /// This is used to derive a list of camera configurations available on the device at runtime
    /// to select from.
    ///
    /// NOTE: It is possible to select options in such a way that some devices will
    /// have no available configurations at runtime. In this case, your app will not run.
    ///
    /// Beginning with ARCore SDK 1.15.0, some devices support additional camera configs with lower
    /// GPU texture resolutions than the device's default GPU texture resolution. See the
    /// <a href="https://developers.google.com/ar/discover/supported-devices">ARCore supported
    /// devices</a> for an up to date list of affected devices.
    ///
    /// An app may adjust its capabilities at runtime by selecting a wider range of config filters
    /// and using <see cref="ARCoreSession.RegisterChooseCameraConfigurationCallback(
    /// ARCoreSession.OnChooseCameraConfigurationDelegate)"/> to specify a selection function.
    /// In that function the app may then adjust its runtime settings and select an appropriate
    /// camera configuration. If no callback is registered, ARCore will use the first
    /// <see cref="CameraConfig"/> in the list of available configurations.
    /// </remarks>
    [CreateAssetMenu(
        fileName = "ARCoreCameraConfigFilter",
        menuName = "Google ARCore/CameraConfigFilter",
        order = 2)]
    public class ARCoreCameraConfigFilter : ScriptableObject
    {
        /// <summary>
        /// This is the camera frame rates filter for the currently selected camera.
        /// </summary>
        public TargetCameraFramerateFilter TargetCameraFramerate;

        /// <summary>
        /// This allows an app to use or disable a hardware depth sensor if present on the device.
        /// </summary>
        public DepthSensorUsageFilter DepthSensorUsage;

        /// <summary>
        /// Unity OnValidate.
        /// </summary>
        public void OnValidate()
        {
            if (!TargetCameraFramerate.Target30FPS && !TargetCameraFramerate.Target60FPS)
            {
                Debug.LogError("No options in Target Camera Framerate are selected, " +
                    "there will be no camera configs and this app will fail to run.");
            }
            else if (!TargetCameraFramerate.Target30FPS)
            {
                Debug.LogWarning("Framerate30FPS is not selected, this may cause " +
                    "no camera config be available for this filter and " +
                    "the app may not run on all devices.");
            }

            if (!DepthSensorUsage.DoNotUse && !DepthSensorUsage.RequireAndUse)
            {
                Debug.LogError("No options in Depth Sensor Usage are selected, " +
                    "there will be no camera configs and this app will fail to run.");
            }
            else if (!DepthSensorUsage.DoNotUse)
            {
                Debug.LogWarning("DoNotUseDepthSensor is not selected, this may cause " +
                    "no camera config be available for this filter and " +
                    "the app may not run on all devices.");
            }
        }

        /// <summary>
        /// This is the camera frame rates filter for the currently selected camera.
        /// </summary>
        [Serializable]
        public class TargetCameraFramerateFilter
        {
            /// <summary>
            /// Target 30fps camera capture frame rate.
            ///
            /// Available on all ARCore supported devices.
            /// </summary>
            [Tooltip("Target 30fps camera capture frame rate. " +
                     "Available on all ARCore supported devices.")]
            public bool Target30FPS = true;

            /// <summary>
            /// Target 60fps camera capture frame rate.
            ///
            /// Increases power consumption and may increase app memory usage.
            ///
            /// See the <a href="https://developers.google.com/ar/discover/supported-devices">
            /// ARCore supported devices</a> page for a list of
            /// devices that currently support 60fps.
            /// </summary>
            [Tooltip("Target 60fps camera capture frame rate on supported devices.")]
            public bool Target60FPS = true;
        }

        /// <summary>
        /// This allows an app to use or disable a hardware depth sensor if present on the device.
        /// </summary>
        [Serializable]
        public class DepthSensorUsageFilter
        {
            /// <summary>
            /// Filters for camera configs that require a depth sensor to be present on the device,
            /// and that will be used by ARCore.
            ///
            /// See the <a href="https://developers.google.com/ar/discover/supported-devices">
            /// ARCore supported devices</a> page for a list of
            /// devices that currently have supported depth sensors.
            /// </summary>
            [Tooltip("ARCore requires a depth sensor to be present and will use it. " +
                     "Not supported on all devices.")]
            public bool RequireAndUse = true;

            /// <summary>
            /// Filters for camera configs where a depth sensor is not present, or is present but
            /// will not be used by ARCore.
            ///
            /// Most commonly used to filter camera configurations when the app requires exclusive
            /// access to the depth sensor outside of ARCore, for example to support 3D mesh
            /// reconstruction. Available on all ARCore supported devices.
            /// </summary>
            [Tooltip("ARCore will not use the depth sensor, even if it is present. " +
                     "Available on all supported devices.")]
            public bool DoNotUse = true;
        }
    }
}
