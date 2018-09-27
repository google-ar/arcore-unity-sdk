//-----------------------------------------------------------------------
// <copyright file="AugmentedImage.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// An image in the real world detected and tracked by ARCore.
    ///
    /// ARCore will return an AugmentedImage once it initially detects the image in the environment, even if it does
    /// not yet have enough information to estimate its pose and physical size. In that case, the AugmentedImage will have
    /// the tracking state Paused. Specifying the optional physical size in the Unity editor
    /// will help speed up the initial pose and physical size estimation process. Note that an AugmentedImage
    /// can have tracking state Paused even if the session state is Tracking.
    /// </summary>
    public class AugmentedImage : Trackable
    {
        internal AugmentedImage(IntPtr nativeHandle, NativeSession nativeApi)
            : base(nativeHandle, nativeApi)
        {
        }

        /// <summary>
        /// Gets the zero-based positional index of this augmented image from
        /// its originating image database.
        ///
        /// This index serves as the unique identifier for the image
        /// in the database.
        /// </summary>
        public int DatabaseIndex
        {
            get
            {
                return m_NativeSession.AugmentedImageApi.GetDatabaseIndex(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the name of the image.
        ///
        /// The image name is not guaranteed to be unique.
        /// </summary>
        public string Name
        {
            [SuppressMemoryAllocationError(IsWarning = true, Reason = "Allocates new string")]
            get
            {
                return m_NativeSession.AugmentedImageApi.GetName(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the position and orientation of the image's center in Unity world coordinates. The Y-axis points
        /// perpendicular out of the image. The X-axis points from left to right on the image, and the Z-axis
        /// points from bottom to top on the image.
        ///
        /// If the tracking state is Paused/Stopped, this returns the pose when the image state was last Tracking,
        /// or the identity pose if the image state has never been Tracking.
        /// </summary>
        public Pose CenterPose
        {
            get
            {
                return m_NativeSession.AugmentedImageApi.GetCenterPose(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the estimated width, in meters, of the corresponding physical image, as measured along
        /// the local X-axis (point from image left to image right) of the coordinate space centered on the
        /// image.
        ///
        /// ARCore will attempt to estimate the physical image's width based on its understanding of the world. If the
        /// optional physical size is specified in the Unity editor, this estimation process will
        /// happen more quickly. However, the estimated size may be different from the specified size.
        ///
        /// If the tracking state is Paused/Stopped, this returns the estimated width when the image state was
        /// last Tracking, or <c>0</c> if the image state has never been Tracking.
        /// </summary>
        public float ExtentX
        {
            get
            {
                return m_NativeSession.AugmentedImageApi.GetExtentX(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the estimated height, in meters, of the corresponding physical image, as measured along the local
        /// Z-axis (pointing from image bottom to image top) of the coordinate space centered on the image.
        ///
        /// ARCore will attempt to estimate the physical image's height based on its understanding of the world. If
        /// an optional physical size is specified in the Unity editor, this estimation process will happen
        /// more quickly. However, the estimated size may be different from the specified size.
        ///
        /// If the tracking state is Paused/Stopped, this returns the estimated height when the image state was
        /// last Tracking, or <c>0</c> if the image state has never been Tracking.
        /// </summary>
        public float ExtentZ
        {
            get
            {
                return m_NativeSession.AugmentedImageApi.GetExtentZ(m_TrackableNativeHandle);
            }
        }
    }
}
