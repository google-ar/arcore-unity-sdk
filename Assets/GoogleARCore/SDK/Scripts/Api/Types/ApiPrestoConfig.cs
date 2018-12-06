//-----------------------------------------------------------------------
// <copyright file="ApiPrestoConfig.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using GoogleARCoreInternal.CrossPlatform;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
    Justification = "Internal")]
    [StructLayout(LayoutKind.Sequential)]
    internal struct ApiPrestoConfig
    {
        public ApiUpdateMode UpdateMode;
        public ApiPlaneFindingMode PlaneFindingMode;
        public ApiLightEstimationMode LightEstimationMode;
        public ApiCloudAnchorMode CloudAnchorMode;
        public IntPtr ArPrestoAugmentedImageDatabase;
        public ApiCameraFocusMode CameraFocusMode;

        /// <summary>
        /// Wrap an ARCoreSessionConfig in an API config.
        /// </summary>
        /// <param name="config">Config to wrap.</param>
        public ApiPrestoConfig(ARCoreSessionConfig config)
        {
            UpdateMode = config.MatchCameraFramerate ?
                ApiUpdateMode.Blocking : ApiUpdateMode.LatestCameraImage;
            var planeFindingMode = ApiPlaneFindingMode.Disabled;
            switch (config.PlaneFindingMode)
            {
            case DetectedPlaneFindingMode.Horizontal:
                planeFindingMode = ApiPlaneFindingMode.Horizontal;
                break;
            case DetectedPlaneFindingMode.Vertical:
                planeFindingMode = ApiPlaneFindingMode.Vertical;
                break;
            case DetectedPlaneFindingMode.HorizontalAndVertical:
                planeFindingMode = ApiPlaneFindingMode.HorizontalAndVertical;
                break;
            default:
                break;
            }

            PlaneFindingMode = planeFindingMode;
            LightEstimationMode = config.EnableLightEstimation ?
                ApiLightEstimationMode.AmbientIntensity : ApiLightEstimationMode.Disabled;
            CloudAnchorMode = config.EnableCloudAnchor ?
                ApiCloudAnchorMode.Enabled : ApiCloudAnchorMode.Disabled;

            if (config.AugmentedImageDatabase != null)
            {
                ArPrestoAugmentedImageDatabase = config.AugmentedImageDatabase.m_ArPrestoDatabaseHandle;
            }
            else
            {
                ArPrestoAugmentedImageDatabase = IntPtr.Zero;
            }

            switch (config.CameraFocusMode)
            {
                case GoogleARCore.CameraFocusMode.Fixed:
                    CameraFocusMode = ApiCameraFocusMode.Fixed;
                    break;
                case GoogleARCore.CameraFocusMode.Auto:
#if UNITY_EDITOR
                    CameraFocusMode = ApiCameraFocusMode.Fixed;
                    UnityEngine.Debug.LogWarningFormat(
                        "Camera focus mode '{0}' not supported in editor.\n" +
                        "Using camera focus mode '{1}' instead.", config.CameraFocusMode, CameraFocusMode);
#else
                    CameraFocusMode = ApiCameraFocusMode.Auto;
#endif
                    break;
                default:
                    CameraFocusMode = ApiCameraFocusMode.Fixed;
                    break;
            }
        }
    }
}
