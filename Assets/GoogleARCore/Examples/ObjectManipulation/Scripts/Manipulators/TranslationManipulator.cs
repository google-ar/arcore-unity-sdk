//-----------------------------------------------------------------------
// <copyright file="TranslationManipulator.cs" company="Google">
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
    /// Manipulates the position of an object via a drag gesture.
    /// If not selected, the object will be selected when the drag gesture starts.
    /// </summary>
    public class TranslationManipulator : Manipulator
    {
        /// <summary>
        /// The translation mode of this object.
        /// </summary>
        public TranslationMode ObjectTranslationMode;

        /// <summary>
        /// The maximum translation distance of this object.
        /// </summary>
        public float MaxTranslationDistance;

        private const float k_PositionSpeed = 12.0f;
        private const float k_DiffThreshold = 0.0001f;

        private bool m_IsActive = false;
        private Vector3 m_DesiredLocalPosition;
        private Quaternion m_DesiredRotation;
        private TrackableHit m_LastHit;

        /// <summary>
        /// Translation mode.
        /// </summary>
        public enum TranslationMode
        {
            Horizontal,
            Vertical,
            Any,
        }

        /// <summary>
        /// The Unity's Start method.
        /// </summary>
        protected void Start()
        {
            m_DesiredLocalPosition = new Vector3(0, 0, 0);
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
        /// Continues the translation.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnContinueManipulation(DragGesture gesture)
        {
            m_IsActive = true;
            TrackableHit hit;
            if (Frame.Raycast(gesture.Position.x, gesture.Position.y, TrackableHitFlags.PlaneWithinBounds, out hit))
            {
                if (hit.Trackable is DetectedPlane)
                {
                    DetectedPlane plane = hit.Trackable as DetectedPlane;
                    if (IsPlaneTypeAllowed(plane.PlaneType))
                    {
                        Vector3 desiredPosition = hit.Pose.position;

                        m_DesiredLocalPosition = transform.parent.InverseTransformPoint(desiredPosition);

                        // Rotate if the plane direction has changed.
                        if (((hit.Pose.rotation * Vector3.up) - transform.up).magnitude > k_DiffThreshold)
                        {
                            m_DesiredRotation = hit.Pose.rotation;
                        }
                        else
                        {
                            m_DesiredRotation = transform.rotation;
                        }

                        // If desired position is lower than current position, don't drop it until it's finished.
                        if (m_DesiredLocalPosition.y < transform.localPosition.y)
                        {
                            m_DesiredLocalPosition.y = transform.localPosition.y;
                        }

                        if (m_DesiredLocalPosition.magnitude > MaxTranslationDistance)
                        {
                            m_DesiredLocalPosition = m_DesiredLocalPosition.normalized * MaxTranslationDistance;
                        }

                        m_LastHit = hit;
                    }
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

            Pose desiredPose = m_LastHit.Pose;

            Vector3 desiredLocalPosition = transform.parent.InverseTransformPoint(desiredPose.position);

            if (desiredLocalPosition.magnitude > MaxTranslationDistance)
            {
                desiredLocalPosition = desiredLocalPosition.normalized * MaxTranslationDistance;
            }

            desiredPose.position = transform.parent.TransformPoint(desiredLocalPosition);

            Anchor newAnchor = m_LastHit.Trackable.CreateAnchor(desiredPose);
            transform.parent = newAnchor.transform;

            Destroy(oldAnchor);

            m_DesiredLocalPosition = Vector3.zero;

            // Rotate if the plane direction has changed.
            if (((desiredPose.rotation * Vector3.up) - transform.up).magnitude > k_DiffThreshold)
            {
                m_DesiredRotation = desiredPose.rotation;
            }
            else
            {
                m_DesiredRotation = transform.rotation;
            }
        }

        private void UpdatePosition()
        {
            if (!m_IsActive)
            {
                return;
            }

            // Lerp position.
            Vector3 oldLocalPosition = transform.localPosition;
            Vector3 newLocalPosition =
                Vector3.Lerp(oldLocalPosition, m_DesiredLocalPosition, Time.deltaTime * k_PositionSpeed);

            float diffLenght = (m_DesiredLocalPosition - newLocalPosition).magnitude;
            if (diffLenght < k_DiffThreshold)
            {
                newLocalPosition = m_DesiredLocalPosition;
                m_IsActive = false;
            }

            transform.localPosition = newLocalPosition;

            // Lerp rotation.
            Quaternion oldRotation = transform.rotation;
            Quaternion newRotation =
                Quaternion.Lerp(oldRotation, m_DesiredRotation, Time.deltaTime * k_PositionSpeed);

            transform.rotation = newRotation;
        }

        private bool IsPlaneTypeAllowed(DetectedPlaneType planeType)
        {
            if (ObjectTranslationMode == TranslationMode.Any)
            {
                return true;
            }

            if (ObjectTranslationMode == TranslationMode.Horizontal &&
               (planeType == DetectedPlaneType.HorizontalDownwardFacing ||
                planeType == DetectedPlaneType.HorizontalUpwardFacing))
            {
                return true;
            }

            if (ObjectTranslationMode == TranslationMode.Vertical &&
               planeType == DetectedPlaneType.Vertical)
            {
                return true;
            }

            return false;
        }
    }
}
