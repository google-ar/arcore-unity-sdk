//-----------------------------------------------------------------------
// <copyright file="Frame.cs" company="Google LLC">
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
    using System;
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Provides a snapshot of the state of ARCore at a specific timestamp associated with the
    /// current frame.  Frame holds information about ARCore's state including tracking status, the
    /// pose of the camera relative to the world, estimated lighting parameters, and information on
    /// updates to objects (like Planes or Point Clouds) that ARCore is tracking.
    /// </summary>
    public class Frame
    {
        //// @cond EXCLUDE_FROM_DOXYGEN

        private static List<TrackableHit> _tmpTrackableHitList = new List<TrackableHit>();

        //// @endcond

        /// <summary>
        /// Gets the pose of the ARCore device for the frame in Unity world coordinates.
        /// </summary>
        public static Pose Pose
        {
            get
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    return Pose.identity;
                }

                var cameraHandle = nativeSession.FrameApi.AcquireCamera();
                Pose result = nativeSession.CameraApi.GetPose(cameraHandle);
                nativeSession.CameraApi.Release(cameraHandle);
                return result;
            }
        }

        /// <summary>
        /// Gets the current light estimate for this frame.
        /// </summary>
        public static LightEstimate LightEstimate
        {
            get
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                var sessionComponent = LifecycleManager.Instance.SessionComponent;
                if (nativeSession == null || sessionComponent == null ||
                    sessionComponent.SessionConfig.LightEstimationMode ==
                    LightEstimationMode.Disabled)
                {
                    return new LightEstimate(LightEstimateState.NotValid, 0.0f, Color.black,
                        Quaternion.LookRotation(Vector3.down), Color.white, null, -1);
                }

                return nativeSession.FrameApi.GetLightEstimate();
            }
        }

        /// <summary>
        /// Performs a raycast against objects being tracked by ARCore.
        /// Output the closest hit from the camera.
        /// Note that the Unity's screen coordinate (0, 0)
        /// starts from bottom left.
        /// </summary>
        /// <param name="x">Horizontal touch position in Unity screen coordinates.</param>
        /// <param name="y">Vertical touch position in Unity screen coordinates.</param>
        /// <param name="filter">A filter bitmask where each set bit in
        /// <see cref="TrackableHitFlags"/>
        /// represents a category of raycast hits the method call should consider valid.</param>
        /// <param name="hitResult">A <see cref="TrackableHit"/> that will be set if the raycast is
        /// successful.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "List could be resized")]
        public static bool Raycast(float x, float y, TrackableHitFlags filter,
            out TrackableHit hitResult)
        {
            hitResult = new TrackableHit();
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return false;
            }

            // Note that the Unity's screen coordinate (0, 0) starts from bottom left.
            bool foundHit = nativeSession.HitTestApi.Raycast(
                nativeSession.FrameHandle, x, Screen.height - y, filter, _tmpTrackableHitList);

            if (foundHit && _tmpTrackableHitList.Count != 0)
            {
                hitResult = _tmpTrackableHitList[0];
            }

            return foundHit;
        }

        /// <summary>
        /// Performs a ray cast that can return a result before ARCore establishes full tracking.
        ///
        /// The pose and apparent scale of attached objects depends on the <see
        /// cref="InstantPlacementPoint"/> tracking method and the provided
        /// approximateDistanceMeters. A discussion of the different tracking methods and the
        /// effects of apparent object scale are described in <see cref="InstantPlacementPoint"/>.
        ///
        /// This function will succeed only if <see cref="InstantPlacementMode"/> is
        /// <c>InstantPlacementMode.LocalYUp</c> in the ARCore session configuration, the ARCore
        /// session status is <see cref="SessionStatus"/>.<c>Tracking</c>, and there are sufficent
        /// feature points to track the point in screen space.
        ///
        /// </summary>
        /// <param name="x">Horizontal touch position in Unity screen coordinates.</param>
        /// <param name="y">Vertical touch position in Unity screen coordinates.</param>
        /// <param name="approximateDistanceMeters">The distance at which to create an <see
        /// cref="InstantPlacementPoint"/>. This is only used while the tracking method for the
        /// returned point is
        /// <c>InstantPlacementPointTrackingMethod.ScreenspaceWithApproximateDistance</c>.</param>
        /// <param name="hitResult">If successful a <see cref="HitResult"/> with a trackable of type
        /// <see cref="InstantPlacementPoint"/>.</param>
        /// <returns><c>true</c> if successful, otherwise <c>false</c>.</returns>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "List could be resized")]
        public static bool RaycastInstantPlacement(float x, float y,
            float approximateDistanceMeters, out TrackableHit hitResult)
        {
            hitResult = new TrackableHit();
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return false;
            }

            // Note that the Unity's screen coordinate (0, 0) starts from bottom left.
            bool foundHit = nativeSession.HitTestApi.Raycast(nativeSession.FrameHandle,
                x, Screen.height - y, approximateDistanceMeters, _tmpTrackableHitList);
            if (foundHit && _tmpTrackableHitList.Count != 0)
            {
                hitResult = _tmpTrackableHitList[0];
            }

            return foundHit;
        }

        /// <summary>
        /// Performs a raycast against objects being tracked by ARCore.
        /// Output the closest hit from the origin.
        /// </summary>
        /// <param name="origin">The starting point of the ray in world coordinates.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="hitResult">A <see cref="TrackableHit"/> that will be set if the raycast is
        /// successful.</param>
        /// <param name="maxDistance">The max distance the ray should check for collisions.</param>
        /// <param name="filter">A filter bitmask where each set bit in
        /// <see cref="TrackableHitFlags"/>
        /// represents a category of raycast hits the method call should consider valid.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "List could be resized")]
        public static bool Raycast(
            Vector3 origin, Vector3 direction, out TrackableHit hitResult,
            float maxDistance = Mathf.Infinity,
            TrackableHitFlags filter = TrackableHitFlags.Default)
        {
            hitResult = new TrackableHit();
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return false;
            }

            bool foundHit =
                nativeSession.HitTestApi.Raycast(
                    nativeSession.FrameHandle, origin, direction, maxDistance, filter,
                    _tmpTrackableHitList);

            if (foundHit && _tmpTrackableHitList.Count != 0)
            {
                hitResult = _tmpTrackableHitList[0];
            }

            return foundHit;
        }

        /// <summary>
        /// Performs a raycast against objects being tracked by ARCore.
        /// Output all hits from the camera.
        /// Note that the Unity's screen coordinate (0, 0)
        /// starts from bottom left.
        /// </summary>
        /// <param name="x">Horizontal touch position in Unity screen coordinates.</param>
        /// <param name="y">Vertical touch position in Unity screen coordinates.</param>
        /// <param name="filter">A filter bitmask where each set bit in
        /// <see cref="TrackableHitFlags"/>
        /// represents a category of raycast hits the method call should consider valid.</param>
        /// <param name="hitResults">A list of <see cref="TrackableHit"/> that will be set if the
        /// raycast is successful.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "List could be resized")]
        public static bool RaycastAll(
            float x, float y, TrackableHitFlags filter, List<TrackableHit> hitResults)
        {
            hitResults.Clear();
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return false;
            }

            return nativeSession.HitTestApi.Raycast(
                nativeSession.FrameHandle, x, Screen.height - y, filter, hitResults);
        }

        /// <summary>
        /// Performs a raycast against objects being tracked by ARCore.
        /// Output all hits from the origin.
        /// </summary>
        /// <param name="origin">The starting point of the ray in world coordinates.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="hitResults">A list of <see cref="TrackableHit"/> that will be set if the
        /// raycast is successful.</param>
        /// <param name="maxDistance">The max distance the ray should check for collisions.</param>
        /// <param name="filter">A filter bitmask where each set bit in
        /// <see cref="TrackableHitFlags"/> represents a category
        /// of raycast hits the method call should consider valid.</param>
        /// <returns><c>true</c> if the raycast had a hit, otherwise <c>false</c>.</returns>
        [SuppressMemoryAllocationError(IsWarning = true, Reason = "List could be resized")]
        public static bool RaycastAll(
            Vector3 origin, Vector3 direction, List<TrackableHit> hitResults,
            float maxDistance = Mathf.Infinity,
            TrackableHitFlags filter = TrackableHitFlags.Default)
        {
            hitResults.Clear();
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return false;
            }

            return nativeSession.HitTestApi.Raycast(
                nativeSession.FrameHandle, origin, direction, maxDistance, filter, hitResults);
        }

        /// <summary>
        /// Container for state related to the ARCore camera image metadata for the frame.
        /// </summary>
        public static class CameraMetadata
        {
            /// <summary>
            /// Get camera image metadata value. The querying value type needs to match the returned
            /// type.
            /// The type could be checked in CameraMetadata.cs.
            /// </summary>
            /// <param name="metadataTag">Metadata type.</param>
            /// <param name="outMetadataList">Result list of the requested values.</param>
            /// <returns><c>true</c> if getting metadata value successfully, otherwise
            /// <c>false</c>.</returns>
            public static bool TryGetValues(
                CameraMetadataTag metadataTag, List<CameraMetadataValue> outMetadataList)
            {
                outMetadataList.Clear();
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    return false;
                }

                IntPtr metadataHandle = IntPtr.Zero;
                if (!nativeSession.FrameApi.AcquireImageMetadata(ref metadataHandle))
                {
                    return false;
                }

                var isSuccess = nativeSession.CameraMetadataApi.TryGetValues(
                    metadataHandle, metadataTag, outMetadataList);
                nativeSession.CameraMetadataApi.Release(metadataHandle);
                return isSuccess;
            }

            /// <summary>
            /// Get all available tags in the current frame's metadata.
            /// </summary>
            /// <param name="outMetadataTags">Result list of the tags.</param>
            /// <returns><c>true</c> if getting tags successfully, otherwise <c>false</c>.</returns>
            public static bool GetAllCameraMetadataTags(List<CameraMetadataTag> outMetadataTags)
            {
                outMetadataTags.Clear();
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    return false;
                }

                IntPtr metadataHandle = IntPtr.Zero;
                if (!nativeSession.FrameApi.AcquireImageMetadata(ref metadataHandle))
                {
                    return false;
                }

                var isSuccess =
                    nativeSession.CameraMetadataApi.GetAllCameraMetadataTags(
                        metadataHandle, outMetadataTags);
                nativeSession.CameraMetadataApi.Release(metadataHandle);
                return isSuccess;
            }
        }

        /// <summary>
        /// Container for state related to the ARCore point cloud for the frame.
        /// </summary>
        public static class PointCloud
        {
            /// <summary>
            /// Gets a value indicating whether new point cloud data became available in the current
            /// frame.
            /// </summary>
            /// <returns><c>true</c> if new point cloud data became available in the current frame,
            /// otherwise <c>false</c>.</returns>
            public static bool IsUpdatedThisFrame
            {
                get
                {
                    if (LifecycleManager.Instance.IsSessionChangedThisFrame)
                    {
                        return true;
                    }

                    var nativeSession = LifecycleManager.Instance.NativeSession;
                    if (nativeSession == null)
                    {
                        return false;
                    }

                    return nativeSession.IsPointCloudNew;
                }
            }

            /// <summary>
            /// Gets the count of point cloud points in the frame.
            /// </summary>
            public static int PointCount
            {
                get
                {
                    var nativeSession = LifecycleManager.Instance.NativeSession;
                    if (nativeSession == null)
                    {
                        return 0;
                    }

                     return nativeSession.PointCloudApi.GetNumberOfPoints(
                         nativeSession.PointCloudHandle);
                }
            }

            /// <summary>
            /// Gets a point from the point cloud at a given index.
            /// The point returned will be a Vector4 in the form <x,y,z,c> where the first three
            /// dimensions describe the position of the point in the world and the last represents a
            /// confidence estimation in the range [0, 1).
            /// </summary>
            /// <param name="index">The index of the point cloud point to get.</param>
            /// <returns>The point from the point cloud at <c>index</c> along with its
            /// confidence.</returns>
            /// @deprecated Please use Frame.PointCloud.GetPointAsStruct instead.

            [System.Obsolete("Frame.PointCloud.GetPoint has been deprecated. " +
             "Please use Frame.PointCloud.GetPointAsStruct instead.")]
            public static Vector4 GetPoint(int index)
            {
                var point = GetPointAsStruct(index);
                return new Vector4(
                    point.Position.x, point.Position.y, point.Position.z, point.Confidence);
            }

            /// <summary>
            /// Gets a point from the point cloud at the given index.  If the point is inaccessible
            /// due to session state or an out-of-range index a point will be returns with the
            /// <c>Id</c> field set to <c>PointCloudPoint._invalidPointId</c>.
            /// </summary>
            /// <param name="index">The index of the point cloud point to get.</param>
            /// <returns>The point from the point cloud at <c>index</c>.</returns>
            public static PointCloudPoint GetPointAsStruct(int index)
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null || index >= PointCount)
                {
                    return new PointCloudPoint(PointCloudPoint.InvalidPointId, Vector3.zero, 0.0f);
                }

                return nativeSession.PointCloudApi.GetPoint(nativeSession.PointCloudHandle, index);
            }

            /// <summary>
            /// Copies the point cloud into the supplied parameter <c>points</c>.
            /// Each point will be a Vector4 in the form <x,y,z,c> where the first three dimensions
            /// describe the position of the point in the world and the last represents a confidence
            /// estimation in the range [0, 1).
            /// </summary>
            /// <param name="points">A list that will be filled with point cloud points by this
            /// method call.</param>
            /// @deprecated Please copy points manually instead.
            [System.Obsolete("Frame.PointCloud.CopyPoints has been deprecated. " +
                             "Please copy points manually instead.")]
            public static void CopyPoints(List<Vector4> points)
            {
                points.Clear();
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    return;
                }

                for (int i = 0; i < PointCount; i++)
                {
                    var point = GetPointAsStruct(i);
                    points.Add(new Vector4(
                        point.Position.x, point.Position.y, point.Position.z, point.Confidence));
                }
            }
        }

        /// <summary>
        /// Container for state related to the ARCore camera for the frame.
        /// </summary>
        public static class CameraImage
        {
            /// <summary>
            /// Gets a texture used from the device's rear camera.
            /// </summary>
            public static Texture Texture
            {
                get
                {
                    var nativeSession = LifecycleManager.Instance.NativeSession;
                    if (nativeSession == null || nativeSession.FrameApi.GetTimestamp() == 0)
                    {
                        return null;
                    }

                    return ARCoreAndroidLifecycleManager.Instance.BackgroundTexture;
                }
            }

            //// @cond EXCLUDE_FROM_DOXYGEN

            [Obsolete(
                "This field has been deprecated. Please use Frame.CameraImage.TextureDisplayUvs.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules",
                "SA1600:ElementsMustBeDocumented", Justification = "Deprecated")]
            public static DisplayUvCoords DisplayUvCoords
            {
                get
                {
                    return TextureDisplayUvs;
                }
            }

            //// @endcond

            /// <summary>
            /// Gets UVs that map the orientation and aspect ratio of
            /// <see cref="Frame.CameraImage.Texture"/> to those of the device's display.
            /// </summary>
            public static DisplayUvCoords TextureDisplayUvs
            {
                get
                {
                    DisplayUvCoords displayUvCoords = DisplayUvCoords.FullScreenUvCoords;

                    // Use deprecated 'TransformDisplayUvCoords' when running Instant Preview.
                    if (InstantPreviewManager.IsProvidingPlatform)
                    {
                        var nativeSession = LifecycleManager.Instance.NativeSession;
                        if (nativeSession == null)
                        {
                            return new DisplayUvCoords();
                        }

                        var apiCoords = new ApiDisplayUvCoords(
                            displayUvCoords.TopLeft, displayUvCoords.TopRight,
                            displayUvCoords.BottomLeft, displayUvCoords.BottomRight);
                        nativeSession.FrameApi.TransformDisplayUvCoords(ref apiCoords);
                        return apiCoords.ToDisplayUvCoords();
                    }

                    displayUvCoords.TopLeft = TransformCoordinate(
                        displayUvCoords.TopLeft,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundTexture);
                    displayUvCoords.TopRight = TransformCoordinate(
                        displayUvCoords.TopRight,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundTexture);
                    displayUvCoords.BottomLeft = TransformCoordinate(
                        displayUvCoords.BottomLeft,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundTexture);
                    displayUvCoords.BottomRight = TransformCoordinate(
                        displayUvCoords.BottomRight,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundTexture);

                    return displayUvCoords;
                }
            }

            /// <summary>
            /// Gets UVs that map the orientation and aspect ratio of the image returned by
            /// <see cref="Frame.CameraImage.AcquireCameraImageBytes"/> to that of the device's display.
            /// </summary>
            public static DisplayUvCoords ImageDisplayUvs
            {
                get
                {
                    if (InstantPreviewManager.IsProvidingPlatform)
                    {
                        InstantPreviewManager.LogLimitedSupportMessage(
                            "access CPU image display UVs");
                        return DisplayUvCoords.FullScreenUvCoords;
                    }

                    DisplayUvCoords displayUvCoords = DisplayUvCoords.FullScreenUvCoords;

                    displayUvCoords.TopLeft = TransformCoordinate(
                        displayUvCoords.TopLeft,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundImage);
                    displayUvCoords.TopRight = TransformCoordinate(
                        displayUvCoords.TopRight,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundImage);
                    displayUvCoords.BottomLeft = TransformCoordinate(
                        displayUvCoords.BottomLeft,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundImage);
                    displayUvCoords.BottomRight = TransformCoordinate(
                        displayUvCoords.BottomRight,
                        DisplayUvCoordinateType.UnityScreen,
                        DisplayUvCoordinateType.BackgroundImage);

                    return displayUvCoords;
                }
            }

            /// <summary>
            /// Gets the unrotated and uncropped intrinsics for the texture (GPU) stream.
            /// </summary>
            public static CameraIntrinsics TextureIntrinsics
            {
                get
                {
                    var nativeSession = LifecycleManager.Instance.NativeSession;
                    if (nativeSession == null)
                    {
                        return new CameraIntrinsics();
                    }

                    var cameraHandle = nativeSession.FrameApi.AcquireCamera();
                    CameraIntrinsics result =
                        nativeSession.CameraApi.GetTextureIntrinsics(cameraHandle);
                    nativeSession.CameraApi.Release(cameraHandle);
                    return result;
                }
            }

            /// <summary>
            /// Gets the unrotated and uncropped intrinsics for the image (CPU) stream.
            /// </summary>
            public static CameraIntrinsics ImageIntrinsics
            {
                get
                {
                    var nativeSession = LifecycleManager.Instance.NativeSession;
                    if (nativeSession == null)
                    {
                        return new CameraIntrinsics();
                    }

                    var cameraHandle = nativeSession.FrameApi.AcquireCamera();
                    CameraIntrinsics result =
                        nativeSession.CameraApi.GetImageIntrinsics(cameraHandle);
                    nativeSession.CameraApi.Release(cameraHandle);
                    return result;
                }
            }

            /// <summary>
            /// Transforms a coordinate between the <c>source</c> and <c>target</c> display UV
            /// coordinate types.
            /// </summary>
            /// <remarks>
            /// This can be used for the conversion of coordinates accessed in the same Unity
            /// update.
            /// </remarks>
            /// <param name="coordinate">The coordinate to transform.</param>
            /// <param name="sourceType">The source type of the desired transformation
            /// matrix.</param>
            /// <param name="targetType">The target type of the desired transformation
            /// matrix.</param>
            /// <returns>A corresponding position in the target frame.</returns>
            public static Vector2 TransformCoordinate(
                Vector2 coordinate, DisplayUvCoordinateType sourceType,
                DisplayUvCoordinateType targetType)
            {
                if (InstantPreviewManager.IsProvidingPlatform)
                {
                    InstantPreviewManager.LogLimitedSupportMessage(
                        "access 'Frame.TransformCoordinate'");
                    return Vector2.zero;
                }

                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    Debug.LogError("Cannot transform coordinate when native session is null.");
                    return Vector2.zero;
                }

                nativeSession.FrameApi.TransformCoordinates2d(
                    ref coordinate, sourceType, targetType);
                return coordinate;
            }

            /// <summary>
            /// Attempts to acquire the camera image for CPU access that corresponds to the current
            /// frame.
            /// </summary>
            /// <remarks>
            /// Depending on device performance, this can fail for several frames after session
            /// start, and for a few frames at a time while the session is running.
            /// </remarks>
            /// <returns>A <c>CameraImageBytes</c> struct with <c>IsAvailable</c> property set to
            /// <c>true</c> if successful and <c>false</c> if the image could not be
            /// acquired.</returns>
            public static GoogleARCore.CameraImageBytes AcquireCameraImageBytes()
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    return new CameraImageBytes(IntPtr.Zero);
                }

                return nativeSession.FrameApi.AcquireCameraImageBytes();
            }

            /// <summary>
            /// Gets the projection matrix for the frame.
            /// </summary>
            /// <param name="nearClipping">The near clipping plane for the projection
            /// matrix.</param>
            /// <param name="farClipping">The far clipping plane for the projection matrix.</param>
            /// <returns>The projection matrix for the frame.</returns>
            public static Matrix4x4 GetCameraProjectionMatrix(float nearClipping, float farClipping)
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null || Texture == null)
                {
                    return Matrix4x4.identity;
                }

                var cameraHandle = nativeSession.FrameApi.AcquireCamera();
                var result = nativeSession.CameraApi.GetProjectionMatrix(
                    cameraHandle, nearClipping, farClipping);
                nativeSession.CameraApi.Release(cameraHandle);
                return result;
            }

            /// <summary>
            /// Updates the input texture with the latest depth data from ARCore.
            /// If there is no new data, or an error occurs, the contents of the
            /// texture will remain unchanged. See <see cref="DepthStatus"/> for a
            /// complete list of reasons.
            /// </summary>
            /// <param name="depthTexture">The texture to hold the depth data.</param>
            /// <returns><see cref="DepthStatus"/>.<c>Success</c> if
            /// successful.</returns>
            public static DepthStatus UpdateDepthTexture(ref Texture2D depthTexture)
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                var sessionComponent = LifecycleManager.Instance.SessionComponent;
                if (nativeSession == null || sessionComponent == null ||
                    sessionComponent.SessionConfig.DepthMode == DepthMode.Disabled)
                {
                    return DepthStatus.InternalError;
                }

                return nativeSession.FrameApi.UpdateDepthTexture(ref depthTexture);
            }
        }

    }
}
