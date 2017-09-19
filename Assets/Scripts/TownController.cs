//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
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

    /// <summary>
    /// Controlls the AR People.
    /// </summary>
    public class TownController : MonoBehaviour
    {
        /// <summary>
        /// Controller with ARCore-specific logic.
        /// </summary>
        public ARCoreForTownController m_arCoreController;

        /// <summary>
        /// The camera used when running on the Unity Editor.
        /// </summary>
        public Camera m_editorCamera;

        /// <summary>
        /// </summary>
        public GameObject m_townPrefab;

        private Camera m_activeCamera;


        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start ()
        {
#if UNITY_EDITOR
            m_editorCamera.gameObject.SetActive (true);
            m_activeCamera = m_editorCamera;
#elif UNITY_ANDROID
            m_activeCamera = m_arCoreController.Init ();
#elif UNITY_IOS
            // TODO: add ARKit Init
#endif
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update ()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown (0)) {
                Vector3 position = new Vector3 (0, 0, 0);

                PlaceTown (position, null);
            }
#elif UNITY_ANDROID
            m_arCoreController.ARUpdate ();
#elif UNITY_IOS
            // TODO: add ARKit Update
#endif
        }

        public GameObject PlaceTown (Vector3 position, Transform parent)
        {
            // Intanstiate town
            GameObject townObject = Instantiate (m_townPrefab, position, Quaternion.identity, parent);
            // Adjust size for Simple Citizens prefabs
            float sizeMultiplier = 0.1f;
            townObject.transform.localScale = new Vector3 (sizeMultiplier, sizeMultiplier, sizeMultiplier);
            GetComponent<AudioSource> ().Play ();

            return townObject;
        }
    }
}
