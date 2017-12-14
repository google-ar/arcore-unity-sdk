//-----------------------------------------------------------------------
// <copyright file="PoseApi.cs" company="Google">
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
    Justification = "Internal")]
    public class PoseApi
    {
        private NativeApi m_NativeApi;

        public PoseApi(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public IntPtr Create()
        {
            return Create(Pose.identity);
        }

        public IntPtr Create(Pose pose)
        {
            ApiPoseData rawPose = new ApiPoseData(pose);

            IntPtr poseHandle = IntPtr.Zero;
            ExternApi.ArPose_create(m_NativeApi.SessionHandle, ref rawPose, ref poseHandle);
            return poseHandle;
        }

        public void Destroy(IntPtr nativePose)
        {
            ExternApi.ArPose_destroy(nativePose);
        }

        public Pose ExtractPoseValue(IntPtr poseHandle)
        {
            ApiPoseData poseValue = new ApiPoseData(Pose.identity);
            ExternApi.ArPose_getPoseRaw(m_NativeApi.SessionHandle, poseHandle, ref poseValue);
            return poseValue.ToUnityPose();
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPose_create(IntPtr session, ref ApiPoseData rawPose, ref IntPtr poseHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPose_destroy(IntPtr poseHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArPose_getPoseRaw(IntPtr sessionHandle, IntPtr poseHandle,
                ref ApiPoseData rawPose);
        }
    }
}
