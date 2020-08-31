//-----------------------------------------------------------------------
// <copyright file="Anchor.cs" company="Google LLC">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Attaches a GameObject to an ARCore <see cref="Trackable"/>.  The transform of the GameObject will
    /// be updated to maintain the semantics of the attachment relationship, which varies between
    /// sub-types of Trackable.
    /// </summary>
    [HelpURL("https://developers.google.com/ar/reference/unity/class/GoogleARCore/Anchor")]
    public class Anchor : MonoBehaviour
    {
        private static Dictionary<IntPtr, Anchor> _anchorDict =
            new Dictionary<IntPtr, Anchor>(new IntPtrEqualityComparer());

        private TrackingState _lastFrameTrackingState = TrackingState.Stopped;

        private bool _isSessionDestroyed = false;

        /// <summary>
        /// Gets the tracking state of the anchor.
        /// </summary>
        public TrackingState TrackingState
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    // Anchors from another session are considered stopped.
                    return TrackingState.Stopped;
                }

                return _nativeSession.AnchorApi.GetTrackingState(_nativeHandle);
            }
        }

        internal NativeSession _nativeSession { get; private set; }

        internal IntPtr _nativeHandle { get; private set; }

        internal static Anchor Factory(NativeSession nativeApi, IntPtr anchorNativeHandle,
            bool isCreate = true)
        {
            if (anchorNativeHandle == IntPtr.Zero)
            {
                return null;
            }

            Anchor result;
            if (_anchorDict.TryGetValue(anchorNativeHandle, out result))
            {
                // Release acquired handle and return cached result
                AnchorApi.Release(anchorNativeHandle);
                return result;
            }

            if (isCreate)
            {
               Anchor anchor = (new GameObject()).AddComponent<Anchor>();
               anchor.gameObject.name = "Anchor";
               anchor._nativeHandle = anchorNativeHandle;
               anchor._nativeSession = nativeApi;
               anchor.Update();

               _anchorDict.Add(anchorNativeHandle, anchor);
               return anchor;
            }

            return null;
        }

        /// <summary>
        /// Unity Update.
        /// </summary>
        internal void Update()
        {
            if (_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError(
                    "Anchor components instantiated outside of ARCore are not supported. " +
                    "Please use a 'Create' method within ARCore to instantiate anchors.");
                return;
            }

            if (IsSessionDestroyed())
            {
                return;
            }

            var pose = _nativeSession.AnchorApi.GetPose(_nativeHandle);
            transform.position = pose.position;
            transform.rotation = pose.rotation;

            TrackingState currentFrameTrackingState = TrackingState;
            if (_lastFrameTrackingState != currentFrameTrackingState)
            {
                bool isAnchorTracking = currentFrameTrackingState == TrackingState.Tracking;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(isAnchorTracking);
                }

                _lastFrameTrackingState = currentFrameTrackingState;
            }
        }

        private void OnDestroy()
        {
            if (_nativeHandle == IntPtr.Zero)
            {
                return;
            }

            if (_nativeSession != null && !_nativeSession.IsDestroyed)
            {
                _nativeSession.AnchorApi.Detach(_nativeHandle);
            }

            _anchorDict.Remove(_nativeHandle);
            AnchorApi.Release(_nativeHandle);
        }

        private bool IsSessionDestroyed()
        {
            if (!_isSessionDestroyed)
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession != _nativeSession)
                {
                    Debug.LogErrorFormat(
                        "The session which created this anchor has been destroyed. " +
                        "The anchor on GameObject {0} can no longer update.",
                        this.gameObject != null ? this.gameObject.name : "Unknown");
                    _isSessionDestroyed = true;
                }
            }

            return _isSessionDestroyed;
        }
    }
}
