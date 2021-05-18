//-----------------------------------------------------------------------
// <copyright file="Track.cs" company="Google LLC">
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
    /// Definition of a track to record on. Data recorded to a given track will be muxed into a
    /// corresponding MP4 stream.
    /// </summary>
    public struct Track
    {
        /// <summary>
        /// Unique ID for the track.
        /// </summary>
        public Guid Id;

        /// <summary>
        /// Arbitrary byte array describing the track. The encoding is the user's choice. This is
        /// a null-terminated string.
        /// </summary>
        public byte[] Metadata;

        /// <summary>
        /// MIME type of the track data as a null terminated string.
        /// </summary>
        public string MimeType;
    }
}
