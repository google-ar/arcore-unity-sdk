//-----------------------------------------------------------------------
// <copyright file="FrameApi.cs" company="Google LLC">
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
        private NativeSession _nativeSession;

        private float[,] _ambientSH = new float[9, 3];

        public FrameApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public void Release(IntPtr frameHandle)
        {
            ExternApi.ArFrame_release(frameHandle);
        }

        public long GetTimestamp()
        {
            long timestamp = 0;
            ExternApi.ArFrame_getTimestamp(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle, ref timestamp);
            return timestamp;
        }

        public IntPtr AcquireCamera()
        {
            IntPtr cameraHandle = IntPtr.Zero;
            ExternApi.ArFrame_acquireCamera(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle, ref cameraHandle);
            return cameraHandle;
        }

        public CameraImageBytes AcquireCameraImageBytes()
        {
            IntPtr cameraImageHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArFrame_acquireCameraImage(_nativeSession.SessionHandle,
                _nativeSession.FrameHandle, ref cameraImageHandle);
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
            ApiArStatus status = ExternApi.ArFrame_acquirePointCloud(_nativeSession.SessionHandle,
                _nativeSession.FrameHandle, ref pointCloudHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogWarningFormat("Failed to acquire point cloud with status {0}", status);
                return false;
            }

            return true;
        }

        public bool AcquireImageMetadata(ref IntPtr imageMetadataHandle)
        {
            var status = ExternApi.ArFrame_acquireImageMetadata(_nativeSession.SessionHandle,
                _nativeSession.FrameHandle, ref imageMetadataHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat(
                    "Failed to aquire camera image metadata with status {0}", status);
                return false;
            }

            return true;
        }

        public LightEstimate GetLightEstimate()
        {
            IntPtr lightEstimateHandle = _nativeSession.LightEstimateApi.Create();
            ExternApi.ArFrame_getLightEstimate(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle, lightEstimateHandle);

            LightEstimateState state =
                _nativeSession.LightEstimateApi.GetState(lightEstimateHandle);
            Color colorCorrection =
                _nativeSession.LightEstimateApi.GetColorCorrection(lightEstimateHandle);
            long timestamp = _nativeSession.LightEstimateApi.GetTimestamp(
                _nativeSession.SessionHandle, lightEstimateHandle);

            Quaternion mainLightRotation = Quaternion.identity;
            Color mainLightColor = Color.black;
            _nativeSession.LightEstimateApi.GetMainDirectionalLight(
                _nativeSession.SessionHandle, lightEstimateHandle,
                out mainLightRotation, out mainLightColor);
            _nativeSession.LightEstimateApi.GetAmbientSH(_nativeSession.SessionHandle,
                lightEstimateHandle, _ambientSH);

            _nativeSession.LightEstimateApi.Destroy(lightEstimateHandle);
            return new LightEstimate(state, colorCorrection.a,
                new Color(colorCorrection.r, colorCorrection.g, colorCorrection.b, 1f),
                mainLightRotation, mainLightColor, _ambientSH, timestamp);
        }

        public Cubemap GetReflectionCubemap()
        {
            IntPtr lightEstimateHandle = _nativeSession.LightEstimateApi.Create();
            ExternApi.ArFrame_getLightEstimate(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle, lightEstimateHandle);
            LightEstimateState state =
                _nativeSession.LightEstimateApi.GetState(lightEstimateHandle);
            if (state != LightEstimateState.Valid)
            {
                return null;
            }

            Cubemap cubemap = _nativeSession.LightEstimateApi.GetReflectionCubemap(
                _nativeSession.SessionHandle, lightEstimateHandle);
            _nativeSession.LightEstimateApi.Destroy(lightEstimateHandle);

            return cubemap;
        }

        public void TransformDisplayUvCoords(ref ApiDisplayUvCoords uv)
        {
            ApiDisplayUvCoords uvOut = new ApiDisplayUvCoords();
            ExternApi.ArFrame_transformDisplayUvCoords(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle,
                ApiDisplayUvCoords.NumFloats, ref uv, ref uvOut);

            uv = uvOut;
        }

        public void TransformCoordinates2d(ref Vector2 uv, DisplayUvCoordinateType inputType,
            DisplayUvCoordinateType outputType)
        {
            Vector2 uvOut = new Vector2(uv.x, uv.y);
            ExternApi.ArFrame_transformCoordinates2d(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle,
                inputType.ToApiCoordinates2dType(), 1, ref uv, outputType.ToApiCoordinates2dType(),
                ref uvOut);

            uv = uvOut;
        }

        public void GetUpdatedTrackables(List<Trackable> trackables)
        {
            IntPtr listHandle = _nativeSession.TrackableListApi.Create();
            ExternApi.ArFrame_getUpdatedTrackables(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle,
                ApiTrackableType.BaseTrackable, listHandle);

            trackables.Clear();
            int count = _nativeSession.TrackableListApi.GetCount(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackableHandle =
                    _nativeSession.TrackableListApi.AcquireItem(listHandle, i);

                // TODO:: Remove conditional when b/75291352 is fixed.
                ApiTrackableType trackableType =
                    _nativeSession.TrackableApi.GetType(trackableHandle);
                if ((int)trackableType == 0x41520105)
                {
                    _nativeSession.TrackableApi.Release(trackableHandle);
                    continue;
                }

                Trackable trackable = _nativeSession.TrackableFactory(trackableHandle);
                if (trackable != null)
                {
                    trackables.Add(trackable);
                }
                else
                {
                    _nativeSession.TrackableApi.Release(trackableHandle);
                }
            }

            _nativeSession.TrackableListApi.Destroy(listHandle);
        }

        public int GetCameraTextureName()
        {
            int textureId = -1;
            ExternApi.ArFrame_getCameraTextureName(
                _nativeSession.SessionHandle, _nativeSession.FrameHandle, ref textureId);
            return textureId;
        }

        public DepthStatus UpdateDepthTexture(ref Texture2D depthTexture)
        {
            IntPtr depthImageHandle = IntPtr.Zero;

            // Get the current depth image.
            ApiArStatus status =
                (ApiArStatus)ExternApi.ArFrame_acquireDepthImage(
                    _nativeSession.SessionHandle,
                    _nativeSession.FrameHandle,
                    ref depthImageHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("[ARCore] failed to acquire depth image " +
                    "with status {0}", status.ToString());
                return status.ToDepthStatus();
            }

            // Update the depth texture.
            if (!UpdateDepthTexture(ref depthTexture, depthImageHandle))
            {
                return DepthStatus.InternalError;
            }

            return DepthStatus.Success;
        }

        public DepthStatus UpdateRawDepthTexture(ref Texture2D depthTexture)
        {
            IntPtr depthImageHandle = IntPtr.Zero;

            // Get the current depth image.
            ApiArStatus status = (ApiArStatus)ExternApi.ArFrame_acquireRawDepthImage(
                _nativeSession.SessionHandle,
                _nativeSession.FrameHandle,
                ref depthImageHandle);

            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("[ARCore] failed to acquire raw depth image with status {0}",
                                     status.ToString());
                return status.ToDepthStatus();
            }

            if (!UpdateDepthTexture(ref depthTexture, depthImageHandle))
            {
                return DepthStatus.InternalError;
            }

            return DepthStatus.Success;
        }

        public DepthStatus UpdateRawDepthConfidenceTexture(ref Texture2D confidenceTexture)
        {
            IntPtr confidenceImageHandle = IntPtr.Zero;
            ApiArStatus status = (ApiArStatus)ExternApi.ArFrame_acquireRawDepthConfidenceImage(
                _nativeSession.SessionHandle,
                _nativeSession.FrameHandle,
                ref confidenceImageHandle);

            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat(
                  "[ARCore] failed to acquire raw depth confidence image with status {0}",
                  status.ToString());
                return status.ToDepthStatus();
            }

            if (!UpdateDepthTexture(ref confidenceTexture, confidenceImageHandle))
            {
                return DepthStatus.InternalError;
            }

            return DepthStatus.Success;
        }

        public RecordingResult RecordTrackData(Guid trackId, byte[] data)
        {
            ApiArStatus status = ApiArStatus.ErrorFatal;

            GCHandle trackIdHandle = GCHandle.Alloc(trackId.ToByteArray(), GCHandleType.Pinned);
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            status = ExternApi.ArFrame_recordTrackData(
                _nativeSession.SessionHandle,
                _nativeSession.FrameHandle,
                trackIdHandle.AddrOfPinnedObject(),
                dataHandle.AddrOfPinnedObject(),
                data.Length);

            if (trackIdHandle.IsAllocated)
            {
                trackIdHandle.Free();
            }

            if (dataHandle.IsAllocated)
            {
                dataHandle.Free();
            }

            return status.ToRecordingResult();
        }

        public List<TrackData> GetUpdatedTrackData(Guid trackId)
        {
            List<TrackData> trackDataList = new List<TrackData>();
            IntPtr listHandle = _nativeSession.TrackDataListApi.Create();

            GCHandle trackIdHandle = GCHandle.Alloc(trackId.ToByteArray(),
                                                    GCHandleType.Pinned);

            ExternApi.ArFrame_getUpdatedTrackData(_nativeSession.SessionHandle,
                                                  _nativeSession.FrameHandle,
                                                  trackIdHandle.AddrOfPinnedObject(),
                                                  listHandle);

            if (trackIdHandle.IsAllocated)
            {
                trackIdHandle.Free();
            }

            int count = _nativeSession.TrackDataListApi.GetCount(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackDataHandle =
                    _nativeSession.TrackDataListApi.AcquireItem(listHandle, i);

                TrackData trackData;
                trackData.FrameTimestamp = _nativeSession.TrackDataApi.GetFrameTimestamp(
                    trackDataHandle);
                trackData.Data = _nativeSession.TrackDataApi.GetData(trackDataHandle);

                trackDataList.Add(trackData);
            }

            _nativeSession.TrackDataListApi.Destroy(listHandle);

            return trackDataList;
        }

        private bool UpdateDepthTexture(ref Texture2D texture, IntPtr imageHandle)
        {
            // Get the size of the depth data.
            int width = _nativeSession.ImageApi.GetWidth(imageHandle);
            int height = _nativeSession.ImageApi.GetHeight(imageHandle);

            // Access the depth image surface data.
            IntPtr planeDoublePtr = IntPtr.Zero;
            int planeSize = 0;
            _nativeSession.ImageApi.GetPlaneData(imageHandle, 0, ref planeDoublePtr, ref planeSize);
            IntPtr planeDataPtr = new IntPtr(planeDoublePtr.ToInt64());

            int pixelStride = 0;
            ExternApi.ArImage_getPlanePixelStride(_nativeSession.SessionHandle, imageHandle, 0,
                                                  ref pixelStride);

            // Lazy initialization of the image.
            if (width != texture.width || height != texture.height)
            {
                // Pixel stride is 2 for depth images and 1 for confidence images.
                TextureFormat format =
                    pixelStride == 2 ? TextureFormat.RGB565 : TextureFormat.Alpha8;
                if (!texture.Resize(width, height, format, false))
                {
                    Debug.LogErrorFormat(
                        "Unable to resize texture. Current: width {0} height {1} format {2} " +
                        "Desired: width {3} height {4} format {5} ", texture.width, texture.height,
                        texture.format.ToString(), width, height, format);

                    _nativeSession.ImageApi.Release(imageHandle);
                    return false;
                }
            }

            // Copy the raw depth data to the texture.
            texture.LoadRawTextureData(planeDataPtr, planeSize);
            texture.Apply();

            _nativeSession.ImageApi.Release(imageHandle);

            return true;
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
            public static extern void ArFrame_acquireCamera(
                IntPtr sessionHandle, IntPtr frameHandle, ref IntPtr cameraHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquireCameraImage(
                IntPtr sessionHandle, IntPtr frameHandle, ref IntPtr imageHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquirePointCloud(
                IntPtr sessionHandle, IntPtr frameHandle, ref IntPtr pointCloudHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_transformDisplayUvCoords(
                IntPtr session,
                IntPtr frame,
                int numElements,
                ref ApiDisplayUvCoords uvsIn,
                ref ApiDisplayUvCoords uvsOut);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_transformCoordinates2d(
                IntPtr session,
                IntPtr frame,
                ApiCoordinates2dType inputType,
                int numVertices,
                ref Vector2 uvsIn,
                ApiCoordinates2dType outputType,
                ref Vector2 uvsOut);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getUpdatedTrackables(
                IntPtr sessionHandle, IntPtr frameHandle, ApiTrackableType filterType,
                IntPtr outTrackableList);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getLightEstimate(
                IntPtr sessionHandle, IntPtr frameHandle, IntPtr lightEstimateHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquireImageMetadata(
                IntPtr sessionHandle, IntPtr frameHandle, ref IntPtr outMetadata);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getCameraTextureName(
                IntPtr sessionHandle, IntPtr frameHandle, ref int outTextureId);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_recordTrackData(
                IntPtr sessionHandle, IntPtr frameHandle, IntPtr trackIdBytes, IntPtr dataBytes,
                int dataSize);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_getUpdatedTrackData(
                IntPtr sessionHandle, IntPtr frameHandle, IntPtr trackId, IntPtr trackDataList);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquireDepthImage(
                IntPtr sessionHandle, IntPtr frameHandle, ref IntPtr imageHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArImage_getPlanePixelStride(IntPtr sessionHandle,
                                                                  IntPtr imageHandle,
                                                                  int planeIndex,
                                                                  ref int pixelStride);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquireRawDepthImage(
                IntPtr sessionHandle, IntPtr frameHandle, ref IntPtr imageHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArFrame_acquireRawDepthConfidenceImage(
                IntPtr sessionHandle, IntPtr frameHandle, ref IntPtr imageHandle);
#pragma warning restore 626
        }
    }
}
