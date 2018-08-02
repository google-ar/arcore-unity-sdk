//-----------------------------------------------------------------------
// <copyright file="Session.cs" company="Google">
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
    using System;
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Represents an ARCore session, which is an attachment point from the app
    /// to the ARCore service. Holds information about the global state for
    /// ARCore, manages tracking of Anchors and Planes, and performs hit tests
    /// against objects ARCore is tracking in the world.
    /// </summary>
    public static class Session
    {
        /// <summary>
        /// Gets current session status.
        /// </summary>
        public static SessionStatus Status
        {
            get
            {
                return LifecycleManager.Instance.SessionStatus;
            }
        }

        /// <summary>
        /// Creates a new Anchor at the given <c>Pose</c> that is attached to the <c>Trackable</c>.
        /// If trackable is null, it creates a new anchor at a world pose.
        /// As ARCore updates its understading of the space, it will update the
        /// virtual pose of the of the anchor to attempt to keep the anchor in the same real world location.
        /// </summary>
        /// <param name="pose">The Unity world pose where the anchor is to be creates.</param>
        /// <param name="trackable">The Trackable to attach the Anchor to.</param>
        /// <returns>The newly created anchor or null.</returns>
        public static Anchor CreateAnchor(Pose pose, Trackable trackable = null)
        {
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return null;
            }

            if (trackable == null)
            {
                return nativeSession.SessionApi.CreateAnchor(pose);
            }
            else
            {
                return trackable.CreateAnchor(pose);
            }
        }

        /// <summary>
        /// Gets Trackables ARCore has tracked.
        /// </summary>
        /// <typeparam name="T">The Trackable type to get.</typeparam>
        /// <param name="trackables">A reference to a list of T that will be filled by the method call.</param>
        /// <param name="filter">A filter on the type of data to return.</param>
        public static void GetTrackables<T>(List<T> trackables, TrackableQueryFilter filter = TrackableQueryFilter.All) where T : Trackable
        {
            trackables.Clear();
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return;
            }

            nativeSession.GetTrackables<T>(trackables, filter);
        }

        /// <summary>
        /// Get the camera configuration the ARCore session is currently running with.
        /// </summary>
        /// <returns>The CameraConfig that the ARCore session is currently running with. The value is only currect
        /// when there is a valid running ARCore session. </returns>
        public static CameraConfig GetCameraConfig()
        {
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return new CameraConfig();
            }

            return nativeSession.SessionApi.GetCameraConfig();
        }

        /// <summary>
        /// Checks the availability of the ARCore APK on the device.
        /// </summary>
        /// <returns>An AsyncTask that completes with an ApkAvailabilityStatus when the availability is known.</returns>
        public static AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
            return LifecycleManager.Instance.CheckApkAvailability();
        }

        /// <summary>
        /// Requests an installation of the ARCore APK on the device.
        /// </summary>
        /// <param name="userRequested">Whether the installation was requested explicity by a user action.</param>
        /// <returns>An AsyncTask that completes with an ApkInstallationStatus when the installation
        /// status is resolved.</returns>
        public static AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested)
        {
            return LifecycleManager.Instance.RequestApkInstallation(userRequested);
        }
    }
}
