//-----------------------------------------------------------------------
// <copyright file="XPSession.cs" company="Google">
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

namespace GoogleARCore.CrossPlatform
{
    using GoogleARCoreInternal.CrossPlatform;
    using UnityEngine;

#if ARCORE_IOS_SUPPORT
    using UnityEngine.XR.iOS;
#endif

    /// <summary>
    /// Represents a cross-platform ARCore session.
    /// </summary>
    public static class XPSession
    {
        /// <summary>
        /// Attempts to asynchronously host a new cloud anchor.
        /// </summary>
        /// <param name="anchor">The anchor to host.</param>
        /// <returns>A task that will complete when the attempt to host a new cloud anchor has
        /// finished.  The result will be a <c>CloudAnchorResult</c> associated with the operation.
        /// </returns>
        public static AsyncTask<CloudAnchorResult> CreateCloudAnchor(Anchor anchor)
        {
            return CloudServiceManager.Instance.CreateCloudAnchor(anchor);
        }

#if ARCORE_IOS_SUPPORT
        /// <summary>
        /// Attempts to asynchronously host a new cloud anchor.
        /// </summary>
        /// <param name="anchor">The anchor to host.</param>
        /// <returns>A task that will complete when the attempt to host a new cloud anchor has
        /// finished.  The result will be a <c>CloudAnchorResult</c> associated with the operation.
        /// </returns>
        public static AsyncTask<CloudAnchorResult> CreateCloudAnchor(
            UnityARUserAnchorComponent anchor)
        {
            return CloudServiceManager.Instance.CreateCloudAnchor(
                new Pose(anchor.transform.position, anchor.transform.rotation));
        }
#endif

        /// <summary>
        /// Attempts to asynchronous resolve a cloud anchor.
        /// </summary>
        /// <param name="cloudAnchorId">The id of the cloud anchor to resolve.</param>
        /// <returns>A task that will complete when the attempt to host a new cloud anchor has
        /// finished.  The result will be a <c>CloudAnchorResult</c> associated with the operation.
        /// </returns>
        public static AsyncTask<CloudAnchorResult> ResolveCloudAnchor(string cloudAnchorId)
        {
            return CloudServiceManager.Instance.ResolveCloudAnchor(cloudAnchorId);
        }
    }
}
