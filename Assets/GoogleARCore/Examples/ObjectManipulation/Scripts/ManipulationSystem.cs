//-----------------------------------------------------------------------
// <copyright file="ManipulationSystem.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Manipulation system allows the user to manipulate virtual objects (select, translate,
    /// rotate, scale and elevate) through gestures (tap, drag, twist, swipe).
    /// Manipulation system also handles the current selected object and its visualization.
    ///
    /// To enable it add one ManipulationSystem to your scene and one Manipulator as parent of each
    /// of your virtual objects.
    /// </summary>
    public class ManipulationSystem : MonoBehaviour
    {
        private static ManipulationSystem _instance = null;

        private DragGestureRecognizer _dragGestureRecognizer = new DragGestureRecognizer();

        private PinchGestureRecognizer _pinchGestureRecognizer = new PinchGestureRecognizer();

        private TwoFingerDragGestureRecognizer _twoFingerDragGestureRecognizer =
            new TwoFingerDragGestureRecognizer();

        private TapGestureRecognizer _tapGestureRecognizer = new TapGestureRecognizer();

        private TwistGestureRecognizer _twistGestureRecognizer = new TwistGestureRecognizer();

        /// <summary>
        /// Gets the ManipulationSystem instance.
        /// </summary>
        public static ManipulationSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var manipulationSystems = FindObjectsOfType<ManipulationSystem>();
                    if (manipulationSystems.Length > 0)
                    {
                        _instance = manipulationSystems[0];
                    }
                    else
                    {
                        Debug.LogError("No instance of ManipulationSystem exists in the scene.");
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets the Drag gesture recognizer.
        /// </summary>
        public DragGestureRecognizer DragGestureRecognizer
        {
            get
            {
                return _dragGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Pinch gesture recognizer.
        /// </summary>
        public PinchGestureRecognizer PinchGestureRecognizer
        {
            get
            {
                return _pinchGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the two finger drag gesture recognizer.
        /// </summary>
        public TwoFingerDragGestureRecognizer TwoFingerDragGestureRecognizer
        {
            get
            {
                return _twoFingerDragGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Tap gesture recognizer.
        /// </summary>
        public TapGestureRecognizer TapGestureRecognizer
        {
            get
            {
                return _tapGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Twist gesture recognizer.
        /// </summary>
        public TwistGestureRecognizer TwistGestureRecognizer
        {
            get
            {
                return _twistGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the current selected object.
        /// </summary>
        public GameObject SelectedObject { get; private set;  }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            if (Instance != this)
            {
                Debug.LogWarning("Multiple instances of ManipulationSystem detected in the scene." +
                                 " Only one instance can exist at a time. The duplicate instances" +
                                 " will be destroyed.");
                DestroyImmediate(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            DragGestureRecognizer.Update();
            PinchGestureRecognizer.Update();
            TwoFingerDragGestureRecognizer.Update();
            TapGestureRecognizer.Update();
            TwistGestureRecognizer.Update();
        }

        /// <summary>
        /// Deselects the selected object if any.
        /// </summary>
        internal void Deselect()
        {
            SelectedObject = null;
        }

        /// <summary>
        /// Select an object.
        /// </summary>
        /// <param name="target">The object to select.</param>
        internal void Select(GameObject target)
        {
            if (SelectedObject == target)
            {
                return;
            }

            Deselect();
            SelectedObject = target;
        }
    }
}
