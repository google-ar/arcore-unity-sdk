//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabaseInspector.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCore;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(AugmentedImageDatabase))]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class AugmentedImageDatabaseInspector : Editor
    {
        private const float k_ImageSpacerHeight = 55f;
        private const int k_PageSize = 5;
        private const float k_HeaderHeight = 30f;
        private static readonly Vector2 k_ContainerStart = new Vector2(14f, 87f);

        private static BackgroundJobExecutor s_QualityBackgroundExecutor =
            new BackgroundJobExecutor();

        private static AugmentedImageDatabase s_DatabaseForQualityJobs = null;
        private static Dictionary<string, string> s_UpdatedQualityScores =
            new Dictionary<string, string>();

        private int m_PageIndex = 0;

        public override void OnInspectorGUI()
        {
            AugmentedImageDatabase database = target as AugmentedImageDatabase;
            if (database == null)
            {
                return;
            }

            _RunDirtyQualityJobs(database);

            m_PageIndex = Mathf.Min(m_PageIndex, database.Count / k_PageSize);

            _DrawTitle();
            _DrawContainer();
            _DrawColumnNames();

            // Draw the rows for AugmentedImageDatabaseEntry in the database,
            // update the database if the entry's image is replaced and
            // check whether the entry is deleted.
            int displayedImageCount = 0;
            int removeAt = -1;
            int pageStartIndex = m_PageIndex * k_PageSize;
            int pageEndIndex = Mathf.Min(database.Count, pageStartIndex + k_PageSize);
            for (int i = pageStartIndex; i < pageEndIndex; i++, displayedImageCount++)
            {
                AugmentedImageDatabaseEntry updatedImage;
                bool wasRemoved;

                _DrawImageField(database[i], out updatedImage, out wasRemoved);

                if (wasRemoved)
                {
                    removeAt = i;
                }
                else if (!database[i].Equals(updatedImage))
                {
                    database[i] = updatedImage;
                }
            }

            if (removeAt > -1)
            {
                database.RemoveAt(removeAt);
            }

            // Add picked image to the database and ignore the image if duplicate.
            // It MUST operates after updating the database which also triggers
            // "ObjectSelectorClosed" and modifies the ObjectPicker object.
            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                Texture2D textureToAdd = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
                if (textureToAdd != null)
                {
                    string textureToAddGUID =
                        AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(textureToAdd));
                    bool isDuplicate = false;
                    for (int i = 0; i < database.Count; i++)
                    {
                        if (database[i].TextureGUID.Equals(textureToAddGUID))
                        {
                            isDuplicate = true;
                            break;
                        }
                    }

                    if (!isDuplicate)
                    {
                        database.Add(
                            new AugmentedImageDatabaseEntry(textureToAdd.name, textureToAdd));
                    }
                }
            }

            _DrawImageSpacers(displayedImageCount);
            _DrawPageField(database.Count);
        }

        private static void _RunDirtyQualityJobs(AugmentedImageDatabase database)
        {
            if (database == null)
            {
                return;
            }

            if (s_DatabaseForQualityJobs != database)
            {
                // If another database is already running quality evaluation,
                // stop all pending jobs to prioritise the current database.
                if (s_DatabaseForQualityJobs != null)
                {
                    s_QualityBackgroundExecutor.RemoveAllPendingJobs();
                }

                s_DatabaseForQualityJobs = database;
            }

            _UpdateDatabaseQuality(database);

            // Set database dirty to refresh inspector UI for each frame that there are still
            // pending jobs.
            // Otherwise if there exists one frame with no newly finished jobs, the UI will never
            // get refreshed.
            // EditorUtility.SetDirty can only be called from main thread.
            if (s_QualityBackgroundExecutor.PendingJobsCount > 0)
            {
                EditorUtility.SetDirty(database);
                return;
            }

            List<AugmentedImageDatabaseEntry> dirtyEntries = database.GetDirtyQualityEntries();
            if (dirtyEntries.Count == 0)
            {
                return;
            }

            string cliBinaryPath;
            if (!AugmentedImageDatabase.FindCliBinaryPath(out cliBinaryPath))
            {
                return;
            }

            for (int i = 0; i < dirtyEntries.Count; ++i)
            {
                AugmentedImageDatabaseEntry image = dirtyEntries[i];
                var imagePath = AssetDatabase.GetAssetPath(image.Texture);
                var textureGUID = image.TextureGUID;
                s_QualityBackgroundExecutor.PushJob(() =>
                {
                    string quality;
                    string error;
                    ShellHelper.RunCommand(
                        cliBinaryPath,
                        string.Format("eval-img --input_image_path \"{0}\"", imagePath),
                        out quality,
                        out error);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                        quality = "ERROR";
                    }

                    lock (s_UpdatedQualityScores)
                    {
                        s_UpdatedQualityScores.Add(textureGUID, quality);
                    }
                });
            }

            // For refreshing inspector UI as new jobs have been enqueued.
            EditorUtility.SetDirty(database);
        }

        private static void _UpdateDatabaseQuality(AugmentedImageDatabase database)
        {
            lock (s_UpdatedQualityScores)
            {
                if (s_UpdatedQualityScores.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < database.Count; ++i)
                {
                    if (s_UpdatedQualityScores.ContainsKey(database[i].TextureGUID))
                    {
                        AugmentedImageDatabaseEntry updatedImage = database[i];
                        updatedImage.Quality = s_UpdatedQualityScores[updatedImage.TextureGUID];
                        database[i] = updatedImage;
                    }
                }

                s_UpdatedQualityScores.Clear();
            }

            // For refreshing inspector UI for updated quality scores.
            EditorUtility.SetDirty(database);
        }

        private void _DrawTitle()
        {
            const string TITLE_STRING = "Images in Database";
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.stretchWidth = true;
            titleStyle.fontSize = 14;
            titleStyle.normal.textColor = Color.white;
            titleStyle.padding.bottom = 15;

            EditorGUILayout.BeginVertical();
            GUILayout.Space(15);
            EditorGUILayout.LabelField(TITLE_STRING, titleStyle);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        private void _DrawContainer()
        {
            var containerRect = new Rect(
                k_ContainerStart.x, k_ContainerStart.y, EditorGUIUtility.currentViewWidth - 30,
                (k_PageSize * k_ImageSpacerHeight) + k_HeaderHeight);
            GUI.Box(containerRect, string.Empty);
        }

        private void _DrawColumnNames()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(14);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(10, 10, 3, 0);
            if (GUILayout.Button("+", buttonStyle))
            {
                EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, string.Empty, 0);
            }

            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;

            GUILayoutOption[] options =
                {
                    GUILayout.Height(k_HeaderHeight - 10),
                    GUILayout.MaxWidth(80f)
                };
            EditorGUILayout.LabelField("Name", style, options);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Width(m)", style, options);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Quality", style, options);
            GUILayout.FlexibleSpace();
            GUILayout.Space(60);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private string _QualityForDisplay(string quality)
        {
            if (string.IsNullOrEmpty(quality))
            {
                return "Calculating...";
            }

            if (quality == "?")
            {
                return "?";
            }

            return quality + "/100";
        }

        private void _DrawImageField(
            AugmentedImageDatabaseEntry image, out AugmentedImageDatabaseEntry updatedImage,
            out bool wasRemoved)
        {
            updatedImage = new AugmentedImageDatabaseEntry();

            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(10, 10, 13, 0);

            wasRemoved = GUILayout.Button("X", buttonStyle);

            var textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.margin = new RectOffset(5, 5, 15, 0);
            updatedImage.Name =
                EditorGUILayout.TextField(image.Name, textFieldStyle, GUILayout.MaxWidth(80f));

            GUILayout.Space(5);
            updatedImage.Width =
                EditorGUILayout.FloatField(image.Width, textFieldStyle, GUILayout.MaxWidth(80f));

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            GUILayout.Space(5);
            EditorGUILayout.LabelField(_QualityForDisplay(image.Quality), labelStyle,
                                       GUILayout.Height(42), GUILayout.MaxWidth(80f));

            GUILayout.FlexibleSpace();

            updatedImage.Texture = EditorGUILayout.ObjectField(
                image.Texture, typeof(Texture2D), false, GUILayout.Height(45), GUILayout.Width(45))
                as Texture2D;
            if (updatedImage.TextureGUID == image.TextureGUID)
            {
                updatedImage.Quality = image.Quality;
            }

            GUILayout.Space(15);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        private void _DrawImageSpacers(int displayedImageCount)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space((k_PageSize - displayedImageCount) * k_ImageSpacerHeight);
            EditorGUILayout.EndVertical();
        }

        private void _DrawPageField(int imageCount)
        {
            var lastPageIndex = Mathf.Max(imageCount - 1, 0) / k_PageSize;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.LabelField(string.Format("{0} Total Images", imageCount), labelStyle,
                GUILayout.Height(42), GUILayout.Width(100));

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(
                "Page", labelStyle, GUILayout.Height(42), GUILayout.Width(30));

            var textStyle = new GUIStyle(GUI.skin.textField);
            textStyle.margin = new RectOffset(0, 0, 15, 0);
            var pageString = EditorGUILayout.TextField(
                (m_PageIndex + 1).ToString(), textStyle, GUILayout.Width(30));
            int pageNumber;
            int.TryParse(pageString, out pageNumber);
            m_PageIndex = Mathf.Clamp(pageNumber - 1, 0, lastPageIndex);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(10, 10, 13, 0);

            GUI.enabled = m_PageIndex > 0;
            bool moveLeft = GUILayout.Button("<", buttonStyle);
            GUI.enabled = m_PageIndex < lastPageIndex;
            bool moveRight = GUILayout.Button(">", buttonStyle);
            GUI.enabled = true;

            m_PageIndex = moveLeft ? m_PageIndex - 1 : m_PageIndex;
            m_PageIndex = moveRight ? m_PageIndex + 1 : m_PageIndex;

            GUILayout.Space(15);
            EditorGUILayout.EndHorizontal();
        }
    }
}
