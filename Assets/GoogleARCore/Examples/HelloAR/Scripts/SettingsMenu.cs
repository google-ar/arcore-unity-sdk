//-----------------------------------------------------------------------
// <copyright file="SettingsMenu.cs" company="Google LLC">
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

namespace GoogleARCore.Examples.HelloAR
{
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Setting menu for Hello AR Sample, including settings for different features.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Common Settings")]

        /// <summary>
        /// Scene object that contains all setting menu UI elements.
        /// </summary>
        [SerializeField]
        private GameObject _menuWindow = null;

        /// <summary>
        /// Setting menu that contains options for different features.
        /// </summary>
        [SerializeField]
        private GameObject _settingMenuUi = null;

        /// <summary>
        /// Setting button for opening menu windows.
        /// </summary>
        [SerializeField]
        private Button _settingButton = null;

        /// <summary>
        /// The plane discovery guide visuals that guide users to scan surroundings
        /// and discover planes.
        /// </summary>
        [SerializeField]
        private PlaneDiscoveryGuide _planeDiscoveryGuide = null;

        [Header("Depth Settings")]

        /// <summary>
        /// Depth setting menu that is used for configuring depth options.
        /// </summary>
        [SerializeField]
        private GameObject _depthMenuUi = null;

        /// <summary>
        /// The button to open depth menu.
        /// </summary>
        [SerializeField]
        private Button _depthButton = null;

        /// <summary>
        /// The component to control depth menu.
        /// </summary>
        [SerializeField]
        private DepthMenu _depthMenu = null;

        [Header("Instant Placement Settings")]

        /// <summary>
        /// Instant Placement menu for configuring instant placement options.
        /// </summary>
        [SerializeField]
        private GameObject _instantPlacementMenuUi = null;

        /// <summary>
        /// The button to open instant placement menu.
        /// </summary>
        [SerializeField]
        private Button _instantPlacementButton = null;

        /// <summary>
        /// Unity's Start() method.
        /// </summary>
        public void Start()
        {
            _menuWindow.SetActive(false);
            _settingMenuUi.SetActive(false);
            _settingButton.onClick.AddListener(OnMenuButtonClick);

            _depthMenuUi.SetActive(false);
            _depthButton.onClick.AddListener(OnClickDepthMenu);
            _instantPlacementMenuUi.SetActive(false);
            _instantPlacementButton.onClick.AddListener(OnClickInstantPlacementMenu);
        }

        /// <summary>
        /// Unity's OnDestroy() method.
        /// </summary>
        public void OnDestroy()
        {
            _settingButton.onClick.RemoveListener(OnMenuButtonClick);
            _depthButton.onClick.RemoveListener(OnClickDepthMenu);
            _instantPlacementButton.onClick.RemoveListener(OnClickInstantPlacementMenu);
        }

        /// <summary>
        /// Callback event for closing the setting menu.
        /// </summary>
        public void OnMenuClosed()
        {
            _menuWindow.SetActive(false);
            _settingMenuUi.SetActive(false);
            _depthMenuUi.SetActive(false);
            _instantPlacementMenuUi.SetActive(false);
            _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        /// <summary>
        /// Callback event for option button.
        /// </summary>
        private void OnMenuButtonClick()
        {
            _menuWindow.SetActive(true);
#if ARCORE_FEATURE_INSTANT_PLACEMENT // Both Depth and IP are enabled.
            _settingMenuUi.SetActive(true);
#else // Only Depth is enabled.
            _depthMenuUi.SetActive(true);
            _depthMenu.OnMenuButtonClicked();
#endif
            _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(false);
        }

        /// <summary>
        /// Callback event for opening depth menu.
        /// </summary>
        private void OnClickDepthMenu()
        {
            _settingMenuUi.SetActive(false);
            _depthMenuUi.SetActive(true);
            _depthMenu.OnMenuButtonClicked();
        }

        /// <summary>
        /// Callback event for opening instant placement menu.
        /// </summary>
        private void OnClickInstantPlacementMenu()
        {
            _settingMenuUi.SetActive(false);
            _instantPlacementMenuUi.SetActive(true);
        }
    }
}
