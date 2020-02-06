//-----------------------------------------------------------------------
// <copyright file="BackgroundJobExecutor.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class BackgroundJobExecutor
    {
        private AutoResetEvent m_Event = new AutoResetEvent(false);
        private Queue<Action> m_JobsQueue = new Queue<Action>();
        private Thread m_Thread;
        private bool m_Running = false;

        public BackgroundJobExecutor()
        {
            m_Thread = new Thread(Run);
            m_Thread.Start();
        }

        public int PendingJobsCount
        {
            get
            {
                lock (m_JobsQueue)
                {
                    return m_JobsQueue.Count + (m_Running ? 1 : 0);
                }
            }
        }

        public void PushJob(Action job)
        {
            lock (m_JobsQueue)
            {
                m_JobsQueue.Enqueue(job);
            }

            m_Event.Set();
        }

        public void RemoveAllPendingJobs()
        {
            lock (m_JobsQueue)
            {
                m_JobsQueue.Clear();
            }
        }

        private void Run()
        {
            while (true)
            {
                if (PendingJobsCount == 0)
                {
                    m_Event.WaitOne();
                }

                Action job = null;
                lock (m_JobsQueue)
                {
                    if (m_JobsQueue.Count == 0)
                    {
                        continue;
                    }

                    job = m_JobsQueue.Dequeue();
                    m_Running = true;
                }

                job();
                lock (m_JobsQueue)
                {
                    m_Running = false;
                }
            }
        }
    }
}
