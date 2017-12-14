//-----------------------------------------------------------------------
// <copyright file="CameraMetadataManager.cs" company="Google">
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
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class CameraMetadataManager
    {
        private NativeApi m_NativeApi;

        private IntPtr m_ArCameraMetadataHandle = IntPtr.Zero;

        public CameraMetadataManager(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public void UpdateFrame(IntPtr frameHandle)
        {
            if (m_ArCameraMetadataHandle != IntPtr.Zero)
            {
                // After first frame, release previous frame's image metadata.
                m_NativeApi.CameraMetadata.Release(m_ArCameraMetadataHandle);
                m_ArCameraMetadataHandle = IntPtr.Zero;
            }

            m_ArCameraMetadataHandle = m_NativeApi.Frame.AcquireImageMetadata(frameHandle);
        }

        public bool TryGetValues(CameraMetadataTag metadataTag, List<CameraMetadataValue> outMetadataList)
        {
            if (m_ArCameraMetadataHandle == IntPtr.Zero)
            {
                // Intentionally ignored logging as the camera metadata is expected to be
                // unavailable in between frames.
                return false;
            }

            return m_NativeApi.CameraMetadata.TryGetValues(m_ArCameraMetadataHandle,
                metadataTag, outMetadataList);
        }

        public bool GetAllCameraMetadataTags(List<CameraMetadataTag> outMetadataTags)
        {
            return m_NativeApi.CameraMetadata.GetAllCameraMetadataTags(m_ArCameraMetadataHandle, 
                outMetadataTags);
        }
    }
}
