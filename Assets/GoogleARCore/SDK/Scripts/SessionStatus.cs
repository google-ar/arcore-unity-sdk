//-----------------------------------------------------------------------
// <copyright file="SessionStatus.cs" company="Google">
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
    /// Possible states for the ARCore session.
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// The ARCore session has not been initialized.
        /// </summary>
        None = 0,

        /// <summary>
        /// The ARCore session is initializing.
        /// </summary>
        Initializing = 1,

        /// <summary>
        /// The ARCore session is tracking.
        /// </summary>
        Tracking = 100,

        /// <summary>
        /// The ARCore session has lost tracking and is attempting to recover.
        /// </summary>
        LostTracking = 101,

        /// <summary>
        /// The ARCore session is paused.
        /// </summary>
        NotTracking = 102,

        /// <summary>
        /// The ARCore session cannot begin tracking because a fatal error was encountered.
        /// </summary>
        FatalError = 200,

        /// <summary>
        /// The ARCore session cannot begin tracking because the ARCore service APK is not available
        /// on the device.
        /// </summary>
        ErrorApkNotAvailable = 201,

        /// <summary>
        /// The ARCore session cannot begin tracking because an Android permission is not granted
        /// (e.g. android.permission.CAMERA).
        ///
        /// Use <see cref="AndroidPermissionsManager.IsPermissionGranted"> to check if
        /// the required Android permission has been granted.
        /// </summary>
        ErrorPermissionNotGranted = 202,

        /// <summary>
        /// The ARCore session cannot begin tracking because the session configuration supplied is
        /// not supported or no session configuration was supplied.
        ///
        /// To recover, fix the configuration and ensure ARCoreSession is not enabled. Once
        /// SessionStatus is SessionStatus.NotTracking, ARCoreSession can be enabled.
        /// </summary>
        ErrorSessionConfigurationNotSupported = 203,
    }
}
