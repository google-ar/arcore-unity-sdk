//-----------------------------------------------------------------------
// <copyright file="Frame.cs" company="Google">
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
    using System;
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Provides a snapshot of the state of ARCore at a specific timestamp associated with the current frame.  Frame
    /// holds information about ARCore's state including tracking status, the pose of the camera relative to the world,
    /// estimated lighting parameters, and information on updates to objects (like Planes or Point Clouds) that ARCore
    /// is tracking.
    /// </summary>
    public class Frame
    {
        //// @cond EXCLUDE_FROM_DOXYGEN

        /// <summary>
        /// Gets the manager for the static frame.
        /// </summary>
        private static FrameManager s_FrameManager;

        //// @endcond

        /// <summary>
        /// Gets the tracking state of the ARCore device for the frame.  If the state is not <c>Tracking</c>,
        /// the values in the frame may be very inaccurate and generally should not be used.
        /// Tracking loss is often caused when the camera does not have enough visual features to track (e.g. a white
        /// wall) or the device is being moved very rapidly.
        /// </summary>
        public static TrackingState TrackingState
        {
            get
            {
                if (s_FrameManager == null)
                {
                    return TrackingState.Stopped;
                }

                return s_FrameManager.GetCameraTrackingState();
            }
        }

        /// <summary>
        /// Gets the pose of the ARCore device for the frame in Unity world coordinates.
        /// </summary>
        public static Pose Pose
        {
            get
            {
                if (s_FrameManager == null)
                {
                    return Pose.identity;
                }

                return s_FrameManager.GetPose();
            }
        }

        /// <summary>
        /// Gets the current light estimate for this frame.
        /// </summary>
        public static LightEstimate LightEstimate
        {
            get
            {
                if (s_FrameManager == null)
                {
                    return new LightEstimate(LightEstimateState.NotValid, 0.0f);
                }

                return s_FrameManager.GetLightEstimate();
            }
        }

        /// <summary>
        /// Gets planes ARCore has tracked.
        /// </summary>
        /// <param name="planes">A reference to a list of TrackedPlane that will be filled by the method call.</param>
        /// <param name="filter">A filter on the type of data to return.</param>
        public static void GetPlanes(List<TrackedPlane> planes, TrackableQueryFilter filter = TrackableQueryFilter.All)
        {
            if (s_FrameManager == null)
            {
                planes.Clear();
                return;
            }

            s_FrameManager.GetTrackables<TrackedPlane>(planes, filter);
        }

        //// @cond EXCLUDE_FROM_DOXYGEN

        /// <summary>
        /// Initializes the static Frame.
        /// </summary>
        /// <param name="frameManager">The manager for the static frame.</param>
        public static void Initialize(FrameManager frameManager)
        {
            Frame.s_FrameManager = frameManager;
        }

        /// <summary>
        /// Cleans up the static frame.
        /// </summary>
        public static void Destroy()
        {
            s_FrameManager = null;
        }

        //// @endcond

        /// <summary>
        /// Container for state related to the ARCore camera image metadata for the Frame.
        /// </summary>
        public static class CameraMetadata
        {
            /// <summary>
            /// Get camera image metadata value. The querying value type needs to match the returned type.
            /// The type could be checked in CameraMetadata.cs.
            /// </summary>
            /// <param name="metadataTag">Metadata type.</param>
            /// <param name="outMetadataList">Result list of the requested values.</param>
            /// <returns><c>true</c> if getting metadata value successfully, otherwise <c>false</c>.</returns>
            public static bool TryGetValues(CameraMetadataTag metadataTag,
                    List<CameraMetadataValue> outMetadataList)
            {
                return Frame.s_FrameManager.CameraMetadataManager.TryGetValues(metadataTag, outMetadataList);
            }

            /// <summary>
            /// Get all available tags in the current frame's metadata.
            /// </summary>
            /// <param name="outMetadataTags">Result list of the tags.</param>
            /// <returns><c>true</c> if getting tags successfully, otherwise <c>false</c>.</returns>
            public static bool GetAllCameraMetadataTags(List<CameraMetadataTag> outMetadataTags)
            {
                return Frame.s_FrameManager.CameraMetadataManager.GetAllCameraMetadataTags(outMetadataTags);
            }
        }

        /// <summary>
        /// Container for state related to the ARCore point cloud for the Frame.
        /// </summary>
        public static class PointCloud
        {
            /// <summary>
            /// Gets a value indicating whether new point cloud data became available in the current frame.
            /// </summary>
            /// <returns><c>true</c> if new point cloud data became available in the current frame, otherwise
            /// <c>false</c>.</returns>
            public static bool IsUpdatedThisFrame
            {
                get
                {
                    if (Frame.s_FrameManager == null)
                    {
                        return false;
                    }

                    return Frame.s_FrameManager.PointCloudManager.GetIsUpdatedThisFrame();
                }
            }

            /// <summary>
            /// Gets the count of point cloud points in the frame.
            /// </summary>
            public static int PointCount
            {
                get
                {
                    if (Frame.s_FrameManager == null)
                    {
                        return 0;
                    }

                     return Frame.s_FrameManager.PointCloudManager.GetPointCount();
                }
            }

            /// <summary>
            /// Gets a point from the point cloud collection at an index.
            /// </summary>
            /// <param name="index">The index of the point cloud point to get.</param>
            /// <returns>The point from the point cloud at <c>index</c>.</returns>
            public static Vector3 GetPoint(int index)
            {
                if (Frame.s_FrameManager == null)
                {
                    return Vector3.zero;
                }

                return Frame.s_FrameManager.PointCloudManager.GetPoint(index);
            }

            /// <summary>
            /// Copies the point cloud for a frame into a supplied list reference.
            /// </summary>
            /// <param name="points">A list that will be filled with point cloud points by this method call.</param>
            public static void CopyPoints(List<Vector4> points)
            {
                if (Frame.s_FrameManager == null)
                {
                    points.Clear();
                    return;
                }

                Frame.s_FrameManager.PointCloudManager.CopyPoints(points);
            }
        }

        /// <summary>
        /// Container for state related to the ARCore camera for the Frame.
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
                    if (Frame.s_FrameManager == null)
                    {
                        return null;
                    }

                    return Frame.s_FrameManager.GetCameraTexture();
                }
            }

            /// <summary>
            /// Gets a ApiDisplayUvCoords to properly display the camera texture.
            /// </summary>
            public static ApiDisplayUvCoords DisplayUvCoords
            {
                get
                {
                    ApiDisplayUvCoords displayUvCoords = new ApiDisplayUvCoords(new Vector2(0, 1),
                        new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0));

                    if (Frame.s_FrameManager == null)
                    {
                        return displayUvCoords;
                    }

                    Frame.s_FrameManager.TransformDisplayUvCoords(ref displayUvCoords);
                    return displayUvCoords;
                }
            }

            /// <summary>
            /// Gets the current projection matrix for this frame.
            /// <param name=“nearClipping”>The near clipping plane for the projection matrix.</param>
            /// <param name="farClipping”>The far clipping plane for the projection matrix.</param>
            /// <returns>The projection matrix for this frame.</returns>
            /// </summary>
            public static Matrix4x4 GetCameraProjectionMatrix(float nearClipping, float farClipping)
            {
                if (Frame.s_FrameManager == null)
                {
                    return Matrix4x4.identity;
                }

                return Frame.s_FrameManager.GetCameraProjectionMatrix(nearClipping, farClipping);
            }
        }
    }
}
