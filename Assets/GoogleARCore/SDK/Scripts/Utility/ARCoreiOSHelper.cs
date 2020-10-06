//-----------------------------------------------------------------------
// <copyright file="ARCoreiOSHelper.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
#if UNITY_IOS && ARCORE_IOS_SUPPORT
namespace GoogleARCoreInternal
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.XR.iOS;


    internal class ARCoreiOSHelper : IARCoreiOSHelper
    {
        private const string _cloudServicesApiKeyPath = "RuntimeSettings/CloudServicesApiKey";

        public string GetCloudServicesApiKey()
        {
            string key = (Resources.Load(_cloudServicesApiKeyPath) as TextAsset).text;
            return key;
        }

        public IntPtr GetARKitSessionPtr()
        {
            var _session =
                UnityEngine.XR.iOS.UnityARSessionNativeInterface.GetARSessionNativeInterface();
            var sessionField = _session.GetType().GetField(
                "m_NativeARSession", BindingFlags.NonPublic | BindingFlags.Instance);
            var val = sessionField.GetValue(_session);
            return ExternApi.ARCoreARKitIntegration_castUnitySessionToARKitSession(
                (System.IntPtr)val);
        }

        public IntPtr GetARKitFramePtr(IntPtr arkitSessionPtr)
        {
            return ExternApi.ARCoreARKitIntegration_getCurrentFrame(arkitSessionPtr);
        }

        public void RegisterFrameUpdateEvent(
            UnityARSessionNativeInterface.ARFrameUpdate onFrameUpdate)
        {
            UnityEngine.XR.iOS.UnityARSessionNativeInterface.ARFrameUpdatedEvent += onFrameUpdate;
        }

        public void UnregisterFrameUpdateEvent(
            UnityARSessionNativeInterface.ARFrameUpdate onFrameUpdate)
        {
            UnityEngine.XR.iOS.UnityARSessionNativeInterface.ARFrameUpdatedEvent -= onFrameUpdate;
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreARKitIntegrationApi)]
            public static extern IntPtr ARCoreARKitIntegration_castUnitySessionToARKitSession(
                IntPtr sessionToCast);

            [DllImport(ApiConstants.ARCoreARKitIntegrationApi)]
            public static extern IntPtr ARCoreARKitIntegration_getCurrentFrame(
                IntPtr arkitSessionHandle);
        }
    }
}
#endif
