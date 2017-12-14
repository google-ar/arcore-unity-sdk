//-----------------------------------------------------------------------
// <copyright file="TextureReader.cs" company="Google">
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
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// Component that provides CPU access to ArCore GPU texture.
    /// </summary>
    public class TextureReader : MonoBehaviour
    {
        /// <summary>
        /// Output image width, in pixels.
        /// </summary>
        public int ImageWidth = k_ARCoreTextureWidth;

        /// <summary>
        /// Output image height, in pixels.
        /// </summary>
        public int ImageHeight = k_ARCoreTextureHeight;
        
        /// <summary>
        /// Output image sampling option.
        /// </summary>
        public SampleMode ImageSampleMode = SampleMode.CoverFullViewport;

        /// <summary>
        /// Output image format.
        /// </summary>
        public TextureReaderApi.ImageFormatType ImageFormat = TextureReaderApi.ImageFormatType.ImageFormatGrayscale;

        private const int k_ARCoreTextureWidth = 1920;
        private const int k_ARCoreTextureHeight = 1080;

        private TextureReaderApi m_TextureReaderApi = null;

        private CommandType m_Command = CommandType.None;

        private int m_ImageBufferIndex = -1;

        /// <summary>
        /// Callback function type for receiving the output images.
        /// </summary>
        /// <param name="format">The format of the image.</param>
        /// <param name="width">The width of the image, in pixels.</param>
        /// <param name="height">The height of the image, in pixels.</param>
        /// <param name="pixelBuffer">The pointer to the raw buffer of the image pixels.</param>
        /// <param name="bufferSize">The size of the image buffer, in bytes.</param>
        public delegate void OnImageAvailableCallbackFunc(TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer, int bufferSize);

        /// <summary>
        /// Callback function handle for receiving the output images.
        /// </summary>
        public event OnImageAvailableCallbackFunc OnImageAvailableCallback = null;   

        /// <summary>
        /// Options to sample the output image.
        /// </summary>
        public enum SampleMode
        {
            /// <summary>
            /// Keeps the same aspect ratio as the GPU texture. Crop image if necessary.
            /// </summary>
            KeepAspectRatio,

            /// <summary>
            /// Samples the entire texture and does not crop. The aspect ratio may be different from the texture aspect ratio.
            /// </summary>
            CoverFullViewport
        }

        private enum CommandType
        {
            None,
            ProcessNextFrame,
            Create,
            Reset,
            ReleasePreviousBuffer
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        public void Start()
        {
            if (m_TextureReaderApi == null)
            {
                m_TextureReaderApi = new TextureReaderApi();
                m_Command = CommandType.Create;
                m_ImageBufferIndex = -1;
            }
        }

        /// <summary>
        /// This function should be called after any public property is changed.
        /// </summary>
        public void Apply()
        {
            m_Command = CommandType.Reset;
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public void Update()
        {
            if (!enabled)
            {
                return;
            }

            // Process command.
            switch (m_Command)
            {
            case CommandType.Create:
            {
                m_TextureReaderApi.Create(ImageFormat, ImageWidth, ImageHeight, ImageSampleMode == SampleMode.KeepAspectRatio);
                break;
            }

            case CommandType.Reset:
            {
                m_TextureReaderApi.ReleaseFrame(m_ImageBufferIndex);
                m_TextureReaderApi.Destroy();
                m_TextureReaderApi.Create(ImageFormat, ImageWidth, ImageHeight, ImageSampleMode == SampleMode.KeepAspectRatio);
                m_ImageBufferIndex = -1;
                break;
            }

            case CommandType.ReleasePreviousBuffer:
            {
                // Clear previously used buffer, and submits a new request.
                m_TextureReaderApi.ReleaseFrame(m_ImageBufferIndex);
                m_ImageBufferIndex = -1;
                break;
            }

            case CommandType.ProcessNextFrame:
            {
                if (m_ImageBufferIndex >= 0)
                {
                    // Get image pixels from previously submitted request.
                    int bufferSize = 0;
                    IntPtr pixelBuffer = m_TextureReaderApi.AcquireFrame(m_ImageBufferIndex, ref bufferSize);

                    if (pixelBuffer != IntPtr.Zero && OnImageAvailableCallback != null)
                    {
                        OnImageAvailableCallback(ImageFormat, ImageWidth, ImageHeight, pixelBuffer, bufferSize);
                    }

                    // Release the texture reader internal buffer.
                    m_TextureReaderApi.ReleaseFrame(m_ImageBufferIndex);
                }

                break;
            }

            case CommandType.None:
            default:
                break;
            }

            // Submit reading request for the next frame.
            int textureId = Frame.CameraImage.Texture.GetNativeTexturePtr().ToInt32();
            m_ImageBufferIndex = m_TextureReaderApi.SubmitFrame(textureId, k_ARCoreTextureWidth, k_ARCoreTextureHeight);

            // Set next command.
            m_Command = CommandType.ProcessNextFrame;
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_TextureReaderApi != null)
            {
                m_TextureReaderApi.Destroy();
                m_TextureReaderApi = null;
            }
        }
        
        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            // Force to release previously used buffer.
            m_Command = CommandType.ReleasePreviousBuffer;
        }
    }
}