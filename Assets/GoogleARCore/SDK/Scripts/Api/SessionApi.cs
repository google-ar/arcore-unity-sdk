//-----------------------------------------------------------------------
// <copyright file="SessionApi.cs" company="Google">
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
    public class SessionApi
    {
        private NativeApi m_NativeApi;

        public SessionApi(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public void ReportEngineType()
        {
            ExternApi.ArSession_reportEngineType(m_NativeApi.SessionHandle, "Unity",
                Application.unityVersion);
        }

        public ApiArStatus CheckSupported(ARCoreSessionConfig config)
        {
            IntPtr configHandle;
            if (config == null)
            {
                configHandle = IntPtr.Zero;
                return ApiArStatus.ErrorUnsupportedConfiguration;
            }
            else
            {
                configHandle = m_NativeApi.SessionConfig.Create();
                m_NativeApi.SessionConfig.UpdateApiConfigWithArCoreSessionConfig(configHandle, config);
            }

            ApiArStatus ret = ExternApi.ArSession_checkSupported(m_NativeApi.SessionHandle, configHandle);
            m_NativeApi.SessionConfig.Destroy(configHandle);
            return ret;
        }

        public bool SetConfiguration(ARCoreSessionConfig sessionConfig)
        {
            IntPtr configHandle = m_NativeApi.SessionConfig.Create();
            m_NativeApi.SessionConfig.UpdateApiConfigWithArCoreSessionConfig(configHandle, sessionConfig);

            bool ret = ExternApi.ArSession_configure(m_NativeApi.SessionHandle, configHandle) == 0;
            m_NativeApi.SessionConfig.Destroy(configHandle);

            return ret;
        }

        public void GetAllTrackables(List<Trackable> trackables)
        {
            IntPtr listHandle = m_NativeApi.TrackableList.Create();
            ExternApi.ArSession_getAllTrackables(m_NativeApi.SessionHandle, ApiTrackableType.BaseTrackable, listHandle);

            trackables.Clear();
            int count = m_NativeApi.TrackableList.GetCount(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackableHandle = m_NativeApi.TrackableList.AcquireItem(listHandle, i);
                trackables.Add(m_NativeApi.TrackableFactory(trackableHandle));
            }

            m_NativeApi.TrackableList.Destroy(listHandle);
        }

        public Anchor CreateAnchor(Pose pose)
        {
            IntPtr poseHandle = m_NativeApi.Pose.Create(pose);
            IntPtr anchorHandle = IntPtr.Zero;
            ExternApi.ArSession_acquireNewAnchor(m_NativeApi.SessionHandle, poseHandle, ref anchorHandle);
            var anchorResult = Anchor.AnchorFactory(anchorHandle, m_NativeApi);
            m_NativeApi.Pose.Destroy(poseHandle);

            return anchorResult;
        }

        public void SetDisplayGeometry(ScreenOrientation orientation, int width, int height)
        {
            const int androidRotation0 = 0;
            const int androidRotation90 = 1;
            const int androidRotation180 = 2;
            const int androidRotation270 = 3;

            int androidOrientation = 0;
            switch (orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    androidOrientation = androidRotation90;
                    break;
                case ScreenOrientation.LandscapeRight:
                    androidOrientation = androidRotation270;
                    break;
                case ScreenOrientation.Portrait:
                    androidOrientation = androidRotation0;
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    androidOrientation = androidRotation180;
                    break;
            }

            ExternApi.ArSession_setDisplayGeometry(m_NativeApi.SessionHandle, androidOrientation, width, height);
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_destroy(IntPtr sessionHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_checkSupported(IntPtr sessionHandle, IntPtr config);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArSession_configure(IntPtr sessionHandle, IntPtr config);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setDisplayGeometry(IntPtr sessionHandle, int rotation, int width,
                int height);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getAllTrackables(IntPtr sessionHandle, ApiTrackableType filterType,
                IntPtr trackableList);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_reportEngineType(IntPtr sessionHandle,
                string engineType,
                string engineVersion);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArSession_acquireNewAnchor(IntPtr sessionHandle, IntPtr poseHandle,
                ref IntPtr anchorHandle);
        }
    }
}