//-----------------------------------------------------------------------
// <copyright file="TrackableManager.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
        private Dictionary<IntPtr, Trackable> _trackableDict =
            new Dictionary<IntPtr, Trackable>(new IntPtrEqualityComparer());

        private NativeSession _nativeSession;

        private int _lastUpdateFrame = -1;

        private List<Trackable> _newTrackables = new List<Trackable>();

        private List<Trackable> _allTrackables = new List<Trackable>();

        private List<Trackable> _updatedTrackables = new List<Trackable>();

        private HashSet<Trackable> _oldTrackables = new HashSet<Trackable>();

        public TrackableManager(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
            LifecycleManager.Instance.OnResetInstance += ClearCachedTrackables;
        }

        /// <summary>
        /// Factory method for creating and reusing Trackable references from native handles.
        /// </summary>
        /// <param name="nativeHandle">A native handle to a plane that has been acquired.  RELEASE
        /// WILL BE HANDLED BY THIS METHOD.</param>
        /// <returns>A reference to the Trackable. May return <c>null</c> on error or if Trackable
        /// is not handled.</returns>
        public Trackable TrackableFactory(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                return null;
            }

            Trackable result;
            if (_trackableDict.TryGetValue(nativeHandle, out result))
            {
                // Release aquired handle and return cached result.
                _nativeSession.TrackableApi.Release(nativeHandle);
                return result;
            }

            // This block needs to construct classes marked Obsolete since those versions are always
            // the most derived type.
#pragma warning disable 618 // Obsolete warning
            ApiTrackableType trackableType = _nativeSession.TrackableApi.GetType(nativeHandle);
            if (trackableType == ApiTrackableType.Plane)
            {
                result = new TrackedPlane(nativeHandle, _nativeSession);
            }
            else if (trackableType == ApiTrackableType.Point)
            {
                result = new TrackedPoint(nativeHandle, _nativeSession);
            }
            else if (trackableType == ApiTrackableType.InstantPlacementPoint)
            {
                result = new InstantPlacementPoint(nativeHandle, _nativeSession);
            }
            else if (trackableType == ApiTrackableType.AugmentedImage)
            {
                result = new AugmentedImage(nativeHandle, _nativeSession);
            }
            else if (trackableType == ApiTrackableType.AugmentedFace)
            {
                result = new AugmentedFace(nativeHandle, _nativeSession);
            }
            else if (trackableType == ApiTrackableType.DepthPoint)
            {
                result = new DepthPoint(nativeHandle, _nativeSession);
            }
            else if (ExperimentManager.Instance.IsManagingTrackableType((int)trackableType))
            {
                result =
                    ExperimentManager.Instance.TrackableFactory((int)trackableType, nativeHandle);
            }
            else
            {
                Debug.LogWarning(
                    "TrackableFactory::No constructor for requested trackable type.");
            }
#pragma warning restore 618

            if (result != null)
            {
                _trackableDict.Add(nativeHandle, result);
            }

            return result;
        }

        public void GetTrackables<T>(
            List<T> trackables, TrackableQueryFilter filter) where T : Trackable
        {
            if (_lastUpdateFrame < Time.frameCount)
            {
                // Get trackables updated this frame.
                _nativeSession.FrameApi.GetUpdatedTrackables(_updatedTrackables);

                // Get all the trackables in the session.
                _nativeSession.SessionApi.GetAllTrackables(_allTrackables);

                // Find trackables that are not in the hashset (new).
                _newTrackables.Clear();
                for (int i = 0; i < _allTrackables.Count; i++)
                {
                    Trackable trackable = _allTrackables[i];
                    if (!_oldTrackables.Contains(trackable))
                    {
                        _newTrackables.Add(trackable);
                        _oldTrackables.Add(trackable);
                    }
                }

                _lastUpdateFrame = Time.frameCount;
            }

            trackables.Clear();

            if (filter == TrackableQueryFilter.All)
            {
                for (int i = 0; i < _allTrackables.Count; i++)
                {
                    SafeAdd<T>(_allTrackables[i], trackables);
                }
            }
            else if (filter == TrackableQueryFilter.New)
            {
                for (int i = 0; i < _newTrackables.Count; i++)
                {
                    SafeAdd<T>(_newTrackables[i], trackables);
                }
            }
            else if (filter == TrackableQueryFilter.Updated)
            {
                for (int i = 0; i < _updatedTrackables.Count; i++)
                {
                    SafeAdd<T>(_updatedTrackables[i], trackables);
                }
            }
        }

        private void SafeAdd<T>(Trackable trackable, List<T> trackables) where T : Trackable
        {
            if (trackable is T)
            {
                trackables.Add(trackable as T);
            }
        }

        private void ClearCachedTrackables()
        {
            _trackableDict.Clear();
            _newTrackables.Clear();
            _allTrackables.Clear();
            _updatedTrackables.Clear();
            _oldTrackables.Clear();
        }
    }
}
