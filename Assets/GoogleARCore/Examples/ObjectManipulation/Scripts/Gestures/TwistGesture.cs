//-----------------------------------------------------------------------
// <copyright file="TwistGesture.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    /// Gesture for when the user performs a two-finger twist motion on the touch screen.
    /// </summary>
    public class TwistGesture : Gesture<TwistGesture>
    {
        private Vector2 _previousPosition1;
        private Vector2 _previousPosition2;

        /// <summary>
        /// Constructs a PinchGesture gesture.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        public TwistGesture(TwistGestureRecognizer recognizer, Touch touch1, Touch touch2) :
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
        /// Gets the delta rotation of the gesture.
        /// </summary>
        public float DeltaRotation { get; private set; }

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

            // Check that both fingers are moving.
            if (touch1.deltaPosition == Vector2.zero || touch2.deltaPosition == Vector2.zero)
            {
                return false;
            }

            float rotation = CalculateDeltaRotation(
                touch1.position, touch2.position, StartPosition1, StartPosition2);
            if (Mathf.Abs(rotation) < TwistGestureRecognizer._slopRotation)
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

            Touch touch1, touch2;
            GestureTouchesUtility.TryFindTouch(FingerId1, out touch1);
            GestureTouchesUtility.TryFindTouch(FingerId2, out touch2);
            _previousPosition1 = touch1.position;
            _previousPosition2 = touch2.position;
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
                float rotation = CalculateDeltaRotation(
                                     touch1.position,
                                     touch2.position,
                                     _previousPosition1,
                                     _previousPosition2);

                DeltaRotation = rotation;
                _previousPosition1 = touch1.position;
                _previousPosition2 = touch2.position;
                return true;
            }

            _previousPosition1 = touch1.position;
            _previousPosition2 = touch2.position;
            DeltaRotation = 0.0f;
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

        private static float CalculateDeltaRotation(
            Vector2 currentPosition1,
            Vector2 currentPosition2,
            Vector2 previousPosition1,
            Vector2 previousPosition2)
        {
            Vector2 currentDirection = (currentPosition1 - currentPosition2).normalized;
            Vector2 previousDirection = (previousPosition1 - previousPosition2).normalized;

            float sign = Mathf.Sign((previousDirection.x * currentDirection.y) -
                                    (previousDirection.y * currentDirection.x));
            return Vector2.Angle(currentDirection, previousDirection) * sign;
        }
    }
}
