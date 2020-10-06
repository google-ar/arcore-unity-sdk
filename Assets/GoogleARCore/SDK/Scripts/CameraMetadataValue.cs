//-----------------------------------------------------------------------
// <copyright file="CameraMetadataValue.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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

namespace GoogleARCore
{
    using System;
    using System.Runtime.InteropServices;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Struct to contain camera metadata's value. When querying data from the struct, caller is
    /// responsible for making sure the querying data type matches the ValueType.
    ///
    /// For example: if ValueType is typeof(byte), caller should only use
    /// CameraMetadataValue.AsByte() to access the value.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CameraMetadataValue
    {
        [FieldOffset(0)]
        private ArCameraMetadataType _type;
        [FieldOffset(4)]
        private sbyte _byteValue;
        [FieldOffset(4)]
        private int _intValue;
        [FieldOffset(4)]
        private long _longValue;
        [FieldOffset(4)]
        private float _floatValue;
        [FieldOffset(4)]
        private double _doubleValue;
        [FieldOffset(4)]
        private CameraMetadataRational _rationalValue;

        /// <summary>
        /// Constructs CameraMetadataValue using sbyte. This constructor only sets the sbyte field
        /// in the struct, leaving the rest of the data to default value.
        /// </summary>
        /// <param name="byteValue">The byte value set to the struct.</param>
        public CameraMetadataValue(sbyte byteValue)
        {
            _intValue = 0;
            _longValue = 0;
            _floatValue = 0;
            _doubleValue = 0;
            _rationalValue = new CameraMetadataRational();

            _type = ArCameraMetadataType.Byte;
            _byteValue = byteValue;
        }

        /// <summary>
        /// Constructs CameraMetadataValue using int. This constructor only sets the int field
        /// in the struct, leaving the rest of the data to default value.
        /// </summary>
        /// <param name="intValue">The int value set to the struct.</param>
        public CameraMetadataValue(int intValue)
        {
            _byteValue = 0;
            _longValue = 0;
            _floatValue = 0;
            _doubleValue = 0;
            _rationalValue = new CameraMetadataRational();

            _type = ArCameraMetadataType.Int32;
            _intValue = intValue;
        }

        /// <summary>
        /// Constructs CameraMetadataValue using long. This constructor only sets the long field
        /// in the struct, leaving the rest of the data to default value.
        /// </summary>
        /// <param name="longValue">The long value set to the struct.</param>
        public CameraMetadataValue(long longValue)
        {
            _byteValue = 0;
            _intValue = 0;
            _floatValue = 0;
            _doubleValue = 0;
            _rationalValue = new CameraMetadataRational();

            _type = ArCameraMetadataType.Int64;
            _longValue = longValue;
        }

        /// <summary>
        /// Constructs CameraMetadataValue using float. This constructor only sets the float field
        /// in the struct, leaving the rest of the data to default value.
        /// </summary>
        /// <param name="floatValue">The float value set to the struct.</param>
        public CameraMetadataValue(float floatValue)
        {
            _byteValue = 0;
            _intValue = 0;
            _longValue = 0;
            _doubleValue = 0;
            _rationalValue = new CameraMetadataRational();

            _type = ArCameraMetadataType.Float;
            _floatValue = floatValue;
        }

        /// <summary>
        /// Constructs CameraMetadataValue using double. This constructor only sets the double field
        /// in the struct, leaving the rest of the data to default value.
        /// </summary>
        /// <param name="doubleValue">The double value set to the struct.</param>
        public CameraMetadataValue(double doubleValue)
        {
            _byteValue = 0;
            _intValue = 0;
            _longValue = 0;
            _floatValue = 0;
            _rationalValue = new CameraMetadataRational();

            _type = ArCameraMetadataType.Double;
            _doubleValue = doubleValue;
        }

        /// <summary>
        /// Constructs CameraMetadataValue using CameraMetadataRational. This constructor only sets
        /// the CameraMetadataRational field in the struct, leaving the rest of the data to default
        /// value.
        /// </summary>
        /// <param name="rationalValue">The CameraMetadataRational value set to the struct.</param>
        public CameraMetadataValue(CameraMetadataRational rationalValue)
        {
            _byteValue = 0;
            _intValue = 0;
            _longValue = 0;
            _floatValue = 0;
            _doubleValue = 0;

            _type = ArCameraMetadataType.Rational;
            _rationalValue = rationalValue;
        }

        /// <summary>
        /// Gets the Type of the CameraMetadataValue. This Type must be used to call the proper
        /// query function.
        /// </summary>
        public Type ValueType
        {
            get
            {
                switch (_type)
                {
                case ArCameraMetadataType.Byte:
                    return typeof(Byte);
                case ArCameraMetadataType.Int32:
                    return typeof(int);
                case ArCameraMetadataType.Float:
                    return typeof(float);
                case ArCameraMetadataType.Int64:
                    return typeof(long);
                case ArCameraMetadataType.Double:
                    return typeof(double);
                case ArCameraMetadataType.Rational:
                    return typeof(CameraMetadataRational);
                default:
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets sbyte value from the struct. This function checks if the querying type matches the
        /// internal type field, and logs error if the types do not match.
        /// </summary>
        /// <returns>Returns sbyte value stored in the struct.</returns>
        public sbyte AsByte()
        {
            if (_type != ArCameraMetadataType.Byte)
            {
                LogError(ArCameraMetadataType.Byte);
            }

            return _byteValue;
        }

        /// <summary>
        /// Gets int value from the struct. This function checks if the querying type matches the
        /// internal type field, and logs error if the types do not match.
        /// </summary>
        /// <returns>Returns int value stored in the struct.</returns>
        public int AsInt()
        {
            if (_type != ArCameraMetadataType.Int32)
            {
                LogError(ArCameraMetadataType.Int32);
            }

            return _intValue;
        }

        /// <summary>
        /// Gets float value from the struct. This function checks if the querying type matches the
        /// internal type field, and logs error if the types do not match.
        /// </summary>
        /// <returns>Returns float value stored in the struct.</returns>
        public float AsFloat()
        {
            if (_type != ArCameraMetadataType.Float)
            {
                LogError(ArCameraMetadataType.Float);
            }

            return _floatValue;
        }

        /// <summary>
        /// Gets long value from the struct. This function checks if the querying type matches the
        /// internal type field, and logs error if the types do not match.
        /// </summary>
        /// <returns>Returns long value stored in the struct.</returns>
        public long AsLong()
        {
            if (_type != ArCameraMetadataType.Int64)
            {
                LogError(ArCameraMetadataType.Int64);
            }

            return _longValue;
        }

        /// <summary>
        /// Gets double value from the struct. This function checks if the querying type matches the
        /// internal type field, and logs error if the types do not match.
        /// </summary>
        /// <returns>Returns double value stored in the struct.</returns>
        public double AsDouble()
        {
            if (_type != ArCameraMetadataType.Double)
            {
                LogError(ArCameraMetadataType.Double);
            }

            return _doubleValue;
        }

        /// <summary>
        /// Gets CameraMetadataRational value from the struct. This function checks if the querying
        /// type matches the internal type field, and logs error if the types do not match.
        /// </summary>
        /// <returns>Returns CameraMetadataRational value stored in the struct.</returns>
        public CameraMetadataRational AsRational()
        {
            if (_type != ArCameraMetadataType.Rational)
            {
                LogError(ArCameraMetadataType.Rational);
            }

            return _rationalValue;
        }

        private void LogError(ArCameraMetadataType requestedType)
        {
            ARDebug.LogErrorFormat(
                "Error getting value from CameraMetadataType due to type mismatch. " +
                "requested type = {0}, internal type = {1}\n" +
                "Are you sure you are querying the correct type?", requestedType, _type);
        }
    }

    /// <summary>
    /// CameraMetadataRational follows the layout of ACameraMetadata_rational struct in NDK.
    /// Please refer to NdkCameraMetadata.h for documentation:
    /// https://developer.android.com/ndk/reference/ndk_camera_metadata_8h.html .
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraMetadataRational
    {
        /// <summary>
        /// The numerator of the metadata rational.
        /// </summary>
        public int Numerator;

        /// <summary>
        /// The denominator of the metadata rational.
        /// </summary>
        public int Denominator;
    }
}
