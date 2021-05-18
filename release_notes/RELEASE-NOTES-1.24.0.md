-------------------------------------------------------------------------------
# RELEASE NOTES TEMPLATE / COPY-PASTE THIS INTO A NEW SECTION AS NEEDED
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.24.0 (2021-05-17)

|| TODO: At release time, merge in C/Java API release notes from:
||       //third_party/arcore/RELEASE-NOTES.md

## Breaking & behavioral changes
  * Performing `Frame.Raycast()` or `Frame.RaycastAll()` when the parameter
    `filter` is set to `TrackableHitFlags.Default` and
    `ARCoreCoreSessionConfig.DepthMode is `DepthMode.Automatic` will now include
    `DepthPoint` types in the `TrackableHit`. These values are sampled from the
    latest depth image, which yields more accurate results on non-planar or
    low-texture areas in the environment.

## New APIs and capabilities
  * Added new **Raw Depth API** that provides a depth image without image-space filtering.
    * All devices that support the existing Depth API in [ARCore supported devices](https://developers.google.com/ar/discover/supported-devices) now also support the new Raw Depth API.
    * Developer guide, Frame.UpdateRawDepthTexture, Frame.UpdateRawDepthConfidenceTexture, ARCoreSessionConfig.DepthMode.
  * New trackable type [`DepthPoint`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/DepthPoint) that provides a calculated depth pose with each frame by hit testing.
  * New setting option **Do Not Use** is added into **ARCore Project Settings > Android Authentication Strategy** and **iOS Authentication Strategy**.
  * New External Recording & Playback APIs, [Developer Guide](https://developers.google.com/ar/develop/unity/recording-and-playback)
    * Added [`ARCoreRecordingConfig.Tracks`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreRecordingConfig#tracks) list to allow the specification of additional tracks to record to when producing a dataset.
    * Added [`Frame.RecordTrackData`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame#recordtrackdata) to record data to specified additional tracks when producing a dataset.
    * Added [`Frame.GetUpdatedTrackData`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame#getupdatedtrackdata) to playback data recorded to specified additional tracks in datasets.

## Deprecations
None

## Other changes
  * …

## Bug fixes
  * Various bug fixes and performance improvements.
