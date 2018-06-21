//-----------------------------------------------------------------------
// <copyright file="SessionConfigApi.cs" company="Google">
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
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class SessionConfigApi
    {
        private NativeSession m_NativeSession;

        public SessionConfigApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public IntPtr Create()
        {
            IntPtr configHandle = IntPtr.Zero;
            ExternApi.ArConfig_create(m_NativeSession.SessionHandle, ref configHandle);
            return configHandle;
        }

        public void Destroy(IntPtr configHandle)
        {
            ExternApi.ArConfig_destroy(configHandle);
        }

        public void UpdateApiConfigWithArCoreSessionConfig(IntPtr configHandle, ARCoreSessionConfig arCoreSessionConfig)
        {
            var lightingMode = ApiLightEstimationMode.Disabled;
            if (arCoreSessionConfig.EnableLightEstimation)
            {
                lightingMode = ApiLightEstimationMode.AmbientIntensity;
            }

            ExternApi.ArConfig_setLightEstimationMode(m_NativeSession.SessionHandle, configHandle, lightingMode);

            var planeFindingMode = ApiPlaneFindingMode.Disabled;
            switch (arCoreSessionConfig.PlaneFindingMode)
            {
            case DetectedPlaneFindingMode.Horizontal:
                planeFindingMode = ApiPlaneFindingMode.Horizontal;
                break;
            case DetectedPlaneFindingMode.Vertical:
                planeFindingMode = ApiPlaneFindingMode.Vertical;
                break;
            case DetectedPlaneFindingMode.HorizontalAndVertical:
                planeFindingMode = ApiPlaneFindingMode.HorizontalAndVertical;
                break;
            default:
                break;
            }

            ExternApi.ArConfig_setPlaneFindingMode(m_NativeSession.SessionHandle, configHandle, planeFindingMode);

            var updateMode = ApiUpdateMode.LatestCameraImage;
            if (arCoreSessionConfig.MatchCameraFramerate)
            {
               updateMode = ApiUpdateMode.Blocking;

               // Set vSyncCount to 0 so frame in rendered only when we have a new background texture.
               QualitySettings.vSyncCount = 0;
            }

            ExternApi.ArConfig_setUpdateMode(m_NativeSession.SessionHandle, configHandle, updateMode);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_create(IntPtr session, ref IntPtr out_config);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_destroy(IntPtr config);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setLightEstimationMode(IntPtr session, IntPtr config,
                ApiLightEstimationMode light_estimation_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setPlaneFindingMode(IntPtr session, IntPtr config,
                ApiPlaneFindingMode plane_finding_mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setUpdateMode(IntPtr session, IntPtr config,
                ApiUpdateMode update_mode);
#pragma warning restore 626
        }
    }
}
