//-----------------------------------------------------------------------
// <copyright file="FrameManager.cs" company="Google">
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
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class FrameManager
    {
        private NativeApi m_NativeApi;

        private IntPtr m_FrameHandle = IntPtr.Zero;

        private Texture2D m_BackgroundTexture;

        private TrackableManager m_TrackableManager;

        private List<TrackableHit> m_TrackableHitList = new List<TrackableHit>();

        public FrameManager(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
            m_TrackableManager = new TrackableManager(nativeApi);
            PointCloudManager = new PointCloudManager(nativeApi);
            CameraMetadataManager = new CameraMetadataManager(nativeApi);
        }

        public PointCloudManager PointCloudManager { get; private set; }

        public CameraMetadataManager CameraMetadataManager { get; private set; }

        public Pose GetPose()
        {
            var cameraHandle = m_NativeApi.Frame.AcquireCamera(m_FrameHandle);
            Pose result = m_NativeApi.Camera.GetPose(cameraHandle);
            m_NativeApi.Camera.Release(cameraHandle);
            return result;
        }

        public LightEstimate GetLightEstimate()
        {
            return m_NativeApi.Frame.GetLightEstimate(m_FrameHandle);
        }

        public Texture GetCameraTexture()
        {
            return m_BackgroundTexture;
        }

        public void TransformDisplayUvCoords(ref ApiDisplayUvCoords uvQuad)
        {
            m_NativeApi.Frame.TransformDisplayUvCoords(m_FrameHandle, ref uvQuad);
        }

        public Matrix4x4 GetCameraProjectionMatrix(float nearClipping, float farClipping)
        {
            var cameraHandle = m_NativeApi.Frame.AcquireCamera(m_FrameHandle);
            var result = m_NativeApi.Camera.GetProjectionMatrix(cameraHandle, nearClipping, farClipping);
            m_NativeApi.Camera.Release(cameraHandle);
            return result;
        }

        public TrackingState GetCameraTrackingState()
        {
            var cameraHandle = m_NativeApi.Frame.AcquireCamera(m_FrameHandle);
            TrackingState result = m_NativeApi.Camera.GetTrackingState(cameraHandle);
            m_NativeApi.Camera.Release(cameraHandle);
            return result;
        }

         public void GetTrackables<T>(List<T> trackables, TrackableQueryFilter filter) where T : Trackable
         {
             m_TrackableManager.GetTrackables(trackables, filter);
         }

        public bool Raycast(float x, float y, TrackableHitFlags filter, out TrackableHit hitResult)
        {
            hitResult = new TrackableHit();

            // Note that the Unity's screen coordinate (0, 0) starts from bottom left.
            bool ret = m_NativeApi.HitTest.Raycast(m_FrameHandle, x, Screen.height - y, filter, m_TrackableHitList,
                true);
            if (ret && m_TrackableHitList.Count != 0)
            {
                hitResult = m_TrackableHitList[0];
            }

            return ret;
        }

        public bool RaycastAll(float x, float y, TrackableHitFlags filter, List<TrackableHit> hitResults)
        {
            // Note that the Unity's screen coordinate (0, 0) starts from bottom left.
            return m_NativeApi.HitTest.Raycast(m_FrameHandle, x, Screen.height - y, filter, hitResults, true);
        }

        public void UpdateFrame(IntPtr frameHandle, uint cameraTextureId)
        {
            m_FrameHandle = frameHandle;
            _UpdateTextureIfNeeded(cameraTextureId);
            PointCloudManager.UpdateFrame(m_FrameHandle);
            CameraMetadataManager.UpdateFrame(m_FrameHandle);
        }

        private void _UpdateTextureIfNeeded(uint cameraTextureId)
        {
            if (m_BackgroundTexture != null &&
                m_BackgroundTexture.GetNativeTexturePtr().ToInt32() == cameraTextureId)
            {
                return;
            }

            if (m_BackgroundTexture == null)
            {
                // The Unity-cached size and format of the texture (0x0, ARGB) is not the
                // actual format of the texture. This is okay because the texture is not
                // accessed by pixels, it is accessed with UV coordinates.
                m_BackgroundTexture = Texture2D.CreateExternalTexture(0, 0, TextureFormat.ARGB32, false,
                    false, new IntPtr(cameraTextureId));
            }
            else
            {
                 m_BackgroundTexture.UpdateExternalTexture(new IntPtr(cameraTextureId));
            }
        }
    }
}
