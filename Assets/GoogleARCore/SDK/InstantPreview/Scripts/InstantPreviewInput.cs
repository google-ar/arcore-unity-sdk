//-----------------------------------------------------------------------
// <copyright file="InstantPreviewInput.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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

namespace GoogleARCore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Helper class that provides touch input in lieu of Input.GetTouch when
    /// running the Unity Editor.
    /// </summary>
    public static class InstantPreviewInput
    {
        private static Touch[] _touches = new Touch[0];
        private static List<Touch> _touchList = new List<Touch>();

        /// <summary>
        /// Gets the inputString from Instant Preview since the last update.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
         "US1109:PublicPropertiesMustBeUpperCamelCase", Justification = "Overridden field.")]
        public static string inputString
        {
            get
            {
                return Input.inputString;
            }
        }

        /// <summary>
        /// Gets the available touch inputs from Instant Preview since the last
        /// update.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
         "US1109:PublicPropertiesMustBeUpperCamelCase", Justification = "Overridden field.")]
        public static Touch[] touches
        {
            get
            {
                NativeApi.UnityGotTouches();
                return _touches;
            }
        }

        /// <summary>
        /// Gets the number of touches available from Instant preview since the
        /// last update.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
         "US1109:PublicPropertiesMustBeUpperCamelCase", Justification = "Overridden field.")]
        public static int touchCount
        {
            get
            {
                return touches.Length;
            }
        }

        /// <summary>
        /// Gets return value of Input.mousePosition.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
         "US1109:PublicPropertiesMustBeUpperCamelCase", Justification = "Overridden field.")]
        public static Vector3 mousePosition
        {
            get
            {
                return Input.mousePosition;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a mouse device is detected.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
         "US1109:PublicPropertiesMustBeUpperCamelCase", Justification = "Overridden field.")]
        public static bool mousePresent
        {
            get
            {
                return Input.mousePresent;
            }
        }

        /// <summary>
        /// Gets a specific touch input from Instant Preview by index.
        /// </summary>
        /// <param name="index">Index of touch input to get.</param>
        /// <returns>Touch data.</returns>
        public static Touch GetTouch(int index)
        {
            return touches[index];
        }

        /// <summary>
        /// Passthrough function to Input.GetKey.
        /// </summary>
        /// <param name="keyCode">Key parameter to pass to Input.GetKey.</param>
        /// <returns>Key state returned from Input.GetKey.</returns>
        public static bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        /// <summary>
        /// Passthrough function to Input.GetMouseButton.
        /// </summary>
        /// <param name="button">Button index.</param>
        /// <returns>Return value of Input.GetMouseButton.</returns>
        public static bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        /// <summary>
        /// Passthrough function to Input.GetMouseButtonDown.
        /// </summary>
        /// <param name="button">Button index.</param>
        /// <returns>Return value of Input.GetMouseButtonDown.</returns>
        public static bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        /// <summary>
        /// Passthrough function to Input.GetMouseButtonUp.
        /// </summary>
        /// <param name="button">Button index.</param>
        /// <returns>Return value of Input.GetMouseButtonUp.</returns>
        public static bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }

        /// <summary>
        /// Refreshes touch inputs from Instant Preview to reflect the state
        /// since the last time Update was called.
        /// </summary>
        public static void Update()
        {
            if (!InstantPreviewManager.IsProvidingPlatform)
            {
                return;
            }

            // Removes ended touches, and converts moves to stationary.
            for (int i = 0; i < _touchList.Count; ++i)
            {
                if (_touchList[i].phase == TouchPhase.Ended)
                {
                    _touchList.RemoveAt(i);
                    --i;
                    continue;
                }

                var curTouch = _touchList[i];
                curTouch.phase = TouchPhase.Stationary;
                curTouch.deltaPosition = Vector2.zero;
                _touchList[i] = curTouch;
            }

            // Updates touches.
            IntPtr nativeTouchesPtr;
            int nativeTouchCount;
            NativeApi.GetTouches(out nativeTouchesPtr, out nativeTouchCount);

            var structSize = Marshal.SizeOf(typeof(NativeTouch));
            for (var i = 0; i < nativeTouchCount; ++i)
            {
                var source = new IntPtr(nativeTouchesPtr.ToInt64() + (i * structSize));
                NativeTouch nativeTouch =
                    (NativeTouch)Marshal.PtrToStructure(source, typeof(NativeTouch));

                var newTouch = new Touch()
                {
                    fingerId = nativeTouch.Id,
                    phase = nativeTouch.Phase,
                    pressure = nativeTouch.Pressure,

                    // NativeTouch values are normalized and must be converted to screen
                    // coordinates.
                    // Note that the Unity's screen coordinate (0, 0) starts from bottom left.
                    position = new Vector2(
                        Screen.width * nativeTouch.X, Screen.height * (1f - nativeTouch.Y)),
                };

                var index = _touchList.FindIndex(touch => touch.fingerId == newTouch.fingerId);

                // Adds touch if not found, otherwise updates it.
                if (index < 0)
                {
                    _touchList.Add(newTouch);
                }
                else
                {
                    var prevTouch = _touchList[index];
                    newTouch.deltaPosition += newTouch.position - prevTouch.position;
                    _touchList[index] = newTouch;
                }
            }

            _touches = _touchList.ToArray();
        }

        private struct NativeTouch
        {
#pragma warning disable 649
            public TouchPhase Phase;
            public float X;
            public float Y;
            public float Pressure;
            public int Id;
#pragma warning restore 649
        }

        private struct NativeApi
        {
            [DllImport(InstantPreviewManager.InstantPreviewNativeApi)]
            public static extern void GetTouches(out IntPtr touches, out int count);

            [DllImport(InstantPreviewManager.InstantPreviewNativeApi)]
            public static extern void UnityGotTouches();
        }
    }
}
