//-----------------------------------------------------------------------
// <copyright file="BackgroundJobExecutor.cs" company="Google LLC">
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
        private AutoResetEvent _event = new AutoResetEvent(false);
        private Queue<Action> _jobsQueue = new Queue<Action>();
        private Thread _thread;
        private bool _running = false;

        public BackgroundJobExecutor()
        {
            _thread = new Thread(Run);
            _thread.Start();
        }

        public int PendingJobsCount
        {
            get
            {
                lock (_jobsQueue)
                {
                    return _jobsQueue.Count + (_running ? 1 : 0);
                }
            }
        }

        public void PushJob(Action job)
        {
            lock (_jobsQueue)
            {
                _jobsQueue.Enqueue(job);
            }

            _event.Set();
        }

        public void RemoveAllPendingJobs()
        {
            lock (_jobsQueue)
            {
                _jobsQueue.Clear();
            }
        }

        private void Run()
        {
            while (true)
            {
                if (PendingJobsCount == 0)
                {
                    _event.WaitOne();
                }

                Action job = null;
                lock (_jobsQueue)
                {
                    if (_jobsQueue.Count == 0)
                    {
                        continue;
                    }

                    job = _jobsQueue.Dequeue();
                    _running = true;
                }

                job();
                lock (_jobsQueue)
                {
                    _running = false;
                }
            }
        }
    }
}
