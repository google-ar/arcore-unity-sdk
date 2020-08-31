//-----------------------------------------------------------------------
// <copyright file="ElevationManipulator.cs" company="Google LLC">
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

namespace GoogleARCore.Examples.ObjectManipulation
{
    using UnityEngine;

    /// <summary>
    /// Controls the elevation of an object via a two finger drag gesture.
    /// If an object is selected, then doing a two finger drag along the vertical
    /// axis will elevate the object.
    /// </summary>
    [RequireComponent(typeof(SelectionManipulator))]
    public class ElevationManipulator : Manipulator
    {
        /// <summary>
        /// The line renderer used to visualize the elevation manipulation.
        /// </summary>
        public LineRenderer LineRenderer;

        private Vector3 _origin;

        /// <summary>
        /// Returns true if the transformation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the transformation can be started.</returns>
        protected override bool CanStartManipulationForGesture(TwoFingerDragGesture gesture)
        {
            if (!IsSelected())
            {
                return false;
            }

            if (gesture.TargetObject != null)
            {
                return false;
            }

            if (transform.parent.up != Vector3.up && transform.parent.up != Vector3.down)
            {
                // Don't allow elevation on vertical planes.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the elevation.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnStartManipulation(TwoFingerDragGesture gesture)
        {
            _origin = transform.localPosition;
            _origin.y = transform.InverseTransformPoint(transform.parent.position).y;
            _origin = transform.TransformPoint(_origin);
            OnStartElevationVisualization(_origin, transform.position);
        }

        /// <summary>
        /// Continues the elevation.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnContinueManipulation(TwoFingerDragGesture gesture)
        {
            float elevationScale = 0.25f;

            Quaternion cameraRotation = Camera.main.transform.rotation;
            Vector3 rotatedDelta = cameraRotation * gesture.Delta;

            float elevationAmount = (rotatedDelta.y / Screen.dpi) * elevationScale;
            transform.Translate(0.0f, elevationAmount, 0.0f);

            // We cannot move it below the original position.
            if (transform.localPosition.y < transform.parent.InverseTransformPoint(_origin).y)
            {
                transform.position = transform.parent.TransformPoint(
                    new Vector3(
                        transform.localPosition.x,
                        transform.parent.InverseTransformPoint(_origin).y,
                        transform.localPosition.z));
            }

            GetComponent<SelectionManipulator>().OnElevationChangedScaled(
                Mathf.Abs(transform.position.y - _origin.y));
            OnContinueElevationVisualization(transform.position);
        }

        /// <summary>
        /// Finishes the elevation.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnEndManipulation(TwoFingerDragGesture gesture)
        {
            OnEndElevationVisualization();
        }

        /// <summary>
        /// Called when an ElevationManipulator manipulation is started.
        /// </summary>
        /// <param name="startPosition">The start position of the object.</param>
        /// <param name="currentPosition">The current position of the object.</param>
        private void OnStartElevationVisualization(Vector3 startPosition, Vector3 currentPosition)
        {
            if (LineRenderer != null)
            {
                LineRenderer.SetPosition(0, startPosition);
                LineRenderer.SetPosition(1, currentPosition);
                LineRenderer.enabled = true;
            }
        }

        /// <summary>
        /// Called when an ElevationManipulator manipulation is continued to a new position.
        /// </summary>
        /// <param name="currentPosition">The current position of the object.</param>
        private void OnContinueElevationVisualization(Vector3 currentPosition)
        {
            if (LineRenderer != null)
            {
                LineRenderer.SetPosition(1, currentPosition);
            }
        }

        /// <summary>
        /// Called when an ElevationManipulator manipulation is ended.
        /// </summary>
        private void OnEndElevationVisualization()
        {
            if (LineRenderer != null)
            {
                LineRenderer.enabled = false;
            }
        }
    }
}
