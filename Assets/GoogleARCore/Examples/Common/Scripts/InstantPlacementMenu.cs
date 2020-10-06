//-----------------------------------------------------------------------
// <copyright file="InstantPlacementMenu.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Instant placement setting menu to configure instant placement options in runtime.
    /// </summary>
    public class InstantPlacementMenu : MonoBehaviour
    {
        /// <summary>
        /// The ARCoreSession component used in this scene.
        /// </summary>
        [SerializeField]
        private ARCoreSession _arCoreSession = null;

        /// <summary>
        /// Toggle element to configure instant placement mode in runtime.
        /// </summary>
        [SerializeField]
        private Toggle _instantPlacementToggle = null;

        /// <summary>
        /// The button to apply current settings.
        /// </summary>
        [SerializeField]
        private Button _applyButton = null;

        /// <summary>
        /// The button to reset current settings.
        /// </summary>
        [SerializeField]
        private Button _cancelButton = null;

        /// <summary>
        /// Unity's Start() method.
        /// </summary>
        public void Start()
        {
            _applyButton.onClick.AddListener(ApplySettings);
            _cancelButton.onClick.AddListener(ResetSettings);
        }

        /// <summary>
        /// Unity's OnDestroy() method.
        /// </summary>
        public void OnDestroy()
        {
            _applyButton.onClick.RemoveListener(ApplySettings);
            _cancelButton.onClick.RemoveListener(ResetSettings);
        }

        /// <summary>
        /// Check whether Instant Placement is current enabled.
        /// </summary>
        /// <returns><c>true</c> when Instant Placement is enabled,
        /// otherwise, returns <c>false</c>.</returns>
        public bool IsInstantPlacementEnabled()
        {
            return _arCoreSession.SessionConfig.InstantPlacementMode !=
                InstantPlacementMode.Disabled;
        }

        private void ApplySettings()
        {
            if (_instantPlacementToggle == null || _arCoreSession == null)
            {
                return;
            }

            var instantPlacementMode = _instantPlacementToggle.isOn ?
                InstantPlacementMode.LocalYUp : InstantPlacementMode.Disabled;
            _arCoreSession.SessionConfig.InstantPlacementMode = instantPlacementMode;
        }

        private void ResetSettings()
        {
            if (_instantPlacementToggle == null || _arCoreSession == null)
            {
                return;
            }

            _instantPlacementToggle.isOn =
                _arCoreSession.SessionConfig.InstantPlacementMode != InstantPlacementMode.Disabled;
        }
    }
}
