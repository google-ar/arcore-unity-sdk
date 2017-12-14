//-----------------------------------------------------------------------
// <copyright file="ApiPoseData.cs" company="Google">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Pose data container stored in ARCore frame.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
    Justification = "Internal")]
    public struct ApiPoseData
    {
        [MarshalAs(UnmanagedType.R4)]
        public float Qx;

        [MarshalAs(UnmanagedType.R4)]
        public float Qy;

        [MarshalAs(UnmanagedType.R4)]
        public float Qz;

        [MarshalAs(UnmanagedType.R4)]
        public float Qw;

        [MarshalAs(UnmanagedType.R4)]
        public float X;

        [MarshalAs(UnmanagedType.R4)]
        public float Y;

        [MarshalAs(UnmanagedType.R4)]
        public float Z;

        public ApiPoseData(UnityEngine.Pose unityPose)
        {
            ConversionHelper.UnityPoseToApiPose(unityPose, out this);
        }

        public UnityEngine.Pose ToUnityPose()
        {
            Pose unityPose;
            ConversionHelper.ApiPoseToUnityPose(this, out unityPose);
            return unityPose;
        }

        public override string ToString()
        {
            return string.Format("qx: {0}, qy: {1}, qz: {2}, qw: {3}, x: {4}, y: {5}, z: {6}",
                Qx, Qy, Qz, Qw, X, Y, Z);
        }
    }
}
