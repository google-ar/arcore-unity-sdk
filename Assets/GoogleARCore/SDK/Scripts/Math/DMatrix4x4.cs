//-----------------------------------------------------------------------
// <copyright file="DMatrix4x4.cs" company="Google">
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
    using System.Collections;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Double percision matrix class.
    /// </summary>
    public struct DMatrix4x4
    {
        // Column major matrix.
        // Row is first number, column is the second number.
        public double m00;
        public double m10;
        public double m20;
        public double m30;
        public double m01;
        public double m11;
        public double m21;
        public double m31;
        public double m02;
        public double m12;
        public double m22;
        public double m32;
        public double m03;
        public double m13;
        public double m23;
        public double m33;

        /// <summary>
        /// Element wise construction.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules",
                                                         "SA1107:CodeMustNotContainMultipleStatementsOnOneLine",
                                                         Justification = "Matrix layout.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules",
                                                         "SA1611:ElementParametersMustBeDocumented",
                                                         Justification = "Matrix layout.")]
        public DMatrix4x4(double m00, double m01, double m02, double m03,
                          double m10, double m11, double m12, double m13,
                          double m20, double m21, double m22, double m23,
                          double m30, double m31, double m32, double m33)
        {
            this.m00 = m00; this.m01 = m01; this.m02 = m02; this.m03 = m03;
            this.m10 = m10; this.m11 = m11; this.m12 = m12; this.m13 = m13;
            this.m20 = m20; this.m21 = m21; this.m22 = m22; this.m23 = m23;
            this.m30 = m30; this.m31 = m31; this.m32 = m32; this.m33 = m33;
        }

        /// <summary>
        /// Creates a new double-precision matrix from the given
        /// single-precision matrix.
        /// </summary>
        /// <param name="matrix">A single-precision matrix.</param>
        public DMatrix4x4(Matrix4x4 matrix)
        {
            m00 = matrix.m00;
            m10 = matrix.m10;
            m20 = matrix.m20;
            m30 = matrix.m30;

            m01 = matrix.m01;
            m11 = matrix.m11;
            m21 = matrix.m21;
            m31 = matrix.m31;

            m02 = matrix.m02;
            m12 = matrix.m12;
            m22 = matrix.m22;
            m32 = matrix.m32;

            m03 = matrix.m03;
            m13 = matrix.m13;
            m23 = matrix.m23;
            m33 = matrix.m33;
        }

        /// <summary>
        /// Construct matrix from array.
        /// </summary>
        /// <param name="arr">Array of matrix's elements.</param>
        public DMatrix4x4(double[] arr)
        {
            m00 = arr[0];
            m10 = arr[1];
            m20 = arr[2];
            m30 = arr[3];

            m01 = arr[4];
            m11 = arr[5];
            m21 = arr[6];
            m31 = arr[7];

            m02 = arr[8];
            m12 = arr[9];
            m22 = arr[10];
            m32 = arr[11];

            m03 = arr[12];
            m13 = arr[13];
            m23 = arr[14];
            m33 = arr[15];
        }

        /// <summary>
        /// Gets identity matrix.
        /// </summary>
        /// <value>DMatrix4x4 value.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules",
                                                         "SA1107:CodeMustNotContainMultipleStatementsOnOneLine",
                                                         Justification = "Matrix layout.")]
        public static DMatrix4x4 Identity
        {
            get
            {
                return new DMatrix4x4
                {
                    m00 = 1.0, m01 = 0.0, m02 = 0.0, m03 = 0.0,
                    m10 = 0.0, m11 = 1.0, m12 = 0.0, m13 = 0.0,
                    m20 = 0.0, m21 = 0.0, m22 = 1.0, m23 = 0.0,
                    m30 = 0.0, m31 = 0.0, m32 = 0.0, m33 = 1.0
                };
            }
        }

        /// <summary>
        /// Gets inverse matrix.
        /// </summary>
        /// <value>DMatrix4x4 value.</value>
        public DMatrix4x4 Inverse
        {
            get
            {
                // Invert a 4 x 4 matrix using Cramer's Rule
                DMatrix4x4 ret = new DMatrix4x4();

                // transpose matrix
                double src0  = m00;
                double src4  = m10;
                double src8  = m20;
                double src12 = m30;

                double src1  = m01;
                double src5  = m11;
                double src9  = m21;
                double src13 = m31;

                double src2  = m02;
                double src6  = m12;
                double src10 = m22;
                double src14 = m32;

                double src3  = m03;
                double src7  = m13;
                double src11 = m23;
                double src15 = m33;

                // calculate pairs for first 8 elements (cofactors)
                double atmp0  = src10 * src15;
                double atmp1  = src11 * src14;
                double atmp2  = src9  * src15;
                double atmp3  = src11 * src13;
                double atmp4  = src9  * src14;
                double atmp5  = src10 * src13;
                double atmp6  = src8  * src15;
                double atmp7  = src11 * src12;
                double atmp8  = src8  * src14;
                double atmp9  = src10 * src12;
                double atmp10 = src8  * src13;
                double atmp11 = src9  * src12;

                // calculate first 8 elements (cofactors)
                double dst0  = ((atmp0 * src5) + (atmp3 * src6) + (atmp4 * src7)) -
                    ((atmp1 * src5) + (atmp2 * src6) + (atmp5 * src7));
                double dst1  = ((atmp1 * src4) + (atmp6 * src6) + (atmp9 * src7))
                    - ((atmp0 * src4) + (atmp7 * src6) + (atmp8 * src7));
                double dst2  = ((atmp2 * src4) + (atmp7 * src5) + (atmp10 * src7))
                    - ((atmp3 * src4) + (atmp6 * src5) + (atmp11 * src7));
                double dst3  = ((atmp5 * src4) + (atmp8 * src5) + (atmp11 * src6))
                    - ((atmp4 * src4) + (atmp9 * src5) + (atmp10 * src6));
                double dst4  = ((atmp1 * src1) + (atmp2 * src2) + (atmp5  * src3))
                    - ((atmp0 * src1) + (atmp3 * src2) + (atmp4  * src3));
                double dst5  = ((atmp0 * src0) + (atmp7 * src2) + (atmp8  * src3))
                    - ((atmp1 * src0) + (atmp6 * src2) + (atmp9  * src3));
                double dst6  = ((atmp3 * src0) + (atmp6 * src1) + (atmp11 * src3))
                    - ((atmp2 * src0) + (atmp7 * src1) + (atmp10 * src3));
                double dst7  = ((atmp4 * src0) + (atmp9 * src1) + (atmp10 * src2))
                    - ((atmp5 * src0) + (atmp8 * src1) + (atmp11 * src2));

                // calculate pairs for second 8 elements (cofactors)
                double btmp0  = src2 * src7;
                double btmp1  = src3 * src6;
                double btmp2  = src1 * src7;
                double btmp3  = src3 * src5;
                double btmp4  = src1 * src6;
                double btmp5  = src2 * src5;
                double btmp6  = src0 * src7;
                double btmp7  = src3 * src4;
                double btmp8  = src0 * src6;
                double btmp9  = src2 * src4;
                double btmp10 = src0 * src5;
                double btmp11 = src1 * src4;

                // calculate second 8 elements (cofactors)
                double dst8  = ((btmp0  * src13) + (btmp3  * src14) + (btmp4  * src15))
                    - ((btmp1  * src13) + (btmp2  * src14) + (btmp5  * src15));
                double dst9  = ((btmp1  * src12) + (btmp6 * src14) + (btmp9 * src15))
                    - ((btmp0  * src12) + (btmp7  * src14) + (btmp8  * src15));
                double dst10 = ((btmp2  * src12) + (btmp7 * src13) + (btmp10 * src15))
                    - ((btmp3  * src12) + (btmp6  * src13) + (btmp11 * src15));
                double dst11 = ((btmp5  * src12) + (btmp8 * src13) + (btmp11 * src14))
                    - ((btmp4  * src12) + (btmp9  * src13) + (btmp10 * src14));
                double dst12 = ((btmp2 * src10) + (btmp5 * src11) + (btmp1 * src9))
                    - ((btmp4  * src11) + (btmp0  * src9) + (btmp3  * src10));
                double dst13 = ((btmp8  * src11) + (btmp0 * src8) + (btmp7  * src10))
                    - ((btmp6 * src10) + (btmp9 * src11) + (btmp1 * src8));
                double dst14 = ((btmp6 * src9) + (btmp11 * src11) + (btmp3 * src8))
                    - ((btmp10 * src11) + (btmp2 * src8) + (btmp7 * src9));
                double dst15 = ((btmp10 * src10) + (btmp4 * src8) + (btmp9 * src9))
                    - ((btmp8  * src9)  + (btmp11 * src10) + (btmp5 * src8));

                // calculate determinant
                double det = (src0 * dst0) + (src1 * dst1) + (src2 * dst2) + (src3 * dst3);

                if (det == 0.0)
                {
                    ARDebug.LogError("Matrix is not invertable.");
                    return new DMatrix4x4();
                }

                // calculate matrix inverse
                double invdet = 1.0 / det;
                ret.m00 = dst0  * invdet;
                ret.m10 = dst1  * invdet;
                ret.m20 = dst2  * invdet;
                ret.m30 = dst3  * invdet;

                ret.m01 = dst4  * invdet;
                ret.m11 = dst5  * invdet;
                ret.m21 = dst6  * invdet;
                ret.m31 = dst7  * invdet;

                ret.m02 = dst8  * invdet;
                ret.m12 = dst9  * invdet;
                ret.m22 = dst10 * invdet;
                ret.m32 = dst11 * invdet;

                ret.m03 = dst12 * invdet;
                ret.m13 = dst13 * invdet;
                ret.m23 = dst14 * invdet;
                ret.m33 = dst15 * invdet;

                return ret;
            }
        }

        /// <summary>
        /// Matrix indexer based on row and colmn index.
        /// </summary>
        /// <param name="row">Row index.</param>
        /// <param name="column">Column index.</param>
        /// <returns>Specific element's value.</returns>
        public double this[int row, int column]
        {
            get
            {
                return this[row + (column * 4)];
            }

            set
            {
                this[row + (column * 4)] = value;
            }
        }

        /// <summary>
        /// Matrix indexer.
        /// </summary>
        /// <param name="index">Index number.</param>
        /// <returns>Specific index's value.</returns>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.m00;
                    case 1:
                        return this.m10;
                    case 2:
                        return this.m20;
                    case 3:
                        return this.m30;
                    case 4:
                        return this.m01;
                    case 5:
                        return this.m11;
                    case 6:
                        return this.m21;
                    case 7:
                        return this.m31;
                    case 8:
                        return this.m02;
                    case 9:
                        return this.m12;
                    case 10:
                        return this.m22;
                    case 11:
                        return this.m32;
                    case 12:
                        return this.m03;
                    case 13:
                        return this.m13;
                    case 14:
                        return this.m23;
                    case 15:
                        return this.m33;
                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        this.m00 = value;
                        break;
                    case 1:
                        this.m10 = value;
                        break;
                    case 2:
                        this.m20 = value;
                        break;
                    case 3:
                        this.m30 = value;
                        break;
                    case 4:
                        this.m01 = value;
                        break;
                    case 5:
                        this.m11 = value;
                        break;
                    case 6:
                        this.m21 = value;
                        break;
                    case 7:
                        this.m31 = value;
                        break;
                    case 8:
                        this.m02 = value;
                        break;
                    case 9:
                        this.m12 = value;
                        break;
                    case 10:
                        this.m22 = value;
                        break;
                    case 11:
                        this.m32 = value;
                        break;
                    case 12:
                        this.m03 = value;
                        break;
                    case 13:
                        this.m13 = value;
                        break;
                    case 14:
                        this.m23 = value;
                        break;
                    case 15:
                        this.m33 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
        }

        /// <summary>
        /// Create a translation and rotation matrix.
        /// </summary>
        /// <param name="translation">Translation in unity vector.</param>
        /// <param name="orientation">Orientation in unity quaternion.</param>
        /// <returns>Double matrix.</returns>
        public static DMatrix4x4 TR(Vector3 translation, Quaternion orientation)
        {
            Double[] dTranslation = new double[3];
            Double[] dOrientation = new double[4];

            dTranslation[0] = translation.x;
            dTranslation[1] = translation.y;
            dTranslation[2] = translation.z;

            dOrientation[0] = orientation.x;
            dOrientation[1] = orientation.y;
            dOrientation[2] = orientation.z;
            dOrientation[3] = orientation.w;

            return DMatrix4x4.TR(dTranslation, dOrientation);
        }

        /// <summary>
        /// Create a translation and rotation matrix.
        /// </summary>
        /// <param name="translation">Translation as 3 doubles in a DVector3 struct.</param>
        /// <param name="orientation">Orientation as 4 doubles in a DVector4 struct.</param>
        /// <returns>Double matrix.</returns>
        public static DMatrix4x4 TR(DVector3 translation, DVector4 orientation)
        {
            double[] dTranslation = new double[3];
            double[] dOrientation = new double[4];

            dTranslation[0] = translation.x;
            dTranslation[1] = translation.y;
            dTranslation[2] = translation.z;

            dOrientation[0] = orientation.x;
            dOrientation[1] = orientation.y;
            dOrientation[2] = orientation.z;
            dOrientation[3] = orientation.w;

            return DMatrix4x4.TR(dTranslation, dOrientation);
        }

        /// <summary>
        /// Multiplication operator overloading.
        /// </summary>
        /// <param name="lhs">Matrix on the left.</param>
        /// <param name="rhs">Matrix on the right.</param>
        /// <returns>Result matrix.</returns>
        public static DMatrix4x4 operator *(DMatrix4x4 lhs, DMatrix4x4 rhs)
        {
            return new DMatrix4x4
            {
                m00 = (lhs.m00 * rhs.m00) + (lhs.m01 * rhs.m10) + (lhs.m02 * rhs.m20) + (lhs.m03 * rhs.m30),
                m01 = (lhs.m00 * rhs.m01) + (lhs.m01 * rhs.m11) + (lhs.m02 * rhs.m21) + (lhs.m03 * rhs.m31),
                m02 = (lhs.m00 * rhs.m02) + (lhs.m01 * rhs.m12) + (lhs.m02 * rhs.m22) + (lhs.m03 * rhs.m32),
                m03 = (lhs.m00 * rhs.m03) + (lhs.m01 * rhs.m13) + (lhs.m02 * rhs.m23) + (lhs.m03 * rhs.m33),
                m10 = (lhs.m10 * rhs.m00) + (lhs.m11 * rhs.m10) + (lhs.m12 * rhs.m20) + (lhs.m13 * rhs.m30),
                m11 = (lhs.m10 * rhs.m01) + (lhs.m11 * rhs.m11) + (lhs.m12 * rhs.m21) + (lhs.m13 * rhs.m31),
                m12 = (lhs.m10 * rhs.m02) + (lhs.m11 * rhs.m12) + (lhs.m12 * rhs.m22) + (lhs.m13 * rhs.m32),
                m13 = (lhs.m10 * rhs.m03) + (lhs.m11 * rhs.m13) + (lhs.m12 * rhs.m23) + (lhs.m13 * rhs.m33),
                m20 = (lhs.m20 * rhs.m00) + (lhs.m21 * rhs.m10) + (lhs.m22 * rhs.m20) + (lhs.m23 * rhs.m30),
                m21 = (lhs.m20 * rhs.m01) + (lhs.m21 * rhs.m11) + (lhs.m22 * rhs.m21) + (lhs.m23 * rhs.m31),
                m22 = (lhs.m20 * rhs.m02) + (lhs.m21 * rhs.m12) + (lhs.m22 * rhs.m22) + (lhs.m23 * rhs.m32),
                m23 = (lhs.m20 * rhs.m03) + (lhs.m21 * rhs.m13) + (lhs.m22 * rhs.m23) + (lhs.m23 * rhs.m33),
                m30 = (lhs.m30 * rhs.m00) + (lhs.m31 * rhs.m10) + (lhs.m32 * rhs.m20) + (lhs.m33 * rhs.m30),
                m31 = (lhs.m30 * rhs.m01) + (lhs.m31 * rhs.m11) + (lhs.m32 * rhs.m21) + (lhs.m33 * rhs.m31),
                m32 = (lhs.m30 * rhs.m02) + (lhs.m31 * rhs.m12) + (lhs.m32 * rhs.m22) + (lhs.m33 * rhs.m32),
                m33 = (lhs.m30 * rhs.m03) + (lhs.m31 * rhs.m13) + (lhs.m32 * rhs.m23) + (lhs.m33 * rhs.m33)
            };
        }

        /// <summary>
        /// Convert from float matrix to double matrix.
        /// </summary>
        /// <param name="m">Float matrix.</param>
        /// <returns>Double matrix.</returns>
        public static DMatrix4x4 FromMatrix4x4(Matrix4x4 m)
        {
            DMatrix4x4 dm = new DMatrix4x4((double)m.m00, (double)m.m01, (double)m.m02, (double)m.m03,
                                           (double)m.m10, (double)m.m11, (double)m.m12, (double)m.m13,
                                           (double)m.m20, (double)m.m21, (double)m.m22, (double)m.m23,
                                           (double)m.m30, (double)m.m31, (double)m.m32, (double)m.m33);
            return dm;
        }

        /// <summary>
        /// Returns a column of the matrix.
        ///
        /// The i-th column is returned as a DVector4. i must be from 0 to 3 inclusive.
        /// </summary>
        /// <param name="i">Column index.</param>
        /// <returns>The i-th column as a DVector4.</returns>
        public DVector4 GetColumn(int i)
        {
            return new DVector4(this[0, i], this[1, i], this[2, i], this[3, i]);
        }

        /// <summary>
        /// Returns a row of the matrix.
        ///
        /// The i-th column is returned as a DVector4. i must be from 0 to 3 inclusive.
        /// </summary>
        /// <param name="i">Row index.</param>
        /// <returns>The i-th column as a DVector4.</returns>
        public DVector4 GetRow(int i)
        {
            return new DVector4(this[i, 0], this[i, 1], this[i, 2], this[i, 3]);
        }

        /// <summary>
        /// Transforms a position by this matrix (generic).
        ///
        /// Returns a position v transformed by the current fully arbitrary matrix. If the matrix is a regular 3D
        /// transformation matrix, it is much faster to use MultiplyPoint3x4 instead. MultiplyPoint is slower, but can
        /// handle projective transformations as well.
        /// </summary>
        /// <returns>The transformed point.</returns>
        /// <param name="v">The point to be transformed.</param>
        public DVector3 MultiplyPoint(DVector3 v)
        {
            DVector3 transformed = new DVector3(
                (this.m00 * v.x) + (this.m01 * v.y) + (this.m02 * v.z) + this.m03,
                (this.m10 * v.x) + (this.m11 * v.y) + (this.m12 * v.z) + this.m13,
                (this.m20 * v.x) + (this.m21 * v.y) + (this.m22 * v.z) + this.m23);

            double proj = (this.m30 * v.x) + (this.m31 * v.y) + (this.m32 * v.z) + this.m33;
            transformed.x /= proj;
            transformed.y /= proj;
            transformed.z /= proj;
            return transformed;
        }

        /// <summary>
        /// Transforms a position by this matrix (fast).
        ///
        /// Returns a position transformed by the current transformation matrix. This function is a faster version of
        /// MultiplyPoint; but it can only handle regular 3D transformations. MultiplyPoint is slower, but can handle
        /// projective transformations as well.
        ///
        /// </summary>
        /// <returns>The transformed position.</returns>
        /// <param name="v">The position to transform.</param>
        public DVector3 MultiplyPoint3x4(DVector3 v)
        {
            DVector3 transformed = new DVector3(
                (this.m00 * v.x) + (this.m01 * v.y) + (this.m02 * v.z) + this.m03,
                (this.m10 * v.x) + (this.m11 * v.y) + (this.m12 * v.z) + this.m13,
                (this.m20 * v.x) + (this.m21 * v.y) + (this.m22 * v.z) + this.m23);
            return transformed;
        }

        /// <summary>
        /// Convert DMatrix4x4 to unity Matrix4x4.
        /// </summary>
        /// <returns>Matrix in float.</returns>
        public Matrix4x4 ToMatrix4x4()
        {
            Matrix4x4 mat = new Matrix4x4();
            mat.m00 = (float)m00;
            mat.m01 = (float)m01;
            mat.m02 = (float)m02;
            mat.m03 = (float)m03;

            mat.m10 = (float)m10;
            mat.m11 = (float)m11;
            mat.m12 = (float)m12;
            mat.m13 = (float)m13;

            mat.m20 = (float)m20;
            mat.m21 = (float)m21;
            mat.m22 = (float)m22;
            mat.m23 = (float)m23;

            mat.m30 = (float)m30;
            mat.m31 = (float)m31;
            mat.m32 = (float)m32;
            mat.m33 = (float)m33;
            return mat;
        }

        /// <summary>
        /// Format to string for output.
        /// </summary>
        /// <returns>String of this matrix.</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}\n" +
                                 "{4}, {5}, {6}, {7}\n" +
                                 "{8}, {9}, {10}, {11}\n" +
                                 "{12}, {13}, {14}, {15}\n",
                                 m00.ToString("F3"), m01.ToString("F3"), m02.ToString("F3"), m03.ToString("F3"),
                                 m10.ToString("F3"), m11.ToString("F3"), m12.ToString("F3"), m13.ToString("F3"),
                                 m20.ToString("F3"), m21.ToString("F3"), m22.ToString("F3"), m23.ToString("F3"),
                                 m30.ToString("F3"), m31.ToString("F3"), m32.ToString("F3"), m33.ToString("F3"));
        }

        /// <summary>
        /// Convert matrix's elements to an array.
        /// </summary>
        /// <returns>Double array of matrix's elements.</returns>
        public double[] ToArray()
        {
            double[] arr = new double[16];
            for (int i = 0; i < 16; ++i)
            {
                arr[i] = this[i];
            }

            return arr;
        }

        /// <summary>
        /// Retrive translation vector from matrix.
        /// </summary>
        /// <param name="translation">Translation in vector double array, x, y, z.</param>
        /// <param name="orientation">Orientation in quaternion double array, x, y, z, w.</param>
        /// <returns>Double matrix.</returns>
        private static DMatrix4x4 TR(double[] translation, double[] orientation)
        {
            DMatrix4x4 dmat = new DMatrix4x4();
            double sqw = orientation[3] * orientation[3];
            double sqx = orientation[0] * orientation[0];
            double sqy = orientation[1] * orientation[1];
            double sqz = orientation[2] * orientation[2];

            // invs (inverse square length) is only required if quaternion is not already normalised
            double invs = 1 / (sqx + sqy + sqz + sqw);
            dmat.m00 = (sqx - sqy - sqz + sqw) * invs;
            dmat.m11 = (-sqx + sqy - sqz + sqw) * invs;
            dmat.m22 = (-sqx - sqy + sqz + sqw) * invs;

            double tmp1 = orientation[0] * orientation[1];
            double tmp2 = orientation[2] * orientation[3];
            dmat.m10 = 2.0 * (tmp1 + tmp2) * invs;
            dmat.m01 = 2.0 * (tmp1 - tmp2) * invs;

            tmp1 = orientation[0] * orientation[2];
            tmp2 = orientation[1] * orientation[3];
            dmat.m20 = 2.0 * (tmp1 - tmp2) * invs;
            dmat.m02 = 2.0 * (tmp1 + tmp2) * invs;
            tmp1 = orientation[1] * orientation[2];
            tmp2 = orientation[0] * orientation[3];
            dmat.m21 = 2.0 * (tmp1 + tmp2) * invs;
            dmat.m12 = 2.0 * (tmp1 - tmp2) * invs;

            dmat.m03 = translation[0];
            dmat.m13 = translation[1];
            dmat.m23 = translation[2];
            dmat.m33 = 1.0;

            return dmat;
        }
    }
}
