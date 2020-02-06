// <copyright file="SafeAreaScaler.cs" company="Google">
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

namespace GoogleARCore.Examples.Common
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// A helper component that scale the UI rect to the same size as the safe area.
    /// </summary>
    public class SafeAreaScaler : MonoBehaviour
    {
        private Rect m_ScreenSafeArea = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Unity's Awake() method.
        /// </summary>
        public void Update()
        {
            Rect safeArea;
#if UNITY_2017_2_OR_NEWER
            safeArea = Screen.safeArea;
#else
            safeArea = new Rect(0, 0, Screen.width, Screen.height);
#endif

            if (m_ScreenSafeArea != safeArea)
            {
                m_ScreenSafeArea = safeArea;
                _MatchRectTransformToSafeArea();
            }
        }

        private void _MatchRectTransformToSafeArea()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            // lower left corner offset
            Vector2 offsetMin = new Vector2(m_ScreenSafeArea.xMin,
                Screen.height - m_ScreenSafeArea.yMax);

            // upper right corner offset
            Vector2 offsetMax = new Vector2(m_ScreenSafeArea.xMax - Screen.width,
                -m_ScreenSafeArea.yMin);

            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
