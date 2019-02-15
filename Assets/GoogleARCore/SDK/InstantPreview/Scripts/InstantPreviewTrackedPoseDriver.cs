//-----------------------------------------------------------------------
// <copyright file="InstantPreviewTrackedPoseDriver.cs" company="Google">
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
    using UnityEngine;

    /// <summary>
    /// Drives cameras when using Instant Preview, since there is no easy way to
    /// provide data to Unity's TrackedPoseDriver.
    /// </summary>
    public class InstantPreviewTrackedPoseDriver : MonoBehaviour
    {
        /// <summary>
        /// Updates the game object's local transform to that of the latest pose
        /// received by Instant Preview.
        /// </summary>
        public void Update()
        {
            if (!Application.isEditor)
            {
                return;
            }

            transform.localPosition = Frame.Pose.position;
            transform.localRotation = Frame.Pose.rotation;
        }
    }
}
