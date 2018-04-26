//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabaseEntry.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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

#if UNITY_EDITOR
        public AugmentedImageDatabaseEntry(string name, Texture2D texture, float width)
        {
            Name = name;
            TextureGUID = "";
            Width = width;
            Quality = "";
            Texture = texture;
        }


        public AugmentedImageDatabaseEntry(string name, Texture2D texture)
        {
            Name = name;
            TextureGUID = "";
            Width = 0;
            Quality = "";
            Texture = texture;
        }

        public AugmentedImageDatabaseEntry(Texture2D texture)
        {
            Name = "Unnamed";
            TextureGUID = "";
            Width = 0;
            Quality = "";
            Texture = texture;
        }

        public Texture2D Texture
        {
            get
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(TextureGUID));
            }
            set
            {
                TextureGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
            }
        }
#endif
    }
}
