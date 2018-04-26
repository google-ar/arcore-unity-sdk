//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabaseContextMenu.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using GoogleARCore;
    using UnityEditor;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public static class AugmentedImageDatabaseContextMenu
    {
        private const string k_SupportedImageFormatListMessage = "PNG and JPEG";

        private static readonly List<string> k_SupportedImageExtensions = new List<string>()
        {
            ".png", ".jpg", ".jpeg"
        };

        private static readonly List<string> k_UnsupportedImageExtensions = new List<string>()
        {
            ".psd", ".tiff", ".tga", ".gif", ".bmp", ".iff", ".pict"
        };

        [MenuItem("Assets/Create/GoogleARCore/AugmentedImageDatabase", false, 2)]
        private static void AddAssetsToNewAugmentedImageDatabase()
        {
            var selectedImagePaths = new List<string>();
            bool unsupportedImagesSelected = false;

            selectedImagePaths = _GetSelectedImagePaths(out unsupportedImagesSelected);
            if (unsupportedImagesSelected)
            {
                var message = string.Format("Some selected images could not be added to the AugmentedImageDatabase because " +
                    "they are not in a supported format.  Supported image formats are {0}.",
                    k_SupportedImageFormatListMessage);
                Debug.LogWarningFormat(message);
                EditorUtility.DisplayDialog("Unsupported Images Selected", message, "Ok");
            }

            if (selectedImagePaths.Count > 0)
            {
                var newDatabase = ScriptableObject.CreateInstance<AugmentedImageDatabase>();

                var newEntries = new List<AugmentedImageDatabaseEntry>();
                foreach (var imagePath in selectedImagePaths)
                {
                    var fileName = Path.GetFileName(imagePath);
                    var imageName = fileName.Replace(Path.GetExtension(fileName), string.Empty);
                    newEntries.Add(new AugmentedImageDatabaseEntry(imageName,
                        AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath)));
                }

                newEntries = newEntries.OrderBy(x => x.Name).ToList();

                foreach (var entry in newEntries)
                {
                    newDatabase.Add(entry);
                }

                string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (selectedPath == string.Empty)
                {
                    selectedPath = "Assets";
                }
                else if (Path.GetExtension(selectedPath) != string.Empty)
                {
                    selectedPath = selectedPath.Replace(Path.GetFileName(selectedPath), string.Empty);
                }

                var newAssetPath = AssetDatabase.GenerateUniqueAssetPath(
                    Path.Combine(selectedPath, "New Database.asset"));
                AssetDatabase.CreateAsset(newDatabase, newAssetPath);
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newDatabase;
            }
        }

        [MenuItem("Assets/Create/GoogleARCore/AugmentedImageDatabase", true, 2)]
        private static bool AddAssetsToNewAugmentedImageDatabaseValidation()
        {
            bool unsupportedSelected;
            return _GetSelectedImagePaths(out unsupportedSelected).Count > 0;
        }

        private static List<string> _GetSelectedImagePaths(out bool unsupportedImagesSelected)
        {
            var selectedImagePaths = new List<string>();

            unsupportedImagesSelected = false;
            foreach (var GUID in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(GUID);
                var extension = Path.GetExtension(path).ToLower();

                if (k_SupportedImageExtensions.Contains(extension))
                {
                    selectedImagePaths.Add(AssetDatabase.GUIDToAssetPath(GUID));
                }
                else if (k_UnsupportedImageExtensions.Contains(extension))
                {
                    unsupportedImagesSelected = true;
                }
            }

            return selectedImagePaths;
        }
    }
}
