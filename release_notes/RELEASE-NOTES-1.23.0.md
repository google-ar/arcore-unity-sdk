# ARCore SDK for Unity v1.23.0 (2021-02-17)

|| TODO: At release time, merge in C/Java API release notes from:
||       //third_party/arcore/RELEASE-NOTES.md

## Known issues
None.

## Breaking & behavioral changes
  * Changing the camera direction in an active session now requires disabling and enabling the session to take effect. [`SessionStatus.ErrorInvalidCameraConfig`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#sessionstatus) will be thrown if the new camera direction is incompatible with the session configuration.

  * AR-enabled apps built using **ARCore SDK 1.11.0 or earlier are no longer able to host or resolve Cloud Anchors**.
    * Cloud Anchors returned by [`XPSession.CreateCloudAnchor(Anchor)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#createcloudanchor_1) and [XPSession.ResolveCloudAnchor(string)](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#resolvecloudanchor) will always have state [`CloudServiceResponse.ErrorInternal`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore/CrossPlatform#cloudserviceresponse).
    * Apps built with ARCore SDK 1.12.0 or later are unaffected. Their use of Cloud Anchors APIs is covered by the [deprecation policy](https://developers.google.com/ar/distribute/deprecation-policy).

## New APIs and capabilities
  * Developers can [enable ARCore API call logging](https://developers.google.com/ar/develop/unity/debugging-tools/call-logging) to the [Android debug log](https://developer.android.com/studio/debug/am-logcat) by sending a broadcast intent.
  * Developers can [enable the ARCore performance overlay](https://developers.google.com/ar/develop/unity/debugging-tools/performance-overlay) by sending a broadcast intent.
  * Added new API for Facing Direction in `CameraConfig`: [Developer guide](https://developers.google.com/ar/develop/unity/camera-configs), addition of [`FacingDirection`](https://developers.google.com/ar/reference/c/group/ar-camera-config#facingdirection) property to `CameraConfig`.
  * [`Session.Status`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#sessionstatus) now returns errror status [`SessionStatus.ErrorInvalidCameraConfig`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#sessionstatus) when ARCore Session failed to find a valid camera config during resuming.

## Deprecations
None.

## Other changes
  * Upgraded **ExternalDependencyManager** plugin (formerly **PlayServicesResolver**) to v1.2.162.
  * Android build now includes Keyless dependencies when [`ARCoreSessionConfig.CloudAnchorMode`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreSessionConfig#cloudanchormode) is not `Disabled` and Keyless authentication is selected in **Edit > Project Settings > Google ARCore**.
  * Removed the unused blur pass parameters from the `Depth Effect` component.


## Bug fixes
  * Various bug fixes and performance improvements.
