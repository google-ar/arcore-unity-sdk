//-----------------------------------------------------------------------
// <copyright file="ARCoreRecordingConfig.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using UnityEngine.Serialization;

    /// <summary>
    /// Configuration to record camera and sensor data from an ARCore session.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ARCoreRecordingConfig",
        menuName = "Google ARCore/ARCore Recording Config",
        order = 3)]
    public class ARCoreRecordingConfig : ScriptableObject
    {
        /// <summary>
        /// A full path and filename on the device where the MP4 recording will be
        /// saved. The recording consists of video data from the camera along with
        /// data from the device sensors. If the file already exists it will be
        /// overwritten.
        /// </summary>
        public string Mp4DatasetFilepath;

        /// <summary>
        /// When an ARCore session is paused, recording may continue, during this
        /// time the camera feed will be recorded as a black screen, but sensor
        /// data will continue to be captured. Set <c>true</c> to cause the
        /// recording to stop automatically when the session is paused, or
        /// <c>false</c> to allow the recording to continue until the session is
        /// destroyed, or the recording is stopped manually.
        /// </summary>
        public bool AutoStopOnPause = true;

        /// <summary>
        /// The list of <c><see cref="Track"/></c> to add to the recording config.
        /// </summary>
        [HideInInspector]
        public List<Track> Tracks = new List<Track>();
    }
}
