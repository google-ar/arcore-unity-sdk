//-----------------------------------------------------------------------
// <copyright file="ApiCoordinateFramePair.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityTango = GoogleAR.UnityNative;

    /// <summary>
    /// The TangoCoordinateFramePair struct contains a pair of coordinate frames of reference.
    ///
    /// Tango pose data is calculated as a transformation between two frames
    /// of reference (so, for example, you can be asking for the pose of the
    /// device within a learned area).
    ///
    /// This struct is used to specify the desired base and target frames of
    /// reference when requesting pose data.  You can also use it when you have
    /// a TangoPoseData structure returned from the API and want to examine which
    /// frames of reference were used to get that pose.
    ///
    /// For more information, including which coordinate frame pairs are valid,
    /// see our page on
    /// <a href ="/project-tango/overview/frames-of-reference">frames of reference</a>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ApiCoordinateFramePair
    {
        /// <summary>
        /// Base frame of reference to compare against when requesting pose data.
        /// For example, if you have loaded an area and want to find out where the
        /// device is within it, you would use the
        /// <code>TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION</code> frame of reference
        /// as your base.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public ApiCoordinateFrameType baseFrame;

        /// <summary>
        /// Target frame of reference when requesting pose data, compared to the
        /// base. For example, if you want the device's pose data, use
        /// <code>TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE</code>.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public ApiCoordinateFrameType targetFrame;

        /// <summary>
        /// Constructs a new ApiCoordinateFramePair from a UnityTango.CoordinateFramePair.
        /// </summary>
        /// <param name="unityPair">The unity CoordinateFramePair to copy.</param>
        public ApiCoordinateFramePair(UnityTango.CoordinateFramePair unityPair)
        {
            baseFrame = unityPair.baseFrame.ToApiType();
            targetFrame = unityPair.targetFrame.ToApiType();
        }

        /// <summary>
        /// Returns an equivalent unity CoordinateFramePair.
        /// </summary>
        /// <returns>An equivalent unity CoordinateFramePair.</returns>
        public UnityTango.CoordinateFramePair ToUnityType()
        {
            UnityTango.CoordinateFramePair unityFramePair = new UnityTango.CoordinateFramePair();
            unityFramePair.baseFrame = baseFrame.ToUnityType();
            unityFramePair.targetFrame = targetFrame.ToUnityType();
            return unityFramePair;
        }
    }
}