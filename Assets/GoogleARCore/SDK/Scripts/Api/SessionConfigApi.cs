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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class SessionConfigApi
    {
        private NativeApi m_NativeApi;

        public SessionConfigApi(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public IntPtr Create()
        {
            IntPtr configHandle = IntPtr.Zero;
            ExternApi.ArConfig_create(m_NativeApi.SessionHandle, ref configHandle);
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

            ExternApi.ArConfig_setLightEstimationMode(m_NativeApi.SessionHandle, configHandle, lightingMode);

            var planeFindingMode = ApiPlaneFindingMode.Disabled;
            if (arCoreSessionConfig.EnablePlaneFinding)
            {
                planeFindingMode = ApiPlaneFindingMode.Horizontal;
            }

            ExternApi.ArConfig_setPlaneFindingMode(m_NativeApi.SessionHandle, configHandle, planeFindingMode);

            var updateMode = ApiUpdateMode.LatestCameraImage;
            if (arCoreSessionConfig.MatchCameraFramerate)
            {
               updateMode = ApiUpdateMode.Blocking;

               // Set vSyncCount to 0 so frame in rendered only when we have a new background texture.
               QualitySettings.vSyncCount = 0;
            }

            ExternApi.ArConfig_setUpdateMode(m_NativeApi.SessionHandle, configHandle, updateMode);
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_create(IntPtr session, ref IntPtr out_config);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_destroy(IntPtr config);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setLightEstimationMode(IntPtr session, IntPtr config,
                ApiLightEstimationMode light_estimation_mode);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setPlaneFindingMode(IntPtr session, IntPtr config,
                ApiPlaneFindingMode plane_finding_mode);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setUpdateMode(IntPtr session, IntPtr config,
                ApiUpdateMode update_mode);
        }
    }
}
