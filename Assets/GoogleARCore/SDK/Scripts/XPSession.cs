//-----------------------------------------------------------------------
// <copyright file="XPSession.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    using GoogleARCoreInternal;
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
        /// Attempts to asynchronously host a new <see cref="Cloud Anchor"/>.
        /// </summary>
        /// <param name="anchor">The anchor to host.</param>
        /// <returns>
        /// A task that will complete when the attempt to host a new <see cref="Cloud Anchor"/> has finished.
        /// The result will be a <see cref="CloudAnchorResult"/> associated with the operation.
        /// </returns>
        public static AsyncTask<CloudAnchorResult> CreateCloudAnchor(Anchor anchor)
        {
            return CloudServiceManager.Instance.CreateCloudAnchor(anchor);
        }

#if ARCORE_IOS_SUPPORT
        /// <summary>
        /// <b>(iOS only)</b>Attempts to asynchronously host a new <see cref="Cloud Anchor"/>.
        /// </summary>
        /// <param name="anchor">The anchor to host.</param>
        /// <returns>
        /// A task that will complete when the attempt to host a new <see cref="Cloud Anchor"/> has finished.
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
        /// Attempts to asynchronously resolve a <see cref="Cloud Anchor"/>. You don't need to wait for a call to
        /// resolve a <see cref="Cloud Anchor"/> to complete before initiating another call. A session can have
        /// up to 20 Cloud Anchors and pending AsyncTasks at a given time.
        /// The task will continue to retry in the background indefinitely,
        /// until it is successfully resolved, cancelled, or reaches a terminal error state.
        /// </summary>
        /// <param name="cloudAnchorId">The id of the <see cref="Cloud Anchor"/> to resolve.</param>
        /// <returns>
        /// A task that will complete when the attempt to resolve a <see cref="Cloud Anchor"/> has finished.
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
        /// <param name="cloudAnchorId">The id of the <see cref="Cloud Anchor"/> that is being watched or
        /// resolved.</param>
        public static void CancelCloudAnchorAsyncTask(string cloudAnchorId)
        {
            CloudServiceManager.Instance.CancelCloudAnchorAsyncTask(cloudAnchorId);
        }

        /// <summary>
        /// Attempts to asynchronously create a new <see cref="Cloud Anchor"/> with a given lifetime in days,
        /// using the pose of the provided <paramref name="anchor"/>.
        /// </summary>
        /// <remarks>
        /// The initial pose of the returned anchor will be set to the pose of the provided
        /// <paramref name="anchor"/>. However, the returned anchor is completely independent of
        /// the original <paramref name="anchor"/>, and the two poses might diverge over time.
        /// Hosting requires an active session for which the
        /// <see cref="GoogleARCore.TrackingState"/> is
        /// <see cref="GoogleARCore.TrackingState"/>.<c>Tracking</c>, as well as a working internet
        /// connection. The task will continue to retry silently in the background if it is unable
        /// to establish a connection to the ARCore <see cref="Cloud Anchor"/> service.
        /// </remarks>
        /// <param name="anchor">The anchor to host.</param>
        /// <param name="ttlDays">The lifetime of the anchor in days. Must be positive. The
        /// maximum allowed value is 1 if using an API Key to authenticate with the
        /// ARCore <see cref="Cloud Anchor"/> service, otherwise the maximum allowed value is 365.</param>
        /// <returns>A task that will complete when the attempt to create a new <see cref="Cloud Anchor"/> has
        /// finished. The result will be a <c>CloudAnchorResult</c> associated with the operation.
        /// </returns>
        public static GoogleARCore.AsyncTask<CloudAnchorResult> CreateCloudAnchor(
            GoogleARCore.Anchor anchor, int ttlDays)
        {
            return CloudServiceManager.Instance.CreateCloudAnchor(anchor, ttlDays);
        }

#if ARCORE_IOS_SUPPORT
        /// <summary>
        /// <b>(iOS only)</b>Attempts to asynchronously create a new <see cref="Cloud Anchor"/>
        /// with a given lifetime in days, using the pose of the provided <paramref name="anchor"/>.
        /// </summary>
        /// <remarks>
        /// The initial pose of the returned anchor will be set to the pose of the provided
        /// <paramref name="anchor"/>. However, the returned anchor is completely independent of
        /// the original <paramref name="anchor"/>, and the two poses might diverge over time.
        /// Hosting requires an active session for which the <see cref="ARTrackingState"/> is
        /// <see cref="ARTrackingState.ARTrackingStateNormal"/>, as well as a working internet
        /// connection. The task will continue to retry silently in the background if it is unable
        /// to establish a connection to the ARCore <see cref="Cloud Anchor"/> service.
        /// </remarks>
        /// <param name="anchor">The anchor to host.</param>
        /// <param name="ttlDays">The lifetime of the anchor in days. Must be positive. The
        /// maximum allowed value is 1 if using an API Key to authenticate with the
        /// ARCore <see cref="Cloud Anchor"/> service, otherwise the maximum allowed value is 365.</param>
        /// <returns>A task that will complete when the attempt to create a new <see cref="Cloud Anchor"/> has
        /// finished. The result will be a <c>CloudAnchorResult</c> associated with the operation.
        /// </returns>
        public static GoogleARCore.AsyncTask<CloudAnchorResult> CreateCloudAnchor(
            UnityARUserAnchorComponent anchor, int ttlDays)
        {
            return CloudServiceManager.Instance.CreateCloudAnchor(
                new Pose(anchor.transform.position, anchor.transform.rotation), ttlDays);
        }

        /// <summary>
        /// <b>(iOS only)</b>Set the token to use when authenticating with the ARCore
        /// <see cref="Cloud Anchor"/> service on the iOS platform. If an API Key was provided, the
        /// token will be ignored and an error will be logged. Otherwise, the most recent valid auth
        /// token passed in will be used. Call this method each time you refresh your token.
        /// Note: This can only be called after the ARCore session is created, and should be called
        /// each time the application's token is refreshed.
        /// </summary>
        /// <param name="authToken">The token to use when authenticating with the ARCore
        /// <see cref="Cloud Anchor"/> service. This must be a nonempty ASCII string with no spaces
        /// or control characters. This will be used until another token is passed in. See
        /// [documentation](https://developers.google.com/ar/develop/unity/cloud-anchors/persistence)
        /// for supported token types.</param>
        public static void SetAuthToken(string authToken)
        {
            CloudServiceManager.Instance.SetAuthToken(authToken);
        }
#endif // ARCORE_IOS_SUPPORT

        /// <summary>
        /// Estimates the quality of the visual feature points seen by ARCore in the
        /// preceding few seconds and visible from the provided camera <paramref name="pose"/>.
        /// Cloud Anchors hosted using higher quality features will generally result
        /// in easier and more accurately resolved <see cref="Cloud Anchor"/> poses. If
        /// feature map quality cannot be estimated for given <paramref name="pose"/>,
        /// warning message will be logged and <see cref="FeatureMapQuality"/>.<c>Insufficient</c>
        /// is returned.
        /// </summary>
        /// <returns>The estimated feature map quality.</returns>
        /// <param name="pose">The camera pose to use in estimating the quality.</param>
        public static FeatureMapQuality EstimateFeatureMapQualityForHosting(Pose pose)
        {
            return CloudServiceManager.Instance.EstimateFeatureMapQualityForHosting(pose);
        }
    }
}
