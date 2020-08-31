//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabase.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using GoogleARCoreInternal;
    using UnityEngine;
    using UnityEngine.Serialization;

#if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
#endif

    /// <summary>
    /// A database storing a list of images to be detected and tracked by ARCore.
    ///
    /// An image database supports up to 1000 images. Only one image database can be in use at any
    /// given time.
    /// </summary>
    public class AugmentedImageDatabase : ScriptableObject
    {
        private IntPtr _arAugmentedImageDatabase = IntPtr.Zero;

        [FormerlySerializedAs("m_Images")]
        [SerializeField]
        private List<AugmentedImageDatabaseEntry> _images =
            new List<AugmentedImageDatabaseEntry>();

        [SuppressMessage("UnityRules.UnityStyleRules", "CS0169:FieldIsNeverUsedIssue",
         Justification = "Used in editor.")]
        [FormerlySerializedAs("m_RawData")]
        [SerializeField]
        private byte[] _rawData = null;

        // Fixes unused variable warning when not in editor.
#pragma warning disable 414
        [FormerlySerializedAs("m_IsRawDataDirty")]
        [SerializeField]
        private bool _isRawDataDirty = true;

        [FormerlySerializedAs("m_CliVersion")]
        [SerializeField]
        private string _cliVersion = string.Empty;
#pragma warning restore 414

        /// <summary>
        /// Constructs a new <c>AugmentedImageDatabase</c>.
        /// </summary>
        public AugmentedImageDatabase()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Gets the number of images in the database.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_images)
                {
                    return _images.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the AugmentedImageDatabase is dirty and has to be reset
        /// in ArCore.
        /// </summary>
        internal bool _isDirty { get; private set; }

        /// <summary>
        /// Gets the native handle for an associated ArAugmentedImageDatabase.
        /// </summary>
        internal IntPtr _nativeHandle
        {
            get
            {
                if (_arAugmentedImageDatabase == IntPtr.Zero)
                {
                    var nativeSession = LifecycleManager.Instance.NativeSession;
                    if (nativeSession == null || InstantPreviewManager.IsProvidingPlatform)
                    {
                        return IntPtr.Zero;
                    }

                    _arAugmentedImageDatabase =
                        nativeSession.AugmentedImageDatabaseApi.Create(_rawData);
                }

                _isDirty = false;
                return _arAugmentedImageDatabase;
            }

            private set
            {
                _arAugmentedImageDatabase = value;
            }
        }

        /// <summary>
        /// Gets or sets the image at the specified <c>index</c>.
        ///
        /// You can only modify the database in the Unity editor.
        /// </summary>
        /// <param name="index">The zero-based index of the image entry to get or set.</param>
        /// <returns>The image entry at <c>index</c>.</returns>
        public AugmentedImageDatabaseEntry this[int index]
        {
            get
            {
                lock (_images)
                {
                    return _images[index];
                }
            }

#if UNITY_EDITOR
            set
            {
                var oldValue = _images[index];
                _images[index] = value;

                if (oldValue.TextureGUID != _images[index].TextureGUID
                    || oldValue.Name != _images[index].Name
                    || oldValue.Width != _images[index].Width)
                {
                    _isRawDataDirty = true;
                }

                EditorUtility.SetDirty(this);
            }
#endif
        }

        /// <summary>
        /// Adds an image to this database.
        ///
        /// This function takes time to perform non-trivial image processing (20ms -
        /// 30ms), and should be run on a background thread.
        /// </summary>
        /// <param name="name">The name of the image.</param>
        /// <param name="image">The image to be added.</param>
        /// <param name="width">The physical width of the image in meters, or 0 if the width is
        /// unknown.</param>
        /// <returns>The index of the added image in this database or -1 if there was an
        /// error.</returns>
        /// @deprecated Please use another 'AddImage' instead.
        [SuppressMemoryAllocationError(Reason = "Allocates memory for the image.")]
        public int AddImage(string name, Texture2D image, float width = 0)
        {
            return AddImage(name, new AugmentedImageSrc(image), width);
        }

        /// <summary>
        /// Adds an image to this database.
        ///
        /// This function takes time to perform non-trivial image processing (20ms -
        /// 30ms), and should be run on a background thread.
        /// </summary>
        /// <param name="name">The name of the image.</param>
        /// <param name="imageSrc">Source image to be added.</param>
        /// <param name="width">The physical width of the image in meters, or 0 if the width is
        /// unknown.</param>
        /// <returns>The index of the added image in this database or -1 if there was an
        /// error.</returns>
        [SuppressMemoryAllocationError(Reason = "Allocates memory for the image.")]
        public int AddImage(string name, AugmentedImageSrc imageSrc, float width = 0)
        {
            var nativeSession = LifecycleManager.Instance.NativeSession;
            if (nativeSession == null)
            {
                return -1;
            }

            int imageIndex = nativeSession.AugmentedImageDatabaseApi.AddAugmentedImageAtRuntime(
                _nativeHandle, name, imageSrc, width);

            if (imageIndex != -1)
            {
                lock (_images)
                {
                    _images.Add(new AugmentedImageDatabaseEntry(name, width));
                    _isDirty = true;
                }
            }

            return imageIndex;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Adds an image entry to the end of the database.
        /// </summary>
        /// <param name="entry">The image entry to add.</param>
        public void Add(AugmentedImageDatabaseEntry entry)
        {
            _images.Add(entry);
            _isRawDataDirty = true;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Removes an image entry at a specified zero-based index.
        /// </summary>
        /// <param name="index">The index of the image entry to remove.</param>
        public void RemoveAt(int index)
        {
            _images.RemoveAt(index);
            _isRawDataDirty = true;
            EditorUtility.SetDirty(this);
        }

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Checks if the database needs to be rebuilt.
        /// </summary>
        /// <returns><c>true</c> if the database needs to be rebuilt, <c>false</c>
        /// otherwise.</returns>
        public bool IsBuildNeeded()
        {
            return _isRawDataDirty;
        }
        /// @endcond


        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Rebuilds the database asset, if needed.
        /// </summary>
        /// <param name="error">An error string that will be set if the build was
        /// unsuccessful.</param>
        public void BuildIfNeeded(out string error)
        {
            error = "";
            if (!_isRawDataDirty)
            {
                return;
            }

            string cliBinaryPath;
            if (!FindCliBinaryPath(out cliBinaryPath))
            {
                return;
            }

            var tempDirectoryPath = FileUtil.GetUniqueTempPathInProject();
            Directory.CreateDirectory(tempDirectoryPath);
            var inputImagesFile = Path.Combine(tempDirectoryPath, "inputImages");
            string[] fileLines = new string[_images.Count];
            for (int i = 0; i < _images.Count; i++)
            {
                var imagePath = AssetDatabase.GetAssetPath(_images[i].Texture);
                StringBuilder sb = new StringBuilder();
                sb.Append(_images[i].Name).Append('|').Append(imagePath);
                if (_images[i].Width > 0)
                {
                    sb.Append('|').Append(_images[i].Width);
                }

                fileLines[i] = sb.ToString();
            }

            File.WriteAllLines(inputImagesFile, fileLines);
            var rawDatabasePath = Path.Combine(tempDirectoryPath, "out_database");
            string output;
#if !UNITY_EDITOR_WIN
            ShellHelper.RunCommand("chmod", "+x \"" + cliBinaryPath + "\"", out output, out error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning(error);
                return;
            }
#endif
            ShellHelper.RunCommand(cliBinaryPath,
                string.Format(
                    "build-db --input_image_list_path {0} --output_db_path {1}",
                    inputImagesFile, rawDatabasePath), out output, out error);
            if (!string.IsNullOrEmpty(error))
            {
                return;
            }

            _rawData = File.ReadAllBytes(rawDatabasePath + ".imgdb");
            _isRawDataDirty = false;
            EditorUtility.SetDirty(this);

            // Force a save to make certain build process will get updated asset.
            AssetDatabase.SaveAssets();

            const int BYTES_IN_KBYTE = 1024;
            Debug.LogFormat(
                "Built AugmentedImageDatabase '{0}' ({1} Images, {2} KBytes)", name, Count,
                _rawData.Length/BYTES_IN_KBYTE);

            // TODO:: Remove this log when all errors/warnings are moved to stderr for CLI tool.
            Debug.Log(output);
        }
        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Gets the image entries that require updating of the image quality score.
        /// </summary>
        /// <returns>A list of image entries that require updating of the image quality
        /// score.</returns>
        public List<AugmentedImageDatabaseEntry> GetDirtyQualityEntries()
        {
            var dirtyEntries = new List<AugmentedImageDatabaseEntry>();
            string cliBinaryPath;
            if (!FindCliBinaryPath(out cliBinaryPath))
            {
                return dirtyEntries;
            }

            string currentCliVersion;
            {
                string error;
#if !UNITY_EDITOR_WIN
                string output;
                ShellHelper.RunCommand("chmod", "+x " + cliBinaryPath, out output, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning(error);
                    return dirtyEntries;
                }
#endif
                ShellHelper.RunCommand(cliBinaryPath, "version", out currentCliVersion, out error);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning(error);
                    return dirtyEntries;
                }
            }

            bool cliUpdated = _cliVersion != currentCliVersion;
            // When CLI is updated, mark all entries dirty.
            if (cliUpdated)
            {
                for (int i = 0; i < _images.Count; ++i)
                {
                    AugmentedImageDatabaseEntry updatedImage = _images[i];
                    updatedImage.Quality = string.Empty;
                    _images[i] = updatedImage;
                }

                _cliVersion = currentCliVersion;
                EditorUtility.SetDirty(this);
            }

            for (int i = 0; i < _images.Count; ++i)
            {
                if (!string.IsNullOrEmpty(_images[i].Quality))
                {
                    continue;
                }

                dirtyEntries.Add(_images[i]);
            }

            return dirtyEntries;
        }
        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Finds the path to the command-line tool used to generate a database.
        /// </summary>
        /// <param name="path">The path to the command-line tool that will be set if a valid path
        /// was found.</param>
        /// <returns><c>true</c> if a valid path was found, <c>false</c> otherwise.</returns>
        public static bool FindCliBinaryPath(out string path)
        {
            var binaryName = ApiConstants.AugmentedImageCliBinaryName;
            string[] cliBinaryGuid = AssetDatabase.FindAssets(binaryName);
            if (cliBinaryGuid.Length == 0)
            {
                Debug.LogErrorFormat(
                    "Could not find required tool for building AugmentedImageDatabase: {0}. " +
                    "Was it removed from the ARCore SDK?", binaryName);
                path = string.Empty;
                return false;
            }

            // Remove the '/Assets' from the project path since it will be added in the path below.
            string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            path = Path.Combine(projectPath, AssetDatabase.GUIDToAssetPath(cliBinaryGuid[0]));
            return !string.IsNullOrEmpty(path);
        }
        /// @endcond

        /// <summary>
        /// Unity OnDestroy.
        /// </summary>
        private void OnDestroy()
        {
            if (_arAugmentedImageDatabase != IntPtr.Zero)
            {
                AugmentedImageDatabaseApi.Release(_arAugmentedImageDatabase);
            }
        }
#endif
    }
}
