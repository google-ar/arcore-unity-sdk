//-----------------------------------------------------------------------
// <copyright file="GestureRecognizer.cs" company="Google">
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

namespace GoogleARCore.Examples.ObjectManipulationInternal
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

    /// <summary>
    /// Base class for all Gesture Recognizers (i.e. TapGestureRecognizer).
    ///
    /// A Gesture recognizer processes touch input to determine if a gesture should start.
    /// and fires an event when the gesture is started.
    ///
    /// To determine when an gesture is finished/updated, listen to the events on the
    /// gesture object.
    /// </summary>
    /// <typeparam name="T">The actual gesture.</typeparam>
    public abstract class GestureRecognizer<T> where T : Gesture<T>
    {
        /// <summary>
        /// List of current active gestures.
        /// </summary>
        protected List<T> m_Gestures = new List<T>();

        /// <summary>
        /// Event fired when a gesture is started.
        /// To receive an event when the gesture is finished/updated, listen to
        /// events on the Gesture object.
        /// </summary>
        public event Action<T> onGestureStarted;

        /// <summary>
        /// Updates this gesture recognizer.
        /// </summary>
        public void Update()
        {
            // Instantiate gestures based on touch input.
            // Just because a gesture was created, doesn't mean that it is started.
            // For example, a DragGesture is created when the user touch's down,
            // but doesn't actually start until the touch has moved beyond a threshold.
            TryCreateGestures();

            // Update gestures and determine if they should start.
            for (int i = 0; i < m_Gestures.Count; i++)
            {
                Gesture<T> gesture = m_Gestures[i];

                gesture.Update();
            }
        }

        /// <summary>
        /// Try to recognize and create gestures.
        /// </summary>
        protected internal abstract void TryCreateGestures();

        /// <summary>
        /// Helper function for creating one finger gestures when a touch begins.
        /// </summary>
        /// <typeparam name="createGestureFunction">Function to be executed to create the
        /// gesture.</param>
        protected internal void TryCreateOneFingerGestureOnTouchBegan(
            Func<Touch, T> createGestureFunction)
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                Touch touch = Input.touches[i];
                if (touch.phase == TouchPhase.Began
                    && !GestureTouchesUtility.IsFingerIdRetained(touch.fingerId)
                    && !GestureTouchesUtility.IsTouchOffScreenEdge(touch))
                {
                    T gesture = createGestureFunction(touch);
                    gesture.onStart += OnStart;
                    gesture.onFinished += OnFinished;
                    m_Gestures.Add(gesture);
                }
            }
        }

        /// <summary>
        /// Helper function for creating two finger gestures when a touch begins.
        /// </summary>
        /// <typeparam name="createGestureFunction">Function to be executed to create the
        /// gesture.</param>
        protected internal void TryCreateTwoFingerGestureOnTouchBegan(
            Func<Touch, Touch, T> createGestureFunction)
        {
            if (Input.touches.Length < 2)
            {
                return;
            }

            for (int i = 0; i < Input.touches.Length; i++)
            {
                TryCreateGestureTwoFingerGestureOnTouchBeganForTouchIndex(i, createGestureFunction);
            }
        }

        private void TryCreateGestureTwoFingerGestureOnTouchBeganForTouchIndex(
            int touchIndex,
            Func<Touch, Touch, T> createGestureFunction)
        {
            if (Input.touches[touchIndex].phase != TouchPhase.Began)
            {
                return;
            }

            Touch touch = Input.touches[touchIndex];
            if (GestureTouchesUtility.IsFingerIdRetained(touch.fingerId)
                || GestureTouchesUtility.IsTouchOffScreenEdge(touch))
            {
                return;
            }

            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (i == touchIndex)
                {
                    continue;
                }

                // Prevents the same two touches from creating two gestures if both touches began on
                // the same frame.
                if (i < touchIndex && Input.touches[i].phase == TouchPhase.Began)
                {
                    continue;
                }

                Touch otherTouch = Input.touches[i];
                if (GestureTouchesUtility.IsFingerIdRetained(otherTouch.fingerId)
                    || GestureTouchesUtility.IsTouchOffScreenEdge(otherTouch))
                {
                    continue;
                }

                T gesture = createGestureFunction(touch, otherTouch);
                gesture.onStart += OnStart;
                gesture.onFinished += OnFinished;
                m_Gestures.Add(gesture);
            }
        }

        private void OnStart(T gesture)
        {
            if (onGestureStarted != null)
            {
                onGestureStarted(gesture);
            }
        }

        private void OnFinished(T gesture)
        {
            m_Gestures.Remove(gesture);
        }
    }
}
