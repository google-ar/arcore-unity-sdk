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

    internal class SessionApi
    {
        private NativeSession m_NativeSession;

        public SessionApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public void ReportEngineType()
        {
            ExternApi.ArSession_reportEngineType(
                m_NativeSession.SessionHandle, "Unity", Application.unityVersion);
        }

        public bool SetConfiguration(ARCoreSessionConfig sessionConfig)
        {
            IntPtr configHandle = m_NativeSession.SessionConfigApi.Create();
            m_NativeSession.SessionConfigApi.UpdateApiConfigWithArCoreSessionConfig(
                configHandle, sessionConfig);

            bool ret =
                ExternApi.ArSession_configure(m_NativeSession.SessionHandle, configHandle) == 0;
            m_NativeSession.SessionConfigApi.Destroy(configHandle);

            return ret;
        }

        public void GetSupportedCameraConfigurations(
            IntPtr cameraConfigListHandle, List<IntPtr> supportedCameraConfigHandles,
            List<CameraConfig> supportedCameraConfigs, DeviceCameraDirection cameraFacingDirection)
        {
            ExternApi.ArSession_getSupportedCameraConfigs(
                m_NativeSession.SessionHandle, cameraConfigListHandle);

            supportedCameraConfigHandles.Clear();
            supportedCameraConfigs.Clear();
            int listSize = m_NativeSession.CameraConfigListApi.GetSize(cameraConfigListHandle);

            for (int i = 0; i < listSize; i++)
            {
                IntPtr cameraConfigHandle = m_NativeSession.CameraConfigApi.Create();
                m_NativeSession.CameraConfigListApi.GetItemAt(
                    cameraConfigListHandle, i, cameraConfigHandle);

                // Skip camera config that has a different camera facing direction.
                DeviceCameraDirection configDirection =
                    m_NativeSession.CameraConfigApi.GetFacingDirection(cameraConfigHandle)
                        .ToDeviceCameraDirection();
                if (configDirection != cameraFacingDirection)
                {
                    continue;
                }

                supportedCameraConfigHandles.Add(cameraConfigHandle);
                supportedCameraConfigs.Add(_CreateCameraConfig(cameraConfigHandle));
            }
        }

        public ApiArStatus SetCameraConfig(IntPtr cameraConfigHandle)
        {
            return ExternApi.ArSession_setCameraConfig(
                m_NativeSession.SessionHandle, cameraConfigHandle);
        }

        public CameraConfig GetCameraConfig()
        {
            IntPtr cameraConfigHandle = m_NativeSession.CameraConfigApi.Create();
            ExternApi.ArSession_getCameraConfig(m_NativeSession.SessionHandle, cameraConfigHandle);
            CameraConfig currentCameraConfig = _CreateCameraConfig(cameraConfigHandle);
            m_NativeSession.CameraConfigApi.Destroy(cameraConfigHandle);
            return currentCameraConfig;
        }

        public void GetAllTrackables(List<Trackable> trackables)
        {
            IntPtr listHandle = m_NativeSession.TrackableListApi.Create();
            ExternApi.ArSession_getAllTrackables(
                m_NativeSession.SessionHandle, ApiTrackableType.BaseTrackable, listHandle);

            trackables.Clear();
            int count = m_NativeSession.TrackableListApi.GetCount(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr trackableHandle =
                    m_NativeSession.TrackableListApi.AcquireItem(listHandle, i);

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

            ExternApi.ArSession_setDisplayGeometry(
                m_NativeSession.SessionHandle, androidOrientation, width, height);
        }

        public Anchor CreateAnchor(Pose pose)
        {
            IntPtr poseHandle = m_NativeSession.PoseApi.Create(pose);
            IntPtr anchorHandle = IntPtr.Zero;
            ExternApi.ArSession_acquireNewAnchor(
                m_NativeSession.SessionHandle, poseHandle, ref anchorHandle);
            var anchorResult = Anchor.Factory(m_NativeSession, anchorHandle);
            m_NativeSession.PoseApi.Destroy(poseHandle);
            return anchorResult;
        }

        public ApiArStatus CreateCloudAnchor(
            IntPtr platformAnchorHandle, out IntPtr cloudAnchorHandle)
        {
            cloudAnchorHandle = IntPtr.Zero;
            var result =
                ExternApi.ArSession_hostAndAcquireNewCloudAnchor(
                    m_NativeSession.SessionHandle, platformAnchorHandle, ref cloudAnchorHandle);
            return result;
        }

        public ApiArStatus ResolveCloudAnchor(String cloudAnchorId, out IntPtr cloudAnchorHandle)
        {
            cloudAnchorHandle = IntPtr.Zero;
            return ExternApi.ArSession_resolveAndAcquireNewCloudAnchor(
                m_NativeSession.SessionHandle, cloudAnchorId, ref cloudAnchorHandle);
        }

        private CameraConfig _CreateCameraConfig(IntPtr cameraConfigHandle)
        {
            int imageWidth = 0;
            int imageHeight = 0;
            int textureWidth = 0;
            int textureHeight = 0;
            m_NativeSession.CameraConfigApi.GetImageDimensions(
                cameraConfigHandle, out imageWidth, out imageHeight);
            m_NativeSession.CameraConfigApi.GetTextureDimensions(
                cameraConfigHandle, out textureWidth, out textureHeight);

            return new CameraConfig(new Vector2(imageWidth, imageHeight),
                new Vector2(textureWidth, textureHeight));
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArSession_configure(IntPtr sessionHandle, IntPtr config);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getSupportedCameraConfigs(
                IntPtr sessionHandle, IntPtr cameraConfigListHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_setCameraConfig(
                IntPtr sessionHandle, IntPtr cameraConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getCameraConfig(
                IntPtr sessionHandle, IntPtr cameraConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getAllTrackables(
                IntPtr sessionHandle, ApiTrackableType filterType, IntPtr trackableList);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setDisplayGeometry(
                IntPtr sessionHandle, int rotation, int width, int height);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern int ArSession_acquireNewAnchor(
                IntPtr sessionHandle, IntPtr poseHandle, ref IntPtr anchorHandle);
#pragma warning restore 626
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_reportEngineType(
                IntPtr sessionHandle, string engineType, string engineVersion);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_resolveAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                String cloudAnchorId,
                ref IntPtr cloudAnchorHandle);
        }
    }
}
