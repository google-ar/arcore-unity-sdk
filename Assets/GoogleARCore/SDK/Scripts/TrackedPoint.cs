//-----------------------------------------------------------------------
// <copyright file="TrackedPoint.cs" company="Google">
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
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A point in the real world tracked by ARCore.
    /// </summary>
    public class TrackedPoint : Trackable
    {
        //// @cond EXCLUDE_FROM_DOXYGEN

        /// <summary>
        /// Construct TrackedPoint from a native handle.
        /// </summary>
        /// <param name="nativeHandle">A handle to the native ARCore API Trackable.</param>
        /// <param name="nativeApi">The ARCore native api.</param>
        public TrackedPoint(IntPtr nativeHandle, NativeSession nativeApi) : base(nativeHandle, nativeApi)
        {
        }

        //// @endcond

        /// <summary>
        /// Gets the pose of the TrackedPoint.
        /// </summary>
        public Pose Pose
        {
            get
            {
                return m_NativeSession.PointApi.GetPose(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the orientation mode of the TrackedPoint.
        /// </summary>
        public TrackedPointOrientationMode OrientationMode
        {
            get
            {
                return m_NativeSession.PointApi.GetOrientationMode(m_TrackableNativeHandle);
            }
        }
    }
}
