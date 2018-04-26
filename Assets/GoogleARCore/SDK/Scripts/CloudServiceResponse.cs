//-----------------------------------------------------------------------
// <copyright file="CloudServiceResponse.cs" company="Google">
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
    /// <summary>
    /// A response from an AR cloud service request.
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
        /// The Google AR Cloud Service could not be reached via the network connection.
        /// </summary>
        ErrorServiceUnreachable,

        /// <summary>
        /// The authorization provided by the application is not valid; The API key included in the application manifest
        /// should be checked for accuracy.
        /// </summary>
        ErrorNotAuthorized,

        /// <summary>
        /// The request exceeded the allotted quota for the application's API key.
        /// </summary>
        ErrorApiQuotaExceeded,

        /// <summary>
        /// The device needs to gather additional tracking data from the environment before the Google AR Cloud
        /// Service can fulfill the request.
        /// </summary>
        ErrorDatasetInadequate,

        /// <summary>
        /// The request referenced a cloud id that was not found.
        /// </summary>
        ErrorCloudIdNotFound,

        /// <summary>
        /// The Google AR Cloud Service failed to localize.
        /// </summary>
        ErrorLocalizationFailed,

        /// <summary>
        /// The SDK version is too old to be compatible with the Google AR Cloud Service.
        /// </summary>
        ErrorSDKTooOld,

        /// <summary>
        /// The SDK version is too new to be compatible with the Google AR Cloud Service.
        /// </summary>
        ErrorSDKTooNew,

        /// <summary>
        /// The Google AR Cloud Service experienced an internal error when processing the request.
        /// </summary>
        ErrorInternal,
    }
}
