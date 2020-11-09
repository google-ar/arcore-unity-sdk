//-----------------------------------------------------------------------
// <copyright file="PlaybackResult.cs" company="Google LLC">
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
    /// Results from attempting to set playback dataset filepath.
    /// </summary>
    public enum PlaybackResult
    {
        /// <summary>
        /// The request completed successfully.
        /// </summary>
        OK,

        /// <summary>
        /// The session was not paused when setting the playback dataset.
        /// </summary>
        ErrorSessionNotPaused,

        /// <summary>
        /// Operation is unsupported with the current session.
        /// </summary>
        ErrorSessionUnsupported,

        /// <summary>
        /// Playback failed.
        /// </summary>
        ErrorPlaybackFailed,
    }
}
