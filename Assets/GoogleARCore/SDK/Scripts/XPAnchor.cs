//-----------------------------------------------------------------------
// <copyright file="XPAnchor.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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

namespace GoogleARCore.CrossPlatform
{
    using System;
    using System.Collections.Generic;
    using GoogleARCoreInternal;
    using GoogleARCoreInternal.CrossPlatform;
    using UnityEngine;

    /// <summary>
    /// A cross-platform anchor.
    /// </summary>
    [HelpURL("https://developers.google.com/ar/reference/unity/class/GoogleARCore/CrossPlatform/" +
             "XPAnchor")]
    public class XPAnchor : MonoBehaviour
    {
        private static Dictionary<IntPtr, XPAnchor> _anchorDict =
            new Dictionary<IntPtr, XPAnchor>(new GoogleARCoreInternal.IntPtrEqualityComparer());

        private XPTrackingState _lastFrameTrackingState = XPTrackingState.Stopped;

        private bool _isSessionDestroyed = false;

        /// <summary>
        /// Gets the cloud id associated with this anchor or null if none exists.  Only anchors
        /// created via <c>XPSession.CreateCloudAnchor</c> and <c>XPSession.ResolveCloudAnchor</c>
        /// will have a cloud id.
        /// </summary>
        public string CloudId { get; private set; }

        /// <summary>
        /// Gets the tracking state of the cross-platform anchor.
        /// </summary>
        public XPTrackingState TrackingState
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    // Anchors from another session are considered stopped.
                    return XPTrackingState.Stopped;
                }

                return _nativeSession.AnchorApi.GetTrackingState(_nativeHandle)
                    .ToXPTrackingState();
            }
        }

        internal NativeSession _nativeSession { get; private set; }

        internal IntPtr _nativeHandle { get; private set; }

        internal static XPAnchor Factory(NativeSession nativeSession, IntPtr anchorHandle,
            bool isCreate = true)
        {
            if (anchorHandle == IntPtr.Zero)
            {
                return null;
            }

            XPAnchor result;
            if (_anchorDict.TryGetValue(anchorHandle, out result))
            {
                // Release acquired handle and return cached result
                AnchorApi.Release(anchorHandle);
                return result;
            }

            if (isCreate)
            {
               XPAnchor anchor = (new GameObject()).AddComponent<XPAnchor>();
               anchor.gameObject.name = "XPAnchor";
               anchor.CloudId = nativeSession.AnchorApi.GetCloudAnchorId(anchorHandle);
               anchor._nativeHandle = anchorHandle;
               anchor._nativeSession = nativeSession;
               anchor.Update();

               _anchorDict.Add(anchorHandle, anchor);
               return anchor;
            }

            return null;
        }

        /// <summary>
        /// Unity Update.
        /// </summary>
        private void Update()
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

            var currentFrameTrackingState = TrackingState;
            if (_lastFrameTrackingState != currentFrameTrackingState)
            {
                bool isAnchorTracking = currentFrameTrackingState == XPTrackingState.Tracking;
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
