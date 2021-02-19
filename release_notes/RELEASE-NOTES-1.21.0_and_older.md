# ARCore SDK for Unity release notes

## INSTRUCTIONS

* This file is the source of truth for all ARCore SDK for Unity release notes,
  as published on https://github.com/google-ar/arcore-unity-sdk/releases

* With each release, the version specific release notes are manually copied to
  the GitHub [releases page](https://github.com/google-ar/arcore-unity-sdk/releases).

* The actual published release notes below are written and formatted in
  [GitHub flavored markdown](https://help.github.com/articles/about-writing-and-formatting-on-github/).

* URL requirements:

  * URLs must use the `https` scheme, not `http`.

  * URLs with implied scheme, in other words, starting with `//`, are not supported.
    URLs must all start with `https://`.

* Formatting conventions:

  * To prevent poor text wrapping, each bullet should (unlike the text you are
    reading in this bullet) be written on one long line, without line breaks.

  * Filenames, directory names and file system paths should be wrapped with ``.

  * Class names, method names and code fragments should be wrapped with ``.

  * Method names should include parentheses, so `foo.bar()`, instead of
    `foo.bar`.

  * Menu paths should be bold, in other words, **Edit > Preferences**.

  * Names of samples (**CloudVision** sample), scenes (**HelloAR** scene),
    components (**ARCore Session** component), prefabs (**Point Cloud** prefab),
    and so on. should be bold.

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.21.0 (2020-11-09)

## Upcoming breaking change affecting Cloud Anchors apps built using ARCore SDK 1.11.0 or earlier

Beginning in **January 2021**, AR-enabled apps built using **ARCore SDK 1.11.0 or earlier will no longer be able to host or resolve Cloud Anchors**. Specifically, Cloud Anchors returned by [`XPSession.CreateCloudAnchor(Anchor)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#createcloudanchor) and [`XPSession.ResolveCloudAnchor(string)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#resolvecloudanchor) will always have the state [`CloudServiceResponse.ErrorInternal`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore/CrossPlatform#cloudserviceresponse).

Apps built with ARCore SDK 1.12.0 or later are unaffected. Their use of Cloud Anchors APIs is covered by the [Cloud Anchors deprecation policy](https://developers.google.com/ar/develop/cloud-anchors/deprecation-policy).

## Known issues
  * Apps may crash when calling `Application.Quit()`. This affects all ARCore app. See details in [Unity's Issue Tracker](https://issuetracker.unity3d.com/issues/arcore-android-application-crashes-when-exiting-the-app).
  * [Issue 141500087](https://issuetracker.google.com/141500087): When using Android Emulator `x86_64` system images on macOS with ARCore SDK 1.16.0 or later, Google Play Services for AR will crash. As a workaround, use an `x86` system image.

## Breaking & behavioral changes
None.

## New APIs and capabilities
  * Added **Recording** and **Playback** of ARCore datasets. The Recording feature lets the app capture the data required to replay the AR session. [Developer guide](https://developers.google.com/ar/develop/unity/recording-and-playback), [`Session.RecordingStatus`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Session#recordingstatus), [`Session.StartRecording(ARCoreRecordingConfig)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Session#startrecording), [`Session.StopRecording()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Session#stoprecording), [`Session.PlaybackStatus`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Session#playbackstatus), [`Session.SetPlaybackDataset(string)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Session#setplaybackdataset).
  * Added new APIs for Stereo Camera usage in `CameraConfigFilter` and `CameraConfig`: [Developer guide](https://developers.google.com/ar/develop/unity/camera-configs), addition of [`StereoCameraUsage`](https://developers.google.com/ar/reference/c/group/ar-camera-config#stereocamerausage) property to `CameraConfig`, [`ARCoreCameraConfigFilter.StereoCameraUsage`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreCameraConfigFilter#stereocamerausage).


## Deprecations
None.

## Other changes
  * Changes to the **HelloAR** sample:
    * Added a Depth check to reduce unnecessary logging.
    * Added a setting to toggle Instant Placement mode.
  * Changed minimum supported version of Unity to 2017.4.40.

## Bug fixes
None.

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.20.0 (2020-09-28)

## Known issues
  * Apps may crash when calling `Application.Quit()`. This affects all ARCore app. See details in [Unity's Issue Tracker](https://issuetracker.unity3d.com/issues/arcore-android-application-crashes-when-exiting-the-app).
  * [Issue 141500087](https://issuetracker.google.com/141500087): When using Android Emulator `x86_64` system images on macOS with ARCore SDK 1.16.0 or later, Google Play Services for AR will crash. As a workaround, use an `x86` system image.
  * [Issue 1276856](https://issuetracker.unity3d.com/product/unity/issues/guid/1276856/) Unity removed support for `OpenGLES2` in "ARCore Supported" apps in 2018.4.26. Apps must disable _Project Settings > Other Settings > Auto Graphics API_, and then only include `OpenGLES3` in the `Graphics APIs` list.

## Breaking & behavioral changes
  * `CameraConfigDepthSensorUsages` has been renamed to [`CameraConfigDepthSensorUsage`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#cameraconfigdepthsensorusage).
  * Beginning in **December 2020**, AR-enabled apps built using **ARCore SDK 1.11.0 or earlier will no longer be able to host or resolve Cloud Anchors**. Apps built with ARCore SDK 1.12.0 or later are unaffected. To learn more about this breaking change, and for instructions on how to update your app, see the [Cloud Anchors deprecation policy](https://developers.google.com/ar/develop/cloud-anchors/deprecation-policy).

## New APIs and capabilities
  * Added [**Persistent Cloud Anchors**](https://developers.google.com/ar/develop/unity/cloud-anchors/persistence), which let you increase the time-to-live (TTL) of Cloud Anchors to 365 days. With the [Cloud Anchor Management API](https://developers.google.com/ar/develop/cloud-anchors/management-api), you can also extend the lifetime of a Cloud Anchor. Use of Persistent Cloud Anchors is covered by the new [Cloud Anchors deprecation policy](https://developers.google.com/ar/develop/cloud-anchors/deprecation-policy).
    * Added [`XPSession.CreateCloudAnchor(Anchor, int)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#createcloudanchor_1).
    * Added [`XPSession.EstimateFeatureMapQualityForHosting(Pose)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#estimatefeaturemapqualityforhosting).
    * **iOS only:**
      * Added [`XPSession.SetAuthToken(string)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#setauthtoken).
      * Added [`XPSession.CreateCloudAnchor(UnityARUserAnchorComponent, int)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#createcloudanchor_3)

## Deprecations
None.

## Other changes
  * Added a Depth check to the **HelloAR** sample to reduce unnecessary logging.
  * The **HelloAR** sample now includes the settings to toggle Instant Placement mode in runtime.
  * Changed the **HelloAR** sample to acquire depth images only when tracking is active. This avoids logging unactionable errors when tracking is not active.
  * CloudAnchors and PersistentCloudAnchors scenes now use the camera included in **`ARCoreDevice`** prefab.

## Bug fixes
  * Fixed `NullReferenceException` in `PointcloudVisualizerEditor` affecting newer versions of Unity, due to Unity's renaming of the internal `_script` property to `m_Script`.

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.19.0 (2020-08-13)

|| TODO: At release time, merge in C/Java API release notes from:
||       //third_party/arcore/RELEASE-NOTES.md

## Breaking change affecting previously published 32-bit-only apps

_Google Play Services for AR_ (ARCore) has removed support for 32-bit-only ARCore-enabled apps running on 64-bit devices. Support for 32-bit apps running on 32-bit devices is unaffected.

If you have published a 32-bit-only (`armeabi-v7a`) version of your ARCore-enabled app without publishing a corresponding 64-bit (`arm64-v8a`) version, you must update your app to include 64-bit native libraries. 32-bit-only ARCore-enabled apps that are not updated may crash when attempting to start an augmented reality (AR) session.

To learn more about this breaking change, and for instructions on how to update your app, see https://developers.google.com/ar/64bit.

## Known issues
  * Apps may crash when calling `Application.Quit()`. This affects all ARCore apps. See details in [Unity's Issue Tracker](https://issuetracker.unity3d.com/issues/arcore-android-application-crashes-when-exiting-the-app).
  * [Issue 141500087](https://issuetracker.google.com/141500087): When using Android Emulator `x86_64` system images on macOS with ARCore SDK 1.16.0 or later, Google Play Services for AR will crash. As a workaround, use an `x86` system image.
  * Unity 2018.4.24f1 is the minimum supported 2018.4 version that allows use of a custom Gradle template. See details in [2018.4.24f1 Release Notes](https://unity3d.com/unity/whats-new/2018.4.24).
  * Unity 2019.3.7f1 is the minimum supported 2019.3 version that allows overwriting launcher and main Gradle templates. See details in [2019.3.7f1 Release Notes](https://unity3d.com/unity/whats-new/2019.3.7).

## Breaking & behavioral changes
  * To support Android 11, the ARCore SDK for Unity now requires Gradle version 5.6.4 or later. For details, refer to [Android 11](https://developers.google.com/ar/develop/unity/android-11-build).

## New APIs and capabilities
  * Added new [Instant Placement APIs](https://developers.google.com/ar/develop/unity/instant-placement):
    * New [`InstantPlacementMode`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#instantplacementmode) Enum type.
    * New `InstantPlacementMode` property in [`ARCoreSessionConfig`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreSessionConfig) to enable Instant Placement in ARCore session.
    * New [`InstantPlacementPoint`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/InstantPlacementPoint) Trackable.
    * Added [`Frame.RaycastInstantPlacement()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame#raycastinstantplacement) to perform a raycast against an Instant Placement Point.

## Deprecations
None.

## Bug fixes
  * Fixed [#710](https://github.com/google-ar/arcore-unity-sdk/issues/710): Unity Editor crashes when running Instant Preview.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.18.0 (2020-06-25)

## Upcoming breaking change affecting previously published 32-bit-only apps

In **August 2020**, _Google Play Services for AR_ (ARCore) will remove support
for 32-bit-only ARCore-enabled apps running on 64-bit devices. Support for
32-bit apps running on 32-bit devices is unaffected.

If you have published a 32-bit-only (`armeabi-v7a`) version of your
ARCore-enabled app without publishing a corresponding 64-bit (`arm64-v8a`)
version, you must update your app to include 64-bit native libraries before
August 2020. 32-bit-only ARCore-enabled apps that are not updated by this time
may crash when attempting to start an augmented reality (AR) session.

To learn more about this breaking change, and for instructions on how to update
your app, see https://developers.google.com/ar/64bit.

## Known issues
  * Apps may crash when calling `Application.Quit()`. This affects all ARCore app. See details in [Unity's Issue Tracker](https://issuetracker.unity3d.com/issues/arcore-android-application-crashes-when-exiting-the-app).
  * [Issue 141500087](https://issuetracker.google.com/141500087): When using Android Emulator `x86_64` system images on macOS with ARCore SDK 1.16.0 or later, Google Play Services for AR will crash. As a workaround, use an `x86` system image.

## Breaking & behavioral changes
  * `targetSdkVersion` inside the ARCore Android Archive (AAR) file has been updated to API level 29. Specifying a `targetSdkVersion` in your project's `build.gradle` or `AndroidManifest.xml` will override the ARCore value.

## New APIs and capabilities
  * Added new [**Depth API**](https://developers.google.com/ar/develop/unity/depth/developer-guide). Check the list of [ARCore supported devices](https://developers.google.com/ar/discover/supported-devices) to see which devices support the Depth API.
    * New [`DepthMode`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#depthmode) enum type.
    * New `DepthMode` property in [`ARCoreSessionConfig`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreSessionConfig) to enable Depth in ARCore session.
    * Added [`Session.IsDepthModeSupported()`](ttps://developers.google.com/ar/reference/unity/class/GoogleARCore/Session#isdepthmodesupported) to check whether the depth mode is supported on the device.
    * Added [`Frame.UpdateDepthTexture()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame/CameraImage#updatedepthtexture) to update the input texture using the latest depth data from ARCore.

## Deprecations
None.

## Other changes
  * Added [`CloudServiceResponse.ErrorTooManyCloudAnchors`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore/CrossPlatform#cloudserviceresponse) enum value.
  * The **HelloAR** sample now includes support for occlusion using the ARCore Depth API.
  * Updated C# style for non-public variables and methods. For example, `private float m_Foo` now reads `private float _foo` and `private void _Bar()` now reads `private void Bar()`.

## Bug fixes
  * Fixed [Cloud Anchors Privacy](https://developers.google.com/ar/cloud-anchors-privacy) link in the [`CloudAnchors`](https://github.com/google-ar/arcore-unity-sdk/tree/master/Assets/GoogleARCore/Examples/CloudAnchors) sample app. The incorrect link now redirects to the correct link, so existing apps with the incorrect link don't need to be updated.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.17.0 (2020-05-14)

## Upcoming breaking change affecting previously published 32-bit-only apps

In **August 2020**, _Google Play Services for AR_ (ARCore) will remove support
for 32-bit-only ARCore-enabled apps running on 64-bit devices. Support for
32-bit apps running on 32-bit devices is unaffected.

If you have published a 32-bit-only (`armeabi-v7a`) version of your
ARCore-enabled app without publishing a corresponding 64-bit (`arm64-v8a`)
version, you must update your app to include 64-bit native libraries before
August 2020. 32-bit-only ARCore-enabled apps that are not updated by this time
may crash when attempting to start an augmented reality (AR) session.

To learn more about this breaking change, and for instructions on how to update
your app, see https://developers.google.com/ar/64bit.

## Known Issues
  * Apps may crash when calling `Application.Quit()`. This affects all ARCore apps. See details in [Unity's Issue Tracker](https://issuetracker.unity3d.com/issues/arcore-android-application-crashes-when-exiting-the-app).
  * [Issue 141500087](https://issuetracker.google.com/141500087): When using Android Emulator `x86_64` system images on macOS with ARCore SDK 1.16.0 or later, Google Play Services for AR will crash. As a workaround, use an `x86` system image.

## Breaking & behavioral changes
  * Added [`SessionStatus.ErrorCameraNotAvailable`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#namespaceGoogleARCore_1a33759b4917069ee919d9113ff71bca11a136c1093dfcce32496789900fe5514f2) and [`SessionStatus.ErrorIllegalState`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#namespaceGoogleARCore_1a33759b4917069ee919d9113ff71bca11aa159778b7b38795d50ce027542e329f0) to indicate specific ARCore session errors.

## New APIs and capabilities
  * Multithreaded Rendering on Android (_Project Settings > Player > Android > Other Settings > Multithreaded Rendering_) is now supported for **Unity 2018.2 and later**. However, 3D assets may not always render correctly when the app places a high load on the rendering thread. On **Unity 2018.1 and earlier**, Multithreaded Rendering will not work when using the front-facing (selfie) camera.

## Deprecations
None

## Other changes
None.

## Bug fixes
  * Fixed [#99](https://github.com/google-ar/arcore-unity-sdk/issues/99): `Frame.Raycast` throws exception when calling `ArFrame_hitTestRay` while using Instant Preview.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.16.0 (2020-03-19)

## Upcoming breaking change affecting previously published 32-bit-only apps

In **August 2020**, _Google Play Services for AR_ (ARCore) will remove support
for 32-bit-only ARCore-enabled apps running on 64-bit devices. Support for
32-bit apps running on 32-bit devices is unaffected.

If you have published a 32-bit-only (`armeabi-v7a`) version of your
ARCore-enabled app without publishing a corresponding 64-bit (`arm64-v8a`)
version, you must update your app to include 64-bit native libraries before
August 2020. 32-bit-only ARCore-enabled apps that are not updated by this time
may crash when attempting to start an augmented reality (AR) session.

To learn more about this breaking change, and for instructions on how to update
your app, see https://developers.google.com/ar/64bit.

## Known Issues
  * [Issue 141500087](https://issuetracker.google.com/141500087): When using Android Emulator `x86_64` system images on macOS with ARCore SDK 1.16.0 or later, Google Play Services for AR will crash. As a workaround, use an `x86` system image.

## Breaking changes
None.

## New APIs
None.

## Deprecations
None.

## Behavioral changes
  * Beginning with ARCore SDK 1.16.0, most devices will now return additional supported camera configs with lower GPU texture resolutions than the device's default GPU texture resolution. See the [ARCore supported devices](https://developers.google.com/ar/discover/supported-devices) for details.

## Other changes
  * Removed XPSessionStatus.cs, as it is not used in the SDK anymore.
  * As of Unity v2020.1, the *Player Settings* > *XR Settings* has been removed. The *ARCore Supported* option is no longer needed. The ARCore SDK for Unity automatically detects the presence of the SDK, or warn if there is a conflict with components of AR Foundation.

## Bug fixes
  * Fixed [#660](https://github.com/google-ar/arcore-unity-sdk/issues/660): The pawn in the Object Manipulation sample can't be translated.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.15.0 (2020-02-06)

## Breaking changes
  * **Update: A bug caused this feature to not work in 1.15.0, which is fixed in 1.16.0.**<br>~~Beginning with ARCore SDK 1.15.0, some devices will now return additional supported camera configs with lower GPU texture resolutions than the device's default GPU texture resolution. See the [ARCore supported devices](https://developers.google.com/ar/discover/supported-devices) for details.~~

## New APIs
None.

## Deprecations
None.

## Behavioral changes
None.

## Other changes
  * Vertical plane detection works better on surfaces with low visual texture.

## Bug fixes
  * Fixed build system errors on Unity 2018.3 and later in CloudAnchorPreprocessBuild.cs. Now uses `EditorPrefs.GetString("JdkPath")` only when `EditorPrefs.GetBool("JdkUseEmbedded")` is not true.
  * Setting the `AugmentedImageDatabase` to null in the ARCore Session now correctly disables Augmented Images in the ARCore session.
  * `ARCoreAugmentedFaceMeshFilter` and `ARCoreAugmentedFaceRig` components now can get the correct AugmentedFace trackable in AutoBind mode after a session reset.

## Known issues
  * Instant Preview may freeze Unity when using Android 9 and a USB 3 cable. To remedy, update to Android 10 or use a USB 2 cable.
  * Instant Preview may fail to display on device when Unity's game view resolution is too high. To remedy, lower Unity's game view resolution in the Editor.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.14.0 (2019-12-12)

## Breaking changes
  * Renamed `CameraFocusMode.Auto` to `CameraFocusMode.AutoFocus`.
  * Renamed `CameraFocusMode.Fixed` to `CameraFocusMode.FixedFocus`.

## New APIs
None.

## Deprecations
None.

## Behavioral changes
  * The `Session.LostTrackingReason` may now be `LostTrackingReason.CameraUnavailable` when the camera is in use by another application. Prior to ARCore SDK 1.13 this would have been `LostTrackingReason.None`. After an app regains priority, tracking will resume.

## Other changes
  * Changed minimum supported version of Unity to 2017.4.34f1. Apps submitted to the Play Store after November 2019 are required to target API level 28 (Android 9) or higher, which was not supported in the previously recommended version 2017.4.27f1.

## Bug fixes
  * Fixed known issue from 1.13.0 where Instant Preview shows a white camera image on Pixel 3 and Pixel 4 running Android 10 when on Mac.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.13.0 (2019-10-29)

## Breaking changes
None.

## New APIs
None.

## Deprecations
* The `ErrorAndroidVersionNotSupported` property has been deprecated and merged with `ErrorDeviceNotCompatible`. Use `ErrorDeviceNotCompatible` instead.

## Behavioral changes
  * `LightEstimate.ReflectionProbe` now returns the cubemap texture corresponding to the color space setting in **Project Setting**. When using **Gamma** color space, the texture will be in gamma space. When using **Linear** color space, the texture will be in linear space. Previously, the texture was always returned in Linear color space.

## Bug fixes
  * Fixed an issue where app crashes when `ARCoreSession` is destroyed before destroying anchors.
  * Fixed an issue where Instant Preview fails to install if `ADB_TRACE` is set.

## Known issues
  * Instant Preview does not work with the Pixel 3/3 XL and Pixel 4/4 XL running Android 10.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.12.0 (2019-09-13)

## Breaking changes
The following changes only affect apps built with ARCore SDK 1.12 or later. Apps built with older SDKs will not be affected by these changes, and will continue to observe old behavior.
  * The data captured by `CreateCloudAnchor` and uploaded to the ARCore Cloud Anchor API service has changed. See https://developers.google.com/ar/develop/unity/cloud-anchors/overview-unity#how-hosted to learn more.
  * `CreateCloudAnchor` will no longer timeout or fail when the ARCore Cloud Anchor API service is unreachable, or the anchor cannot be immediately resolved. Instead, the API will continue to attempt to resolve the provided anchors until they are detached.
  * `CloudServiceResponse.ErrorServiceUnreachable` and `CloudServiceResponse.ErrorLocalizationFailed` have been deprecated and will no longer be returned.

## New APIs
  * Added `XPSession.CancelCloudAnchorAsyncTask` API so that a request to resolve Cloud Anchors can be cancelled. When a resolve request is cancelled successfully, the async task response will be set to `CloudServiceResponse.ErrorRequestCancelled`.
  * Added `CloudServiceResponse.ErrorHostingServiceUnavailable` to indicate the hosting service unreachable error.

## Deprecations
  * `CloudServiceResponse.ErrorServiceUnreachable` and `CloudServiceResponse.ErrorLocalizationFailed` enum values in the `CloudServiceResponse` have been deprecated and will no longer be returned.

## Behavioral changes
  * `XPSession.ResolveCloudAnchor` now continues to retry in the background indefinitely until it is successfully resolved, cancelled, or reaches a terminal error state.

## Other changes
  * For iOS, changed Podfile to reference new "ARCore/CloudAnchors" subspec.
  * Added a face texture template file. See `Assets/GoogleARCore/Examples/AugmentedFaces/Models/canonical_face_texture.psd`.
  * Replaced differently colored Andy models in **HelloAR** and **ObjectManipulation** samples with one new ARCore 'pawn' model.
  * **CloudAnchors** sample updates:
    * Added a resolving prepare time so that the app won't start to resolve the anchor until the preparation time passed.
    * Added a customized timeout duration in `AnchorController` to prevent retrying that results in reesolving indefinitely.
    * Added a _Return to Lobby_ button while in AR, and when disconnected from the server.
    * Added a start screen that displays sharing experience instructions on the first launch.
    * Removed the `Assets/GoogleARCore/Examples/CloudAnchors/Scripts/MultiplatformMeshSelector.cs` and moved the mesh setting logic to `StarController` and `AnchorController` to solve the issue where the asset is placed at the identity pose.
    * Replaced the boat anchor 3D asset with a new model to emphasize the orientation of the Cloud Anchor.
    * Added an addition function to `AugmentedImageDatabaseInspector`.

## Bug fixes
  * Fixed bug where configuration is not synchronized after camera direction changed.
  * Fixed invalid assignment of [Screen.sleepTimeout](https://docs.unity3d.com/ScriptReference/Screen-sleepTimeout.html) in samples.
  * Fixed issue where `Assets/GoogleARCore/SDK/Materials/ARBackground.shader` doesn't work on devices with Mali GPU when the app renders on OpenGL ES2.
  * Fixed issue where generating an ARCore Bug Report wouldn't work when illegal characters were in PATH.
  * Fixed [#438](https://github.com/google-ar/arcore-unity-sdk/issues/438) where Augmented Images databases wouldn't recalculate image quality scores after the underlying image had been updated.
  * Fixed [#614](https://github.com/google-ar/arcore-unity-sdk/issues/614): Continuous error in logcat: ArLightEstimate_getEnvironmentalHdrAmbientSphericalHarmonics while using IL2PP on 2019.1.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.11.0 (2019-08-05)

## Breaking changes
None.

## New APIs
  * Added `ARCoreCameraConfigFilter` APIs with options for the camera capture frame rate and depth camera usage.
  * Added `MinFPS`, `MaxFPS`, and `DepthSensorUsage` properties to `CameraConfig`.

## Deprecations
None.

## Behavioral changes
  * **ARCore Device** prefab uses the new `Assets/GoogleARCore/Configurations/DefaultCameraConfigFilter` which will enable 60fps on a subset of ARCore supported devices. For a detailed list, see [ARCore supported devices](https://developers.google.com/ar/discover/supported-devices).

## Other changes
  * The ARCore service has been renamed to [Google Play Services for AR](https://play.google.com/store/apps/details?id=com.google.ar.core). On Google Play devices it is now distributed as part of Google Play Services.
  * Updated `ViewInARIcon.png` loading icon.
  * `DetectedPlaneVisualizer` sample component now uses white for all detected planes.
  * Upgraded **PlayServicesResolver** plugin to v1.2.122.
  * **ComputerVision** sample now selects the highest CPU resolution with highest FPS for the "High Resolution CPU image" button and the lowest CPU resolution with highest FPS for the "Low Resolution CPU image" button.
  * All other samples now select a 60fps configuration when available.
  * In **ComputerVision** sample, moved `RegisterChooseCameraConfigurationCallback` from `Start()` to `Awake()` to ensure it is triggered the first time `ARCoreSession` is enabled.

## Bug fixes
  * Fix bug in **ComputerVision** example where it occasionally displayed a black screen when first launched.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.10.0 (2019-06-20)

## Breaking changes
None.

## New APIs
  * New Environmental HDR modes added to Light Estimation API enable more acurate light estimation:
    * `LightEstimateMode.EnvironmentalHDRWithoutReflections` and `LightEstimateMode.EnvironmentalHDRWithReflections`.
    * Environmental HDR provides developers with three APIs to replicate real world lighting when using the back-facing camera:
      * Main Directional Light: helps with casting shadows in the right direction.
      * Ambient Spherical Harmonics: helps model ambient illumination from all directions.
      * HDR Cubemap: helps model realistic reflections.

## Deprecations
  * Deprecated `ARCoreSessionConfig.EnableLightEstimation`. To choose the light estimation mode, use `ARCoreSessionConfig.LightEstimationMode`. To enable the previous behavior when `ARCoreSessionConfig.EnableLightEstimation` was true, set the light estimation mode to `LightEstimationMode.AmbientIntensity`. See more details in `GoogleARCore.LightEstimationMode`.
  * Deprecated the constructor `LightEstimate(LightEstimateState, float, Color)`. Instead, use `LightEstimate(LightEstimateState, float, Color, Quaternion, Color, float[,], long)`.

## Behavioral changes
None.

## Other changes
  * Upgraded **PlayServicesResolver** plugin to v1.2.105.0.
  * `Assets/GoogleARCore/Configurations/DefaultSessionConfig` now defaults to **Environmental HDR With Reflections** light estimation mode. If you created your own `SessionConfig` asset, it will retain the previously configured light estimation mode.

  * **Environmental HDR With Reflections** light estimation mode is now used in **HelloAR**, **AugmentedImage**, **CloudAnchors**, **ComputerVision** and **ObjectManipulation** samples.
  * Added Directional Light in `Environmental Light.prefab` to support new light estimation modes.
  * Added a log message in `ARCoreSession` component that indicates Light Estimation may not work properly when `EnvironmentalLight` component is not in the scene.
  * Updated `AndyBlue` and `AndyGreen` prefabs to use metallic materials.
  * Added `AndyPurple` prefab and updated **HelloAR** to place it on vertical plane.
  * A new **TransparentShadow** example shader in `Assets\GoogleARCore\Examples\Common` shows how to use light estimation for higher quality shadows.
  * Instant Preview now supports changing update mode, planefinding mode, and focus mode.
  * Instead of relying on `UnityEngine.XR.ARBackgroundRenderer` in Unity, `ARCoreBackgroundRender` now uses its own command buffer to render the camera background.

## Bug fixes
  * Fixed bug in **CloudAnchors** example where a host disconnecting due to a crash or force quit would leave a non-functional room.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.9.0 (2019-05-07)

## Breaking changes
None.

## New APIs
  * Added `AugmentedImage.GetTrackingMethod()` that enables app to tell whether an Augmented Image is currently tracked by camera view or its last known position.

## Deprecations
None.

## Behavioral changes
  * Augmented Images no longer relies solely on the session's tracking state, see [Recognize and Augment Images](https://developers.google.com/ar/develop/unity/augmented-images/) for more information.

## Other changes
  * Added **Help > Capture ARCore Bug Report** menu item to assist in troubleshooting issues with Instant Preview.
  * Removed checking session's tracking state in **AugmentedImages** sample app and added a brief comment to describe `AugmentedImage.TrackingMethod` use case.
  * Instant Preview logs a warning and shows a toast message when touches are detected, but not consumed by `InstantPreviewInput` within one second. To learn more about handling input while using Instant Preview, see https://developers.google.com/ar/develop/unity/instant-preview
  * Instant Preview logs a warning and shows a toast message when attempting to use APIs that are not yet supported by Instant Preview.
  * Added details of how to apply the Apache License to your work to our LICENSE file.

## Bug fixes
  * Fixed typo, renamed `AumgnetedFaceSessionConfiguration` to `AugmentedFaceSessionConfiguration`.
  * Fixed [#277](https://github.com/google-ar/arcore-unity-sdk/issues/277): Cannot get camera feed when using OPENGLES2 as graphics API.
  * Fixed ARCore Analytics recorded data and option text clipped in the preferences dialog in 2018.3 and later.
  * Fixed [#268](https://github.com/google-ar/arcore-unity-sdk/issues/268): ARBackground shader doesn't work correctly for Linear color space.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.8.0 (2019-03-29)

## Known issue in Unity 2019.1.0b9
  * [Unity Issues ID 1133177](https://issuetracker.unity3d.com/issues/video-importing-arcore-sdk-for-unity-1-dot-7-0-package-crashes-editor-when-importing-hand-eom-dot-webm-video): Unity crashes when importing `Assets/GoogleARCore/Examples/Common/Videos/hand_oem.webm`. This issue should be fixed in Unity 2019.1.0b10.

## Breaking changes
None.

## New APIs
None.

## Deprecations
None.

## Behavioral changes
  * The ARCore SDK for Unity now sends SDK usage analytics to Google when an ARCore-enabled project is opened, or the SDK is imported into an existing Unity project. For more information or to disable analytics, visit [the developer website](https://developer.google.com/ar/develop/unity/unity-sdk-analytics).
  * Disabling, then enabling the `ARCoreSession` component on the **ARCore Device** prefab will pause, then resume ARCore in next frame.

## Other changes
  * Enabled auto focus by default in **AugmentedImages** sample app, to improve tracking of small targets.
  * Upgraded **PlayServicesResolver** plugin to v1.2.100.
  * **CloudAnchors** sample now displays error message using the snack bar instead of using Android toast messages.
  * Reformated code to fit within 100-character lines.
  * Improve translation behavior when moving an object from a higher plane to a lower plane in the Object Manipulation Example.
  * Updated the **CloudAnchors** sample to not create an anchor for stars.
  * Updated the **AugmentedFaces** sample to use its own icon.
  * Updated the **CloudAnchors** and **ComputerVision** samples to not overlap UI with notch on certain phones.
  * Added a build preprocess warning message to indicate that Scriptable Rendering Pipeline Asset is currently not supported by ARCore.

## Bug fixes
  * Fixed [#449](https://github.com/google-ar/arcore-unity-sdk/issues/449) Unnecessary `"android.permission.READ_PHONE_STATE"` added.
  * Fixed issue where closing the help window would also place an Andy in the **HelloAR** sample.
  * Fixed issue where a spurious null-reference exception could be thrown upon `ARCoreSession` component destruction.
  * Fixed issue where cannot get Camera Configuration list at the first time a session is created.
  * Fixed issue in **ComputerVision** sample where selecting "High Resolution CPU Image" does not take effect.
  * Fixed issue where `ChooseCameraConfigurationCallback` may return camera configurations that have the wrong camera facing.
  * Fixed issue in *CloudAnchors* sample where objects that were placed on vertical planes didn't have the correct orientation.
  * Fixed duplicate 'Edit > Project Settings' menu item in Unity 2018.3 and later.
  * Fixed issue where 'Assets > Create > Google ARCore > AugmentedImageDatabase' menu item is grayed out when no image is selected.
  * Fixed [#523](https://github.com/google-ar/arcore-unity-sdk/issues/523) **ObjectManipulation** scene doesn't work with Instant Preview.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.7.0 (2019-02-14)

## User privacy requirements
  * Updated `README.md` to clarify ARCore's **Terms & Conditions** and [User Privacy Requirements](//developers.google.com/ar/distribute/privacy-requirements).

## Breaking changes
None.

## New APIs

  * **Augmented Faces** and **front-facing camera** support
    * Added the ability to use the front-facing (selfie) camera with the `ARCoreSession` component, as well as animation for camera transitions.
      * **Note:** Motion tracking, all types of anchors, Augmented Images, and plane detection are not available when using the front-facing camera.
    * New Augmented Faces mode supported when using the front-facing (selfie) camera.
      * New `AugmentedFaceMode` property in `ARCoreSessionConfig` to enable Augmented Faces in ARCore session.
      * New `AugmentedFace` class (extends `Trackable`) that provides face poses and mesh estimation for detected faces.
      * New **AugmentedFaces** sample scene to demonstrate how to use the new Augmented Faces API.
  * New `VersionInfo.Version` to retrieve a string describing the ARCore SDK for Unity version.
  * Added `Session.LostTrackingReason` that enables apps to provide actionable feedback to users when tracking is lost.
  * Added `Frame.TransformCoordinate()` that transforms between different 2D coordinates systems, see `DisplayUvCoordinateType` enum.

## Deprecations
  * `Frame.CameraImage.DisplayUvCoords()` has been deprecated. Use `Frame.TransformCoordinate()` instead.

## Behavioral changes
  * Enabling and disabling the `ARCoreSession` component on the **ARCore Device** prefab now doesn't take effect until the next frame. Disabling the ARCore session will no longer stall the main thread.

## Other changes
  * New **Plane Discovery** prefab that shows a guide to help users find a plane.
  * New **Feature Point Visualizer** that includes a visual popping effect.
  * New **Object Manipulation** sample showing interactions for manipulating objects in AR.

## Bug fixes
  * Fixed [issue 244](https://github.com/google-ar/arcore-unity-sdk/issues/244), background color green-blue swapped issue with HDR enabled on Samsung Exynos devices.
  * Fixed [issue 439](https://github.com/google-ar/arcore-unity-sdk/issues/439), augmented_image_cli hangs when using uppercase file extensions — gives the message "Calculating..." but never shows final score
  * Fixed application crash for apps that were built with **Player Settings > XR Settings > ARCore Supported** disabled.
  * Instant Preview emits a warning when build platform is not set to Android. Helps avoid Instant Preview stream being scaled incorrectly due to high resolution monitors on 'standalone' (desktop) build platforms restricting the game view scale to 2x and higher.
  * Fixed [issue 389](https://github.com/google-ar/arcore-unity-sdk/issues/389), failure to restore camera projection matrix when `ARCoreBackgroundRenderer` component is disabled.
  * Fixed [issue 327](https://github.com/google-ar/arcore-unity-sdk/issues/327) where the Camera's _global_ position and rotation were updated while running in Instant Preview instead of the _local_ position and rotation.
  * Eliminated console build warnings.
  * Updated Instant Preview console messages for clarity.
  * Updated `ExampleBuildHelper` so that builds the SDK sample scenes no longer overwrites the Android Package Name and Product Name. Application icons are now only updated if they are unset, or are set to one of the included sample icons.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.6.0 (2018-12-06)

## Behavioral changes
  * Instant Preview now briefly shows a warning message as reminder about it's limited support for touch based input.

## Other changes
  * Updated samples to call out current limitations more clearly when running in editor.
  * Play Services Resolver plugin upgraded to version 1.2.95.0.
  * Log messages and ARCore Preferences have been updated for clarity.
  * Added an updated Cloud Anchors example that uses Unity's Multiplayer Services.
  * Binaries in the SDK are now covered by the [Google API TOS](https://developers.google.com/terms/).

## Bug fixes
  * Fixed [#240](https://github.com/google-ar/arcore-unity-sdk/issues/240): Replaced `SetIndices` with `SetTriangles` in `DetectedPlaneVisualizer`.
  * Fixed [#297](https://github.com/google-ar/arcore-unity-sdk/issues/297): "Cannot create Augmented Image database" if the Unity project path has a space in the name.
  * Catch `SocketException` in **CloudAnchors** sample and log details when IP address cannot be determined at runtime.
  * Fixed gradle project compilation error when using Cloud Anchors in an "AR Optional" app.
  * Fixed [#450](https://github.com/google-ar/arcore-unity-sdk/issues/450): Instant Preview correctly streams video on Pixel 3 and Pixel 3 XL.
  * [Issue #563](https://github.com/google-ar/arcore-android-sdk/issues/563): Resolved an crash with a log message "AssetManager has been finalized" that could occur on some devices.
  * [Issue #630](https://github.com/google-ar/arcore-android-sdk/issues/630): Fixed a bug where multiple points in a point cloud could use the same ID.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.5.0 (2018-09-27)

> EDIT 2018-01-12: Removed issue [#297](https://github.com/google-ar/arcore-unity-sdk/issues/297) (_Cannot create Augmented Image database if the Unity project has a space in the name._) from the list of 'Bug fixes' as the issue is not actually fixed in 1.5.0


## Breaking changes
None.

## New APIs
  * API for accessing a unique ID associated with point cloud points, to discern if the same point is seen across multiple frames.  Note: This API is not yet available in Instant Preview. Point cloud IDs will always be 0 when running in play mode in the editor.
  * API for adding images to an Augmented Image database at runtime.

## Deprecations
  * `Frame.PointCloud.CopyPoints()` has been deprecated without replacement. Instead, iterate over the points in the point cloud and manually copy desired points manually.
  * `Frame.PointCloud.GetPoint()` has been deprecated in favor of `Frame.PointCloud.GetPointAsStruct()`, which returns a struct that also contains a unique ID for each point.

## Behavioral changes
  * In Unity 2018.2.1 and later (using the new [`PlayerSettings.Android.ARCoreEnabled`](https://docs.unity3d.com/ScriptReference/PlayerSettings.Android.ARCoreEnabled.html) Unity API), show a warning message when **XR Settings > ARCore Support** is not enabled and build platform is set to Android.
  * When ARCore needs to be installed or updated, [AR Optional](https://developers.google.com/ar/develop/unity/enable-arcore) apps will no longer display an initial screen informing the user that ARCore is required and instead immediately show the install screen.
  * The **First Person Camera** (a child of the **ARCore Device** prefab) has new `TrackedPoseDriver` settings. Specifically, the 'Update Type' member has changed from a value of 'Before Render' to 'Update'. This maintains synchronization of the First Person Camera transform with other elements in the scene by default.
  * `Session.RequestApkInstallation(bool)' now defaults to skipping the user education dialog for [AR Optional](https://developers.google.com/ar/develop/unity/enable-arcore) apps.

## Other changes
  * Added `inputString` to `InstantPreviewInput.cs`.
  * Added a compile time error message when using an unsupported versions of Unity.
  * Added `SuppressMemoryAllocationError` attribute to mark explicitly the methods that can allocate memory.
  * Fix for `ReflectionTypeLoadException` in Unity 2018.3.
  * Added better error handling when resolving a XP Anchor.
  * Added ARM 64 support.
  * Added Android Emulator support.

## Bug fixes
  * Fixed [#212](https://github.com/google-ar/arcore-unity-sdk/issues/212): Unity experimental Linux compile errors.
  * Fixed obsolete warnings in 2018.1 and later.
  * Fixed [#275](https://github.com/google-ar/arcore-unity-sdk/issues/275#issuecomment-401149301): ARCore is causing Unity Cloud Build to fail.
  * Fixed [#357](https://github.com/google-ar/arcore-unity-sdk/issues/357): Instant Preview initializes but does not transmit or receive data.
  * Fixed issue where Instant Preview would stop streaming after pausing or backgrounding the Unity game window.
  * Fixed [#277](https://github.com/google-ar/arcore-unity-sdk/issues/277): Background renderer doesn't work when using OpenGLES2 graphics API.
  * Fixed issue where image names and Cloud Anchor IDs sometimes contain extra characters at the end when using Instant Preview.
  * [ARCore SDK for Android issue #419](https://github.com/google-ar/arcore-android-sdk/issues/419): Added a workaround that should reduce or eliminate cases of poor or no motion tracking on Qualcomm-based Samsung Galaxy S9, S9+ and Note9 devices.
  * [ARCore SDK for Android issue #469](https://github.com/google-ar/arcore-android-sdk/issues/469): Resolved a race condition that could cause ARCore to report a device as unsupported immediately after ARCore is updated.
  * Resolved some cases where `Session.RequestApkInstallation(bool)` could result in a 'Session.Status' of [FatalError](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#namespaceGoogleARCore_1a33759b4917069ee919d9113ff71bca11).



-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.4.1 (2018-08-23)

## Bug fixes
  * Fixed `'UnityEngine.Network' is obsolete: The legacy networking system has been removed in Unity 2018.2. Use Unity Multiplayer and NetworkIdentity instead.'` in Unity 2018.2 and above.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.4.0 (2018-08-02)

## Breaking changes
None.

## New APIs
  * New methods `Frame.Raycast(...)` and `Frame.RaycastAll(...)` that perform a hit test with an arbitrary ray constructed from given origin and direction.
  * New `ARCoreSession.CameraFocusMode` to enable/disable auto-focus for the AR Camera.
  * New ARCore Camera Configuration API: ARCore Camera can be configured by register the ARCoreSession.RegisterChooseCameraConfigurationCallback.

## Deprecations
None.

## Behavioral changes
  * ARCore iOS support is now disabled by default. To enable ARCore iOS support for using ARCore Cloud Anchors on iOS, check the **iOS Support Enabled** checkbox in **ARCore Project Settings** window (**Edit > Project Settings > ARCore**). See [#261](https://github.com/google-ar/arcore-unity-sdk/issues/261)
  * The ARCore SDK no longer has a hard dependency on the Unity ARKit SDK when ARCore iOS support is not enabled.

## Other changes
  * Renamed "CloudAnchor" sample scene to "CloudAnchors", to match API name.
  * ARCore "ComputerVision" sample now supports options to configure the CPU image resolution.
  * `DetectedPlane` now exposes its type (`HorizontalUpwardFacing`, `HorizontalDownwardFacing`, `Vertical`).
  * Renamed Instant Preview plugins to `arcore_instant_preview_*` to prevent conflicts with GoogleVR Instant Preview plugins.
  * "HelloAR" sample places green Andys on planes and blue Andys on feature points.
  * "CloudAnchors" sample shows better error messages when there are network issues when sharing the anchor id among devices.
  * The edge detection algorithm in the "ComputerVision" sample now handles the case that the row stride has more pixel than the image width in camera image.
## Bug fixes
  * Fixed documentation URLs on the `ARCoreBackgroundRenderer`, `ARCoreSession`, `ARCoreSessionConfig`, `Anchor`, `EnvironmentalLight` and `XPAnchor` script components. Clicking on the inspector view's doc icon on these components now launches the corresponding ARCore references docs.
  * Fixed [#274](https://github.com/google-ar/arcore-unity-sdk/issues/274): Unity project contains ARCore SDK cannot be built targeting Standalone.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.3.0 (2018-06-21)

## Breaking changes
None.

## New APIs
  * **Camera Intrinsics API**. Added the methods to get the camera texture/image intrinsics.

## Deprecations
None.

## Behavioral changes
  * The **ComputerVision** sample now displays the camera intrinsics values on screen.
  * ARCore project settings (**Edit > Project Settings > ARCore**) now allow setting a different API Key for Android and iOS apps.

## Other changes
  * The `cloud_anchor_manifest.aar` file is only regenerated when the API Key has changed.
  * **CloudAnchors** example supports the ARKit plugin released in the Asset Store.

## Bug fixes
  * Fixed `NullReferenceException` after stopping Play mode in the editor.
  * Fixed malformed console messages due to missing missing newlines characters in Instant Preview standard output and standard error.
  * Fixed broken build when using IL2CPP.
  * Fixed `InvalidOperationException` when running ARCore application built using _.NET 4.6_.
  * Each ARCore sample app now has a unique icon.
  * Fixed an issue where an `AugmentedImageDatabase` was not properly rebuilt after a name or physical width change.
  * Fixed [#215](https://github.com/google-ar/arcore-unity-sdk/issues/215): macOS: `augmented_image_cli_osx` exception attempting to open `Example Database.asset` in **AugmentedImage** sample.
  * Fixed [#182](https://github.com/google-ar/arcore-unity-sdk/issues/182): Camera orientation issue when orientation locked to landscape mode.
  * Fixed [#188](https://github.com/google-ar/arcore-unity-sdk/issues/188): ARCore materials show as black in Unity Editor.
  * Fixed the crash when ARCore session failed to create on iOS devices.
  * Fixed exception thrown when calling `ArImageMetadata` methods while previewing in editor. These methods are not implemented in Instant Preview. They will now return `ErrorMetadataNotFound` if called from the editor while using Instant Preview.
  * Fixed [#237](https://github.com/google-ar/arcore-unity-sdk/issues/237): Unable to find the Aumgented Image CLI tools on Windows uing .Net 4.6.
  * Fixed the crash when playing ARCore enabled scene in Unity Editor while targeting iOS.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.2.1 (2018-05-30)

## Other Changes
  * iOS `Podfile` updated to require ARCore 1.2.1. See [ARCore SDK for iOS Release Notes](https://github.com/google-ar/arcore-ios-sdk/releases).


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.2.0 (2018-05-08)

> Updated on 2018-05-11 to remove temporary APK sideload instructions.

## Breaking changes
None.

## New APIs
  * **Cloud Anchors API**. Enables developers to build shared AR experiences across iOS and Android, by allowing anchors created on one device to be transformed into cloud anchors and shared with users on other devices.
  * **Augmented Images API**. Enables ARCore apps to detect and track images.
  * **Vertical plane detection**. ARCore now detects both horizontal and vertical planes.

## New Samples
  * New **CloudAnchor** sample that shows how to share anchors between devices on both Android and iOS.
  * New **AugmentedImage** sample that shows how to configure images for detection and pose tracking in ARCore.

## Deprecations
  * `ARCoreSessionConfig.EnablePlaneFinding` has been replaced by `ARCoreSessionConfig.PlaneFindingMode` which now supports enabling vertical and horizontal planes separately. If you get `ARCoreSessionConfig.EnablePlaneFinding` in code, it will be `false` when `PlaneFindingMode` is `Disabled` and `true` otherwise. If you set `ARCoreSessionConfig.EnablePlaneFinding` in code, it will disable both horizontal and vertical plane finding when set to `true` and enable both when set to `false`.
  * `TrackedPoint` has been renamed to `FeaturePoint`, and `TrackedPlane` has been renamed to `DetectedPlane`, to match the respective ARCore feature names. If you use C# reflection you will continue to get `Type` objects for the _deprecated_ types. This will change when the deprecated classes are ultimately removed in a future ARCore release.

## Behavioral changes
  * The default session config now enables both horizontal and vertical plane detection. Previously only horizontal planes were detected.

## Other changes
  * Moved common example assets into `/Assets/GoogleARCore/Examples/Common/` directory.
  * SDK now includes the [PlayServicesResolver](https://github.com/googlesamples/unity-jar-resolver) to enable compatibility with other Google iOS SDKs.
  * Sample apps now build with a new ARCore icon.
  * Computer Vision example displays edge detection in black and white.
  * Computer Vision example shows a full overlay image when edge detection enabled.

## Bug fixes
  * Unity's Game window can now be resized while previewing in the editor with Instant Preview.
  * Instant Preview touch input no longer misses frames.
  * Instant Preview handles multiple touches.
  * Fixed [#166](https://github.com/google-ar/arcore-unity-sdk/issues/166): Instant Preview uses normalized input coordinates so that it will work with any Unity game window size.
  * `EnvironmentalLight` sets `_GlobalLightEstimation` for backward compatibility with SDK version 1.0.
  * Fixed [#146](https://github.com/google-ar/arcore-unity-sdk/issues/146): The ARCore Device prefab's First Person Camera game object has been MainCamera. This means `Camera.main` will now return the ARCore camera.
  * ARCore APK installation can be declined without causing a crash.

## Known Issues
  * If the API key specified for authenticating with the ARCore Cloud Anchor service is invalid, the final cloud anchor state of the anchor will be `ErrorInternal` instead of `ErrorNotAuthorized`. This is a known issue and will be fixed in an upcoming release.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.1.0 (2018-03-29)

ARCore enables AR applications to track a phone’s motion in the real world, detect planes in the environment, and understand lighting in the camera scene.

## New APIs
  * New API for synchronously (in the same frame) acquiring the image from a camera frame (without manually copying the image buffer from the GPU to the CPU). This lets you run your own image processing algorithms on the images from the camera.
  * New API for getting color correction information added to `LightEstimate` for the images captured by the camera. This lets you render your virtual objects in a way that better matches the real-world lighting conditions.

## New features
[Instant Preview](https://developers.google.com/ar/develop/unity/instant-preview) allows you to skip the build process and test ARCore apps instantly on your phone during development. Click _Play_ in the Unity editor to preview your app on your phone using real device input and output.

## API changes
  * `Frame.PointCloud.GetPoint()` now returns a `Vector4`. It returns the `pointcloud` point along with its confidence value. Existing code using `Vector3` will continue to work as `Vector4` implicitly converts to `Vector3`.
  * The `Frame.CameraImage.DisplayUvCoords` property now returns a `DisplayUvCoords` type instead of the internal `ApiDisplayUvCoords` type.
  * Exposed `ValueType` in `CameraMetadataValue`.

## Other changes
  * We have updated the names of game objects in "HelloAR" scene for consistency.

## Known issues
  * Unity Instant Preview currently has a number known [limitations](https://developers.google.com/ar/develop/unity/instant-preview#limitations).

## Bug fixes
  * Fixed [#81](https://github.com/google-ar/arcore-unity-sdk/issues/81): Disabling `ARCoreBackgroundRenderer` sets `ARBackgroundRenderer.mode` to `ARRenderMode.StandardBackground`.
  * Fixed [#118](https://github.com/google-ar/arcore-unity-sdk/issues/118): Denying camera permission and/or APK installation is now possible without restarting the app.
  * Fixed crash when disabling ARCore supported in **Player Settings > XR Settings**.
  * Fixed null dereference in `TexReader` on the first frame.
  * Fixed [#123](https://github.com/google-ar/arcore-unity-sdk/issues/123): Removed hardcoded path in `BuildHelper.cs`.
  * Proper use of pixel intensity light estimate in HelloAR.
  * Fixed: Unneeded `READ_PHONE_STATE` permission will no longer be added to the APK's `AndroidManifest.xml`.
  * Fixed [#124](https://github.com/google-ar/arcore-unity-sdk/issues/124): Removed log _"Unable to find Anchor component"_ after an anchor is destroyed.
  * Fixed: Can now set configuration on `ARCoreSession` created via script, or at startup via `Awake()`, before the session gets enabled.
  * Fixed [#126](https://github.com/google-ar/arcore-unity-sdk/issues/126): Modifying session configuration at runtime is now possible.
  * Fixed [#128](https://github.com/google-ar/arcore-unity-sdk/issues/128): Objects from old sessions are deleted when a new `ARCoreSession` is created.
  * Fixed: `TrackableQueryFilter` now iterates over only `"Updated"` trackables.

## Supported Devices
This release is only supported on, and should only be installed on, qualified devices running Android N and later. See the list of [ARCore supported devices](https://developers.google.com/ar/discover/#supported_devices) for specific device models.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.0.0 (2018-02-23)

ARCore enables AR applications to track a phone’s motion in the real world, detect planes in the environment, and understand lighting in the camera scene. ARCore 1.0 introduces the placement of anchors on textured surfaces (like posters or plant leaves) using oriented points. The Android Emulator in Android Studio 3.1 Beta supports ARCore 1.0.

ARCore 1.0 is available for use in production apps on supported Android devices. ARCore 1.0 introduces the concept of AR required and AR optional apps. Apps should be classified as required or optional to ensure the ARCore service install is properly handled by the Play Store.

## Supported Devices
  * This release is only supported on, and should only be installed on, qualified devices running Android N and later. See the list of [ARCore supported devices](https://developers.google.com/ar/discover/#supported_devices) for specific device models.
  * Apps built with ARCore Developer Preview or ARCore Developer Preview 2 are not supported on with ARCore 1.0.

## Known Issues
  * None.

## Lifecycle issues
  * ARCore does not support picture-in-picture mode.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity Developer Preview 2 (2017-12-15)

> 2018-01-30 EDIT: Added link to ARCore supported devices.

This is a developer preview SDK that enables prototyping AR experiences using ARCore on supported Android devices.

ARCore enables AR applications to track a phone’s motion in the real world, detect planes in the environment, and understand lighting in the camera scene.

This developer preview release is not intended for use in production apps that are shipped to customers. Future versions of ARCore might introduce breaking changes to the API.

# Supported Devices
This release is only supported on, and should only be installed on, qualified devices running Android N and later. See the list of [ARCore supported devices](//developers.google.com/ar/discover/#supported_devices) for specific device models.

Installing this release on phones other than the supported devices might break other software on the device.

# Notes
## Multiple Versions of ARCore
- Apps built with the original ARCore SDK Developer Preview are only compatible with the original `arcore-preview.apk`. Apps built with ARCore SDK Developer Preview 2 are only compatible with `arcore-preview2.apk`.
- A developer can install versions of ARCore provided in both the original ARCore SDK Developer Preview and ARCore SDK Developer Preview 2 at the same time to facilitate using apps built with either SDK. The original arcore-preview.apk will be visible as “Tango Core” in the Android Apps settings whereas arcore-preview2.apk will be visible as “ARCore” in the Android Apps settings.

## Known Issues
- `ArSession_configure()` does not apply settings changes if called while in a resumed state.

# What’s New In Developer Preview 2
## New SDK
- Developer Preview 2 features an Android NDK (C API)
## API Changes
- The API interfaces have undergone significant revision and changes. All key functionality from the original Developer Preview remains, but the methods and function calls have changed.
- The API now natively supports attaching Anchors to Planes.
- ARCore Developer Preview 2 includes Unity and Java samples for camera image data access on the CPU. The provided Computer Vision sample is useful for running custom computer vision algorithms or image analysis on the camera image data.
## Lifecycle Improvements
- ARCore apps may recover planes and anchors if the session is paused and then resumed, provided the user is in roughly the same location and the environment or lighting has not changed significantly. This enables scenarios where AR content is not lost when the user briefly switches apps.
## Unity
- Developer Preview 2 requires Unity 2017.3.0f2 or higher. Note this version is higher than what was required in the original SDK Developer Preview.

## Fixes
- Many stability and performance improvements.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

# ARCore SDK for Unity Developer Preview (2017-08-25)

This is a preview that enables prototyping AR experiences using ARCore on supported Android devices.

ARCore enables AR applications to track a phone’s motion in the real world, detect planes in the environment, and understand lighting in the camera scene.

 > Important: This release is not a production release; it is not intended for use in production apps shipped to customers. Future versions of ARCore may introduce breaking changes to the API.

# Supported Devices
This release is only supported on qualified devices running Android N and later, which include:
 - Google Pixel
 - Google Pixel XL
 - Samsung S8 SM-G950F
 - Samsung S8 SM-G950FD
 - Samsung S8 SM-G950N
 - Samsung S8 SM-G950U
 - Samsung S8 SM-G950U1
 - Samsung S8 SM-G950W

 > Installing this release on phones other than the supported devices may break other software on the device.

# Known Issues
## Lifecycle issues
 - Task switching across two or more ARCore apps is not currently supported and will break AR session tracking
 - ARCore apps will not recover planes and anchors if the session is paused and then resumed. Anchors will stop tracking after a pause() and a subsequent resume() and planes will have to be re-discovered.
## Anchors
 - Each additional anchor reduces performance; using more than a dozen anchors may noticeably reduce app performance.
## Error messages
 - Apps built using ARCore will produce a notification on user debug builds, “Tango Cloud ADFs 0%.” This does not indicate any problems with the application and can be safely ignored.
## Unity SDK
 - Occasionally an app may experience a known issue connecting to the AR session at startup. In this case, ARCore will display an error message toast and quit the application. Restarting should fix the issue.
 - Apps sometimes experience issues when powering the device on and off during an ARCore session. If the app freezes for more than 4 seconds, pausing and resuming should fix the issue.
 - Targeting API 26 or higher will cause permission checks to fail on Pixel devices. Targeting API 24 or 25 will fix the issue.

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
