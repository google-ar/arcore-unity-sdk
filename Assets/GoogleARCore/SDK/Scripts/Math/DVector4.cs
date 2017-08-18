//-----------------------------------------------------------------------
// <copyright file="DVector4.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
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
    using UnityEngine;

    /// <summary>
    /// Double precision vector in 4D.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DVector4
    {
        [MarshalAs(UnmanagedType.R8)]
        public double x;
        [MarshalAs(UnmanagedType.R8)]
        public double y;
        [MarshalAs(UnmanagedType.R8)]
        public double z;
        [MarshalAs(UnmanagedType.R8)]
        public double w;

        /// <summary>
        /// Creates a new double-precision vector with given x, y, z, w components.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        /// <param name="w">The w component.</param>
        public DVector4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Creates a new double-precision vector with given Vector4.
        /// </summary>
        /// <param name="copy">The Vector4 to copy.</param>
        public DVector4(UnityEngine.Vector4 copy)
        {
            this.x = copy.x;
            this.y = copy.y;
            this.z = copy.z;
            this.w = copy.w;
        }

        /// <summary>
        /// Gets identity quaternion in DVector4 format.
        /// </summary>
        /// <value>Identity quaternion.</value>
        public static DVector4 IdentityQuaternion
        {
            get { return new DVector4(0.0, 0.0, 0.0, 1.0); }
        }

        /// <summary>
        /// Gets the length of this vector (Read Only).
        /// </summary>
        /// <value>The length.</value>
        public double Magnitude
        {
            get { return Math.Sqrt(SqrMagnitude); }
        }

        /// <summary>
        /// Gets the squared length of this vector (Read Only).
        /// </summary>
        /// <value>The squared length.</value>
        public double SqrMagnitude
        {
            get { return (x * x) + (y * y) + (z * z) + (w * w); }
        }

        /// <summary>
        /// Get or set x,y,z,w components (double) as 0,1,2,3 - other values throw an IndexOutOfRange exception.
        /// </summary>
        /// <param name="index">Set a component of the quaternion by int index.</param>
        /// <returns>
        /// A <see cref="System.Double"/> in the quaternion.
        /// </returns>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        return;
                    case 1:
                        y = value;
                        return;
                    case 2:
                        z = value;
                        return;
                    case 3:
                        w = value;
                        return;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Returns the distance between a and b.
        /// </summary>
        /// <returns>Euclidean distance as a double.</returns>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        public static double Distance(DVector4 a, DVector4 b)
        {
            return (a - b).Magnitude;
        }

        /// <summary>
        /// Dot Product of two vectors.
        /// </summary>
        /// <returns>Dot product as a double.</returns>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        public static double Dot(DVector4 a, DVector4 b)
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z) + (a.w * b.w);
        }

        /// <summary>
        /// Subtracts one vector from another.
        ///
        /// Subtracts each component of b from a.
        /// </summary>
        /// <returns>Subtraction result.</returns>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        public static DVector4 operator -(DVector4 a, DVector4 b)
        {
            return new DVector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        /// <summary>
        /// Adds two vectors.
        ///
        /// Adds corresponding components together.
        /// </summary>
        /// <returns>Addition result.</returns>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        public static DVector4 operator +(DVector4 a, DVector4 b)
        {
            return new DVector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        /// <summary>
        /// Returns a single-precision quaternion representation of this
        /// double-precision vector.
        /// </summary>
        /// <returns>A single-precision vector.</returns>
        public Quaternion ToQuaternion()
        {
            return new Quaternion((float)x, (float)y, (float)z, (float)w);
        }

        /// <summary>
        /// Overrided ToString function to formatted DVector4 string output.
        /// </summary>
        /// <returns>Formatted string of this DVector4.</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}", x, y, z, w);
        }
    }
}
