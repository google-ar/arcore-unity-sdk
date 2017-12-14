//-----------------------------------------------------------------------
// <copyright file="TrackableManager.cs" company="Google">
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
    public class TrackableManager
    {
        private NativeApi m_NativeApi;

        private int m_LastUpdateFrame = -1;

        private List<Trackable> m_NewTrackables = new List<Trackable>();

        private List<Trackable> m_AllTrackables  = new List<Trackable>();

        private HashSet<Trackable> m_OldTrackables  = new HashSet<Trackable>();

        public TrackableManager(NativeApi nativeApi)
        {
            m_NativeApi = nativeApi;
        }

        public void GetTrackables<T>(List<T> trackables, TrackableQueryFilter filter) where T : Trackable
        {
            if (m_LastUpdateFrame < Time.frameCount)
            {
                // Get all the trackables in the session.
                m_NativeApi.Session.GetAllTrackables(m_AllTrackables);

                // Find trackables that are not in the hashset (new).
                m_NewTrackables.Clear();
                for (int i = 0; i < m_AllTrackables.Count; i++)
                {
                    Trackable trackable = m_AllTrackables[i];
                    if (!m_OldTrackables.Contains(trackable))
                    {
                        m_NewTrackables.Add(trackable);
                        m_OldTrackables.Add(trackable);
                    }
                }

                m_LastUpdateFrame = Time.frameCount;
            }

            trackables.Clear();

            if (filter == TrackableQueryFilter.All)
            {
                for (int i = 0; i < m_AllTrackables.Count; i++)
                {
                    _SafeAdd<T>(m_AllTrackables[i], trackables);
                }
            }
            else if (filter == TrackableQueryFilter.New)
            {
                for (int i = 0; i < m_NewTrackables.Count; i++)
                {
                    _SafeAdd<T>(m_NewTrackables[i], trackables);
                }
            }
        }

        private void _SafeAdd<T>(Trackable trackable, List<T> trackables) where T : Trackable
        {
            if (trackable is T)
            {
                trackables.Add(trackable as T);
            }
        }
    }
}