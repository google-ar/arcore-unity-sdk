//-----------------------------------------------------------------------
// <copyright file="NativeSession.cs" company="Google LLC">
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

    internal class NativeSession
    {
        private PointCloudManager _pointCloudManager = null;

        private TrackableManager _trackableManager = null;

        public NativeSession(IntPtr sessionHandle, IntPtr frameHandle)
        {
            IsDestroyed = false;
            SessionHandle = sessionHandle;
            FrameHandle = frameHandle;
            _pointCloudManager = new PointCloudManager(this);
            _trackableManager = new TrackableManager(this);

            AnchorApi = new AnchorApi(this);
            AugmentedFaceApi = new AugmentedFaceApi(this);
            AugmentedImageApi = new AugmentedImageApi(this);
            AugmentedImageDatabaseApi = new AugmentedImageDatabaseApi(this);
            CameraApi = new CameraApi(this);
            CameraConfigApi = new CameraConfigApi(this);
            CameraConfigFilterApi = new CameraConfigFilterApi(this);
            CameraConfigListApi = new CameraConfigListApi(this);
            CameraMetadataApi = new CameraMetadataApi(this);
            FrameApi = new FrameApi(this);
            HitTestApi = new HitTestApi(this);
            ImageApi = new ImageApi(this);
            LightEstimateApi = new LightEstimateApi(this);
            PlaneApi = new PlaneApi(this);
            PointApi = new PointApi(this);
            PointCloudApi = new PointCloudApi(this);
            PoseApi = new PoseApi(this);
            RecordingConfigApi = new RecordingConfigApi(this);
            SessionApi = new SessionApi(this);
            SessionConfigApi = new SessionConfigApi(this);
            TrackableApi = new TrackableApi(this);
            TrackableListApi = new TrackableListApi(this);

#if !UNITY_EDITOR
            // Engine type is per session. Hence setting it for each
            // native session.
            SessionApi.ReportEngineType();
#endif
        }

        public bool IsDestroyed { get; private set; }

        public IntPtr SessionHandle { get; private set; }

        public IntPtr FrameHandle { get; private set; }

        public IntPtr PointCloudHandle
        {
            get
            {
                return _pointCloudManager.PointCloudHandle;
            }
        }

        public bool IsPointCloudNew
        {
            get
            {
                return _pointCloudManager.IsPointCloudNew;
            }
        }

        public AnchorApi AnchorApi { get; private set; }

        public AugmentedFaceApi AugmentedFaceApi { get; private set; }

        public AugmentedImageApi AugmentedImageApi { get; private set; }

        public AugmentedImageDatabaseApi AugmentedImageDatabaseApi { get; private set; }

        public CameraApi CameraApi { get; private set; }

        public CameraConfigApi CameraConfigApi { get; private set; }

        public CameraConfigFilterApi CameraConfigFilterApi { get; private set; }

        public CameraConfigListApi CameraConfigListApi { get; private set; }

        public CameraMetadataApi CameraMetadataApi { get; private set; }

        public FrameApi FrameApi { get; private set; }

        public HitTestApi HitTestApi { get; private set; }

        public ImageApi ImageApi { get; private set; }

        public LightEstimateApi LightEstimateApi { get; private set; }

        public PlaneApi PlaneApi { get; private set; }

        public PointApi PointApi { get; private set; }

        public PointCloudApi PointCloudApi { get; private set; }

        public PoseApi PoseApi { get; private set; }

        public RecordingConfigApi RecordingConfigApi { get; private set; }

        public SessionApi SessionApi { get; private set; }

        public SessionConfigApi SessionConfigApi { get; private set; }

        public TrackableApi TrackableApi { get; private set; }

        public TrackableListApi TrackableListApi { get; private set; }

        public Trackable TrackableFactory(IntPtr nativeHandle)
        {
            return _trackableManager.TrackableFactory(nativeHandle);
        }

        public void GetTrackables<T>(List<T> trackables, TrackableQueryFilter filter)
            where T : Trackable
        {
            _trackableManager.GetTrackables<T>(trackables, filter);
        }

        public void OnUpdate(IntPtr frameHandle)
        {
            FrameHandle = frameHandle;
            _pointCloudManager.OnUpdate();
        }

        public void MarkDestroyed()
        {
            IsDestroyed = true;
        }
    }
}
