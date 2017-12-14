//-----------------------------------------------------------------------
// <copyright file="NativeApi.cs" company="Google">
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
    public class NativeApi
    {
        private Dictionary<IntPtr, Trackable> m_TrackableDict = new Dictionary<IntPtr, Trackable>(new IntPtrEqualityComparer());

        private IntPtr m_SessionHandle = IntPtr.Zero;

        private EarlyUpdateCallback m_OnEarlyUpdate;

        private NativeApi()
        {
        }

        ~NativeApi()
        {
            Destroy();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EarlyUpdateCallback(IntPtr frameHandle, int textureId);

        public IntPtr SessionHandle
        {
            get
            {
                return m_SessionHandle;
            }
        }

        public AnchorApi Anchor { get; private set; }

        public CameraApi Camera { get; private set; }

        public FrameApi Frame { get; private set; }

        public HitTestApi HitTest { get; private set; }

        public LightEstimateApi LightEstimate { get; private set; }

        public PlaneApi Plane { get; private set; }

        public PointApi Point { get; private set; }

        public PointCloudApi PointCloud { get; private set; }

        public PoseApi Pose { get; private set; }

        public SessionApi Session { get; private set; }

        public SessionConfigApi SessionConfig { get; private set; }

        public TrackableApi Trackable { get; private set; }

        public TrackableListApi TrackableList { get; private set; }

        public CameraMetadataApi CameraMetadata { get; private set; }

        public static NativeApi CreateSession()
        {
            NativeApi nativeApi = new NativeApi();
            ExternApi.ArCoreUnity_createSession(ref nativeApi.m_SessionHandle);

            if (nativeApi.m_SessionHandle == IntPtr.Zero)
            {
                Debug.LogError("ARCore failed to create a session.");
                return null;
            }

            nativeApi.Anchor = new AnchorApi(nativeApi);
            nativeApi.Camera = new CameraApi(nativeApi);
            nativeApi.Frame = new FrameApi(nativeApi);
            nativeApi.HitTest = new HitTestApi(nativeApi);
            nativeApi.LightEstimate = new LightEstimateApi(nativeApi);
            nativeApi.Plane = new PlaneApi(nativeApi);
            nativeApi.Point = new PointApi(nativeApi);
            nativeApi.PointCloud = new PointCloudApi(nativeApi);
            nativeApi.Pose = new PoseApi(nativeApi);
            nativeApi.Session = new SessionApi(nativeApi);
            nativeApi.SessionConfig = new SessionConfigApi(nativeApi);
            nativeApi.Trackable = new TrackableApi(nativeApi);
            nativeApi.TrackableList = new TrackableListApi(nativeApi);
            nativeApi.CameraMetadata = new CameraMetadataApi(nativeApi);

            nativeApi.Session.ReportEngineType();

            return nativeApi;
        }

        public void Destroy()
        {
            if (m_SessionHandle != IntPtr.Zero)
            {
                ExternApi.ArCoreUnity_destroySession();
                m_SessionHandle = IntPtr.Zero;
            }
        }
        
        public bool Resume(EarlyUpdateCallback onEarlyUpdate)
        {
            m_OnEarlyUpdate = onEarlyUpdate;
            return ExternApi.ArCoreUnity_resumeSession(m_OnEarlyUpdate);
        }

        /// <summary>
        /// Factory method for creating and reusing TrackedPlane references from native handles.
        /// </summary>
        /// <param name="nativeHandle">A native handle to a plane that has been acquired.  RELEASE WILL BE HANDLED BY
        /// THIS METHOD.</param>
        /// <returns>A reference to a tracked plane. </returns>
        public Trackable TrackableFactory(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                return null;
            }

            Trackable result;
            if (m_TrackableDict.TryGetValue(nativeHandle, out result))
            {
                // Release aquired handle and return cached result.
                Trackable.Release(nativeHandle);
                return result;
            }

            ApiTrackableType trackableType = Trackable.GetType(nativeHandle);
            if (trackableType == ApiTrackableType.Plane)
            {
                result = new TrackedPlane(nativeHandle, this);
            }
            else if (trackableType == ApiTrackableType.Point)
            {
                result = new TrackedPoint(nativeHandle, this);
            }
            else
            {
                UnityEngine.Debug.LogFormat("Cant find {0}", trackableType);
                throw new NotImplementedException("TrackableFactory:: No contructor for requested trackable type.");
            }

            m_TrackableDict.Add(nativeHandle, result);
            return result;
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArCoreUnity_createSession(ref IntPtr sessionHandle);

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern void ArCoreUnity_destroySession();

            [DllImport(ApiConstants.ARCoreShimApi)]
            public static extern bool ArCoreUnity_resumeSession(EarlyUpdateCallback earlyUpdate);
        }
    }
}
