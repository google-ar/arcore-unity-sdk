//-----------------------------------------------------------------------
// <copyright file="ApiPrestoStatusExtensions.cs" company="Google">
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
    using GoogleARCore;

    internal static class ApiPrestoStatusExtensions
    {
        public static SessionStatus ToSessionStatus(this ApiPrestoStatus prestoStatus)
        {
            switch (prestoStatus)
            {
                case ApiPrestoStatus.Uninitialized:
                    return SessionStatus.None;
                case ApiPrestoStatus.RequestingApkInstall:
                case ApiPrestoStatus.RequestingPermission:
                    return SessionStatus.Initializing;
                case ApiPrestoStatus.Resumed:
                    return SessionStatus.Tracking;
                case ApiPrestoStatus.ResumedNotTracking:
                    return SessionStatus.LostTracking;
                case ApiPrestoStatus.Paused:
                    return SessionStatus.NotTracking;
                case ApiPrestoStatus.ErrorFatal:
                    return SessionStatus.FatalError;
                case ApiPrestoStatus.ErrorApkNotAvailable:
                    return SessionStatus.ErrorApkNotAvailable;
                case ApiPrestoStatus.ErrorPermissionNotGranted:
                    return SessionStatus.ErrorPermissionNotGranted;
                case ApiPrestoStatus.ErrorSessionConfigurationNotSupported:
                    return SessionStatus.ErrorSessionConfigurationNotSupported;
                default:
                    UnityEngine.Debug.LogErrorFormat("Unexpected presto status {0}", prestoStatus);
                    return SessionStatus.FatalError;
            }
        }
    }
}