//-----------------------------------------------------------------------
// <copyright file="ManipulationSystem.cs" company="Google">
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
        private static ManipulationSystem s_Instance = null;

        private DragGestureRecognizer m_DragGestureRecognizer = new DragGestureRecognizer();

        private PinchGestureRecognizer m_PinchGestureRecognizer = new PinchGestureRecognizer();

        private TwoFingerDragGestureRecognizer m_TwoFingerDragGestureRecognizer =
            new TwoFingerDragGestureRecognizer();

        private TapGestureRecognizer m_TapGestureRecognizer = new TapGestureRecognizer();

        private TwistGestureRecognizer m_TwistGestureRecognizer = new TwistGestureRecognizer();

        /// <summary>
        /// Gets the ManipulationSystem instance.
        /// </summary>
        public static ManipulationSystem Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var manipulationSystems = FindObjectsOfType<ManipulationSystem>();
                    if (manipulationSystems.Length > 0)
                    {
                        s_Instance = manipulationSystems[0];
                    }
                    else
                    {
                        Debug.LogError("No instance of ManipulationSystem exists in the scene.");
                    }
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// Gets the Drag gesture recognizer.
        /// </summary>
        public DragGestureRecognizer DragGestureRecognizer
        {
            get
            {
                return m_DragGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Pinch gesture recognizer.
        /// </summary>
        public PinchGestureRecognizer PinchGestureRecognizer
        {
            get
            {
                return m_PinchGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the two finger drag gesture recognizer.
        /// </summary>
        public TwoFingerDragGestureRecognizer TwoFingerDragGestureRecognizer
        {
            get
            {
                return m_TwoFingerDragGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Tap gesture recognizer.
        /// </summary>
        public TapGestureRecognizer TapGestureRecognizer
        {
            get
            {
                return m_TapGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Twist gesture recognizer.
        /// </summary>
        public TwistGestureRecognizer TwistGestureRecognizer
        {
            get
            {
                return m_TwistGestureRecognizer;
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
