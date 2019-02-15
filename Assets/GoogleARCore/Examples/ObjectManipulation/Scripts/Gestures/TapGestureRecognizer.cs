//-----------------------------------------------------------------------
// <copyright file="TapGestureRecognizer.cs" company="Google">
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
    /// Gesture Recognizer for when the user performs a tap on the touch screen.
    /// </summary>
    public class TapGestureRecognizer : GestureRecognizer<TapGesture>
    {
        private const float k_SlopInches = 0.1f;
        private const float k_TimeSeconds = 0.3f;

        /// <summary>
        /// Gets the edge slop distance to filter tap gestures.
        /// </summary>
        internal float m_SlopInches
        {
            get
            {
                return k_SlopInches;
            }
        }

        /// <summary>
        /// Gets the max time to be considered a Tap gesture.
        /// </summary>
        internal float m_TimeSeconds
        {
            get
            {
                return k_TimeSeconds;
            }
        }

        /// <summary>
        /// Creates a Tap gesture with the given touch.
        /// </summary>
        /// <param name="touch">The touch that started this gesture.</param>
        /// <returns>The created Tap gesture.</returns>
        internal TapGesture CreateGesture(Touch touch)
        {
            return new TapGesture(this, touch);
        }

        /// <summary>
        /// Tries to create a Tap Gesture.
        /// </summary>
        protected internal override void TryCreateGestures()
        {
            TryCreateOneFingerGestureOnTouchBegan(CreateGesture);
        }
    }
}