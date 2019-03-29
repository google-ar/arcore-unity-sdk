//-----------------------------------------------------------------------
// <copyright file="PinchGesture.cs" company="Google">
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
    using GoogleARCore.Examples.ObjectManipulationInternal;
    using UnityEngine;

    /// <summary>
    /// Gesture for when the user performs a two-finger pinch motion on the touch screen.
    /// </summary>
    public class PinchGesture : Gesture<PinchGesture>
    {
        /// <summary>
        /// Constructs a PinchGesture gesture.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        public PinchGesture(PinchGestureRecognizer recognizer, Touch touch1, Touch touch2) :
            base(recognizer)
        {
            FingerId1 = touch1.fingerId;
            FingerId2 = touch2.fingerId;
            StartPosition1 = touch1.position;
            StartPosition2 = touch2.position;
        }

        /// <summary>
        /// Gets the id of the first finger used in this gesture.
        /// </summary>
        public int FingerId1 { get; private set; }

        /// <summary>
        /// Gets the id of the second finger used in this gesture.
        /// </summary>
        public int FingerId2 { get; private set; }

        /// <summary>
        /// Gets the screen position of the first finger where the gesture started.
        /// </summary>
        public Vector2 StartPosition1 { get; private set; }

        /// <summary>
        /// Gets the screen position of the second finger where the gesture started.
        /// </summary>
        public Vector2 StartPosition2 { get; private set; }

        /// <summary>
        /// Gets the gap between then position of the first and second fingers.
        /// </summary>
        public float Gap { get; private set; }

        /// <summary>
        /// Gets the gap delta between then position of the first and second fingers.
        /// </summary>
        public float GapDelta { get; private set; }

        /// <summary>
        /// Returns true if this gesture can start.
        /// </summary>
        /// <returns>True if the gesture can start.</returns>
        protected internal override bool CanStart()
        {
            if (GestureTouchesUtility.IsFingerIdRetained(FingerId1) ||
                GestureTouchesUtility.IsFingerIdRetained(FingerId2))
            {
                Cancel();
                return false;
            }

            Touch touch1, touch2;
            bool foundTouches = GestureTouchesUtility.TryFindTouch(FingerId1, out touch1);
            foundTouches =
                GestureTouchesUtility.TryFindTouch(FingerId2, out touch2) && foundTouches;

            if (!foundTouches)
            {
                Cancel();
                return false;
            }

            // Check that at least one finger is moving.
            if (touch1.deltaPosition == Vector2.zero && touch2.deltaPosition == Vector2.zero)
            {
                return false;
            }

            PinchGestureRecognizer pinchRecognizer = m_Recognizer as PinchGestureRecognizer;

            Vector3 firstToSecondDirection = (StartPosition1 - StartPosition2).normalized;
            float dot1 = Vector3.Dot(touch1.deltaPosition.normalized, -firstToSecondDirection);
            float dot2 = Vector3.Dot(touch2.deltaPosition.normalized, firstToSecondDirection);
            float dotThreshold =
                Mathf.Cos(pinchRecognizer.m_SlopMotionDirectionDegrees * Mathf.Deg2Rad);

            // Check angle of motion for the first touch.
            if (touch1.deltaPosition != Vector2.zero && Mathf.Abs(dot1) < dotThreshold)
            {
                return false;
            }

            // Check angle of motion for the second touch.
            if (touch2.deltaPosition != Vector2.zero && Mathf.Abs(dot2) < dotThreshold)
            {
                return false;
            }

            float startgap = (StartPosition1 - StartPosition2).magnitude;
            Gap = (touch1.position - touch2.position).magnitude;
            float separation = GestureTouchesUtility.PixelsToInches(Mathf.Abs(Gap - startgap));
            if (separation < pinchRecognizer.m_SlopInches)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Action to be performed when this gesture is started.
        /// </summary>
        protected internal override void OnStart()
        {
            GestureTouchesUtility.LockFingerId(FingerId1);
            GestureTouchesUtility.LockFingerId(FingerId2);
        }

        /// <summary>
        /// Updates this gesture.
        /// </summary>
        /// <returns>True if the update was successful.</returns>
        protected internal override bool UpdateGesture()
        {
            Touch touch1, touch2;
            bool foundTouches = GestureTouchesUtility.TryFindTouch(FingerId1, out touch1);
            foundTouches =
                GestureTouchesUtility.TryFindTouch(FingerId2, out touch2) && foundTouches;

            if (!foundTouches)
            {
                Cancel();
                return false;
            }

            if (touch1.phase == TouchPhase.Canceled || touch2.phase == TouchPhase.Canceled)
            {
                Cancel();
                return false;
            }

            if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
            {
                Complete();
                return false;
            }

            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float newgap = (touch1.position - touch2.position).magnitude;
                GapDelta = newgap - Gap;
                Gap = newgap;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Action to be performed when this gesture is cancelled.
        /// </summary>
        protected internal override void OnCancel()
        {
        }

        /// <summary>
        /// Action to be performed when this gesture is finished.
        /// </summary>
        protected internal override void OnFinish()
        {
            GestureTouchesUtility.ReleaseFingerId(FingerId1);
            GestureTouchesUtility.ReleaseFingerId(FingerId2);
        }
    }
}
