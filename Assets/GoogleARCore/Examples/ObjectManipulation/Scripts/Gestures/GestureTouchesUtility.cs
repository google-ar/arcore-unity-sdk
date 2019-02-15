//-----------------------------------------------------------------------
// <copyright file="GestureTouchesUtility.cs" company="Google">
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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Singleton used by Gesture's and GestureRecognizer's to interact with touch input.
    ///
    /// 1. Makes it easy to find touches by fingerId.
    /// 2. Allows Gestures to Lock/Release fingerIds.
    /// 3. Wraps Input.Touches so that it works both in editor and on device.
    /// 4. Provides helper functions for converting touch coordinates
    ///    and performing raycasts based on touches.
    /// </summary>
    internal class GestureTouchesUtility
    {
        private const float k_EdgeThresholdInches = 0.1f;
        private static GestureTouchesUtility s_Instance;

        private HashSet<int> m_RetainedFingerIds = new HashSet<int>();

        /// <summary>
        /// Initializes a new instance of the GestureTouchesUtility class. Intended for private use
        /// since this class is a Singleton.
        /// </summary>
        private GestureTouchesUtility()
        {
        }

        /// <summary>
        /// Try to find a touch for a particular finger id.
        /// </summary>
        /// <param name="fingerId">The finger id to find the touch.</param>
        /// <param name="touch">The output touch.</param>
        /// <returns>True if a touch was found.</returns>
        public static bool TryFindTouch(int fingerId, out Touch touch)
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (Input.touches[i].fingerId == fingerId)
                {
                    touch = Input.touches[i];
                    return true;
                }
            }

            touch = new Touch();
            return false;
        }

        /// <summary>
        /// Converts Pixels to Inches.
        /// </summary>
        /// <param name="pixels">The amount to convert in pixels.</param>
        /// <returns>The converted amount in inches.</returns>
        public static float PixelsToInches(float pixels)
        {
            return pixels / Screen.dpi;
        }

        /// <summary>
        /// Converts Inches to Pixels.
        /// </summary>
        /// <param name="inches">The amount to convert in inches.</param>
        /// <returns>The converted amount in pixels.</returns>
        public static float InchesToPixels(float inches)
        {
            return inches * Screen.dpi;
        }

        /// <summary>
        /// Used to determine if a touch is off the edge of the screen based on some slop.
        /// Useful to prevent accidental touches from simply holding the device from causing
        /// confusing behavior.
        /// </summary>
        /// <param name="touch">The touch to check.</param>
        /// <returns>True if the touch is off screen edge.</returns>
        public static bool IsTouchOffScreenEdge(Touch touch)
        {
            float slopPixels = InchesToPixels(k_EdgeThresholdInches);

            bool result = touch.position.x <= slopPixels;
            result |= touch.position.y <= slopPixels;
            result |= touch.position.x >= Screen.width - slopPixels;
            result |= touch.position.y >= Screen.height - slopPixels;

            return result;
        }

        /// <summary>
        /// Performs a Raycast from the camera.
        /// </summary>
        /// <param name="screenPos">The screen position to perform the raycast from.</param>
        /// <param name="result">The RaycastHit result.</param>
        /// <returns>True if an object was hit.</returns>
        public static bool RaycastFromCamera(Vector2 screenPos, out RaycastHit result)
        {
            if (Camera.main == null)
            {
                result = new RaycastHit();
                return false;
            }

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                result = hit;
                return true;
            }

            result = hit;
            return false;
        }

        /// <summary>
        /// Locks a finger Id.
        /// </summary>
        /// <param name="fingerId">The finger id to lock.</param>
        public static void LockFingerId(int fingerId)
        {
            if (!IsFingerIdRetained(fingerId))
            {
                _GetInstance().m_RetainedFingerIds.Add(fingerId);
            }
        }

        /// <summary>
        /// Releases a finger Id.
        /// </summary>
        /// <param name="fingerId">The finger id to release.</param>
        public static void ReleaseFingerId(int fingerId)
        {
            if (IsFingerIdRetained(fingerId))
            {
                _GetInstance().m_RetainedFingerIds.Remove(fingerId);
            }
        }

        /// <summary>
        /// Returns true if the finger Id is retained.
        /// </summary>
        /// <param name="fingerId">The finger id to check.</param>
        /// <returns>True if the finger is retained.</returns>
        public static bool IsFingerIdRetained(int fingerId)
        {
            return _GetInstance().m_RetainedFingerIds.Contains(fingerId);
        }

        /// <summary>
        /// Initializes the GestureTouchesUtility singleton if needed and returns a valid instance.
        /// </summary>
        /// <returns>The instance of GestureTouchesUtility.</returns>
        private static GestureTouchesUtility _GetInstance()
        {
            if (s_Instance == null)
            {
                s_Instance = new GestureTouchesUtility();
            }

            return s_Instance;
        }
    }
}
