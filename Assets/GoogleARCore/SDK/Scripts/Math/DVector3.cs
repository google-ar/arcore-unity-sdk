//-----------------------------------------------------------------------
// <copyright file="DVector3.cs" company="Google">
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

namespace GoogleARCore
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Double precision vector in 3D.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DVector3
    {
        [MarshalAs(UnmanagedType.R8)]
        public double x;
        [MarshalAs(UnmanagedType.R8)]
        public double y;
        [MarshalAs(UnmanagedType.R8)]
        public double z;

        /// <summary>
        /// Creates a new double-precision vector with given x, y, z components.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        public DVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Creates a new double-precision vector from a given Vector3.
        /// </summary>
        /// <param name="copy">The Vector3 to copy.</param>
        public DVector3(Vector3 copy)
        {
            this.x = copy.x;
            this.y = copy.y;
            this.z = copy.z;
        }

        /// <summary>
        /// Gets zero DVector3.
        /// </summary>
        /// <value>Zero DVector3.</value>
        public static DVector3 Zero
        {
            get { return new DVector3(0.0, 0.0, 0.0); }
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
            get { return (x * x) + (y * y) + (z * z); }
        }

        /// <summary>
        /// Get or set x,y,z components (double) as 0,1,2 - other values throw an IndexOutOfRange exception.
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
        public static double Distance(DVector3 a, DVector3 b)
        {
            return (a - b).Magnitude;
        }

        /// <summary>
        /// Dot Product of two vectors.
        /// </summary>
        /// <returns>Dot product as a double.</returns>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        public static double Dot(DVector3 a, DVector3 b)
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
        }

        /// <summary>
        /// Subtracts one vector from another.
        ///
        /// Subtracts each component of b from a.
        /// </summary>
        /// <returns>Subtraction result.</returns>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        public static DVector3 operator -(DVector3 a, DVector3 b)
        {
            return new DVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        /// <summary>
        /// Adds two vectors.
        ///
        /// Adds corresponding components together.
        /// </summary>
        /// <returns>Addition result.</returns>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        public static DVector3 operator +(DVector3 a, DVector3 b)
        {
            return new DVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        /// Returns a single-precision vector representation of this
        /// double-precision vector.
        /// </summary>
        /// <returns>A single-precision vector.</returns>
        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>
        /// Overrided ToString function to formatted DVector3 string output.
        /// </summary>
        /// <returns>Formatted string of this DVector3.</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", x, y, z);
        }
    }
}
