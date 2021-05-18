//-----------------------------------------------------------------------
// <copyright file="DepthPoint.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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
    /// A point in the real world based on the Depth map generated and tracked
    /// by ARCore.
    /// </summary>
    public class DepthPoint : Trackable
    {
        internal DepthPoint(IntPtr nativeHandle, NativeSession nativeSession)
            : base(nativeHandle, nativeSession)
        {
        }

        /// <summary>
        /// Gets the pose of the DepthPoint.
        /// </summary>
        public Pose Pose
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "Pose:: Trying to access a session that has already been destroyed.");
                    return new Pose();
                }

                return _nativeSession.PointApi.GetPose(_trackableNativeHandle);
            }
        }
    }
}
