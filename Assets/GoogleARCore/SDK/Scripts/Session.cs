//-----------------------------------------------------------------------
// <copyright file="Session.cs" company="Google LLC">
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

// Certain versions of 2018.1 fail to define UNITY_2017_4_OR_NEWER.
#if !UNITY_2017_4_OR_NEWER && !UNITY_2018_1_OR_NEWER && !ARCORE_SKIP_MIN_VERSION_CHECK
  #error ARCore SDK for Unity requires Unity 2017.4 or later.
#endif  // !UNITY_2017_4_OR_NEWER && !UNITY_2018_1_OR_NEWER

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
        /// Gets the reason for ARCore having lost tracking.
        /// </summary>
        public static LostTrackingReason LostTrackingReason
        {
            get
            {
                return LifecycleManager.Instance.LostTrackingReason;
            }
        }

        /// <summary>
        /// Gets the current state of the recorder.
        /// </summary>
        /// <returns>The current <see cref="RecordingStatus"/>.</returns>
        public static RecordingStatus RecordingStatus
        {
            get
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    return RecordingStatus.None;
                }

                return nativeSession.SessionApi.GetRecordingStatus();
            }
        }

        /// <summary>
        /// Gets the current state of playback.
        /// </summary>
        /// <returns>The current <see cref="PlaybackStatus"/>.</returns>
        public static PlaybackStatus PlaybackStatus
        {
            get
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession == null)
                {
                    return PlaybackStatus.None;
                }

                return nativeSession.SessionApi.GetPlaybackStatus();
            }
        }

        /// <summary>
        /// Creates a new Anchor at the given <c>Pose</c> that is attached to the <c>Trackable</c>.
        /// If trackable is null, it creates a new anchor at a world pose.
        /// As ARCore updates its understading of the space, it will update the
        /// virtual pose of the of the anchor to attempt to keep the anchor in the same real world
        /// location.
        /// </summary>
        /// <param name="pose">The Unity world pose where the anchor is to be creates.</param>
        /// <param name="trackable">The Trackable to attach the Anchor to.</param>
        /// <returns>The newly created anchor or null.</returns>
        [SuppressMemoryAllocationError(Reason = "Could allocate a new Anchor object")]
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
        /// <param name="trackables">A reference to a list of T that will be filled by the method
        /// call.</param>
        /// <param name="filter">A filter on the type of data to return.</param>
        [SuppressMemoryAllocationError(Reason = "List could be resized.")]
        public static void GetTrackables<T>(
            List<T> trackables, TrackableQueryFilter filter = TrackableQueryFilter.All)
            where T : Trackable
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
        /// <returns>The CameraConfig that the ARCore session is currently running with. The value
        /// is only correct when there is a valid running ARCore session. </returns>
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
        /// <returns>An AsyncTask that completes with an ApkAvailabilityStatus when the availability
        /// is known.</returns>
        [SuppressMemoryAllocationError(Reason = "Creates a new AsyncTask")]
        public static AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
            return LifecycleManager.Instance.CheckApkAvailability();
        }

        /// <summary>
        /// Requests an installation of the ARCore APK on the device.
        /// </summary>
        /// <param name="userRequested">Whether the installation was requested explicitly by a user
        /// action.</param>
        /// <returns>An AsyncTask that completes with an ApkInstallationStatus when the installation
        /// status is resolved.</returns>
        [SuppressMemoryAllocationError(Reason = "Creates a new AsyncTask")]
        public static AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested)
        {
            return LifecycleManager.Instance.RequestApkInstallation(userRequested);
        }

        /// <summary>
        /// Check whether the depth mode is supported on this device. Not all
        /// devices support depth, see the
        /// <a href="https://developers.google.com/ar/discover/supported-devices">
        /// ARCore supported devices</a> page for details.
        /// </summary>
        /// <param name="depthMode">The depth mode.</param>
        /// <returns>true if the depth mode is supported, false if it is not
        /// supported or the session has not yet been initialized.</returns>
        public static bool IsDepthModeSupported(DepthMode depthMode)
        {
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return false;
            }

            bool result = nativeSession.SessionApi.IsDepthModeSupported(
                depthMode.ToApiDepthMode());
            return result;
        }

        /// <summary>
        /// Starts a new recording, using the provided
        /// <see cref="ARCoreRecordingConfig"/> to define the location to save the
        /// dataset and other options. If a recording is already in progress this
        /// call will fail, check the <see cref="RecordingStatus"/> before making
        /// this call. When an ARCore session is paused, recording may continue,
        /// during this time the camera feed will be recorded as a black screen,
        /// but sensor data will continue to be captured.
        /// </summary>
        /// <param name="config"><see cref="ARCoreRecordingConfig"/> containing the
        /// path to save the dataset along with other recording options.</param>
        /// <returns><see cref="RecordingResult"/>.<c>OK</c> if the recording is
        /// started (or will start on the next Session resume.) Or a
        /// <see cref="RecordingResult"/> if there was an error.</returns>
        public static RecordingResult StartRecording(ARCoreRecordingConfig config)
        {
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return RecordingResult.ErrorRecordingFailed;
            }

            return nativeSession.SessionApi.StartRecording(config);
        }

        /// <summary>
        /// Stops the current recording. If there is no recording in progress, this
        /// method will return <see cref="RecordingResult"/>.<c>OK</c>.
        /// </summary>
        /// <returns><see cref="RecordingResult"/>.<c>OK</c> if the recording was
        /// stopped successfully, or
        /// <see cref="RecordingResult"/>.<c>ErrorRecordingFailed</c> if there was an
        /// error.</returns>
        public static RecordingResult StopRecording()
        {
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return RecordingResult.ErrorRecordingFailed;
            }

            return nativeSession.SessionApi.StopRecording();
        }

        /// <summary>
        /// Sets an MP4 dataset file to playback instead of using the live camera feed and IMU
        /// sensor data.
        ///
        /// Restrictions:
        /// - Due to the way session data is processed, ARCore APIs may sometimes produce different
        ///   results during playback than during recording and produce different results during
        ///   subsequent playback sessions. For exmaple, the number of detected planes and other
        ///   trackables, the precise timing of their detection and their pose over time may be
        ///   different in subsequent playback sessions.
        /// - Can only be called while the session is paused. Playback of the MP4 dataset file will
        ///   start once the session is resumed.
        /// - The MP4 dataset file must use the same camera facing direction as is configured in the
        ///   session.
        ///
        /// <param name="datasetFilepath"> The filepath of the MP4 dataset. Null if
        /// stopping the playback and resuming a live feed.</param>
        /// <returns><see cref="PlaybackResult"/>.<c>Success</c> if playback filepath was
        /// set without issue. Otherwise, the <see cref="PlaybackResult"/> will indicate the
        /// error.</returns>
        public static PlaybackResult SetPlaybackDataset(string datasetFilepath)
        {
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return PlaybackResult.ErrorPlaybackFailed;
            }

            return nativeSession.SessionApi.SetPlaybackDataset(datasetFilepath);
        }
    }
}
