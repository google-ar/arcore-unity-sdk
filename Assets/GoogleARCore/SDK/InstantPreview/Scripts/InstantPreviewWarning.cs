//-----------------------------------------------------------------------
// <copyright file="InstantPreviewWarning.cs" company="Google">
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
    using UnityEngine;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

    /// <summary>
    /// Logs and dismisses on-screen Instant Preview warning.
    /// </summary>
    public class InstantPreviewWarning : MonoBehaviour
    {
        /// <summary>
        /// Allows developers to disable warning message.
        /// </summary>
        [Tooltip(
            "Whether to show warning in play mode regarding Instant Preview's limited support " +
            "for touch based input.")]
        public bool ShowEditorWarning = true;

        private string m_InstantPreviewDocumentationUrl =
            "https://developers.google.com/ar/develop/unity/instant-preview";

        private void Awake()
        {
            Destroy(gameObject, 4);
        }

        private void Start()
        {
            string prefabPath =
                InstantPreviewManager.InstantPreviewWarningPrefabPath.Replace(".prefab",
                                                                              string.Empty);
            Debug.LogWarningFormat(
                "Instant Preview has limited support for touch based input, see {0} for " +
                "details.\n" +
                "To disable this warning, uncheck 'Show Editor Warning' in the '{1}' prefab.",
                m_InstantPreviewDocumentationUrl, prefabPath);
        }

        private void Update()
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                Destroy(gameObject);
            }
        }
    }
}
