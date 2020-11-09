//-----------------------------------------------------------------------
// <copyright file="PlaybackStatus.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    /// Describes the current playback status.
    /// </summary>
    public enum PlaybackStatus
    {
        /// <summary>
        /// The session is not playing back a dataset.
        /// </summary>
        None,

        /// <summary>
        /// Playback is in process without issues.
        /// </summary>
        OK,

        /// <summary>
        /// Playback has stopped due to an error.
        /// </summary>
        IOError,

        /// <summary>
        /// Playback has finished successfully. The session is waiting on final frame.
        /// of the dataset. Clear the playback filepath to resume live camera feed.
        /// </summary>
        FinishedSuccess,
    }
}
