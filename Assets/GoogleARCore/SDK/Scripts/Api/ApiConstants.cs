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
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    internal static class ApiConstants
    {
#if UNITY_EDITOR
        public const string ARCoreNativeApi = InstantPreviewManager.InstantPreviewNativeApi;
        public const string ARCoreARKitIntegrationApi = "NOT_AVAILABLE";
        public const string ARCoreShimApi = InstantPreviewManager.InstantPreviewNativeApi;
        public const string ARPrestoApi = InstantPreviewManager.InstantPreviewNativeApi;
        public const string MediaNdk = InstantPreviewManager.InstantPreviewNativeApi;
        public const string NdkCameraApi = "NOT_AVAILABLE";
#elif UNITY_ANDROID
        public const string ARCoreNativeApi = "arcore_sdk_c";
        public const string ARCoreARKitIntegrationApi = "NOT_AVAILABLE";
        public const string ARCoreShimApi = "arcore_unity_api";
        public const string ARPrestoApi = "arpresto_api";
        public const string MediaNdk = "mediandk";
        public const string NdkCameraApi = "camera2ndk";
#elif UNITY_IOS
#if ARCORE_IOS_SUPPORT
        public const string ARCoreNativeApi = "__Internal";
        public const string ARCoreARKitIntegrationApi = "__Internal";
#else
        public const string ARCoreNativeApi = "NOT_AVAILABLE";
        public const string ARCoreARKitIntegrationApi = "NOT_AVAILABLE";
#endif
        public const string ARCoreShimApi = "NOT_AVAILABLE";
        public const string ARPrestoApi = "NOT_AVAILABLE";
        public const string MediaNdk = "NOT_AVAILABLE";
        public const string NdkCameraApi = "NOT_AVAILABLE";
#else
        public const string ARCoreNativeApi = "NOT_AVAILABLE";
        public const string ARCoreARKitIntegrationApi = "NOT_AVAILABLE";
        public const string ARCoreShimApi = "NOT_AVAILABLE";
        public const string ARPrestoApi = "NOT_AVAILABLE";
        public const string MediaNdk = "NOT_AVAILABLE";
        public const string NdkCameraApi = "NOT_AVAILABLE";
#endif

#if UNITY_EDITOR_OSX
        public const string AugmentedImageCliBinaryName = "augmented_image_cli_osx";
#elif UNITY_EDITOR_WIN
        public const string AugmentedImageCliBinaryName = "augmented_image_cli_win";
#elif UNITY_EDITOR_LINUX
        public const string AugmentedImageCliBinaryName = "augmented_image_cli_linux";
#endif
    }
}
