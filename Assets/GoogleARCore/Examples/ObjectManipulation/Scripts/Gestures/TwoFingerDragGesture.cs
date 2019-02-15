//-----------------------------------------------------------------------
// <copyright file="TwoFingerDragGesture.cs" company="Google">
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
    /// Gesture for when the user performs a two finger vertical swipe motion on the touch screen.
    /// </summary>
    public class TwoFingerDragGesture : Gesture<TwoFingerDragGesture>
    {
        /// <summary>
        /// Constructs a two finger drag gesture.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        public TwoFingerDragGesture(TwoFingerDragGestureRecognizer recognizer, Touch touch1, Touch touch2) : base(recognizer)
        {
            FingerId1 = touch1.fingerId;
            StartPosition1 = touch1.position;
            FingerId2 = touch2.fingerId;
            StartPosition2 = touch2.position;
            Position = (StartPosition1 + StartPosition2) / 2;
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
        /// Gets the current screen position of the gesture.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Gets the delta screen position of the gesture.
        /// </summary>
        public Vector2 Delta { get; private set; }

        /// <summary>
        /// Returns true if this gesture can start.
        /// </summary>
        /// <returns>True if the gesture can start.</returns>
        protected internal override bool CanStart()
        {
            if (GestureTouchesUtility.IsFingerIdRetained(FingerId1) || GestureTouchesUtility.IsFingerIdRetained(FingerId2))
            {
                Cancel();
                return false;
            }

            Touch touch1, touch2;
            bool foundTouches = GestureTouchesUtility.TryFindTouch(FingerId1, out touch1);
            foundTouches = GestureTouchesUtility.TryFindTouch(FingerId2, out touch2) && foundTouches;

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

            Vector2 pos1 = touch1.position;
            float diff1 = (pos1 - StartPosition1).magnitude;
            Vector2 pos2 = touch2.position;
            float diff2 = (pos2 - StartPosition2).magnitude;
            if (GestureTouchesUtility.PixelsToInches(diff1) < (m_Recognizer as TwoFingerDragGestureRecognizer).m_SlopInches
                || GestureTouchesUtility.PixelsToInches(diff2) < (m_Recognizer as TwoFingerDragGestureRecognizer).m_SlopInches)
            {
                return false;
            }

            TwoFingerDragGestureRecognizer recognizer = m_Recognizer as TwoFingerDragGestureRecognizer;

            // Check both fingers move in the same direction.
            float dot = Vector3.Dot(touch1.deltaPosition.normalized, touch2.deltaPosition.normalized);
            if (dot < Mathf.Cos(recognizer.m_AngleThresholdRadians))
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

            RaycastHit hit1;
            RaycastHit hit2;
            if (GestureTouchesUtility.RaycastFromCamera(StartPosition1, out hit1))
            {
                var gameObject = hit1.transform.gameObject;
                if (gameObject != null)
                {
                    TargetObject = gameObject.GetComponentInParent<Manipulator>().gameObject;
                }
            }
            else if (GestureTouchesUtility.RaycastFromCamera(StartPosition2, out hit2))
            {
                var gameObject = hit2.transform.gameObject;
                if (gameObject != null)
                {
                    TargetObject = gameObject.GetComponentInParent<Manipulator>().gameObject;
                }
            }

            Touch touch1;
            GestureTouchesUtility.TryFindTouch(FingerId1, out touch1);
            Touch touch2;
            GestureTouchesUtility.TryFindTouch(FingerId2, out touch2);
            Position = (touch1.position + touch2.position) / 2;
        }

        /// <summary>
        /// Updates this gesture.
        /// </summary>
        /// <returns>True if the update was successful.</returns>
        protected internal override bool UpdateGesture()
        {
            Touch touch1, touch2;
            bool foundTouches = GestureTouchesUtility.TryFindTouch(FingerId1, out touch1);
            foundTouches = GestureTouchesUtility.TryFindTouch(FingerId2, out touch2) && foundTouches;

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
                Delta = ((touch1.position + touch2.position) / 2) - Position;
                Position = (touch1.position + touch2.position) / 2;
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
