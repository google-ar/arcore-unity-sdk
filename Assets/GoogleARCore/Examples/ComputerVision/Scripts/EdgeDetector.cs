//-----------------------------------------------------------------------
// <copyright file="EdgeDetector.cs" company="Google">
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
namespace GoogleARCore.TextureReader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Detects edges from input grayscale image.
    /// </summary>
    public class EdgeDetector
    {
        private static byte[] s_ImageBuffer = new byte[0];
        private static int s_ImageBufferSize = 0;

        /// <summary>
        /// Detects edges from input grayscale image.
        /// </summary>
        /// <param name="outputImage">Output image buffer, which has a size of width * height.</param>
        /// <param name="pixelBuffer">Pointer to raw image buffer, assuming one byte per pixel.</param>
        /// <param name="width">Width of the input image, in pixels.</param>
        /// <param name="height">Height of the input image, in pixels.</param>
        /// <returns>False if the outputImage buffer is too small, True otherwise.</returns>
        public static bool Detect(byte[] outputImage, IntPtr pixelBuffer, int width, int height)
        {
            if (outputImage.Length < width * height)
            {
                Debug.Log("Input buffer is too small!");
                return false;
            }

            Sobel(outputImage, pixelBuffer, width, height);

            return true;
        }

        private static void Sobel(byte[] outputImage, IntPtr inputImage, int width, int height)
        {
            // Adjust buffer size if necessary.
            int bufferSize = width * height;
            if (bufferSize != s_ImageBufferSize || s_ImageBuffer.Length == 0)
            {
                s_ImageBufferSize = bufferSize;
                s_ImageBuffer = new byte[bufferSize];
            }

            // Move raw data into managed buffer.
            System.Runtime.InteropServices.Marshal.Copy(inputImage, s_ImageBuffer, 0, bufferSize);

            // Detect edges.
            int threshold = 128 * 128;
            
            for (int j = 1; j < height - 1; j++)
            {
                for (int i = 1; i < width - 1; i++)
                {
                    // Offset of the pixel at [i, j] of the input image.
                    int offset = (j * width) + i;

                    // Neighbour pixels around the pixel at [i, j].
                    int a00 = s_ImageBuffer[offset - width - 1];
                    int a01 = s_ImageBuffer[offset - width];
                    int a02 = s_ImageBuffer[offset - width + 1];
                    int a10 = s_ImageBuffer[offset - 1];
                    int a12 = s_ImageBuffer[offset + 1];
                    int a20 = s_ImageBuffer[offset + width - 1];
                    int a21 = s_ImageBuffer[offset + width];
                    int a22 = s_ImageBuffer[offset + width + 1];

                    // Sobel X filter:
                    //   -1, 0, 1, 
                    //   -2, 0, 2, 
                    //   -1, 0, 1 
                    int xSum = -a00 - (2 * a10) - a20 + a02 + (2 * a12) + a22;

                    // Sobel Y filter:
                    //    1, 2, 1, 
                    //    0, 0, 0, 
                    //   -1, -2, -1 
                    int ySum = a00 + (2 * a01) + a02 - a20 - (2 * a21) - a22;
                    
                    if ((xSum * xSum) + (ySum * ySum) > threshold)
                    {
                        outputImage[(j * width) + i] = 0xFF;
                    }
                    else
                    {
                        outputImage[(j * width) + i] = 0x1F;
                    }
                }
            }
        }
    }
}
