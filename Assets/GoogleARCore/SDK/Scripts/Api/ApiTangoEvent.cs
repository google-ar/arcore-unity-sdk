//-----------------------------------------------------------------------
// <copyright file="ApiTangoEvent.cs" company="Google">
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
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ApiTangoEvent
    {
        /// <summary>
        /// Timestamp, in seconds, of the event.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double timestamp;

        /// <summary>
        /// Type of event.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public ApiTangoEventType type;

        /// <summary>
        /// Description of the event key.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string key;

        /// <summary>
        /// Description of the event value.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string value;
    }
}
