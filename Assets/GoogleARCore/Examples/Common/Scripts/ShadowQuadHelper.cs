// <copyright file="ShadowQuadHelper.cs" company="Google LLC">
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
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Helper class to activate/deactivate the light estimation shadow plane.
    /// </summary>
    public class ShadowQuadHelper : MonoBehaviour
    {
        /// <summary>
        /// The Depth Setting Menu.
        /// </summary>
        private DepthMenu _depthMenu;

        /// <summary>
        /// The GameObject of ShadowQuad.
        /// </summary>
        private GameObject _shadowQuad;

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            _shadowQuad = this.gameObject.transform.Find("ShadowQuad").gameObject;
            _depthMenu = FindObjectOfType<DepthMenu>();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // Shadows are cast onto the light estimation shadow plane, which do not respect depth.
            // Shadows are disabled when depth is enabled to prevent undesirable rendering
            // artifacts.
            if (_shadowQuad.activeSelf == _depthMenu.IsDepthEnabled())
            {
                _shadowQuad.SetActive(!_depthMenu.IsDepthEnabled());
            }
        }
    }
}
