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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class SessionManager
    {
        private NativeApi m_NativeApi;

        private SessionManager()
        {
        }

        ~SessionManager()
        {
            Destroy();
        }

        public SessionConnectionState ConnectionState { get; set; }

        public FrameManager FrameManager { get; private set; }

        public static SessionManager CreateSession()
        {
            var sessionManager = new SessionManager();
            sessionManager.m_NativeApi = NativeApi.CreateSession();
            if (sessionManager.m_NativeApi != null)
            {
                sessionManager.FrameManager = new FrameManager(sessionManager.m_NativeApi);
                sessionManager.ConnectionState = SessionConnectionState.Uninitialized;
            }
            else
            {
                // Eventually we will provide more detail here: ARCore not installed, device not
                // supported, ARCore version not supported, etc.; however the API to support these
                // details does not exist yet.
                //
                // For now, just bundle all the possible errors into a generic connection failed.
                sessionManager.ConnectionState = SessionConnectionState.ConnectToServiceFailed;
            }

            return sessionManager;
        }

        public void Destroy()
        {
            if (m_NativeApi != null)
            {
                m_NativeApi.Destroy();
                m_NativeApi = null;
            }
        }

        public bool CheckSupported(ARCoreSessionConfig config)
        {
            return m_NativeApi.Session.CheckSupported(config) == ApiArStatus.Success;
        }

        public bool SetConfiguration(ARCoreSessionConfig config)
        {
            return m_NativeApi.Session.SetConfiguration(config);
        }

        public bool Resume()
        {
            return m_NativeApi.Resume(_EarlyUpdate);
        }

        public Anchor CreateWorldAnchor(Pose pose)
        {
            return m_NativeApi.Session.CreateAnchor(pose);
        }

        private void _EarlyUpdate(IntPtr frameHandle, int textureId)
        {
            m_NativeApi.Session.SetDisplayGeometry(Screen.orientation, Screen.width, Screen.height);
            FrameManager.UpdateFrame(frameHandle, (uint)textureId);
        }
    }
}
