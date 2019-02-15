//-----------------------------------------------------------------------
// <copyright file="RotationManipulator.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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

namespace GoogleARCore.Examples.ObjectManipulation
{
    using UnityEngine;

    /// <summary>
    /// Manipulates the rotation of an object via a drag or a twist gesture.
    /// If an object is selected, then dragging along the horizontal axis
    /// or performing a twist gesture will rotate along the y-axis of the item.
    /// </summary>
    public class RotationManipulator : Manipulator
    {
        private const float k_RotationRateDegreesDrag = 100.0f;
        private const float k_RotationRateDegreesTwist = 2.5f;

        /// <summary>
        /// Returns true if the manipulation can be started for the given Drag gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(DragGesture gesture)
        {
            if (!IsSelected())
            {
                return false;
            }

            if (gesture.TargetObject != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given Twist gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(TwistGesture gesture)
        {
            if (!IsSelected())
            {
                return false;
            }

            if (gesture.TargetObject != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rotates the object around the y-axis via a Drag gesture.
        /// </summary>
        /// <param name="gesture">The current drag gesture.</param>
        protected override void OnContinueManipulation(DragGesture gesture)
        {
            float sign = -1.0f;
            Vector3 forward = Camera.main.transform.TransformPoint(Vector3.forward);
            Quaternion WorldToVerticalOrientedDevice = Quaternion.Inverse(Quaternion.LookRotation(forward, Vector3.up));
            Quaternion DeviceToWorld = Camera.main.transform.rotation;
            Vector3 rotatedDelta = WorldToVerticalOrientedDevice * DeviceToWorld * gesture.Delta;

            float rotationAmount = sign * (rotatedDelta.x / Screen.dpi) * k_RotationRateDegreesDrag;
            transform.Rotate(0.0f, rotationAmount, 0.0f);
        }

        /// <summary>
        /// Rotates the object around the y-axis via a Twist gesture.
        /// </summary>
        /// <param name="gesture">The current twist gesture.</param>
        protected override void OnContinueManipulation(TwistGesture gesture)
        {
            float rotationAmount = -gesture.DeltaRotation * k_RotationRateDegreesTwist;
            transform.Rotate(0.0f, rotationAmount, 0.0f);
        }
    }
}