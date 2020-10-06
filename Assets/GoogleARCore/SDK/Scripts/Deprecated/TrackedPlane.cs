//-----------------------------------------------------------------------
// <copyright file="TrackedPlane.cs" company="Google LLC">
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
namespace GoogleARCore
{
    using System;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Deprecated version of DetectedPlane.
    /// </summary>
    [System.Obsolete(
        "This class has been renamed to DetectedPlane. See " +
        "https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.2.0")]
    public class TrackedPlane : DetectedPlane
    {
        /// <summary>
        /// Construct TrackedPlane from a native handle.
        /// </summary>
        /// <param name="nativeHandle">A handle to the native ARCore API Trackable.</param>
        /// <param name="nativeApi">The ARCore native api.</param>
        internal TrackedPlane(IntPtr nativeHandle, NativeSession nativeApi)
            : base(nativeHandle, nativeApi)
        {
        }

        /// <summary>
        /// Gets a reference to the plane subsuming this plane, if any.  If not null, only the
        /// subsuming plane should be considered valid for rendering.
        /// </summary>
        public new TrackedPlane SubsumedBy
        {
            get
            {
                return (TrackedPlane)base.SubsumedBy;
            }
        }
    }
}
