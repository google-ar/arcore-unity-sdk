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
    using GoogleARCore;
    using UnityEngine;

    internal class TrackableManager
    {
        private Dictionary<IntPtr, Trackable> m_TrackableDict =
            new Dictionary<IntPtr, Trackable>(new IntPtrEqualityComparer());

        private NativeSession m_NativeSession;

        private int m_LastUpdateFrame = -1;

        private List<Trackable> m_NewTrackables = new List<Trackable>();

        private List<Trackable> m_AllTrackables  = new List<Trackable>();

        private List<Trackable> m_UpdatedTrackables  = new List<Trackable>();

        private HashSet<Trackable> m_OldTrackables  = new HashSet<Trackable>();

        public TrackableManager(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        /// <summary>
        /// Factory method for creating and reusing Trackable references from native handles.
        /// </summary>
        /// <param name="nativeHandle">A native handle to a plane that has been acquired.  RELEASE WILL BE HANDLED BY
        /// THIS METHOD.</param>
        /// <returns>A reference to the Trackable.</returns>
        public Trackable TrackableFactory(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                return null;
            }

            Trackable result;
            if (m_TrackableDict.TryGetValue(nativeHandle, out result))
            {
                // Release aquired handle and return cached result.
                m_NativeSession.TrackableApi.Release(nativeHandle);
                return result;
            }

            // This block needs to construct classes marked Obsolete since those versions are always the most derived 
            // type.
#pragma warning disable 618 // Obsolete warning
            ApiTrackableType trackableType = m_NativeSession.TrackableApi.GetType(nativeHandle);
            if (trackableType == ApiTrackableType.Plane)
            {
                result = new TrackedPlane(nativeHandle, m_NativeSession);
            }
            else if (trackableType == ApiTrackableType.Point)
            {
                result = new TrackedPoint(nativeHandle, m_NativeSession);
            }
            else if (trackableType == ApiTrackableType.AugmentedImage)
            {
                result = new AugmentedImage(nativeHandle, m_NativeSession);
            }
            else if (ExperimentManager.Instance.IsManagingTrackableType((int)trackableType))
            {
                result = ExperimentManager.Instance.TrackableFactory((int)trackableType, nativeHandle);
            }
            else
            {
                throw new NotImplementedException(
                    "TrackableFactory::No constructor for requested trackable type.");
            }
#pragma warning restore 618

            m_TrackableDict.Add(nativeHandle, result);
            return result;
        }

        public void GetTrackables<T>(List<T> trackables, TrackableQueryFilter filter) where T : Trackable
        {
            if (m_LastUpdateFrame < Time.frameCount)
            {
                // Get trackables updated this frame.
                m_NativeSession.FrameApi.GetUpdatedTrackables(m_UpdatedTrackables);

                // Get all the trackables in the session.
                m_NativeSession.SessionApi.GetAllTrackables(m_AllTrackables);

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
            else if (filter == TrackableQueryFilter.Updated)
            {
                for (int i = 0; i < m_UpdatedTrackables.Count; i++)
                {
                    _SafeAdd<T>(m_UpdatedTrackables[i], trackables);
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
