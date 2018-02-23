//-----------------------------------------------------------------------
// <copyright file="FrameApi.cs" company="Google">
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
    public class FrameApi
    {
        private NativeSession m_NativeSession;

        public FrameApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public long GetTimestamp()
        {
            long timestamp = 0;
            ExternApi.ArFrame_getTimestamp(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                ref timestamp);
            return timestamp;
        }

        public IntPtr AcquireCamera()
        {
            IntPtr cameraHandle = IntPtr.Zero;
            ExternApi.ArFrame_acquireCamera(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                ref cameraHandle);
            return cameraHandle;
        }

        public IntPtr AcquirePointCloud()
        {
            IntPtr pointCloudHandle = IntPtr.Zero;
            ExternApi.ArFrame_acquirePointCloud(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                ref pointCloudHandle);
            return pointCloudHandle;
        }

        public IntPtr AcquireImageMetadata()
        {
            IntPtr imageMetadataHandle = IntPtr.Zero;
            ExternApi.ArFrame_acquireImageMetadata(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                ref imageMetadataHandle);
            return imageMetadataHandle;
        }

        public LightEstimate GetLightEstimate()
        {
            IntPtr lightEstimateHandle = m_NativeSession.LightEstimateApi.Create();
            ExternApi.ArFrame_getLightEstimate(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                lightEstimateHandle);

            LightEstimateState state = m_NativeSession.LightEstimateApi.GetState(lightEstimateHandle);
            float pixelIntensity = m_NativeSession.LightEstimateApi.GetPixelIntensity(lightEstimateHandle);

            m_NativeSession.LightEstimateApi.Destroy(lightEstimateHandle);

            return new LightEstimate(state, pixelIntensity);
        }

        public void TransformDisplayUvCoords(ref ApiDisplayUvCoords uv)
        {
            ApiDisplayUvCoords uvOut = new ApiDisplayUvCoords();
            ExternApi.ArFrame_transformDisplayUvCoords(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                ApiDisplayUvCoords.NumFloats, ref uv, ref uvOut);

            uv = uvOut;
        }

        public void GetUpdatedTrackables(List<Trackable> trackables)
        {
            IntPtr listHandle = m_NativeSession.TrackableListApi.Create();
            ExternApi.ArFrame_getUpdatedTrackables(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                ApiTrackableType.BaseTrackable, listHandle);

            trackables.Clear();
            int count = m_NativeSession.TrackableListApi.GetCount(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackableHandle = m_NativeSession.TrackableListApi.AcquireItem(listHandle, i);
                trackables.Add(m_NativeSession.TrackableFactory(trackableHandle));
            }

            m_NativeSession.TrackableListApi.Destroy(listHandle);
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getTimestamp(IntPtr sessionHandle,
                IntPtr frame, ref long timestamp);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArFrame_acquireCamera(IntPtr sessionHandle, IntPtr frameHandle,
                ref IntPtr cameraHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArFrame_acquirePointCloud(IntPtr sessionHandle, IntPtr frameHandle,
                ref IntPtr pointCloudHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_transformDisplayUvCoords(IntPtr session, IntPtr frame,
                int numElements, ref ApiDisplayUvCoords uvsIn, ref ApiDisplayUvCoords uvsOut);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getUpdatedTrackables(IntPtr sessionHandle, IntPtr frameHandle,
                ApiTrackableType filterType, IntPtr outTrackableList);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getLightEstimate(IntPtr sessionHandle, IntPtr frameHandle,
                IntPtr lightEstimateHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_acquireImageMetadata(IntPtr sessionHandle, IntPtr frameHandle,
                ref IntPtr outMetadata);
        }
    }
}
