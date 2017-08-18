//-----------------------------------------------------------------------
// <copyright file="SessionManager.cs" company="Google">
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
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityTango = GoogleAR.UnityNative;

    /// <summary>
    /// A container for internal ARCore session managers.
    /// </summary>
    public class SessionManager
    {
        private static SessionManager m_instance;

        private static SessionConnectionState m_connectionState = SessionConnectionState.Uninitialized;

        public static SessionManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    throw new InvalidSessionAccessException(
                        "Attempted to access the ARCore Session while not connected.");
                }

                return m_instance;
            }
        }

        public static SessionConnectionState ConnectionState
        {
            get
            {
                return m_connectionState;
            }
            set
            {
                if (value == SessionConnectionState.Connected)
                {
                    m_instance = new SessionManager();
                }

                Debug.LogFormat("Connection state became {0}", value);
                m_connectionState = value;
            }
        }

        private Queue<ApiTangoEvent> m_eventQueue = new Queue<ApiTangoEvent>();

        private object m_eventQueueLockObject = new object();

        public AnchorManager AnchorManager { get; private set; }

        public LightEstimateManager LightEstimateManager { get; private set; }

        public MotionTrackingManager MotionTrackingManager { get; private set; }

        public PointCloudManager PointCloudManager { get; private set; }

        public RaycastManager RaycastManager { get; private set; }

        public TrackedPlaneManager TrackedPlaneManager { get; private set; }

        public List<ApiTangoEvent> TangoEvents { get; private set; }

        public SessionManager()
        {
            AnchorManager = new AnchorManager();
            LightEstimateManager = new LightEstimateManager();
            MotionTrackingManager = new MotionTrackingManager();
            PointCloudManager = new PointCloudManager();
            RaycastManager = new RaycastManager();
            TrackedPlaneManager = new TrackedPlaneManager();
            TangoEvents = new List<ApiTangoEvent>();
            TangoClientApi.ConnectOnEventAvailable(m_eventQueue, m_eventQueueLockObject);
        }

        public void EarlyUpdate()
        {
            _ProcessEventQueue();
            MotionTrackingManager.EarlyUpdate();
            AnchorManager.EarlyUpdate();
            TrackedPlaneManager.EarlyUpdate();
        }

        public void OnApplicationPause(bool isPaused)
        {
            AnchorManager.OnApplicationPause(isPaused);
            TrackedPlaneManager.OnApplicationPause(isPaused);
        }

        private void _ProcessEventQueue()
        {
            TangoEvents.Clear();
            lock (m_eventQueueLockObject)
            {
                while (m_eventQueue.Count > 0)
                {
                    TangoEvents.Add(m_eventQueue.Dequeue());
                }
            }
        }
    }
}
