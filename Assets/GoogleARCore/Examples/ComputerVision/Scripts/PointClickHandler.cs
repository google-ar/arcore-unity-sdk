//-----------------------------------------------------------------------
// <copyright file="PointClickHandler.cs" company="Google">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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

namespace GoogleARCore.Examples.ComputerVision
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// A Monobehavior that handles pointer click event with customizable action.
    /// </summary>
    public class PointClickHandler : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Action which get called when the PointClickHandler detects a point click.
        /// </summary>
        public event Action OnPointClickDetected;

        /// <summary>
        /// Detect if a click occurs.
        /// </summary>
        /// <param name="pointerEventData">The PointerEventData of the click.</param>
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (OnPointClickDetected != null)
            {
                OnPointClickDetected();
            }
        }
    }
}
