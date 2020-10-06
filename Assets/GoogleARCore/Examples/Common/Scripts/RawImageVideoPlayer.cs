// <copyright file="RawImageVideoPlayer.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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

namespace GoogleARCore.Examples.Common
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;

    /// <summary>
    /// Helper class that plays a video on a RawImage texture.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(VideoPlayer))]
    public class RawImageVideoPlayer : MonoBehaviour
    {
        /// <summary>
        /// The raw image where the video will be played.
        /// </summary>
        public RawImage RawImage;

        /// <summary>
        /// The video player component to be played.
        /// </summary>
        public VideoPlayer VideoPlayer;

        private Texture _rawImageTexture;

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            VideoPlayer.enabled = false;
            _rawImageTexture = RawImage.texture;
            VideoPlayer.prepareCompleted += PrepareCompleted;
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (!Session.Status.IsValid() || Session.Status.IsError())
            {
                VideoPlayer.Stop();
                return;
            }

            if (RawImage.enabled && !VideoPlayer.enabled)
            {
                VideoPlayer.enabled = true;
                VideoPlayer.Play();
            }
            else if (!RawImage.enabled && VideoPlayer.enabled)
            {
                // Stop video playback to save power usage.
                VideoPlayer.Stop();
                RawImage.texture = _rawImageTexture;
                VideoPlayer.enabled = false;
            }
        }

        private void PrepareCompleted(VideoPlayer player)
        {
            RawImage.texture = player.texture;
        }
    }
}
