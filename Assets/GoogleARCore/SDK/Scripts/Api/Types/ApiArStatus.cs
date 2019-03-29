//-----------------------------------------------------------------------
// <copyright file="ApiArStatus.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
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
    using System.Collections;
    using UnityEngine;

    internal enum ApiArStatus
    {
        // The operation was successful.
        Success = 0,

        // One of the arguments was invalid, either null or not appropriate for the operation
        // requested.
        ErrorInvalidArgument = -1,

        // An internal error occurred that the application should not attempt to recover from.
        ErrorFatal = -2,

        // An operation was attempted that requires the session be running, but the session was
        // paused.
        ErrorSessionPaused = -3,

        // An operation was attempted that requires the session be paused, but the session was
        // running.
        ErrorSessionNotPaused = -4,

        // An operation was attempted that the session should be in the TRACKING state, but the
        // session was not.
        ErrorNotTracking = -5,

        // A texture name was not set by calling ArSession_setCameraTextureName() before the first
        // call to ArSession_update().
        ErrorTextureNotSet = -6,

        // An operation required GL context but one was not available.
        ErrorMissingGlContext = -7,

        // The configuration supplied to ArSession_configure() was unsupported. To avoid this error,
        // ensure that Session_checkSupported() returns true.
        ErrorUnsupportedConfiguration = -8,

        // The android camera permission has not been granted prior to calling ArSession_resume().
        ErrorCameraPermissionNotGranted = -9,

        // Acquire failed because the object being acquired is already released. This happens e.g.
        // if the developer holds an old frame for too long, and then tries to acquire a point cloud
        // from it.
        ErrorDeadlineExceeded = -10,

        // There are no available resources to complete the operation.
        ErrorResourceExhausted = -11,

        // Acquire failed because the data isn't available yet for the current frame. For example,
        // acquire the image metadata may fail with this error because the camera hasn't fully
        // started.
        ErrorNotYetAvailable = -12,

        // The android camera has been reallocated to a higher priority app or is otherwise
        // unavailable.
        ErrorCameraNotAvailable = -13,

        // The host/resolve function call failed because the Session is not configured for cloud
        // anchors.
        ErrorCloudAnchorsNotConfigured = -14,

        // ArSession_configure() failed because the specified configuration required the Android
        // INTERNET permission, which the application did not have.
        ErrorInternetPermissionNotGranted = -15,

        // HostCloudAnchor() failed because the anchor is not a type of anchor that is currently
        // supported for hosting.
        ErrorAnchorNotSupportedForHosting = -16,

        // An image with insufficient quality (e.g. too few features) was attempted to be added to
        // the image database.
        ErrorImageInsufficientQuality = -17,

        // The data passed in for this operation was not in a valid format.
        ErrorDataInvalidFormat = -18,

        // The data passed in for this operation is not supported by this version of the SDK.
        ErrorDatatUnsupportedVersion = -19,

        // The ARCore APK is not installed on this device.
        UnavailableArCoreNotInstalled = -100,

        // The device is not currently compatible with ARCore.
        UnavailableDeviceNotCompatible = -101,

        // The ARCore APK currently installed on device is too old and needs to be updated.
        UnavailableApkTooOld = -103,

        // The ARCore APK currently installed no longer supports the ARCore SDK that the application
        // was built with.
        UnavailableSdkTooOld = -104,

        // The user declined installation of the ARCore APK during this run of the application and
        // the current request was not marked as user-initiated.
        UnavailableUserDeclinedInstall = -105,
    }
}
