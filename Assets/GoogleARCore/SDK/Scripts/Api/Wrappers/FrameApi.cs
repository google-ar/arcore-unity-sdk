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
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class FrameApi
    {
        private NativeSession m_NativeSession;

        public FrameApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public void Release(IntPtr frameHandle)
        {
            ExternApi.ArFrame_release(frameHandle);
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

        public CameraImageBytes AcquireCameraImageBytes()
        {
            IntPtr cameraImageHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArFrame_acquireCameraImage(m_NativeSession.SessionHandle,
                m_NativeSession.FrameHandle, ref cameraImageHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogWarningFormat("Failed to acquire camera image with status {0}.", status);
                return new CameraImageBytes(IntPtr.Zero);
            }

            return new CameraImageBytes(cameraImageHandle);
        }

        public bool TryAcquirePointCloudHandle(out IntPtr pointCloudHandle)
        {
            pointCloudHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArFrame_acquirePointCloud(m_NativeSession.SessionHandle,
                m_NativeSession.FrameHandle, ref pointCloudHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogWarningFormat("Failed to acquire point cloud with status {0}", status);
                return false;
            }

            return true;
        }

        public bool AcquireImageMetadata(ref IntPtr imageMetadataHandle)
        {
            var status = ExternApi.ArFrame_acquireImageMetadata(m_NativeSession.SessionHandle,
                m_NativeSession.FrameHandle, ref imageMetadataHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to aquire camera image metadata with status {0}", status);
                return false;
            }

            return true;
        }

        public LightEstimate GetLightEstimate()
        {
            IntPtr lightEstimateHandle = m_NativeSession.LightEstimateApi.Create();
            ExternApi.ArFrame_getLightEstimate(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                lightEstimateHandle);

            LightEstimateState state = m_NativeSession.LightEstimateApi.GetState(lightEstimateHandle);
            Color colorCorrection = m_NativeSession.LightEstimateApi.GetColorCorrection(lightEstimateHandle);

            m_NativeSession.LightEstimateApi.Destroy(lightEstimateHandle);

            return new LightEstimate(state, colorCorrection.a,
                new Color(colorCorrection.r, colorCorrection.g, colorCorrection.b, 1f));
        }

        public void TransformDisplayUvCoords(ref ApiDisplayUvCoords uv)
        {
            ApiDisplayUvCoords uvOut = new ApiDisplayUvCoords();
            ExternApi.ArFrame_transformDisplayUvCoords(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                ApiDisplayUvCoords.NumFloats, ref uv, ref uvOut);

            uv = uvOut;
        }

        public void TransformCoordinates2d(ref Vector2 uv, DisplayUvCoordinateType inputType,
            DisplayUvCoordinateType outputType)
        {
            Vector2 uvOut = new Vector2(uv.x, uv.y);
            ExternApi.ArFrame_transformCoordinates2d(m_NativeSession.SessionHandle, m_NativeSession.FrameHandle,
                inputType.ToApiCoordinates2dType(), 1, ref uv, outputType.ToApiCoordinates2dType(), ref uvOut);

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

                // TODO:: Remove conditional when b/75291352 is fixed.
                ApiTrackableType trackableType = m_NativeSession.TrackableApi.GetType(trackableHandle);
                if ((int)trackableType == 0x41520105)
                {
                    m_NativeSession.TrackableApi.Release(trackableHandle);
                    continue;
                }

                Trackable trackable = m_NativeSession.TrackableFactory(trackableHandle);
                if (trackable != null)
                {
                    trackables.Add(trackable);
                }
                else
                {
                    m_NativeSession.TrackableApi.Release(trackableHandle);
                }
            }

            m_NativeSession.TrackableListApi.Destroy(listHandle);
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_release(IntPtr frame);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getTimestamp(IntPtr sessionHandle,
                IntPtr frame, ref long timestamp);

#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_acquireCamera(IntPtr sessionHandle, IntPtr frameHandle,
                ref IntPtr cameraHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquireCameraImage(IntPtr sessionHandle, IntPtr frameHandle,
                ref IntPtr imageHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquirePointCloud(IntPtr sessionHandle, IntPtr frameHandle,
                ref IntPtr pointCloudHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_transformDisplayUvCoords(IntPtr session, IntPtr frame,
                int numElements, ref ApiDisplayUvCoords uvsIn, ref ApiDisplayUvCoords uvsOut);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_transformCoordinates2d(IntPtr session, IntPtr frame,
                ApiCoordinates2dType inputType, int numVertices, ref Vector2 uvsIn,
                ApiCoordinates2dType outputType, ref Vector2 uvsOut);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getUpdatedTrackables(IntPtr sessionHandle, IntPtr frameHandle,
                ApiTrackableType filterType, IntPtr outTrackableList);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getLightEstimate(IntPtr sessionHandle, IntPtr frameHandle,
                IntPtr lightEstimateHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquireImageMetadata(IntPtr sessionHandle, IntPtr frameHandle,
                ref IntPtr outMetadata);
#pragma warning restore 626
        }
    }
}
