//-----------------------------------------------------------------------
// <copyright file="XPAnchor.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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
        private static Dictionary<IntPtr, XPAnchor> s_AnchorDict =
            new Dictionary<IntPtr, XPAnchor>(new GoogleARCoreInternal.IntPtrEqualityComparer());

        private XPTrackingState m_LastFrameTrackingState = XPTrackingState.Stopped;

        private bool m_IsSessionDestroyed = false;

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
                if (_IsSessionDestroyed())
                {
                    // Anchors from another session are considered stopped.
                    return XPTrackingState.Stopped;
                }

                return m_NativeSession.AnchorApi.GetTrackingState(m_NativeHandle)
                    .ToXPTrackingState();
            }
        }

        internal NativeSession m_NativeSession { get; private set; }

        internal IntPtr m_NativeHandle { get; private set; }

        internal static XPAnchor Factory(NativeSession nativeSession, IntPtr anchorHandle,
            bool isCreate = true)
        {
            if (anchorHandle == IntPtr.Zero)
            {
                return null;
            }

            XPAnchor result;
            if (s_AnchorDict.TryGetValue(anchorHandle, out result))
            {
                // Release acquired handle and return cached result
                result.m_NativeSession.AnchorApi.Release(anchorHandle);
                return result;
            }

            if (isCreate)
            {
               XPAnchor anchor = (new GameObject()).AddComponent<XPAnchor>();
               anchor.gameObject.name = "XPAnchor";
               anchor.CloudId = nativeSession.AnchorApi.GetCloudAnchorId(anchorHandle);
               anchor.m_NativeHandle = anchorHandle;
               anchor.m_NativeSession = nativeSession;
               anchor.Update();

               s_AnchorDict.Add(anchorHandle, anchor);
               return anchor;
            }

            return null;
        }

        /// <summary>
        /// The Unity Update method.
        /// </summary>
        private void Update()
        {
            if (m_NativeHandle == IntPtr.Zero)
            {
                Debug.LogError(
                    "Anchor components instantiated outside of ARCore are not supported. " +
                    "Please use a 'Create' method within ARCore to instantiate anchors.");
                return;
            }

            if (_IsSessionDestroyed())
            {
                return;
            }

            var pose = m_NativeSession.AnchorApi.GetPose(m_NativeHandle);
            transform.position = pose.position;
            transform.rotation = pose.rotation;

            var currentFrameTrackingState = TrackingState;
            if (m_LastFrameTrackingState != currentFrameTrackingState)
            {
                bool isAnchorTracking = currentFrameTrackingState == XPTrackingState.Tracking;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(isAnchorTracking);
                }

                m_LastFrameTrackingState = currentFrameTrackingState;
            }
        }

        private void OnDestroy()
        {
            if (m_NativeHandle == IntPtr.Zero)
            {
                return;
            }

            s_AnchorDict.Remove(m_NativeHandle);
            m_NativeSession.AnchorApi.Detach(m_NativeHandle);
            m_NativeSession.AnchorApi.Release(m_NativeHandle);
        }

        private bool _IsSessionDestroyed()
        {
            if (!m_IsSessionDestroyed)
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession != m_NativeSession)
                {
                    Debug.LogErrorFormat(
                        "The session which created this anchor has been destroyed. " +
                        "The anchor on GameObject {0} can no longer update.",
                        this.gameObject != null ? this.gameObject.name : "Unknown");
                    m_IsSessionDestroyed = true;
                }
            }

            return m_IsSessionDestroyed;
        }
    }
}
