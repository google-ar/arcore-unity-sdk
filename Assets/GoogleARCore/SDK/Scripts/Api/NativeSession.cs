//-----------------------------------------------------------------------
// <copyright file="NativeSession.cs" company="Google">
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
    public class NativeSession
    {
        private IntPtr m_SessionHandle = IntPtr.Zero;

        private IntPtr m_FrameHandle = IntPtr.Zero;

        private IntPtr m_PointCloudHandle = IntPtr.Zero;

        private float m_LastReleasedPointcloudTimestamp = 0.0f;

        private TrackableManager m_TrackableManager = null;

        public NativeSession(IntPtr sessionHandle, IntPtr frameHandle)
        {
            m_SessionHandle = sessionHandle;
            m_FrameHandle = frameHandle;
            m_TrackableManager = new TrackableManager(this);

            AnchorApi = new AnchorApi(this);
            CameraApi = new CameraApi(this);
            CameraMetadataApi = new CameraMetadataApi(this);
            FrameApi = new FrameApi(this);
            HitTestApi = new HitTestApi(this);
            LightEstimateApi = new LightEstimateApi(this);
            PlaneApi = new PlaneApi(this);
            PointApi = new PointApi(this);
            PointCloudApi = new PointCloudApi(this);
            PoseApi = new PoseApi(this);
            SessionApi = new SessionApi(this);
            SessionConfigApi = new SessionConfigApi(this);
            TrackableApi = new TrackableApi(this);
            TrackableListApi = new TrackableListApi(this);
        }

        public IntPtr SessionHandle
        {
            get
            {
                return m_SessionHandle;
            }
        }

        public IntPtr FrameHandle
        {
            get
            {
                return m_FrameHandle;
            }
        }

        public IntPtr PointCloudHandle
        {
            get
            {
                return m_PointCloudHandle;
            }
        }

        public bool IsPointCloudNew
        {
            get
            {
                // TODO (b/73256094): Remove when fixed.
                if (LifecycleManager.Instance.SessionStatus != SessionStatus.Tracking)
                {
                    var previousLastTimestamp = m_LastReleasedPointcloudTimestamp;
                    m_LastReleasedPointcloudTimestamp = 0.0f;
                    return previousLastTimestamp != 0;
                }

                return PointCloudApi.GetTimestamp(PointCloudHandle) != m_LastReleasedPointcloudTimestamp;
            }
        }

        public AnchorApi AnchorApi { get; private set; }

        public CameraApi CameraApi { get; private set; }

        public CameraMetadataApi CameraMetadataApi { get; private set; }

        public FrameApi FrameApi { get; private set; }

        public HitTestApi HitTestApi { get; private set; }

        public LightEstimateApi LightEstimateApi { get; private set; }

        public PlaneApi PlaneApi { get; private set; }

        public PointApi PointApi { get; private set; }

        public PointCloudApi PointCloudApi { get; private set; }

        public PoseApi PoseApi { get; private set; }

        public SessionApi SessionApi { get; private set; }

        public SessionConfigApi SessionConfigApi { get; private set; }

        public TrackableApi TrackableApi { get; private set; }

        public TrackableListApi TrackableListApi { get; private set; }

        public Trackable TrackableFactory(IntPtr nativeHandle)
        {
            return m_TrackableManager.TrackableFactory(nativeHandle);
        }

        public void GetTrackables<T>(List<T> trackables, TrackableQueryFilter filter) where T : Trackable
        {
            m_TrackableManager.GetTrackables<T>(trackables, filter);
        }

        public void OnUpdate()
        {
            // After first frame, release previous frame's point cloud.
            if (m_PointCloudHandle != IntPtr.Zero)
            {
                m_LastReleasedPointcloudTimestamp = PointCloudApi.GetTimestamp(m_PointCloudHandle);
                PointCloudApi.Release(m_PointCloudHandle);
                m_PointCloudHandle = IntPtr.Zero;
            }

            // TODO (b/73256094): Remove when fixed.
            if (LifecycleManager.Instance.SessionStatus == SessionStatus.Tracking)
            {
                m_PointCloudHandle = FrameApi.AcquirePointCloud();
            }
        }
    }
}
