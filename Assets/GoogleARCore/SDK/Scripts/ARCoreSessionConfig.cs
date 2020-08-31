//-----------------------------------------------------------------------
// <copyright file="ARCoreSessionConfig.cs" company="Google LLC">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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
    using UnityEngine.Serialization;

    /// <summary>
    /// Holds settings that are used to configure the session.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ARCoreSessionConfig", menuName = "Google ARCore/SessionConfig", order = 1)]
    [HelpURL(
        "https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreSessionConfig")]
    public class ARCoreSessionConfig : ScriptableObject
    {
        [Header("Performance")]

        /// <summary>
        /// Toggles whether ARCore may introduce a delay into Unity's frame update to
        /// match the rate that the camera sensor is delivering frames (this is 30 frames-per-second
        /// on most devices).  Enabling this setting can reduce power consumption caused by
        /// rendering the same background texture more than once.  Since enabling this setting also
        /// sets QualitySetting.vSyncCount to 0 the entire Unity application (e.g animations, UI)
        /// will also update at the camera sensor frame rate.
        ///
        /// Note that enabling this setting does not guarentee each Unity frame will have a new and
        /// unique camera background texture.  This is because the period of time ARCore will wait
        /// for a new camera frame to become available is capped (currently at 66ms) to avoid a
        /// deadlock.
        /// </summary>
        [Tooltip(
            "Toggles whether the rendering frame rate matches the background camera frame rate")]
        public bool MatchCameraFramerate = true;

        [Header("Plane Finding")]

        /// <summary>
        /// Chooses which plane finding mode will be used.
        /// </summary>
        [Tooltip("Chooses which plane finding mode will be used.")]
        [FormerlySerializedAs("EnablePlaneFinding")]
        public DetectedPlaneFindingMode PlaneFindingMode =
            DetectedPlaneFindingMode.HorizontalAndVertical;

        [Header("Light Estimation")]

        /// <summary>
        /// Choose which light estimation mode will be used.
        /// </summary>
        [Tooltip("Chooses which light estimation mode will be used in ARCore session.")]
        [FormerlySerializedAs("EnableLightEstimation")]
        [Help("When \"Environmental HDR Without Reflections\" light is selected, ARCore:\n" +
              "1. Updates rotation and color of the directional light on the " +
              "EnvironmentalLight component.\n" +
              "2. Updates Skybox ambient lighting Spherical Harmonics.\n\n" +
              "When \"Environmental HDR With Reflections\" light is selected, ARCore also:\n" +
              "3. Overrides the environmental reflections in the scene with a " +
              "realtime reflections cubemap.")]
        public LightEstimationMode LightEstimationMode =
            LightEstimationMode.EnvironmentalHDRWithReflections;

        [Header("Cloud Anchors")]

        /// <summary>
        /// Chooses which Cloud Anchors mode will be used in ARCore session.
        /// </summary>
        [Tooltip("Chooses which Cloud Anchors mode will be used in ARCore session.")]
        [FormerlySerializedAs("EnableCloudAnchor")]
        public CloudAnchorMode CloudAnchorMode = CloudAnchorMode.Disabled;

        [Header("Augmented Images")]

        /// <summary>
        /// The database to use for detecting AugmentedImage Trackables.
        /// When this value is null, Augmented Image detection is disabled.
        /// </summary>
        [Tooltip("The database to use for detecting AugmentedImage Trackables.")]
        public AugmentedImageDatabase AugmentedImageDatabase;

        [Header("Camera")]

        /// <summary>
        /// On supported devices, selects the desired camera focus mode.
        /// </summary>
        /// <remarks>
        /// On these devices, the default desired focus mode is currently
        /// <see cref="GoogleARCore.CameraFocusMode"/>.<c>FixedFocus</c>, although this default
        /// might change in the future. See the
        /// <a href="https://developers.google.com/ar/discover/supported-devices">ARCore supported
        /// devices</a> page for a list of devices on which ARCore does not support changing the
        /// desired focus mode.
        ///
        /// For optimal AR tracking performance, use the focus mode provided by the default session
        /// config. While capturing pictures or video, use
        /// <see cref="GoogleARCore.CameraFocusMode"/>.<c>AutoFocus</c>. For optimal AR tracking,
        /// revert to the default focus mode once auto focus behavior is no longer needed. If your
        /// app requires fixed focus camera, set
        /// <see cref="GoogleARCore.CameraFocusMode"/>.<c>FixedFocus</c> before enabling the AR
        /// session. This ensures that your app always uses fixed focus, even if the default camera
        /// config focus mode changes in a future release.
        /// </remarks>
        [Tooltip("On supported devices, chooses the desired focus mode to be used by the ARCore " +
                 "camera.")]
        [Help("Note, on devices where ARCore does not support auto focus mode due to the use of " +
              "a fixed focus camera, setting focus mode to Auto Focus will be ignored. " +
              "Similarly, on devices where tracking requires auto focus mode, seting focus mode " +
              "to Fixed Focus will be ignored.")]
        public CameraFocusMode CameraFocusMode = CameraFocusMode.FixedFocus;

        /// <summary>
        /// Chooses which <see cref="GoogleARCore.AugmentedFaceMode"/> the ARCore session uses.
        /// </summary>
        public AugmentedFaceMode AugmentedFaceMode = AugmentedFaceMode.Disabled;


        /// <summary>
        /// Chooses which DepthMode will be used in the ARCore session.
        /// </summary>
        [Tooltip("Chooses which DepthMode will be used in the ARCore session.")]
        public DepthMode DepthMode = DepthMode.Disabled;
        [Header("Instant Placement")]

        /// <summary>
        /// Chooses the desired Instant Placement mode.
        /// </summary>
        [Tooltip("Chooses the desired Instant Placement mode.")]
        public InstantPlacementMode InstantPlacementMode = InstantPlacementMode.Disabled;

        /// <summary>
        ///  Gets or sets a value indicating whether PlaneFinding is enabled.
        /// </summary>
        [System.Obsolete(
            "This field has be replaced by ARCoreSessionConfig.DetectedPlaneFindingMode. See " +
            "https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.2.0")]
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
        /// Gets or sets a value indicating whether light estimation is enabled.
        /// </summary>
        /// <value><c>true</c> if enable light estimation; otherwise, <c>false</c>.</value>
        /// @deprecated Please use ARCoreSessionConfig.LightEstimationMode instead.
        [System.Obsolete(
            "This field has been replaced by ARCoreSessionConfig.LightEstimationMode. See " +
            "https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.10.0")]
        public bool EnableLightEstimation
        {
            get
            {
                return LightEstimationMode != LightEstimationMode.Disabled;
            }

            set
            {
                LightEstimationMode = value ? LightEstimationMode.AmbientIntensity :
                    LightEstimationMode.Disabled;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Cloud Anchors are enabled.
        /// </summary>
        /// <value><c>true</c> if enable Cloud Anchors; otherwise, <c>false</c>.</value>
        /// @deprecated Please use ARCoreSessionConfig.CloudAnchorMode instead.
        [System.Obsolete(
            "This field has been replaced by ARCoreSessionConfig.CloudAnchorMode. See " +
            "https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.15.0")]
        public bool EnableCloudAnchor
        {
            get
            {
                return CloudAnchorMode != CloudAnchorMode.Disabled;
            }

            set
            {
                CloudAnchorMode = value ? CloudAnchorMode.Enabled : CloudAnchorMode.Disabled;
            }
        }

        /// <summary>
        /// ValueType check if two SessionConfig objects are equal.
        /// </summary>
        /// <param name="other">The other SessionConfig.</param>
        /// <returns>True if the two SessionConfig objects are value-type equal, otherwise
        /// false.</returns>
        public override bool Equals(object other)
        {
            ARCoreSessionConfig otherConfig = other as ARCoreSessionConfig;
            if (other == null)
            {
                return false;
            }

            if (MatchCameraFramerate != otherConfig.MatchCameraFramerate ||
                PlaneFindingMode != otherConfig.PlaneFindingMode ||
                LightEstimationMode != otherConfig.LightEstimationMode ||
                CloudAnchorMode != otherConfig.CloudAnchorMode ||
                AugmentedImageDatabase != otherConfig.AugmentedImageDatabase ||
                CameraFocusMode != otherConfig.CameraFocusMode ||
                DepthMode != otherConfig.DepthMode ||
                InstantPlacementMode != otherConfig.InstantPlacementMode ||
                AugmentedFaceMode != otherConfig.AugmentedFaceMode)
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
            LightEstimationMode = other.LightEstimationMode;
            CloudAnchorMode = other.CloudAnchorMode;
            AugmentedImageDatabase = other.AugmentedImageDatabase;
            CameraFocusMode = other.CameraFocusMode;
            AugmentedFaceMode = other.AugmentedFaceMode;
            DepthMode = other.DepthMode;
            InstantPlacementMode = other.InstantPlacementMode;
        }

        /// <summary>
        /// Unity OnValidate.
        /// </summary>
        public void OnValidate()
        {
            if ((LightEstimationMode == LightEstimationMode.EnvironmentalHDRWithoutReflections ||
                LightEstimationMode == LightEstimationMode.EnvironmentalHDRWithReflections) &&
                AugmentedFaceMode == AugmentedFaceMode.Mesh)
            {
                Debug.LogErrorFormat("LightEstimationMode.{0} is incompatible with " +
                    "AugmentedFaceMode.{1}, please use other LightEstimationMode or disable " +
                    "Augmented Face.", LightEstimationMode, AugmentedFaceMode);
            }
        }
    }
}
