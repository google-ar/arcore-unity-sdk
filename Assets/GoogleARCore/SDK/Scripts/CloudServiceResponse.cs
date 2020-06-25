//-----------------------------------------------------------------------
// <copyright file="CloudServiceResponse.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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
    /// <summary>
    /// A response from the ARCore <see cref="Cloud Anchor"/> service.
    /// </summary>
    public enum CloudServiceResponse
    {
        /// <summary>
        /// The request was completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The request is not supported by the current configuration.
        /// </summary>
        ErrorNotSupportedByConfiguration,

        /// <summary>
        /// The request can not be completed because the local AR session is not tracking or paused.
        /// </summary>
        ErrorNotTracking,

        /// <summary>
        /// The ARCore <see cref="Cloud Anchor"/> service could not be reached via the network connection.
        /// </summary>
        /// @deprecated This enum value is deprecated.
        [System.Obsolete(
            "In the case of Cloud Anchor creation, this error has been replaced by " +
            "CloudServiceResponse.ErrorHostingServiceUnavailable. See " +
            "https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.12.0 to learn more.")]
        ErrorServiceUnreachable,

        /// <summary>
        /// The authorization provided by the application is not valid; The API key included in the
        /// application manifest should be checked for accuracy.
        /// </summary>
        ErrorNotAuthorized,

        /// <summary>
        /// The request exceeded the allotted quota for the application's API key.
        /// </summary>
        ErrorApiQuotaExceeded,

        /// <summary>
        /// The device needs to gather additional tracking data from the environment before the
        /// ARCore <see cref="Cloud Anchor"/> service can fulfill the request.
        /// </summary>
        ErrorDatasetInadequate,

        /// <summary>
        /// The request referenced a <see cref="Cloud Anchor"/> ID that was not found.
        /// </summary>
        ErrorCloudIdNotFound,

        /// <summary>
        /// The ARCore <see cref="Cloud Anchor"/> service failed to localize.
        /// </summary>
        /// @deprecated This enum value is deprecated.
        [System.Obsolete(
            "This enum has been deprecated. See " +
            "https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.12.0")]
        ErrorLocalizationFailed,

        /// <summary>
        /// The SDK version is too old to be compatible with the ARCore <see cref="Cloud Anchor"/> service.
        /// </summary>
        ErrorSDKTooOld,

        /// <summary>
        /// The SDK version is too new to be compatible with the ARCore <see cref="Cloud Anchor"/> service.
        /// </summary>
        ErrorSDKTooNew,

        /// <summary>
        /// The ARCore <see cref="Cloud Anchor"/> service experienced an internal error when processing the request.
        /// </summary>
        ErrorInternal,

        /// <summary>
        /// The ARCore <see cref="Cloud Anchor"/> service was unreachable. This can happen because
        /// of a number of reasons. The device may is in airplane mode or does not
        /// have a working internet connection. The request sent to the server could
        /// have timed out with no response, there could be a bad network
        /// connection, DNS unavailability, firewall issues, or anything that could
        /// affect the device's ability to connect to the ARCore <see cref="Cloud Anchor"/> service.
        /// </summary>
        ErrorHostingServiceUnavailable,

        /// <summary>
        /// The ARCore <see cref="Cloud Anchor"/> service request has been cancelled.
        /// </summary>
        ErrorRequestCancelled,

        /// <summary>
        /// Too many Cloud Anchors are being hosted and/or resolved. To create more, destroy some
        /// XPAnchor objects or cancel some AsyncTasks to free up resources.
        /// </summary>
        ErrorTooManyCloudAnchors,
    }
}
