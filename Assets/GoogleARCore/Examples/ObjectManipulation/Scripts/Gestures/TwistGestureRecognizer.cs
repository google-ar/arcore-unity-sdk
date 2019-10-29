//-----------------------------------------------------------------------
// <copyright file="TwistGestureRecognizer.cs" company="Google">
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
    /// Gesture Recognizer for when the user performs a two-finger twist motion on the touch screen.
    /// </summary>
    public class TwistGestureRecognizer : GestureRecognizer<TwistGesture>
    {
        private const float k_SlopRotation = 10.0f;

        internal float SlopRotation
        {
            get
            {
                return k_SlopRotation;
            }
        }

        /// <summary>
        /// Creates a Twist gesture with the given touches.
        /// </summary>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        /// <returns>The created Tap gesture.</returns>
        internal TwistGesture CreateGesture(Touch touch1, Touch touch2)
        {
            return new TwistGesture(this, touch1, touch2);
        }

        /// <summary>
        /// Tries to create a Twist Gesture.
        /// </summary>
        protected internal override void TryCreateGestures()
        {
            TryCreateTwoFingerGestureOnTouchBegan(CreateGesture);
        }
    }
}
