//-----------------------------------------------------------------------
// <copyright file="DragGesture.cs" company="Google LLC">
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

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

    /// <summary>
    /// Gesture for when the user performs a drag motion on the touch screen.
    /// </summary>
    public class DragGesture : Gesture<DragGesture>
    {
        /// <summary>
        /// Constructs a DragGesture gesture.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        /// <param name="touch">The touch that started this gesture.</param>
        public DragGesture(DragGestureRecognizer recognizer, Touch touch) : base(recognizer)
        {
            FingerId = touch.fingerId;
            StartPosition = touch.position;
            Position = StartPosition;
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
            if (GestureTouchesUtility.IsFingerIdRetained(FingerId))
            {
                Cancel();
                return false;
            }

            if (Input.touches.Length > 1)
            {
                for (int i = 0; i < Input.touches.Length; i++)
                {
                    Touch currentTouch = Input.touches[i];
                    if (currentTouch.fingerId != FingerId
                        && !GestureTouchesUtility.IsFingerIdRetained(currentTouch.fingerId))
                    {
                        return false;
                    }
                }
            }

            Touch touch;
            if (GestureTouchesUtility.TryFindTouch(FingerId, out touch))
            {
                Vector2 pos = touch.position;
                float diff = (pos - StartPosition).magnitude;
                if (GestureTouchesUtility.PixelsToInches(diff) >= DragGestureRecognizer._slopInches)
                {
                    return true;
                }
            }
            else
            {
                Cancel();
            }

            return false;
        }

        /// <summary>
        /// Action to be performed when this gesture is started.
        /// </summary>
        protected internal override void OnStart()
        {
            GestureTouchesUtility.LockFingerId(FingerId);

            RaycastHit hit;
            if (GestureTouchesUtility.RaycastFromCamera(StartPosition, out hit))
            {
                var gameObject = hit.transform.gameObject;
                if (gameObject != null)
                {
                    TargetObject = gameObject.GetComponentInParent<Manipulator>().gameObject;
                }
            }

            Touch touch;
            GestureTouchesUtility.TryFindTouch(FingerId, out touch);
            Position = touch.position;
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
                if (touch.phase == TouchPhase.Moved)
                {
                    Delta = touch.position - Position;
                    Position = touch.position;
                    return true;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Complete();
                }
                else if (touch.phase == TouchPhase.Canceled)
                {
                    Cancel();
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
            GestureTouchesUtility.ReleaseFingerId(FingerId);
        }
    }
}
