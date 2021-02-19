//-----------------------------------------------------------------------
// <copyright file="ApiPrestoStatus.cs" company="Google LLC">
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

namespace GoogleARCoreInternal
{
    internal enum ApiPrestoStatus
    {
        Uninitialized = 0,
        RequestingApkInstall = 1,
        RequestingPermission = 2,

        Resumed = 100,
        ResumedNotTracking = 101,
        Paused = 102,

        ErrorFatal = 200,
        ErrorApkNotAvailable = 201,
        ErrorPermissionNotGranted = 202,
        ErrorSessionConfigurationNotSupported = 203,
        ErrorCameraNotAvailable = 204,
        ErrorIllegalState = 205,
        ErrorInvalidCameraConfig = 206,
    }
}
