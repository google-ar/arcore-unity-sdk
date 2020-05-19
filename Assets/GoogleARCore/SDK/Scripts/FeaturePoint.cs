//-----------------------------------------------------------------------
// <copyright file="FeaturePoint.cs" company="Google LLC">
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
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A point in the real world tracked by ARCore.
    /// </summary>
    public class FeaturePoint : Trackable
    {
        /// <summary>
        /// Construct FeaturePoint from a native handle.
        /// </summary>
        /// <param name="nativeHandle">A handle to the native ARCore API Trackable.</param>
        /// <param name="nativeApi">The ARCore native api.</param>
        internal FeaturePoint(IntPtr nativeHandle, NativeSession nativeApi) :
            base(nativeHandle, nativeApi)
        {
        }

        /// <summary>
        /// Gets the pose of the FeaturePoint.
        /// </summary>
        public Pose Pose
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    Debug.LogError(
                        "Pose:: Trying to access a session that has already been destroyed.");
                    return new Pose();
                }

                return m_NativeSession.PointApi.GetPose(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the orientation mode of the FeaturePoint.
        /// </summary>
        public FeaturePointOrientationMode OrientationMode
        {
            [SuppressMemoryAllocationError(
                IsWarning = true, Reason = "Requires further investigation.")]
            get
            {
                if (_IsSessionDestroyed())
                {
                    Debug.LogError(
                        "OrientationMode:: Trying to access a session that has already been " +
                        "destroyed.");
                    return FeaturePointOrientationMode.Identity;
                }

                return m_NativeSession.PointApi.GetOrientationMode(m_TrackableNativeHandle);
            }
        }
    }
}
