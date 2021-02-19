# ARCore SDK for Unity v1.22.0 (2020-12-11)

## Upcoming breaking change affecting Cloud Anchors apps built using ARCore SDK 1.11.0 or earlier

Beginning in **January 2021**, AR-enabled apps built using **ARCore SDK 1.11.0 or earlier will no longer be able to host or resolve Cloud Anchors**. Specifically, Cloud Anchors returned by [`XPSession.CreateCloudAnchor(Anchor)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#createcloudanchor) and [`XPSession.ResolveCloudAnchor(string)`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/XPSession#resolvecloudanchor) will always have the state [`CloudServiceResponse.ErrorInternal`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore/CrossPlatform#cloudserviceresponse).

Apps built with ARCore SDK 1.12.0 or later are unaffected. Their use of Cloud Anchors APIs is covered by the [Cloud Anchors deprecation policy](https://developers.google.com/ar/develop/cloud-anchors/deprecation-policy).

## Known issues
  * Support for stereo camera depth is expected to become available in ARCore SDK 1.23.0.
  * Apps may crash when calling `Application.Quit()`. This affects all ARCore app. See details in [Unity's Issue Tracker](https://issuetracker.unity3d.com/issues/arcore-android-application-crashes-when-exiting-the-app).
  * [Issue 141500087](https://issuetracker.google.com/141500087): When using Android Emulator `x86_64` system images on macOS with ARCore SDK 1.16.0 or later, Google Play Services for AR will crash. As a workaround, use an `x86` system image.

## Breaking & behavioral changes
None.

## New APIs and capabilities
None.

## Deprecations
None.

## Other changes
None.

## Bug fixes
None.
