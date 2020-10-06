//-----------------------------------------------------------------------
// <copyright file="TextureReader.cs" company="Google LLC">
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
namespace GoogleARCore.Examples.ComputerVision
{
    using System;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Component that provides CPU access to ArCore GPU texture.
    /// </summary>
    public class TextureReader : MonoBehaviour
    {
        /// <summary>
        /// Output image width, in pixels.
        /// </summary>
        public int ImageWidth = _arCoreTextureWidth;

        /// <summary>
        /// Output image height, in pixels.
        /// </summary>
        public int ImageHeight = _arCoreTextureHeight;

        /// <summary>
        /// Output image sampling option.
        /// </summary>
        public SampleMode ImageSampleMode = SampleMode.CoverFullViewport;

        /// <summary>
        /// Output image format.
        /// </summary>
        public TextureReaderApi.ImageFormatType ImageFormat =
            TextureReaderApi.ImageFormatType.ImageFormatGrayscale;

        private const int _arCoreTextureWidth = 1920;
        private const int _arCoreTextureHeight = 1080;

        private TextureReaderApi _textureReaderApi = null;

        private CommandType _command = CommandType.None;

        private int _imageBufferIndex = -1;

        /// <summary>
        /// Callback function type for receiving the output images.
        /// </summary>
        /// <param name="format">The format of the image.</param>
        /// <param name="width">The width of the image, in pixels.</param>
        /// <param name="height">The height of the image, in pixels.</param>
        /// <param name="pixelBuffer">The pointer to the raw buffer of the image pixels.</param>
        /// <param name="bufferSize">The size of the image buffer, in bytes.</param>
        public delegate void OnImageAvailableCallbackFunc(
            TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer,
            int bufferSize);

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
            /// Samples the entire texture and does not crop. The aspect ratio may be different from
            /// the texture aspect ratio.
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
            if (_textureReaderApi == null)
            {
                _textureReaderApi = new TextureReaderApi();
                _command = CommandType.Create;
                _imageBufferIndex = -1;
            }
        }

        /// <summary>
        /// This function should be called after any public property is changed.
        /// </summary>
        public void Apply()
        {
            _command = CommandType.Reset;
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
            switch (_command)
            {
            case CommandType.Create:
            {
                _textureReaderApi.Create(
                    ImageFormat, ImageWidth, ImageHeight,
                    ImageSampleMode == SampleMode.KeepAspectRatio);
                break;
            }

            case CommandType.Reset:
            {
                _textureReaderApi.ReleaseFrame(_imageBufferIndex);
                _textureReaderApi.Destroy();
                _textureReaderApi.Create(
                    ImageFormat, ImageWidth, ImageHeight,
                    ImageSampleMode == SampleMode.KeepAspectRatio);
                _imageBufferIndex = -1;
                break;
            }

            case CommandType.ReleasePreviousBuffer:
            {
                // Clear previously used buffer, and submits a new request.
                _textureReaderApi.ReleaseFrame(_imageBufferIndex);
                _imageBufferIndex = -1;
                break;
            }

            case CommandType.ProcessNextFrame:
            {
                if (_imageBufferIndex >= 0)
                {
                    // Get image pixels from previously submitted request.
                    int bufferSize = 0;
                    IntPtr pixelBuffer =
                        _textureReaderApi.AcquireFrame(_imageBufferIndex, ref bufferSize);

                    if (pixelBuffer != IntPtr.Zero && OnImageAvailableCallback != null)
                    {
                        OnImageAvailableCallback(
                            ImageFormat, ImageWidth, ImageHeight, pixelBuffer, bufferSize);
                    }

                    // Release the texture reader internal buffer.
                    _textureReaderApi.ReleaseFrame(_imageBufferIndex);
                }

                break;
            }

            case CommandType.None:
            default:
                break;
            }

            // Submit reading request for the next frame.
            if (Frame.CameraImage.Texture != null)
            {
                int textureId = Frame.CameraImage.Texture.GetNativeTexturePtr().ToInt32();
                _imageBufferIndex = _textureReaderApi.SubmitFrame(
                    textureId, _arCoreTextureWidth, _arCoreTextureHeight);
            }

            // Set next command.
            _command = CommandType.ProcessNextFrame;
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (_textureReaderApi != null)
            {
                _textureReaderApi.Destroy();
                _textureReaderApi = null;
            }
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            // Force to release previously used buffer.
            _command = CommandType.ReleasePreviousBuffer;
        }
    }
}
