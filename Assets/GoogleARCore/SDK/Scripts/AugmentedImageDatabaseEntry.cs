//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabaseEntry.cs" company="Google">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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
    using System.IO;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// An entry in a AugmentedImageDatabase.
    /// </summary>
    [Serializable]
    public struct AugmentedImageDatabaseEntry
    {
        /// <summary>
        /// The name assigned to the tracked image.
        /// </summary>
        public string Name;

        /// <summary>
        /// The width of the image in meters.
        /// </summary>
        public float Width;

        /// <summary>
        /// The quality of the image.
        /// </summary>
        public string Quality;

        /// <summary>
        /// The Unity GUID for this entry.
        /// </summary>
        public string TextureGUID;

        /// <summary>
        /// The last modified time for this entry.
        /// </summary>
        public string LastModifiedTime;

        /// <summary>
        /// Constructs a new Augmented Image database entry.
        /// </summary>
        /// <param name="name">The image name.</param>
        /// <param name="width">The image width in meters or 0 if the width is unknown.</param>
        public AugmentedImageDatabaseEntry(string name, float width)
        {
            Name = name;
            TextureGUID = string.Empty;
            Width = width;
            LastModifiedTime = string.Empty;
            Quality = string.Empty;
        }

#if UNITY_EDITOR
        public AugmentedImageDatabaseEntry(string name, Texture2D texture, float width)
        {
            Name = name;
            TextureGUID = string.Empty;
            Width = width;
            Quality = string.Empty;
            LastModifiedTime = string.Empty;
            Texture = texture;
        }

        public AugmentedImageDatabaseEntry(string name, Texture2D texture)
        {
            Name = name;
            TextureGUID = string.Empty;
            Width = 0;
            Quality = string.Empty;
            LastModifiedTime = string.Empty;
            Texture = texture;
        }

        public AugmentedImageDatabaseEntry(Texture2D texture)
        {
            Name = "Unnamed";
            TextureGUID = string.Empty;
            Width = 0;
            Quality = string.Empty;
            LastModifiedTime = string.Empty;
            Texture = texture;
        }

        public Texture2D Texture
        {
            get
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(TextureGUID);
                CheckLastModifiedTime(assetPath);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }

            set
            {
                string assetPath = AssetDatabase.GetAssetPath(value);
                CheckLastModifiedTime(assetPath);
                TextureGUID = AssetDatabase.AssetPathToGUID(assetPath);
            }
        }

        public void CheckLastModifiedTime(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            string lastModifiedTime = File.GetLastWriteTime(assetPath).ToString();
            if (!lastModifiedTime.Equals(LastModifiedTime))
            {
                Quality = string.Empty;
                LastModifiedTime = lastModifiedTime;
            }
        }
#endif
    }
}
