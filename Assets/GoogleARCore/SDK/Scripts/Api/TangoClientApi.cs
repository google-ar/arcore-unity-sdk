//-----------------------------------------------------------------------
// <copyright file="TangoClientApi.cs" company="Google">
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

    /// <summary>
    /// Implementation of ITangoClientApi for Android.
    /// </summary>
    public class TangoClientApi
    {
        /// <summary>
        /// Tango video overlay C callback function signature.
        /// </summary>
        /// <param name="callbackContext">The callback context.</param>
        /// <param name="tangoEvent">Tango event.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ApiOnEventAvailable(IntPtr callbackContext, ref ApiTangoEvent tangoEvent);

        /// <summary>
        /// Tango pose C callback function signature.
        /// </summary>
        /// <param name="callbackContext">Callback context.</param>
        /// <param name="pose">Pose data.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ApiOnPoseAvailable(IntPtr callbackContext, ref ApiPoseData pose);

        private const string TANGO_CLIENT_API2_DLL = "tango_client_api2";

        private static ApiOnEventAvailable m_onEventAvailable;

        private static Queue<ApiTangoEvent> m_eventQueue;

        private static object m_eventQueueLock;

        private static ApiOnPoseAvailable m_onPoseAvailable;

        private static Queue<ApiPoseData> m_poseQueue;

        private static object m_poseQueueLock;

        public static ApiServiceErrorStatus ConnectOnEventAvailable(Queue<ApiTangoEvent> eventQueue,
            object eventQueueLock)
        {
            m_onEventAvailable = new ApiOnEventAvailable(_OnEventAvailableAsync);
            m_eventQueue = eventQueue;
            m_eventQueueLock = eventQueueLock;

            return ExternApi.TangoService_connectOnTangoEvent(m_onEventAvailable);
        }

        public static ApiServiceErrorStatus ResetOnEventAvailable()
        {
            m_onEventAvailable = null;
            return ExternApi.TangoService_connectOnTangoEvent(null);
        }

        public static ApiServiceErrorStatus ConnectOnPoseAvailable(ApiCoordinateFramePair[] framePairs,
            Queue<ApiPoseData> poseQueue, object poseQueueLock)
        {
            m_onPoseAvailable = new ApiOnPoseAvailable(_OnPoseAvailableAsync);
            m_poseQueue = poseQueue;
            m_poseQueueLock = poseQueueLock;

            return ExternApi.TangoService_connectOnPoseAvailable(framePairs.Length, framePairs, m_onPoseAvailable);
        }

        public static ApiServiceErrorStatus ResetOnPoseAvailable()
        {
            m_onPoseAvailable = null;
            return ExternApi.TangoService_connectOnPoseAvailable(0, null, null);
        }

        public static ApiServiceErrorStatus TangoService_Experimental_getPlanes(ref IntPtr planes, ref int planeCount)
        {
            return ExternApi.TangoService_Experimental_getPlanes(ref planes, ref planeCount);
        }

        public static ApiServiceErrorStatus TangoPlaneData_free(IntPtr planes, int planeCount)
        {
            return ExternApi.TangoPlaneData_free(planes, planeCount);
        }

        public static ApiServiceErrorStatus TangoService_getPixelIntensity(
            IntPtr yImage, int width, int height, int rowStride, out float value)
        {
            return ExternApi.TangoService_getPixelIntensity(yImage, width, height, rowStride,
                out value);
        }

        public static ApiServiceErrorStatus TangoService_getLuminance(
            long exposureDurationNs, int sensitivityIso, float lensAperature,
            out float value)
        {
            return ExternApi.TangoService_getLuminance(exposureDurationNs, sensitivityIso,
                lensAperature, out value);
        }

        public static ApiServiceErrorStatus TangoService_IsSupported(
            out bool isSupported)
        {
            isSupported = false;
            return ExternApi.TangoService_isSupported(ref isSupported);
        }

        public static bool CallsToConnectWithoutMatchingDisconnect()
        {
            return ExternApi.CallsToConnectWithoutMatchingDisconnect();
        }

        [AOT.MonoPInvokeCallback(typeof(ApiOnEventAvailable))]
        private static void _OnEventAvailableAsync(IntPtr callbackContext, ref ApiTangoEvent tangoEvent)
        {
            lock (m_eventQueueLock)
            {
                m_eventQueue.Enqueue(tangoEvent);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(ApiOnPoseAvailable))]
        private static void _OnPoseAvailableAsync(IntPtr callbackContext, ref ApiPoseData pose)
        {
            lock (m_poseQueueLock)
            {
                m_poseQueue.Enqueue(pose);
            }
        }

        private struct ExternApi
        {
            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern ApiServiceErrorStatus TangoService_connectOnTangoEvent(ApiOnEventAvailable callback);

            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern ApiServiceErrorStatus TangoService_connectOnPoseAvailable(int count,
                ApiCoordinateFramePair[] framePairs, ApiOnPoseAvailable callback);

            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern ApiServiceErrorStatus TangoService_Experimental_getPlanes(ref IntPtr planes,
                ref int planeCount);

            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern ApiServiceErrorStatus TangoPlaneData_free(IntPtr planes, int planeCount);

            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern ApiServiceErrorStatus TangoService_getPixelIntensity(
                IntPtr yImage, int width, int height, int rowStride, out float value);

            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern ApiServiceErrorStatus TangoService_getLuminance(
                long exposureDurationNs, int sensitivityIso, float lensAperature,
                out float value);

            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern ApiServiceErrorStatus TangoService_isSupported(
                ref bool isSupported);

            [DllImport(TANGO_CLIENT_API2_DLL)]
            public static extern bool CallsToConnectWithoutMatchingDisconnect();
        }
    }
}
