//-----------------------------------------------------------------------
// <copyright file="ApiAvailabilityExtensions.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System.Collections.Generic;
    using GoogleARCore;

    internal static class ApiAvailabilityExtensions
    {
        public static ApkAvailabilityStatus ToApkAvailabilityStatus(this ApiAvailability apiStatus)
        {
            switch (apiStatus)
            {
                case ApiAvailability.UnknownError:
                    return ApkAvailabilityStatus.UnknownError;
                case ApiAvailability.UnknownChecking:
                    return ApkAvailabilityStatus.UnknownChecking;
                case ApiAvailability.UnknownTimedOut:
                    return ApkAvailabilityStatus.UnknownTimedOut;
                case ApiAvailability.UnsupportedDeviceNotCapable:
                    return ApkAvailabilityStatus.UnsupportedDeviceNotCapable;
                case ApiAvailability.SupportedNotInstalled:
                    return ApkAvailabilityStatus.SupportedNotInstalled;
                case ApiAvailability.SupportedApkTooOld:
                    return ApkAvailabilityStatus.SupportedApkTooOld;
                case ApiAvailability.SupportedInstalled:
                    return ApkAvailabilityStatus.SupportedInstalled;
                default:
                    UnityEngine.Debug.LogErrorFormat(
                        "Unexpected ApiAvailability status {0}", apiStatus);
                    return ApkAvailabilityStatus.UnknownError;
            }
        }
    }
}
