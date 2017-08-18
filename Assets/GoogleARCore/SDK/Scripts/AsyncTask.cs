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
    using UnityEngine;
    using GoogleARCoreInternal;

    /// <summary>
    /// A class used for monitoring the status of an asynchronous task.
    /// </summary>
    /// <typeparam name="T">The resultant type of the task.</typeparam>
    public class AsyncTask<T>
    {
        /// <summary>
        /// A collection of actons to perform on the main Unity thread after the task is complete.
        /// </summary>
        private List<Action<T>> actionsUponTaskCompletion;

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Constructor for AsyncTask.
        /// </summary>
        /// <param name="asyncOperationComplete">A callback that, when invoked, changes the status of the task to
        /// complete and sets the result based on the argument supplied.</param>
        public AsyncTask(out Action<T> asyncOperationComplete)
        {
            IsComplete = false;
            asyncOperationComplete = delegate(T result)
            {
                Result = result;
                IsComplete = true;
                if (actionsUponTaskCompletion != null)
                {
                    AsyncTask.PerformActionInUpdate(() =>
                    {
                        for (int i = 0; i < actionsUponTaskCompletion.Count; i++)
                        {
                            actionsUponTaskCompletion[i](result);
                        }
                    });
                }
            };
        }
        /// @endcond

        /// @cond EXCLUDE_FROM_DOXYGEN
        /// <summary>
        /// Constructor for AsyncTask that creates a completed task.
        /// </summary>
        /// <param name="result">The result of the completed task.</param>
        public AsyncTask(T result)
        {
            Result = result;
            IsComplete = true;
        }
        /// @endcond

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
        public AsyncTask<T> ThenAction(Action<T> doAfterTaskComplete)
        {
            // Perform action now if task is already complete.
            if (IsComplete)
            {
                doAfterTaskComplete(Result);
                return this;
            }

            // Allocate list if needed (avoids allocation if then is not used).
            if (actionsUponTaskCompletion == null)
            {
                actionsUponTaskCompletion = new List<Action<T>>();
            }

            actionsUponTaskCompletion.Add(doAfterTaskComplete);
            return this;
        }
    }

    /// @cond EXCLUDE_FROM_DOXYGEN
    /// <summary>
    /// Helper methods for dealing with asynchronous tasks.
    /// </summary>
    public class AsyncTask
    {
        private static Queue<Action> actionQueue = new Queue<Action>();

        private static object lock_object = new object();

        /// <summary>
        /// Encapsulates a delegate method in a thread and returns a task that monitors completion.
        /// </summary>
        /// <param name="taskMethod">The method to perform in a thread.</param>
        /// <typeparam name="T">The resultant type of the task.</typeparam>
        /// <returns>A task that will complete when the supplied method has completed.</returns>
        public static AsyncTask<T> DoTaskInThread<T>(Func<T> taskMethod)
        {
            Action<T> asyncTaskComplete;
            var task = new AsyncTask<T>(out asyncTaskComplete);

            // Spawn thread to perform the task.
            new Thread(() =>
            {
                try
                {
                    T result = taskMethod();
                    asyncTaskComplete(result);
                }
                catch (Exception e)
                {
                    ARDebug.LogErrorFormat("An AsyncTask task produced an uncaught exception::{0}", e.ToString());
                    asyncTaskComplete(default(T));
                }
            }).Start();

            return task;
        }

        /// <summary>
        /// Queues an action to be performed on Unity thread in Update().  This method can be called by any thread.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public static void PerformActionInUpdate(Action action)
        {
            lock(lock_object)
            {
                actionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// An Update handler called from the ARCore SessionComponent in the scene.
        /// </summary>
        public static void EarlyUpdate()
        {
            int count = actionQueue.Count;
            for (int i = 0; i < count; i++)
            {
                Action action = actionQueue.Dequeue();
                action();
            }
        }
    }
    /// @endcond
}
