//-----------------------------------------------------------------------
// <copyright file="ApiConstants.cs" company="Google">
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
    public static class ApiConstants
    {
#if !UNITY_EDITOR
        public const string ARCoreNativeApi = "arcore_sdk_c";
        public const string ARCoreShimApi = "arcore_unity_api";
        public const string MediaNdk = "mediandk";
#else
        public const string ARCoreNativeApi = InstantPreviewManager.InstantPreviewNativeApi;
        public const string ARCoreShimApi = InstantPreviewManager.InstantPreviewNativeApi;
        public const string MediaNdk = InstantPreviewManager.InstantPreviewNativeApi;
#endif

        // NDK camera API is a system API after Android 24.
        public const string NdkCameraApi = "camera2ndk";
    }
}
