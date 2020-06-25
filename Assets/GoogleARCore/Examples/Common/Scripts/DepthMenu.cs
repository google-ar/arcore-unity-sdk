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
        [SerializeField] private PlaneDiscoveryGuide m_PlaneDiscoveryGuide = null;

        /// <summary>
        /// Scene object for visualizing depth data.
        /// </summary>
        [SerializeField] private GameObject m_DebugVisualizer = null;

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        [SerializeField] private Camera m_Camera = null;

        /// <summary>
        /// The Menu Window shows the depth configurations.
        /// </summary>
        [SerializeField] private GameObject m_MenuWindow = null;

        /// <summary>
        /// The Depth Card Window.
        /// </summary>
        [SerializeField] private GameObject m_DepthCardWindow = null;

        /// <summary>
        /// The button to open the menu window.
        /// </summary>
        [SerializeField] private Button m_MenuButton = null;

        /// <summary>
        /// The button to apply the config and close the menu window.
        /// </summary>
        [SerializeField] private Button m_ApplyButton = null;

        /// <summary>
        /// The button to cancel the changs and close the menu window.
        /// </summary>
        [SerializeField] private Button m_CancelButton = null;

        /// <summary>
        /// The button to enable depth.
        /// </summary>
        [SerializeField] private Button m_EnableDepthButton = null;

        /// <summary>
        /// The button to disable depth.
        /// </summary>
        [SerializeField] private Button m_DisableDepthButton = null;

        /// <summary>
        /// The menu text.
        /// </summary>
        [SerializeField] private Text m_MenuText = null;

        /// <summary>
        /// The toggle to enable depth.
        /// </summary>
        [SerializeField] private Toggle m_EnableDepthToggle = null;

        /// <summary>
        /// The toggle label of m_EnableDepthToggle.
        /// </summary>
        [SerializeField] private Text m_EnableDepthToggleLabel = null;

        /// <summary>
        /// The toggle to switch to depth map.
        /// </summary>
        [SerializeField] private Toggle m_DepthMapToggle = null;

        /// <summary>
        /// The toggle label of m_DepthMapToggle.
        /// </summary>
        [SerializeField] private Text m_DepthMapToggleLabel = null;

        private bool m_DepthConfigured = false;

        private DepthState m_DepthState = DepthState.DepthNotAvailable;

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
            m_MenuButton.onClick.AddListener(_OnMenuButtonClicked);
            m_ApplyButton.onClick.AddListener(_OnApplyButtonClicked);
            m_CancelButton.onClick.AddListener(_OnCancelButtonClicked);
            m_EnableDepthButton.onClick.AddListener(_OnEnableDepthButtonClicked);
            m_DisableDepthButton.onClick.AddListener(_OnDisableDepthButtonClicked);
            m_EnableDepthToggle.onValueChanged.AddListener(_OnEnableDepthToggleValueChanged);

            m_MenuWindow.SetActive(false);
            m_DepthCardWindow.SetActive(false);
            m_DebugVisualizer.SetActive(false);
        }

        /// <summary>
        /// Unity's OnDestroy() method.
        /// </summary>
        public void OnDestroy()
        {
            m_MenuButton.onClick.RemoveListener(_OnMenuButtonClicked);
            m_ApplyButton.onClick.RemoveListener(_OnApplyButtonClicked);
            m_CancelButton.onClick.RemoveListener(_OnCancelButtonClicked);
            m_EnableDepthButton.onClick.RemoveListener(_OnEnableDepthButtonClicked);
            m_DisableDepthButton.onClick.RemoveListener(_OnDisableDepthButtonClicked);
            m_EnableDepthToggle.onValueChanged.RemoveListener(_OnEnableDepthToggleValueChanged);
        }

        /// <summary>
        /// Show Depth Card window when first asset placed and device supports depth.
        /// </summary>
        public void ConfigureDepthBeforePlacingFirstAsset()
        {
            if (!m_DepthConfigured)
            {
                // Session might not be initialized when GameOject is inializing.
                // Hence, it would be better NOT to call `IsDepthModeSupported` in start().
                if (Session.IsDepthModeSupported(DepthMode.Automatic))
                {
                    m_DepthCardWindow.SetActive(true);
                    m_PlaneDiscoveryGuide.EnablePlaneDiscoveryGuide(false);
                }
                else
                {
                    m_DepthConfigured = true;
                }
            }
        }

        /// <summary>
        /// Check whether the user could place asset.
        /// </summary>
        /// <returns>Whether the user could place asset.</returns>
        public bool CanPlaceAsset()
        {
            return !m_DepthCardWindow.activeSelf & !m_MenuWindow.activeSelf;
        }

        /// <summary>
        /// Check whether the depth is enabled.
        /// </summary>
        /// <returns>Whether the depth is enabled.</returns>
        public bool IsDepthEnabled()
        {
            return m_DepthState == DepthState.DepthEnabled
                || m_DepthState == DepthState.DepthMap;
        }

        private void _OnMenuButtonClicked()
        {
            if (!m_DepthConfigured)
            {
                // Session might not be initialized when GameOject is inializing.
                // Hence, it would be better NOT to call `IsDepthModeSupported` in start().
                if (Session.IsDepthModeSupported(DepthMode.Automatic))
                {
                    m_DepthState = DepthState.DepthDisabled;
                    m_MenuText.text = "Your device supports depth.";
                }
                else
                {
                    _ConfigureDepth(false);
                    m_DepthState = DepthState.DepthNotAvailable;
                    m_MenuText.text = "Your device doesn't support depth.";
                }

                _ResetToggle();
            }

            m_MenuWindow.SetActive(true);
            m_PlaneDiscoveryGuide.EnablePlaneDiscoveryGuide(false);
        }

        private void _OnApplyButtonClicked()
        {
            _ConfigureDepth(m_EnableDepthToggle.isOn);
            if (m_DepthMapToggle.isOn)
            {
                m_DepthState = DepthState.DepthMap;
                m_DebugVisualizer.SetActive(true);
            }
            else
            {
                m_DebugVisualizer.SetActive(false);
            }

            m_MenuWindow.SetActive(false);
            m_PlaneDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void _OnCancelButtonClicked()
        {
            _ResetToggle();
            m_MenuWindow.SetActive(false);
            m_PlaneDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void _OnEnableDepthButtonClicked()
        {
            _ConfigureDepth(true);
            _ResetToggle();
            m_DepthCardWindow.SetActive(false);
            m_PlaneDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void _OnDisableDepthButtonClicked()
        {
            _ConfigureDepth(false);
            _ResetToggle();
            m_DepthCardWindow.SetActive(false);
            m_PlaneDiscoveryGuide.EnablePlaneDiscoveryGuide(true);
        }

        private void _OnEnableDepthToggleValueChanged(bool enabled)
        {
            if (enabled)
            {
                m_DepthMapToggle.interactable = true;
                m_DepthMapToggleLabel.color = Color.black;
            }
            else
            {
                m_DepthMapToggle.interactable = false;
                m_DepthMapToggle.isOn = false;
                m_DepthMapToggleLabel.color = m_EnableDepthToggle.colors.disabledColor;
            }
        }

        private void _ConfigureDepth(bool depthEnabled)
        {
            (m_Camera.GetComponent(typeof(DepthEffect)) as MonoBehaviour).enabled = depthEnabled;
            m_DepthConfigured = true;
            m_DepthState = depthEnabled ? DepthState.DepthEnabled : DepthState.DepthDisabled;
        }

        private void _ResetToggle()
        {
            switch (m_DepthState)
            {
                case DepthState.DepthEnabled:
                    m_EnableDepthToggle.interactable = true;
                    m_EnableDepthToggleLabel.color = Color.black;
                    m_DepthMapToggle.interactable = true;
                    m_DepthMapToggleLabel.color = Color.black;
                    m_EnableDepthToggle.isOn = true;
                    m_DepthMapToggle.isOn = false;
                    break;
                case DepthState.DepthDisabled:
                    m_EnableDepthToggle.interactable = true;
                    m_EnableDepthToggleLabel.color = Color.black;
                    m_DepthMapToggle.interactable = false;
                    m_DepthMapToggleLabel.color = m_EnableDepthToggle.colors.disabledColor;
                    m_EnableDepthToggle.isOn = false;
                    m_DepthMapToggle.isOn = false;
                    break;
                case DepthState.DepthMap:
                    m_EnableDepthToggle.interactable = true;
                    m_EnableDepthToggleLabel.color = Color.black;
                    m_DepthMapToggle.interactable = true;
                    m_DepthMapToggleLabel.color = Color.black;
                    m_EnableDepthToggle.isOn = true;
                    m_DepthMapToggle.isOn = true;
                    break;
                case DepthState.DepthNotAvailable:
                default:
                    m_EnableDepthToggle.interactable = false;
                    m_EnableDepthToggleLabel.color = m_EnableDepthToggle.colors.disabledColor;
                    m_DepthMapToggle.interactable = false;
                    m_DepthMapToggleLabel.color = m_EnableDepthToggle.colors.disabledColor;
                    m_EnableDepthToggle.isOn = false;
                    m_DepthMapToggle.isOn = false;
                    break;
            }
        }
    }
}
