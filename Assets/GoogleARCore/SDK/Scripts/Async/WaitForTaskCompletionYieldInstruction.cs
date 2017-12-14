//-----------------------------------------------------------------------
// <copyright file="WaitForTaskCompletionYieldInstruction.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// A yield instruction that blocks a coroutine until an AsyncTask has completed.
    /// </summary>
    /// <typeparam name="T">The type of the AsyncTask result.</typeparam>
    public class WaitForTaskCompletionYieldInstruction<T> : CustomYieldInstruction
    {
        /// <summary>
        /// The AsyncTask the yield instruction waits on.
        /// </summary>
        private AsyncTask<T> m_Task;

        /// <summary>
        /// Constructor for WaitForTaskCompletionYieldInstruction.
        /// </summary>
        /// <param name="task">The task to wait for completion.</param>
        public WaitForTaskCompletionYieldInstruction(AsyncTask<T> task)
        {
            m_Task = task;
        }

        /// <summary>
        /// Gets a value indicating whether the coroutine instruction should keep waiting.
        /// </summary>
        /// <value><c>true</c> if the task is incomplete, otherwise <c>false</c>.</value>
        [SuppressMessage("UnityRules.UnityStyleRules", "US1000:FieldsMustBeUpperCamelCase",
         Justification = "Overridden method.")]
        public override bool keepWaiting
        {
            get
            {
                return !m_Task.IsComplete;
            }
        }
    }
}
