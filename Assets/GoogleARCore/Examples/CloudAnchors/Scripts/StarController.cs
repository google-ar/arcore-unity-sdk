//-----------------------------------------------------------------------
// <copyright file="StarController.cs" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
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

namespace GoogleARCore.Examples.CloudAnchors
{
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// A controller for the Star object that handles setting mesh after the world origin has been
    /// placed.
    /// </summary>
    public class StarController : MonoBehaviour
    {
        /// <summary>
        /// The star mesh object.
        /// In order to avoid placing the star on identity pose, the mesh object should be disabled
        /// by default and enabled after the origin has been placed.
        /// </summary>
        private GameObject m_StarMesh;

        /// <summary>
        /// The Cloud Anchors example controller.
        /// </summary>
        private CloudAnchorsExampleController m_CloudAnchorsExampleController;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            m_CloudAnchorsExampleController =
                GameObject.Find("CloudAnchorsExampleController")
                    .GetComponent<CloudAnchorsExampleController>();
            m_StarMesh = transform.Find("StarMesh").gameObject;
            m_StarMesh.SetActive(false);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (m_StarMesh.activeSelf)
            {
                return;
            }

            // Only sets the Star object's mesh after the origin is placed to avoid being placed
            // at identity pose.
            if (!m_CloudAnchorsExampleController.IsOriginPlaced)
            {
                return;
            }

            m_StarMesh.SetActive(true);
        }
    }
}
