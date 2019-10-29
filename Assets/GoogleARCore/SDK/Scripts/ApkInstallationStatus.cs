//-----------------------------------------------------------------------
// <copyright file="ApkInstallationStatus.cs" company="Google">
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
    /// <summary>
    /// Possible statuses for a Google Play Services for AR (ARCore) installation request
    /// on this device.
    /// </summary>
    public enum ApkInstallationStatus
    {
        /// <summary>
        /// Installation of Google Play Services for AR (ARCore) was not initialized.
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        /// Installation of Google Play Services for AR (ARCore) was requested.
        /// The current activity will be paused.
        /// </summary>
        Requested = 1,

        /// <summary>
        /// Google Play Services for AR (ARCore) is already installed.
        /// </summary>
        Success = 100,

        /// <summary>
        /// An internal error occurred while installing Google Play Services for AR (ARCore).
        /// </summary>
        Error = 200,

        /// <summary>
        /// The device is not currently compatible with Google Play Services for AR (ARCore).
        /// </summary>
        ErrorDeviceNotCompatible = 201,

        /// <summary>
        /// The device and Android version are supported, and a version of the Google Play
        /// Services for AR (ARCore) is installed, but the ARCore version is too old.
        /// </summary>
        [System.Obsolete("Merged with ErrorDeviceNotCompatible. Use that instead.")]
        ErrorAndroidVersionNotSupported = 202,

        /// <summary>
        /// The user declined installation of the Google Play Services for AR (ARCore)
        /// during this run of the application and the current request was not marked
        /// as user-initiated.
        /// </summary>
        ErrorUserDeclined = 203,
    }
}
