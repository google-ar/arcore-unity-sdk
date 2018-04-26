//-----------------------------------------------------------------------
// <copyright file="ApiApkInstallStatusExtensions.cs" company="Google">
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

    internal static class ApiApkInstallStatusExtensions
    {
        public static ApkInstallationStatus ToApkInstallationStatus(this ApiApkInstallationStatus apiStatus)
        {
            switch (apiStatus)
            {
                case ApiApkInstallationStatus.Uninitialized:
                    return ApkInstallationStatus.Uninitialized;
                case ApiApkInstallationStatus.Requested:
                    return ApkInstallationStatus.Requested;
                case ApiApkInstallationStatus.Success:
                    return ApkInstallationStatus.Success;
                case ApiApkInstallationStatus.Error:
                    return ApkInstallationStatus.Error;
                case ApiApkInstallationStatus.ErrorDeviceNotCompatible:
                    return ApkInstallationStatus.ErrorDeviceNotCompatible;
                case ApiApkInstallationStatus.ErrorUserDeclined:
                    return ApkInstallationStatus.ErrorUserDeclined;
                default:
                    UnityEngine.Debug.LogErrorFormat("Unexpected ApiApkInstallStatus status {0}", apiStatus);
                    return ApkInstallationStatus.Error;
            }
        }
    }
}
