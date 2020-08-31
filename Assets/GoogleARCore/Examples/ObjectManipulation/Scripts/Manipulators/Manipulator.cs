//-----------------------------------------------------------------------
// <copyright file="Manipulator.cs" company="Google LLC">
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
    /// Base class that manipulates an object via a gesture.
    /// </summary>
    public abstract class Manipulator : MonoBehaviour
    {
        private bool _isManipulating;

        private GameObject _selectedObject;

        /// <summary>
        /// Makes this game object become the Selected Object.
        /// </summary>
        public void Select()
        {
            ManipulationSystem.Instance.Select(gameObject);
        }

        /// <summary>
        /// Unselects this game object if it is currently the Selected Object.
        /// </summary>
        public void Deselect()
        {
            if (IsSelected())
            {
                ManipulationSystem.Instance.Deselect();
            }
        }

        /// <summary>
        /// Whether this game object is the Selected Object.
        /// </summary>
        /// <returns><c>true</c>, if this is the Selected Object, <c>false</c> otherwise.</returns>
        public bool IsSelected()
        {
            return _selectedObject == gameObject;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(DragGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(PinchGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(TapGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(TwistGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(TwoFingerDragGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(DragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(PinchGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(TapGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(TwistGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(TwoFingerDragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(DragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(PinchGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(TapGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(TwistGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(TwoFingerDragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(DragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(PinchGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(TapGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(TwistGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(TwoFingerDragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when this game object becomes the Selected Object.
        /// </summary>
        protected virtual void OnSelected()
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when this game object is deselected if it was the Selected Object.
        /// </summary>
        protected virtual void OnDeselected()
        {
            // Optional override.
        }

        /// <summary>
        /// Enables the manipulator.
        /// </summary>
        protected virtual void OnEnable()
        {
            ConnectToRecognizers();
        }

        /// <summary>
        /// Disables the manipulator.
        /// </summary>
        protected virtual void OnDisable()
        {
            DisconnectFromRecognizers();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        protected virtual void Update()
        {
            if (_selectedObject == gameObject &&
                ManipulationSystem.Instance.SelectedObject != gameObject)
            {
                _selectedObject = ManipulationSystem.Instance.SelectedObject;
                OnDeselected();
            }
            else if (_selectedObject != gameObject &&
                ManipulationSystem.Instance.SelectedObject == gameObject)
            {
                _selectedObject = ManipulationSystem.Instance.SelectedObject;
                OnSelected();
            }
            else
            {
                _selectedObject = ManipulationSystem.Instance.SelectedObject;
            }
        }

        private void ConnectToRecognizers()
        {
            if (ManipulationSystem.Instance == null)
            {
                Debug.LogError("Manipulation system not found in scene.");
                return;
            }

            DragGestureRecognizer dragGestureRecognizer =
                ManipulationSystem.Instance.DragGestureRecognizer;
            if (dragGestureRecognizer != null)
            {
                dragGestureRecognizer.onGestureStarted += OnGestureStarted;
            }

            PinchGestureRecognizer pinchGestureRecognizer =
                ManipulationSystem.Instance.PinchGestureRecognizer;
            if (pinchGestureRecognizer != null)
            {
                pinchGestureRecognizer.onGestureStarted += OnGestureStarted;
            }

            TapGestureRecognizer tapGestureRecognizer =
                ManipulationSystem.Instance.TapGestureRecognizer;
            if (tapGestureRecognizer != null)
            {
                tapGestureRecognizer.onGestureStarted += OnGestureStarted;
            }

            TwistGestureRecognizer twistGestureRecognizer =
                ManipulationSystem.Instance.TwistGestureRecognizer;
            if (twistGestureRecognizer != null)
            {
                twistGestureRecognizer.onGestureStarted += OnGestureStarted;
            }

            TwoFingerDragGestureRecognizer twoFingerDragGestureRecognizer =
                ManipulationSystem.Instance.TwoFingerDragGestureRecognizer;
            if (twoFingerDragGestureRecognizer != null)
            {
                twoFingerDragGestureRecognizer.onGestureStarted += OnGestureStarted;
            }
        }

        private void DisconnectFromRecognizers()
        {
            if (ManipulationSystem.Instance == null)
            {
                Debug.LogError("Manipulation system not found in scene.");
                return;
            }

            DragGestureRecognizer dragGestureRecognizer =
                ManipulationSystem.Instance.DragGestureRecognizer;
            if (dragGestureRecognizer != null)
            {
                dragGestureRecognizer.onGestureStarted -= OnGestureStarted;
            }

            PinchGestureRecognizer pinchGestureRecognizer =
                ManipulationSystem.Instance.PinchGestureRecognizer;
            if (pinchGestureRecognizer != null)
            {
                pinchGestureRecognizer.onGestureStarted -= OnGestureStarted;
            }

            TapGestureRecognizer tapGestureRecognizer =
                ManipulationSystem.Instance.TapGestureRecognizer;
            if (tapGestureRecognizer != null)
            {
                tapGestureRecognizer.onGestureStarted -= OnGestureStarted;
            }

            TwistGestureRecognizer twistGestureRecognizer =
                ManipulationSystem.Instance.TwistGestureRecognizer;
            if (twistGestureRecognizer != null)
            {
                twistGestureRecognizer.onGestureStarted -= OnGestureStarted;
            }

            TwoFingerDragGestureRecognizer twoFingerDragGestureRecognizer =
                ManipulationSystem.Instance.TwoFingerDragGestureRecognizer;
            if (twoFingerDragGestureRecognizer != null)
            {
                twoFingerDragGestureRecognizer.onGestureStarted -= OnGestureStarted;
            }
        }

        private void OnGestureStarted(DragGesture gesture)
        {
            if (_isManipulating)
            {
                return;
            }

            if (CanStartManipulationForGesture(gesture))
            {
                _isManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        private void OnGestureStarted(PinchGesture gesture)
        {
            if (_isManipulating)
            {
                return;
            }

            if (CanStartManipulationForGesture(gesture))
            {
                _isManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        private void OnGestureStarted(TapGesture gesture)
        {
            if (_isManipulating)
            {
                return;
            }

            if (CanStartManipulationForGesture(gesture))
            {
                _isManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        private void OnGestureStarted(TwistGesture gesture)
        {
            if (_isManipulating)
            {
                return;
            }

            if (CanStartManipulationForGesture(gesture))
            {
                _isManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        private void OnGestureStarted(TwoFingerDragGesture gesture)
        {
            if (_isManipulating)
            {
                return;
            }

            if (CanStartManipulationForGesture(gesture))
            {
                _isManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        private void OnUpdated(DragGesture gesture)
        {
            if (!_isManipulating)
            {
                return;
            }

            // Can only transform selected Items.
            if (ManipulationSystem.Instance.SelectedObject != gameObject)
            {
                _isManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        private void OnUpdated(PinchGesture gesture)
        {
            if (!_isManipulating)
            {
                return;
            }

            // Can only transform selected Items.
            if (ManipulationSystem.Instance.SelectedObject != gameObject)
            {
                _isManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        private void OnUpdated(TapGesture gesture)
        {
            if (!_isManipulating)
            {
                return;
            }

            // Can only transform selected Items.
            if (ManipulationSystem.Instance.SelectedObject != gameObject)
            {
                _isManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        private void OnUpdated(TwistGesture gesture)
        {
            if (!_isManipulating)
            {
                return;
            }

            // Can only transform selected Items.
            if (ManipulationSystem.Instance.SelectedObject != gameObject)
            {
                _isManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        private void OnUpdated(TwoFingerDragGesture gesture)
        {
            if (!_isManipulating)
            {
                return;
            }

            // Can only transform selected Items.
            if (ManipulationSystem.Instance.SelectedObject != gameObject)
            {
                _isManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        private void OnFinished(DragGesture gesture)
        {
            _isManipulating = false;
            OnEndManipulation(gesture);
        }

        private void OnFinished(PinchGesture gesture)
        {
            _isManipulating = false;
            OnEndManipulation(gesture);
        }

        private void OnFinished(TapGesture gesture)
        {
            _isManipulating = false;
            OnEndManipulation(gesture);
        }

        private void OnFinished(TwistGesture gesture)
        {
            _isManipulating = false;
            OnEndManipulation(gesture);
        }

        private void OnFinished(TwoFingerDragGesture gesture)
        {
            _isManipulating = false;
            OnEndManipulation(gesture);
        }
    }
}
