//-----------------------------------------------------------------------
// <copyright file="TapGesture.cs" company="Google LLC">
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
    /// Gesture for when the user performs a tap on the touch screen.
    /// </summary>
    public class TapGesture : Gesture<TapGesture>
    {
        private float m_ElapsedTime = 0.0f;

        /// <summary>
        /// Constructs a Tap gesture.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        /// <param name="touch">The touch that started this gesture.</param>
        internal TapGesture(TapGestureRecognizer recognizer, Touch touch) : base(recognizer)
        {
            FingerId = touch.fingerId;
            StartPosition = touch.position;
        }

        /// <summary>
        /// Gets the id of the finger used in this gesture.
        /// </summary>
        public int FingerId { get; private set; }

        /// <summary>
        /// Gets the screen position where the gesture started.
        /// </summary>
        public Vector2 StartPosition { get; private set; }

        /// <summary>
        /// Returns true if this gesture can start.
        /// </summary>
        /// <returns>True if the gesture can start.</returns>
        protected internal override bool CanStart()
        {
            if (GestureTouchesUtility.IsFingerIdRetained(FingerId))
            {
                Cancel();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Action to be performed when this gesture is started.
        /// </summary>
        protected internal override void OnStart()
        {
            RaycastHit hit;
            if (GestureTouchesUtility.RaycastFromCamera(StartPosition, out hit))
            {
                var gameObject = hit.transform.gameObject;
                if (gameObject != null)
                {
                    TargetObject = gameObject.GetComponentInParent<Manipulator>().gameObject;
                }
            }
        }

        /// <summary>
        /// Updates this gesture.
        /// </summary>
        /// <returns>True if the update was successful.</returns>
        protected internal override bool UpdateGesture()
        {
            Touch touch;
            if (GestureTouchesUtility.TryFindTouch(FingerId, out touch))
            {
                TapGestureRecognizer tapRecognizer = Recognizer as TapGestureRecognizer;
                m_ElapsedTime += touch.deltaTime;
                if (m_ElapsedTime > tapRecognizer.TimeSeconds)
                {
                    Cancel();
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    float diff = (touch.position - StartPosition).magnitude;
                    float diffInches = GestureTouchesUtility.PixelsToInches(diff);
                    if (diffInches > tapRecognizer.SlopInches)
                    {
                        Cancel();
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Complete();
                }
            }
            else
            {
                Cancel();
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
        }
    }
}
