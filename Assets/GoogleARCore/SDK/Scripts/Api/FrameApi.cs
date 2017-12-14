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
        private NativeApi m_NativeApi;

        public FrameApi(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public long GetTimestamp(IntPtr frameHandle)
        {
            long timestamp = 0;
            ExternApi.ArFrame_getTimestamp(m_NativeApi.SessionHandle, frameHandle,
                ref timestamp);
            return timestamp;
        }

        public IntPtr AcquireCamera(IntPtr frameHandle)
        {
            if (frameHandle == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            IntPtr cameraHandle = IntPtr.Zero;
            ExternApi.ArFrame_acquireCamera(m_NativeApi.SessionHandle, frameHandle,
                ref cameraHandle);
            return cameraHandle;
        }

        public IntPtr AcquirePointCloud(IntPtr frameHandle)
        {
            IntPtr pointCloudHandle = IntPtr.Zero;
            ExternApi.ArFrame_acquirePointCloud(m_NativeApi.SessionHandle, frameHandle,
                ref pointCloudHandle);
            return pointCloudHandle;
        }

        public IntPtr AcquireImageMetadata(IntPtr frameHandle)
        {
            IntPtr imageMetadataHandle = IntPtr.Zero;
            ExternApi.ArFrame_acquireImageMetadata(m_NativeApi.SessionHandle, frameHandle, ref imageMetadataHandle);
            return imageMetadataHandle;
        }

        public LightEstimate GetLightEstimate(IntPtr frameHandle)
        {
            IntPtr lightEstimateHandle = m_NativeApi.LightEstimate.Create();
            ExternApi.ArFrame_getLightEstimate(m_NativeApi.SessionHandle, frameHandle,
                lightEstimateHandle);

            LightEstimateState state = m_NativeApi.LightEstimate.GetState(lightEstimateHandle);
            float pixelIntensity = m_NativeApi.LightEstimate.GetPixelIntensity(lightEstimateHandle);

            m_NativeApi.LightEstimate.Destroy(lightEstimateHandle);

            return new LightEstimate(state, pixelIntensity);
        }

        public void TransformDisplayUvCoords(IntPtr frameHandle, ref ApiDisplayUvCoords uv)
        {
            ApiDisplayUvCoords uvOut = new ApiDisplayUvCoords();
            ExternApi.ArFrame_transformDisplayUvCoords(m_NativeApi.SessionHandle, frameHandle,
                ApiDisplayUvCoords.NumFloats, ref uv, ref uvOut);

            uv = uvOut;
        }

        public void GetUpdatedTrackables(IntPtr frameHandle, List<Trackable> trackables)
        {
            IntPtr listHandle = m_NativeApi.TrackableList.Create();
            ExternApi.ArFrame_getUpdatedTrackables(m_NativeApi.SessionHandle, frameHandle,
                ApiTrackableType.BaseTrackable, listHandle);

            trackables.Clear();
            int count = m_NativeApi.TrackableList.GetCount(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackableHandle = m_NativeApi.TrackableList.AcquireItem(listHandle, i);
                trackables.Add(m_NativeApi.TrackableFactory(trackableHandle));
            }

            m_NativeApi.TrackableList.Destroy(listHandle);
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
