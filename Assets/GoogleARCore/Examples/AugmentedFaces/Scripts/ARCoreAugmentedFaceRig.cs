//-----------------------------------------------------------------------
// <copyright file="ARCoreAugmentedFaceRig.cs" company="Google LLC">
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

namespace GoogleARCore.Examples.AugmentedFaces
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Helper component to update face regions.
    /// </summary>
    [ExecuteInEditMode]
    public class ARCoreAugmentedFaceRig : MonoBehaviour
    {
        /// <summary>
        /// If true, this component will update itself using the first AugmentedFace detected by ARCore.
        /// </summary>
        public bool AutoBind = false;

        private static readonly Dictionary<AugmentedFaceRegion, string> k_RegionTransformNames =
            new Dictionary<AugmentedFaceRegion, string>()
            {
                { AugmentedFaceRegion.NoseTip, "NOSE_TIP" },
                { AugmentedFaceRegion.ForeheadLeft, "FOREHEAD_LEFT" },
                { AugmentedFaceRegion.ForeheadRight, "FOREHEAD_RIGHT" }
            };

        private AugmentedFace m_AugmentedFace;
        private List<AugmentedFace> m_AugmentedFaceList = new List<AugmentedFace>();
        private Dictionary<AugmentedFaceRegion, Transform> m_RegionGameObjects =
            new Dictionary<AugmentedFaceRegion, Transform>();

        /// <summary>
        /// Gets or sets the ARCore AugmentedFace object that will be used to update the face region.
        /// </summary>
        public AugmentedFace AumgnetedFace
        {
            get
            {
                return m_AugmentedFace;
            }

            set
            {
                m_AugmentedFace = value;
                Update();
            }
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            m_AugmentedFaceList = new List<AugmentedFace>();
            _InitializeFaceRegions();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (AutoBind)
            {
                m_AugmentedFaceList.Clear();
                Session.GetTrackables<AugmentedFace>(m_AugmentedFaceList, TrackableQueryFilter.All);
                if (m_AugmentedFaceList.Count != 0)
                {
                    m_AugmentedFace = m_AugmentedFaceList[0];
                }
            }

            if (m_AugmentedFace == null)
            {
                return;
            }

            _UpdateRegions();
        }

        /// <summary>
        /// Method to initialize face region gameobject if not present.
        /// </summary>
        private void _InitializeFaceRegions()
        {
            foreach (AugmentedFaceRegion region in k_RegionTransformNames.Keys)
            {
                string name = k_RegionTransformNames[region];
                Transform regionTransform = _FindChildTransformRecursive(transform, name);
                if (regionTransform == null)
                {
                    GameObject newRegionObject = new GameObject(name);
                    newRegionObject.transform.SetParent(transform);
                    regionTransform = newRegionObject.transform;
                }

                m_RegionGameObjects[region] = regionTransform;
            }
        }

        private Transform _FindChildTransformRecursive(Transform target, string name)
        {
            if (target.name == name)
            {
                return target;
            }

            foreach (Transform child in target)
            {
                if (child.name.Contains(name))
                {
                    return child;
                }

                Transform result = _FindChildTransformRecursive(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Update all face regions associated with the mesh.
        /// </summary>
        private void _UpdateRegions()
        {
            bool isTracking = m_AugmentedFace.TrackingState == TrackingState.Tracking;

            if (isTracking)
            {
                // Update the root transform;
                transform.position = m_AugmentedFace.CenterPose.position;
                transform.rotation = m_AugmentedFace.CenterPose.rotation;
            }

            foreach (AugmentedFaceRegion region in m_RegionGameObjects.Keys)
            {
                Transform regionTransform = m_RegionGameObjects[region];
                regionTransform.gameObject.SetActive(isTracking);
                if (isTracking)
                {
                    Pose regionPose = m_AugmentedFace.GetRegionPose(region);
                    regionTransform.position = regionPose.position;
                    regionTransform.rotation = regionPose.rotation;
                }
            }
        }
    }
}
