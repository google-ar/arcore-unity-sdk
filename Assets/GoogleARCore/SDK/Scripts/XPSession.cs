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
        /// <returns>
        /// A task that will complete when the attempt to host a new cloud anchor has finished.
        /// The result will be a <see cref="CloudAnchorResult"/> associated with the operation.
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
        /// <returns>
        /// A task that will complete when the attempt to host a new cloud anchor has finished.
        /// The result will be a <see cref="CloudAnchorResult"/> associated with the operation.
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
        /// The task will continue to retry in the background indefinitely,
        /// until it is successfully resolved, cancelled, or reaches a terminal error state.
        /// </summary>
        /// <param name="cloudAnchorId">The id of the cloud anchor to resolve.</param>
        /// <returns>
        /// A task that will complete when the attempt to resolve a cloud anchor has finished.
        /// The result will be a <see cref="CloudAnchorResult"/> associated with the operation.
        /// </returns>
        public static AsyncTask<CloudAnchorResult> ResolveCloudAnchor(string cloudAnchorId)
        {
            return CloudServiceManager.Instance.ResolveCloudAnchor(cloudAnchorId);
        }

        /// <summary>
        /// Attempts to cancel a pending AsyncTask&lt;CloudAnchorResult&gt; initiated by a call to
        /// <see cref="ResolveCloudAnchor(string)"/>.
        /// Any pending AsyncTasks associated with the given <paramref name="cloudAnchorId"/>
        /// will complete with result:
        /// <see cref="CloudServiceResponse"/>.<c>ErrorRequestCancelled</c> and the
        /// <see cref="CloudAnchorResult.Anchor"/> will be null.
        /// If no operation is pending for the given <paramref name="cloudAnchorId"/>,
        /// this call does not take effect and a warning message will be logged.
        /// </summary>
        /// <param name="cloudAnchorId">The id of the Cloud Anchor that is being watched or
        /// resolved.</param>
        public static void CancelCloudAnchorAsyncTask(string cloudAnchorId)
        {
            CloudServiceManager.Instance.CancelCloudAnchorAsyncTask(cloudAnchorId);
        }
    }
}
