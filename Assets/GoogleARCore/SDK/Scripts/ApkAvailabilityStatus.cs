//-----------------------------------------------------------------------
// <copyright file="ApkAvailabilityStatus.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    /// Possible statuses of the Google Play Services for AR (ARCore) APK availability
    /// on a device.
    /// </summary>
    public enum ApkAvailabilityStatus
    {
        /// <summary>
        /// An internal error occurred while determining Google Play Services for AR (ARCore)
        /// availability.
        /// </summary>
        UnknownError = 0,

        /// <summary>
        /// Google Play Services for AR (ARCore) is not installed, and a query has been issued
        /// to check if ARCore is supported on this device.
        /// </summary>
        UnknownChecking = 1,

        /// <summary>
        /// Google Play Services for AR (ARCore) is not installed, and the query to check
        /// if ARCore is supported timed out. This may be due to the device being offline.
        /// </summary>
        UnknownTimedOut = 2,

        /// <summary>
        /// Google Play Services for AR (ARCore) is not supported on this device.
        /// </summary>
        UnsupportedDeviceNotCapable = 100,

        /// <summary>
        /// The device and Android version are supported, but Google Play Services for
        /// AR (ARCore) is not installed.
        /// </summary>
        SupportedNotInstalled = 201,

        /// <summary>
        /// The device and Android version are supported, and a version of the
        /// Google Play Services for AR (ARCore) is installed, but that version is too old.
        /// </summary>
        SupportedApkTooOld = 202,

        /// <summary>
        /// Google Play Services for AR (ARCore) is supported, installed, and available to use.
        /// </summary>
        SupportedInstalled = 203
    }
}
