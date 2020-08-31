//-----------------------------------------------------------------------
// <copyright file="DepthMenu.cs" company="Google LLC">
//
// Copyright 2020 Google LLC. All Rights Reserved.
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
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    /// <summary>
    /// Depth setting menu, including the menu window and depth card window.
    /// </summary>
    public class DepthMenu : MonoBehaviour
    {
        /// <summary>
        /// The plane discovery guide visuals that guide users to scan surroundings
        /// and discover planes.
        /// </summary>
        [FormerlySerializedAs("m_PlaneDiscoveryGuide")]
        [SerializeField]
        private PlaneDiscoveryGuide _planeDiscoveryGuide = null;

        /// <summary>
        /// Scene object for visualizing depth data.
        /// </summary>
        [FormerlySerializedAs("m_DebugVisualizer")]
        [SerializeField]
        private GameObject _debugVisualizer = null;

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        [FormerlySerializedAs("m_Camera")]
        [SerializeField]
        private Camera _camera = null;

        /// <summary>
        /// The Menu Window shows the depth configurations.
        /// </summary>
        [FormerlySerializedAs("m_MenuWindow")]
        [SerializeField]
        private GameObject _menuWindow = null;

        /// <summary>
        /// The Depth Card Window.
        /// </summary>
        [FormerlySerializedAs("m_DepthCardWindow")]
        [SerializeField]
        private GameObject _depthCardWindow = null;

        /// <summary>
        /// The button to open the menu window.
        /// </summary>
        [FormerlySerializedAs("m_MenuButton")]
        [SerializeField]
        private Button _menuButton = null;

        /// <summary>
        /// The button to apply the config and close the menu window.
        /// </summary>
        [FormerlySerializedAs("m_ApplyButton")]
        [SerializeField]
        private Button _applyButton = null;

        /// <summary>
        /// The button to cancel the changs and close the menu window.
        /// </summary>
        [FormerlySerializedAs("m_CancelButton")]
        [SerializeField]
        private Button _cancelButton = null;

        /// <summary>
        /// The button to enable depth.
        /// </summary>
        [FormerlySerializedAs("m_EnableDepthButton")]
        [SerializeField]
        private Button _enableDepthButton = null;

        /// <summary>
        /// The button to disable depth.
        /// </summary>
        [FormerlySerializedAs("m_DisableDepthButton")]
        [SerializeField]
        private Button _disableDepthButton = null;

        /// <summary>
        /// The menu text.
        /// </summary>
        [FormerlySerializedAs("m_MenuText")]
        [SerializeField]
        private Text _menuText = null;

        /// <summary>
        /// The toggle to enable depth.
        /// </summary>
        [FormerlySerializedAs("m_EnableDepthToggle")]
        [SerializeField]
        private Toggle _enableDepthToggle = null;

        /// <summary>
        /// The toggle label of _enableDepthToggle.
        /// </summary>
        [FormerlySerializedAs("m_EnableDepthToggleLabel")]
        [SerializeField]
        private Text _enableDepthToggleLabel = null;

        /// <summary>
        /// The toggle to switch to depth map.
        /// </summary>
        [FormerlySerializedAs("m_DepthMapToggle")]
        [SerializeField]
        private Toggle _depthMapToggle = null;

        /// <summary>
        /// The toggle label of _depthMapToggle.
        /// </summary>
        [FormerlySerializedAs("m_DepthMapToggleLabel")]
        [SerializeField]
        private Text _depthMapToggleLabel = null;

        private bool _depthConfigured = false;

        private DepthState _depthState = DepthState.DepthNotAvailable;

        /// <summary>
        /// Depth state of this sample.
        /// </summary>
        public enum DepthState
        {
            /// <summary>
            /// Depth feature not available on the device.
            /// </summary>
            DepthNotAvailable,

            /// <summary>
            /// Depth feature disabled.
            /// </summary>
            DepthDisabled,

            /// <summary>
            /// Depth feature enabled.
            /// </summary>
            DepthEnabled,

            /// <summary>
            /// Show depth map.
            /// </summary>
            DepthMap
        }

        /// <summary>
        /// Unity's Start() method.
        /// </summary>
        public void Start()
        {
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
            _applyButton.onClick.AddListener(OnApplyButtonClicked);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            _enableDepthButton.onClick.AddListener(OnEnableDepthButtonClicked);
            _disableDepthButton.onClick.AddListener(OnDisableDepthButtonClicked);
            _enableDepthToggle.onValueChanged.AddListener(OnEnableDepthToggleValueChanged);

            _menuWindow.SetActive(false);
            _depthCardWindow.SetActive(false);
            _debugVisualizer.SetActive(false);
        }

        /// <summary>
        /// Unity's OnDestroy() method.
        /// </summary>
        public void OnDestroy()
        {
            _menuButton.onClick.RemoveListener(OnMenuButtonClicked);
            _applyButton.onClick.RemoveListener(OnApplyButtonClicked);
            _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            _enableDepthButton.onClick.RemoveListener(OnEnableDepthButtonClicked);
            _disableDepthButton.onClick.RemoveListener(OnDisableDepthButtonClicked);
            _enableDepthToggle.onValueChanged.RemoveListener(OnEnableDepthToggleValueChanged);
        }

        /// <summary>
        /// Show Depth Card window when first asset placed and device supports depth.
        /// </summary>
        public void ConfigureDepthBeforePlacingFirstAsset()
        {
            if (!_depthConfigured)
            {
                // Session might not be initialized when GameOject is inializing.
                // Hence, it would be better NOT to call `IsDepthModeSupported` in start().
                if (Session.IsDepthModeSupported(DepthMode.Automatic))
                {
                    _depthCardWindow.SetActive(true);
                    _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(false);
                }
                else
                {
                    _depthConfigured = true;
                }
            }
        }

        /// <summary>
        /// Check whether the user could place asset.
        /// </summary>
        /// <returns>Whether the user could place asset.</returns>
        public bool CanPlaceAsset()
        {
            return !_depthCardWindow.activeSelf & !_menuWindow.activeSelf;
        }

        /// <summary>
        /// Check whether the depth is enabled.
        /// </summary>
        /// <returns>Whether the depth is enabled.</returns>
        public bool IsDepthEnabled()
        {
            return _depthState == DepthState.DepthEnabled
                || _depthState == DepthState.DepthMap;
        }

        private void OnMenuButtonClicked()
        {
            if (!_depthConfigured)
            {
                // Session might not be initialized when GameOject is inializing.
                // Hence, it would be better NOT to call `IsDepthModeSupported` in start().
                if (Session.IsDepthModeSupported(DepthMode.Automatic))
                {
                    _depthState = DepthState.DepthDisabled;
                    _menuText.text = "Your device supports depth.";
                }
                else
                {
                    ConfigureDepth(false);
                    _depthState = DepthState.DepthNotAvailable;
                    _menuText.text = "Your device doesn't support depth.";
                }

                ResetToggle();
            }

            _menuWindow.SetActive(true);
            _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(false);
        }

        private void OnApplyButtonClicked()
        {
            ConfigureDepth(_enableDepthToggle.isOn);
            if (_depthMapToggle.isOn)
            {
                _depthState = DepthState.DepthMap;
                _debugVisualizer.SetActive(true);
            }
            else
            {
                _debugVisualizer.SetActive(false);
            }

            _menuWindow.SetActive(false);
            _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void OnCancelButtonClicked()
        {
            ResetToggle();
            _menuWindow.SetActive(false);
            _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void OnEnableDepthButtonClicked()
        {
            ConfigureDepth(true);
            ResetToggle();
            _depthCardWindow.SetActive(false);
            _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void OnDisableDepthButtonClicked()
        {
            ConfigureDepth(false);
            ResetToggle();
            _depthCardWindow.SetActive(false);
            _planeDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void OnEnableDepthToggleValueChanged(bool enabled)
        {
            if (enabled)
            {
                _depthMapToggle.interactable = true;
                _depthMapToggleLabel.color = Color.black;
            }
            else
            {
                _depthMapToggle.interactable = false;
                _depthMapToggle.isOn = false;
                _depthMapToggleLabel.color = _enableDepthToggle.colors.disabledColor;
            }
        }

        private void ConfigureDepth(bool depthEnabled)
        {
            (_camera.GetComponent(typeof(DepthEffect)) as MonoBehaviour).enabled = depthEnabled;
            _depthConfigured = true;
            _depthState = depthEnabled ? DepthState.DepthEnabled : DepthState.DepthDisabled;
        }

        private void ResetToggle()
        {
            switch (_depthState)
            {
                case DepthState.DepthEnabled:
                    _enableDepthToggle.interactable = true;
                    _enableDepthToggleLabel.color = Color.black;
                    _depthMapToggle.interactable = true;
                    _depthMapToggleLabel.color = Color.black;
                    _enableDepthToggle.isOn = true;
                    _depthMapToggle.isOn = false;
                    break;
                case DepthState.DepthDisabled:
                    _enableDepthToggle.interactable = true;
                    _enableDepthToggleLabel.color = Color.black;
                    _depthMapToggle.interactable = false;
                    _depthMapToggleLabel.color = _enableDepthToggle.colors.disabledColor;
                    _enableDepthToggle.isOn = false;
                    _depthMapToggle.isOn = false;
                    break;
                case DepthState.DepthMap:
                    _enableDepthToggle.interactable = true;
                    _enableDepthToggleLabel.color = Color.black;
                    _depthMapToggle.interactable = true;
                    _depthMapToggleLabel.color = Color.black;
                    _enableDepthToggle.isOn = true;
                    _depthMapToggle.isOn = true;
                    break;
                case DepthState.DepthNotAvailable:
                default:
                    _enableDepthToggle.interactable = false;
                    _enableDepthToggleLabel.color = _enableDepthToggle.colors.disabledColor;
                    _depthMapToggle.interactable = false;
                    _depthMapToggleLabel.color = _enableDepthToggle.colors.disabledColor;
                    _enableDepthToggle.isOn = false;
                    _depthMapToggle.isOn = false;
                    break;
            }
        }
    }
}
