//-----------------------------------------------------------------------
// <copyright file="ARCoreSessionConfig.cs" company="Google">
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
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Holds settings that are used to configure the session.
    /// </summary>
    [CreateAssetMenu(fileName = "ARCoreSessionConfig", menuName = "GoogleARCore/SessionConfig", order = 1)]
    public class ARCoreSessionConfig : ScriptableObject
    {
        /// <summary>
        /// Toggles whether the rendering frame rate matches the background camera frame rate.
        /// Setting this to true will also set QualitySetting.vSyncCount to 0, which will make your entire app to run at the background camera frame rate (including animations, UI interaction, etc.).
        /// Setting this to false could incur extra power overhead due to rendering the same background more than once.
        /// </summary>
        [Tooltip("Toggles whether the rendering frame rate matches the background camera frame rate")]
        public bool MatchCameraFramerate = true;

        /// <summary>
        /// Chooses which plane finding mode will be used.
        /// </summary>
        [Tooltip("Chooses which plane finding mode will be used.")]
        [FormerlySerializedAs("EnablePlaneFinding")]
        public DetectedPlaneFindingMode PlaneFindingMode = DetectedPlaneFindingMode.HorizontalAndVertical;

        /// <summary>
        /// Toggles whether light estimation is enabled.
        /// </summary>
        [Tooltip("Toggles whether light estimation is enabled.")]
        public bool EnableLightEstimation = true;

        /// <summary>
        /// Toggles whether cloud anchor is enabled.
        /// </summary>
        [Tooltip("Toggles whether cloud anchor is enabled.")]
        public bool EnableCloudAnchor = false;

        /// <summary>
        /// The database to use for detecting AugmentedImage Trackables.
        /// </summary>
        [Tooltip("The database to use for detecting AugmentedImage Trackables.")]
        public AugmentedImageDatabase AugmentedImageDatabase;

        /// <summary>
        ///  Gets or sets a value indicating whether PlaneFinding is enabled.
        /// </summary>
        [System.Obsolete("This field has be replaced by GoogleARCore.DetectedPlaneFindingMode. See https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.2.0")]
        public bool EnablePlaneFinding
        {
            get
            {
                return PlaneFindingMode != DetectedPlaneFindingMode.Disabled;
            }

            set
            {
                PlaneFindingMode = value ? DetectedPlaneFindingMode.HorizontalAndVertical :
                    DetectedPlaneFindingMode.Disabled;
            }
        }

        /// <summary>
        /// ValueType check if two SessionConfig objects are equal.
        /// </summary>
        /// <param name="other">The other SessionConfig.</param>
        /// <returns>True if the two SessionConfig objects are value-type equal, otherwise false.</returns>
        public override bool Equals(object other)
        {
            ARCoreSessionConfig otherConfig = other as ARCoreSessionConfig;
            if (other == null)
            {
                return false;
            }

            if (MatchCameraFramerate != otherConfig.MatchCameraFramerate ||
                PlaneFindingMode != otherConfig.PlaneFindingMode ||
                EnableLightEstimation != otherConfig.EnableLightEstimation ||
                EnableCloudAnchor != otherConfig.EnableCloudAnchor ||
                AugmentedImageDatabase != otherConfig.AugmentedImageDatabase)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return a hash code for this object.
        /// </summary>
        /// <returns>A hash code value.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// ValueType copy from another SessionConfig object into this one.
        /// </summary>
        /// <param name="other">The SessionConfig to copy from.</param>
        public void CopyFrom(ARCoreSessionConfig other)
        {
            MatchCameraFramerate = other.MatchCameraFramerate;
            PlaneFindingMode = other.PlaneFindingMode;
            EnableLightEstimation = other.EnableLightEstimation;
            EnableCloudAnchor = other.EnableCloudAnchor;
            AugmentedImageDatabase = other.AugmentedImageDatabase;
        }
    }
}
