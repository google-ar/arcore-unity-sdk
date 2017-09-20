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
    public class PeopleController : MonoBehaviour
    {
        /// <summary>
        /// Controller with ARCore-specific logic.
        /// </summary>
        public ARCoreForPeopleController m_arCoreController;

        /// <summary>
        /// The camera used when running on the Unity Editor.
        /// </summary>
        public Camera m_editorCamera;

        /// <summary>
        /// The list of prefabs to place when a raycast from a user touch hits a plane.
        /// </summary>
        public List<GameObject> m_characterPrefabs;

        /// <summary>
        /// Surface for characters and ads to fall on.
        /// </summary>
        public GameObject m_floorPrefab;

        /// <summary>
        /// Whether to show ads or not.
        /// </summary>
        public bool m_adSupported;

        /// <summary>
        /// The prefab to use when spawning an ad.
        /// </summary>
        public GameObject m_adPrefab;

        /// <summary>
        /// How hard to throw the ad.
        /// </summary>
        public float m_adThrowForce;

        /// <summary>
        /// How many characters to spawn between ads.
        /// </summary>
        public int m_adFrequency = 5;

        private int m_spawnCount;

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
            m_spawnCount = 0;
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update ()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown (0)) {
                float randomX = Random.Range (0.0f, 2.0f);
                float randomZ = Random.Range (0.0f, 2.0f);
                Vector3 position = new Vector3 (randomX, 0, randomZ);

                PlaceCharacter (position, null);
            }
#elif UNITY_ANDROID
            m_arCoreController.ARUpdate ();
#elif UNITY_IOS
            // TODO: add ARKit Update
#endif
        }

        public GameObject PlaceCharacter (Vector3 position, Transform parent)
        {
            bool adTime = m_adSupported && (m_spawnCount + 1) % (m_adFrequency + 1) == 0;
            int randomIndex = Random.Range (0, m_characterPrefabs.Count - 1);

            // Add floor to support objects
            Instantiate (m_floorPrefab, position, m_floorPrefab.transform.rotation, null);

            GameObject spawnPrefab = adTime ? m_adPrefab : m_characterPrefabs [randomIndex];

            position.y += 0.5f;
#if UNITY_EDITOR
            // Spawn ad in front of editor camera
            if (adTime) {
                position.x = 1.5f;
                position.z = 0.0f;
            }
#endif

            // Intanstiate a random character object as a child of the anchor; it's transform will now benefit
            // from the anchor's tracking.
            GameObject andyObject = Instantiate (spawnPrefab, position, Quaternion.identity, parent);

            // Adjust size for Simple Citizens prefabs
            float sizeMultiplier = 0.1f;
            andyObject.transform.localScale = new Vector3 (sizeMultiplier, sizeMultiplier, sizeMultiplier);

            // Andy should look at the camera but still be flush with the plane.
            andyObject.transform.LookAt (m_activeCamera.transform);
            andyObject.transform.rotation = Quaternion.Euler (0.0f, andyObject.transform.rotation.eulerAngles.y,
                andyObject.transform.rotation.z);

            m_spawnCount++;

            if (adTime) {
                // Add some force to throw the ad cube
                andyObject.GetComponent<Rigidbody> ().AddForce (andyObject.transform.forward * m_adThrowForce * -0.5f);
                andyObject.GetComponent<Rigidbody> ().AddTorque (andyObject.transform.right * m_adThrowForce * 2.5f);
                andyObject.GetComponent<Rigidbody> ().AddTorque (andyObject.transform.forward * m_adThrowForce * -2.5f);
            } else {
                GetComponent<AudioSource> ().Play ();
            }

            return andyObject;
        }
    }
}
