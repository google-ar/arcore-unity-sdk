-------------------------------------------------------------------------------
# RELEASE NOTES TEMPLATE / COPY-PASTE THIS INTO A NEW SECTION AS NEEDED
-------------------------------------------------------------------------------

# ARCore SDK for Unity v1.24.0 (2021-05-17)

|| TODO: At release time, merge in C/Java API release notes from:
||       //third_party/arcore/RELEASE-NOTES.md

## Breaking & behavioral changes
  * Performing [`Frame.Raycast()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame#raycast) or [`Frame.RaycastAll()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame#raycastall) when the parameter `filter` is set to [`TrackableHitFlags.Default`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#trackablehitflags) and `ARCoreSessionConfig.DepthMode` is [`DepthMode.Automatic`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#depthmode) will now include [`DepthPoint`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/DepthPoint) types in the `TrackableHit`.

## New APIs and capabilities
  * Added new **Raw Depth API** that provides a depth image without image-space filtering.
    * All devices that support the existing Depth API in [ARCore supported devices](https://developers.google.com/ar/devices) now also support the new Raw Depth API.
    * [Developer guide](https://developers.google.com/ar/develop/unity/depth/raw-depth), [`Frame.CameraImage.UpdateRawDepthTexture()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame/CameraImage#updaterawdepthtexture), [`Frame.CameraImage.UpdateRawDepthConfidenceTexture()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame/CameraImage#updaterawdepthconfidencetexture), [`ARCoreSessionConfig.DepthMode`](https://developers.google.com/ar/reference/unity/namespace/GoogleARCore#depthmode).
  * Added new **Custom Data Track Recording/Playback API**, augmentations of the recording and playback features that allow developers to record and playback data to and from custom specified tracks, packaged as MP4 recordings. [Developer Guide](https://developers.google.com/ar/develop/unity/recording-and-playback/custom-data-track)
    * Added [`ARCoreRecordingConfig.Tracks`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/ARCoreRecordingConfig#tracks) list to allow the specification of additional tracks to record to when producing a recording.
    * Added [`Frame.RecordTrackData()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame#recordtrackdata) to record data to specified additional tracks when producing a recording.
    * Added [`Frame.GetUpdatedTrackData()`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/Frame#getupdatedtrackdata) to playback data recorded to specified additional tracks in recordings.
  * Added new trackable type [`DepthPoint`](https://developers.google.com/ar/reference/unity/class/GoogleARCore/DepthPoint) that provides a calculated depth pose with each frame by hit testing. These values are sampled from the latest depth image, which yields more accurate results on non-planar or low-texture areas in the environment.
  * Added new setting option **Do Not Use** in **ARCore Project Settings > Android Authentication Strategy** and **iOS Authentication Strategy**.


## Deprecations
None

## Bug fixes
  * Various bug fixes and performance improvements.
