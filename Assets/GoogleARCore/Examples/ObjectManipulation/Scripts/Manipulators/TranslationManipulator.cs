//-----------------------------------------------------------------------
// <copyright file="TranslationManipulator.cs" company="Google LLC">
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
    using GoogleARCore.Examples.ObjectManipulationInternal;
    using UnityEngine;

    /// <summary>
    /// Manipulates the position of an object via a drag gesture.
    /// If not selected, the object will be selected when the drag gesture starts.
    /// </summary>
    [RequireComponent(typeof(SelectionManipulator))]
    public class TranslationManipulator : Manipulator
    {
        /// <summary>
        /// The translation mode of this object.
        /// </summary>
        public TransformationUtility.TranslationMode ObjectTranslationMode;

        /// <summary>
        /// The maximum translation distance of this object.
        /// </summary>
        public float MaxTranslationDistance;

        private const float _positionSpeed = 12.0f;
        private const float _diffThreshold = 0.0001f;

        private bool _isActive = false;
        private Vector3 _desiredAnchorPosition;
        private Vector3 _desiredLocalPosition;
        private Quaternion _desiredRotation;
        private float _groundingPlaneHeight;
        private TrackableHit _lastHit;

        /// <summary>
        /// The Unity's Start method.
        /// </summary>
        protected void Start()
        {
            _desiredLocalPosition = new Vector3(0, 0, 0);
        }

        /// <summary>
        /// The Unity's Update method.
        /// </summary>
        protected override void Update()
        {
            base.Update();
            UpdatePosition();
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(DragGesture gesture)
        {
            if (gesture.TargetObject == null)
            {
                return false;
            }

            // If the gesture isn't targeting this item, don't start manipulating.
            if (gesture.TargetObject != gameObject)
            {
                return false;
            }

            // Select it.
            Select();

            return true;
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnStartManipulation(DragGesture gesture)
        {
            _groundingPlaneHeight = transform.parent.position.y;
        }

        /// <summary>
        /// Continues the translation.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnContinueManipulation(DragGesture gesture)
        {
            _isActive = true;

            TransformationUtility.Placement desiredPlacement =
                TransformationUtility.GetBestPlacementPosition(
                    transform.parent.position, gesture.Position, _groundingPlaneHeight, 0.03f,
                    MaxTranslationDistance, ObjectTranslationMode);

            if (desiredPlacement.HoveringPosition.HasValue &&
                desiredPlacement.PlacementPosition.HasValue)
            {
                // If desired position is lower than current position, don't drop it until it's
                // finished.
                _desiredLocalPosition = transform.parent.InverseTransformPoint(
                    desiredPlacement.HoveringPosition.Value);

                _desiredAnchorPosition = desiredPlacement.PlacementPosition.Value;

                _groundingPlaneHeight = desiredPlacement.UpdatedGroundingPlaneHeight;

                if (desiredPlacement.PlacementRotation.HasValue)
                {
                    // Rotate if the plane direction has changed.
                    if (((desiredPlacement.PlacementRotation.Value * Vector3.up) - transform.up)
                        .magnitude > _diffThreshold)
                    {
                        _desiredRotation = desiredPlacement.PlacementRotation.Value;
                    }
                    else
                    {
                        _desiredRotation = transform.rotation;
                    }
                }

                if (desiredPlacement.PlacementPlane.HasValue)
                {
                    _lastHit = desiredPlacement.PlacementPlane.Value;
                }
            }
        }

        /// <summary>
        /// Finishes the translation.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnEndManipulation(DragGesture gesture)
        {
            GameObject oldAnchor = transform.parent.gameObject;

            Pose desiredPose = new Pose(_desiredAnchorPosition, _lastHit.Pose.rotation);

            Vector3 desiredLocalPosition =
                transform.parent.InverseTransformPoint(desiredPose.position);

            if (desiredLocalPosition.magnitude > MaxTranslationDistance)
            {
                desiredLocalPosition = desiredLocalPosition.normalized * MaxTranslationDistance;
            }

            desiredPose.position = transform.parent.TransformPoint(desiredLocalPosition);

            Anchor newAnchor = _lastHit.Trackable.CreateAnchor(desiredPose);
            transform.parent = newAnchor.transform;

            Destroy(oldAnchor);

            _desiredLocalPosition = Vector3.zero;

            // Rotate if the plane direction has changed.
            if (((desiredPose.rotation * Vector3.up) - transform.up).magnitude > _diffThreshold)
            {
                _desiredRotation = desiredPose.rotation;
            }
            else
            {
                _desiredRotation = transform.rotation;
            }

            // Make sure position is updated one last time.
            _isActive = true;
        }

        private void UpdatePosition()
        {
            if (!_isActive)
            {
                return;
            }

            // Lerp position.
            Vector3 oldLocalPosition = transform.localPosition;
            Vector3 newLocalPosition = Vector3.Lerp(
                oldLocalPosition, _desiredLocalPosition, Time.deltaTime * _positionSpeed);

            float diffLenght = (_desiredLocalPosition - newLocalPosition).magnitude;
            if (diffLenght < _diffThreshold)
            {
                newLocalPosition = _desiredLocalPosition;
                _isActive = false;
            }

            transform.localPosition = newLocalPosition;

            // Lerp rotation.
            Quaternion oldRotation = transform.rotation;
            Quaternion newRotation =
                Quaternion.Lerp(oldRotation, _desiredRotation, Time.deltaTime * _positionSpeed);
            transform.rotation = newRotation;

            // Avoid placing the selection higher than the object if the anchor is higher than the
            // object.
            float newElevation =
                Mathf.Max(0, -transform.InverseTransformPoint(_desiredAnchorPosition).y);
            GetComponent<SelectionManipulator>().OnElevationChanged(newElevation);
        }
    }
}
