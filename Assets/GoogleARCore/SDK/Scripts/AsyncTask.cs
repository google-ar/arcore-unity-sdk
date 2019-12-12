//-----------------------------------------------------------------------
// <copyright file="AsyncTask.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
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
        private List<Action<T>> m_ActionsUponTaskCompletion;

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
                if (m_ActionsUponTaskCompletion != null)
                {
                    AsyncTask.PerformActionInUpdate(() =>
                    {
                        for (int i = 0; i < m_ActionsUponTaskCompletion.Count; i++)
                        {
                            m_ActionsUponTaskCompletion[i](result);
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
            if (m_ActionsUponTaskCompletion == null)
            {
                m_ActionsUponTaskCompletion = new List<Action<T>>();
            }

            m_ActionsUponTaskCompletion.Add(doAfterTaskComplete);
            return this;
        }
    }

    /// <summary>
    /// Helper methods for dealing with asynchronous tasks.
    /// </summary>
    internal class AsyncTask
    {
        private static Queue<Action> s_UpdateActionQueue = new Queue<Action>();
        private static object s_LockObject = new object();

        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Queues an action to be performed on Unity thread in Update().  This method can be called by any thread.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public static void PerformActionInUpdate(Action action)
        {
            lock (s_LockObject)
            {
                s_UpdateActionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// An Update handler called each frame.
        /// </summary>
        public static void OnUpdate()
        {
            lock (s_LockObject)
            {
                while (s_UpdateActionQueue.Count > 0)
                {
                    Action action = s_UpdateActionQueue.Dequeue();
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
