//-----------------------------------------------------------------------
// <copyright file="TrackData.cs" company="Google LLC">
//
// Copyright 2021 Google LLC
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
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Data that was recorded to a track for a given frame. Retrievable through
    /// <c><see cref="Frame.GetUpdatedTrackData…"/></c>.
    /// </summary>
    public struct TrackData
    {
        /// <summary>
        /// The frame timestamp in nanoseconds when the data was recorded in the track.
        /// </summary>
        public long FrameTimestamp;

        /// <summary>
        /// The byte data that was recorded.
        /// </summary>
        public byte[] Data;
    }
}
