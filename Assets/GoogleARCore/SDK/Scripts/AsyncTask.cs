//-----------------------------------------------------------------------
// <copyright file="AsyncTask.cs" company="Google LLC">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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
    using System.Threading;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A class used for monitoring the status of an asynchronous task.
    /// </summary>
    /// <typeparam name="T">The resultant type of the task.</typeparam>
    public class AsyncTask<T>
    {
        /// <summary>
        /// A collection of actons to perform on the main Unity thread after the task is complete.
        /// </summary>
        private List<Action<T>> _actionsUponTaskCompletion;

        /// <summary>
        /// Constructor for AsyncTask.
        /// </summary>
        /// <param name="asyncOperationComplete">A callback that, when invoked, changes the status of the task to
        /// complete and sets the result based on the argument supplied.</param>
        internal AsyncTask(out Action<T> asyncOperationComplete)
        {
            // Register for early update event.
            if (!AsyncTask.IsInitialized)
            {
                AsyncTask.InitAsyncTask();
            }

            IsComplete = false;
            asyncOperationComplete = delegate(T result)
            {
                this.Result = result;
                IsComplete = true;
                if (_actionsUponTaskCompletion != null)
                {
                    AsyncTask.PerformActionInUpdate(() =>
                    {
                        for (int i = 0; i < _actionsUponTaskCompletion.Count; i++)
                        {
                            _actionsUponTaskCompletion[i](result);
                        }
                    });
                }
            };
        }

        /// <summary>
        /// Constructor for AsyncTask that creates a completed task.
        /// </summary>
        /// <param name="result">The result of the completed task.</param>
        internal AsyncTask(T result)
        {
            Result = result;
            IsComplete = true;
        }

        /// <summary>
        /// Gets a value indicating whether the task is complete.
        /// </summary>
        /// <value><c>true</c> if the task is complete, otherwise <c>false</c>.</value>
        public bool IsComplete { get; private set; }

        /// <summary>
        /// Gets the result of a completed task.
        /// </summary>
        /// <value>The result of the completed task.</value>
        public T Result { get; private set; }

        /// <summary>
        /// Returns a yield instruction that monitors this task for completion within a coroutine.
        /// </summary>
        /// <returns>A yield instruction that monitors this task for completion.</returns>
        [SuppressMemoryAllocationError(Reason = "Creates a new CustomYieldInstruction")]
        public CustomYieldInstruction WaitForCompletion()
        {
            return new WaitForTaskCompletionYieldInstruction<T>(this);
        }

        /// <summary>
        /// Performs an action (callback) in the first Unity Update() call after task completion.
        /// </summary>
        /// <param name="doAfterTaskComplete">The action to invoke when task is complete.  The result of the task will
        /// be passed as an argument to the action.</param>
        /// <returns>The invoking asynchronous task.</returns>
        [SuppressMemoryAllocationError(Reason = "Could allocate new List")]
        public AsyncTask<T> ThenAction(Action<T> doAfterTaskComplete)
        {
            // Perform action now if task is already complete.
            if (IsComplete)
            {
                doAfterTaskComplete(Result);
                return this;
            }

            // Allocate list if needed (avoids allocation if then is not used).
            if (_actionsUponTaskCompletion == null)
            {
                _actionsUponTaskCompletion = new List<Action<T>>();
            }

            _actionsUponTaskCompletion.Add(doAfterTaskComplete);
            return this;
        }
    }

    /// <summary>
    /// Helper methods for dealing with asynchronous tasks.
    /// </summary>
    internal class AsyncTask
    {
        private static Queue<Action> _updateActionQueue = new Queue<Action>();
        private static object _lockObject = new object();

        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Queues an action to be performed on Unity thread in Update().  This method can be called by any thread.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public static void PerformActionInUpdate(Action action)
        {
            lock (_lockObject)
            {
                _updateActionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// An Update handler called each frame.
        /// </summary>
        public static void OnUpdate()
        {
            lock (_lockObject)
            {
                while (_updateActionQueue.Count > 0)
                {
                    Action action = _updateActionQueue.Dequeue();
                    action();
                }
            }
        }

        public static void InitAsyncTask()
        {
            if (IsInitialized)
            {
                return;
            }

            LifecycleManager.Instance.EarlyUpdate += OnUpdate;
            LifecycleManager.Instance.OnResetInstance += ResetAsyncTask;
            IsInitialized = true;
        }

        public static void ResetAsyncTask()
        {
            if (!IsInitialized)
            {
                return;
            }

            LifecycleManager.Instance.EarlyUpdate -= OnUpdate;
            IsInitialized = false;
        }
    }
}
