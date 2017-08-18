//-----------------------------------------------------------------------
// <copyright file="PlaneAttachment.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
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

namespace GoogleARCore.HelloAR
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using GoogleARCore;

    public class PlaneAttachment : MonoBehaviour
    {
        private TrackedPlane m_AttachedPlane;

        private float m_planeYOffset;

        private MeshRenderer[] m_meshRenderers;

        private bool m_isVisible = true;

        /// <summary>
        /// Unity start.
        /// </summary>
        public void Start()
        {
            m_meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        /// <summary>
        /// Unity update.
        /// </summary>
        public void Update()
        {
            // If the plane has been subsumed switch attachment to the subsuming plane.
            while (m_AttachedPlane.SubsumedBy != null)
            {
                m_AttachedPlane = m_AttachedPlane.SubsumedBy;
            }

            // Update visibility of the GameObject based on plane validity.
            if (!m_AttachedPlane.IsValid && m_isVisible)
            {
                for (int i = 0; i < m_meshRenderers.Length; i++)
                {
                    m_meshRenderers[i].enabled = false;
                }

                m_isVisible = false;
            }
            else if (m_AttachedPlane.IsValid && !m_isVisible)
            {
                for (int i = 0; i < m_meshRenderers.Length; i++)
                {
                    m_meshRenderers[i].enabled = true;
                }

                m_isVisible = true;
            }

            transform.position = new Vector3(transform.position.x, m_AttachedPlane.Position.y + m_planeYOffset,
                transform.position.z);
        }

        /// <summary>
        /// Have the GameObject maintain the y-offset to a plane.
        /// </summary>
        /// <param>The plane to attach to.</param>
        public void Attach(TrackedPlane plane)
        {
            m_AttachedPlane = plane;
            m_planeYOffset = transform.position.y - plane.Position.y;
        }
    }
}