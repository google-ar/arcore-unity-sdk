//-----------------------------------------------------------------------
// <copyright file="Gesture.cs" company="Google LLC">
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

namespace GoogleARCore.Examples.ObjectManipulationInternal
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Base class for a gesture.
    ///
    /// A gesture represents a sequence of touch events that are detected to
    /// represent a particular type of motion (i.e. Dragging, Pinching).
    ///
    /// Gestures are created and updated by BaseGestureRecognizer's.
    /// </summary>
    /// <typeparam name="T">The actual gesture.</typeparam>
    public abstract class Gesture<T> where T : Gesture<T>
    {
        private bool _hasStarted;

        /// <summary>
        /// Constructs a Gesture with a given recognizer.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        internal Gesture(GestureRecognizer<T> recognizer)
        {
            _recognizer = recognizer;
        }

        /// <summary>
        /// Action to be performed when a gesture is started.
        /// </summary>
        public event Action<T> onStart;

        /// <summary>
        /// Action to be performed when a gesture is updated.
        /// </summary>
        public event Action<T> onUpdated;

        /// <summary>
        /// Action to be performed when a gesture is finished.
        /// </summary>
        public event Action<T> onFinished;

        /// <summary>
        /// Gets a value indicating whether the gesture was cancelled.
        /// </summary>
        public bool WasCancelled { get; private set; }

        /// <summary>
        /// Gets or sets the object this gesture is targeting.
        /// </summary>
        public GameObject TargetObject { get; protected set; }

        /// <summary>
        /// Gets the gesture recognizer.
        /// </summary>
        protected internal GestureRecognizer<T> _recognizer { get; private set; }

        /// <summary>
        /// Updates this gesture.
        /// </summary>
        internal void Update()
        {
            if (!_hasStarted && CanStart())
            {
                Start();
                return;
            }

            if (_hasStarted)
            {
                if (UpdateGesture() && onUpdated != null)
                {
                    onUpdated(this as T);
                }
            }
        }

        /// <summary>
        /// Cancels this gesture.
        /// </summary>
        internal void Cancel()
        {
            WasCancelled = true;
            OnCancel();
            Complete();
        }

        /// <summary>
        /// Returns true if this gesture can start.
        /// </summary>
        /// <returns>True if the gesture can start.</returns>
        protected internal abstract bool CanStart();

        /// <summary>
        /// Action to be performed when this gesture is started.
        /// </summary>
        protected internal abstract void OnStart();

        /// <summary>
        /// Updates this gesture.
        /// </summary>
        /// <returns>True if the update was successful.</returns>
        protected internal abstract bool UpdateGesture();

        /// <summary>
        /// Action to be performed when this gesture is cancelled.
        /// </summary>
        protected internal abstract void OnCancel();

        /// <summary>
        /// Action to be performed when this gesture is finished.
        /// </summary>
        protected internal abstract void OnFinish();

        /// <summary>
        /// Completes this gesture.
        /// </summary>
        protected internal void Complete()
        {
            OnFinish();
            if (onFinished != null)
            {
                onFinished(this as T);
            }
        }

        private void Start()
        {
            _hasStarted = true;
            OnStart();
            if (onStart != null)
            {
                onStart(this as T);
            }
        }
    }
}
