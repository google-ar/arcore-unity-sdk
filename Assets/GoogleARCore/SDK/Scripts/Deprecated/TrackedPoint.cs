//-----------------------------------------------------------------------
// <copyright file="TrackedPoint.cs" company="Google LLC">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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

    /// <summary>
    /// Deprecated version of FeaturePoint.
    /// </summary>
    [System.Obsolete(
        "This class has been renamed to FeaturePoint. See " +
        "https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.2.0")]
    public class TrackedPoint : FeaturePoint
    {
        /// <summary>
        /// Construct TrackedPoint from a native handle.
        /// </summary>
        /// <param name="nativeHandle">A handle to the native ARCore API Trackable.</param>
        /// <param name="nativeApi">The ARCore native api.</param>
        internal TrackedPoint(IntPtr nativeHandle, NativeSession nativeApi) :
            base(nativeHandle, nativeApi)
        {
        }

        /// <summary>
        /// Gets the orientation mode of the TrackedPoint.
        /// </summary>
        public new TrackedPointOrientationMode OrientationMode
        {
            get
            {
                return (TrackedPointOrientationMode)base.OrientationMode;
            }
        }
    }
}
