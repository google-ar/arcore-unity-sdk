//-----------------------------------------------------------------------
// <copyright file="ApiCameraMetadata.cs" company="Google">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules",
 "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Internal.")]

namespace GoogleARCoreInternal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal enum NdkCameraMetadataType
    {
        Byte = 0,
        Int32 = 1,
        Float = 2,
        Int64 = 3,
        Double = 4,
        Rational = 5,
        NumTypes,
    }

    internal enum NdkCameraStatus
    {
        Ok = 0,
        ErrorBase = -10000,
        ErrorUnknown = ErrorBase,
        ErrorInvalidParameter = ErrorBase - 1,
        ErrorMetadataNotFound = ErrorBase - 4,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct NdkCameraMetadata
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I4)]
        public int Tag;

        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.I1)]
        public NdkCameraMetadataType Type;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.I4)]
        public int Count;

        [FieldOffset(12)]
        public IntPtr Value;
    }
}
