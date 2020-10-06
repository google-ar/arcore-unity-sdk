//-----------------------------------------------------------------------
// <copyright file="InstantPlacementEffect.cs" company="Google LLC">
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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A component that controls holographic effect on a game object that relies on
    /// Instant Placement.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class InstantPlacementEffect : MonoBehaviour
    {
        /// <summary>
        /// The transparent materials and the size should match the original materials.
        /// </summary>
        public Material[] HolographicMaterials;

        /// <summary>
        /// The origianl materials used by this game object.
        /// </summary>
        public Material[] OriginalMaterials;

        private bool _isOn = false;

        private InstantPlacementPoint _instantPlacementPoint = null;

        /// <summary>
        /// Initialize the visual effect.
        /// </summary>
        /// <param name="trackable">The Trackable that's associated to this object.</param>
        public void InitializeWithTrackable(Trackable trackable)
        {
            if (trackable is InstantPlacementPoint)
            {
                _instantPlacementPoint = trackable as InstantPlacementPoint;
                _isOn = _instantPlacementPoint.TrackingMethod !=
                    InstantPlacementPointTrackingMethod.FullTracking;
            }
            else
            {
                _isOn = false;
            }

            if (_isOn)
            {
                var renderer = GetComponent<MeshRenderer>();
                renderer.materials = HolographicMaterials;
            }
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (!_isOn || _instantPlacementPoint == null)
            {
                return;
            }

            if (_instantPlacementPoint.TrackingMethod ==
                    InstantPlacementPointTrackingMethod.FullTracking)
            {
                GetComponent<MeshRenderer>().materials = OriginalMaterials;
                _isOn = false;
            }
        }
    }
}
