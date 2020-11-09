//-----------------------------------------------------------------------
// <copyright file="RecordingConfigApi.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using System.Runtime.InteropServices;
    using GoogleARCore;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class RecordingConfigApi
    {
        private NativeSession _nativeSession;

        public RecordingConfigApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public IntPtr Create(ARCoreRecordingConfig config)
        {
            IntPtr configHandle = IntPtr.Zero;
            ExternApi.ArRecordingConfig_create(
                _nativeSession.SessionHandle, ref configHandle);

            if (config != null)
            {
                ExternApi.ArRecordingConfig_setMp4DatasetFilePath(
                    _nativeSession.SessionHandle,
                    configHandle,
                    config.Mp4DatasetFilepath);
                ExternApi.ArRecordingConfig_setAutoStopOnPause(
                    _nativeSession.SessionHandle,
                    configHandle,
                    config.AutoStopOnPause ? 1 : 0);
            }

            return configHandle;
        }

        public void Destory(IntPtr recordingConfigHandle)
        {
            ExternApi.ArRecordingConfig_destroy(recordingConfigHandle);
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_create(
                IntPtr session, ref IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_destroy(
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_setMp4DatasetFilePath(
                IntPtr session, IntPtr configHandle, String datasetPath);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArRecordingConfig_setAutoStopOnPause(
                IntPtr session, IntPtr configHandle, int configEnabled);
#pragma warning restore 626
        }
    }
}
