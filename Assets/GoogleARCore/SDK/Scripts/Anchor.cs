//-----------------------------------------------------------------------
// <copyright file="Anchor.cs" company="Google">
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
        private static Dictionary<IntPtr, Anchor> s_AnchorDict =
            new Dictionary<IntPtr, Anchor>(new IntPtrEqualityComparer());

        private TrackingState m_LastFrameTrackingState = TrackingState.Stopped;

        private bool m_IsSessionDestroyed = false;

        /// <summary>
        /// Gets the tracking state of the anchor.
        /// </summary>
        public TrackingState TrackingState
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    // Anchors from another session are considered stopped.
                    return TrackingState.Stopped;
                }

                return NativeSession.AnchorApi.GetTrackingState(NativeHandle);
            }
        }

        internal NativeSession NativeSession { get; private set; }

        internal IntPtr NativeHandle { get; private set; }

        internal static Anchor Factory(NativeSession nativeApi, IntPtr anchorNativeHandle,
            bool isCreate = true)
        {
            if (anchorNativeHandle == IntPtr.Zero)
            {
                return null;
            }

            Anchor result;
            if (s_AnchorDict.TryGetValue(anchorNativeHandle, out result))
            {
                // Release acquired handle and return cached result
                AnchorApi.Release(anchorNativeHandle);
                return result;
            }

            if (isCreate)
            {
               Anchor anchor = (new GameObject()).AddComponent<Anchor>();
               anchor.gameObject.name = "Anchor";
               anchor.NativeHandle = anchorNativeHandle;
               anchor.NativeSession = nativeApi;
               anchor.Update();

               s_AnchorDict.Add(anchorNativeHandle, anchor);
               return anchor;
            }

            return null;
        }

        /// <summary>
        /// Unity Update.
        /// </summary>
        internal void Update()
        {
            if (NativeHandle == IntPtr.Zero)
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

            var pose = NativeSession.AnchorApi.GetPose(NativeHandle);
            transform.position = pose.position;
            transform.rotation = pose.rotation;

            TrackingState currentFrameTrackingState = TrackingState;
            if (m_LastFrameTrackingState != currentFrameTrackingState)
            {
                bool isAnchorTracking = currentFrameTrackingState == TrackingState.Tracking;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(isAnchorTracking);
                }

                m_LastFrameTrackingState = currentFrameTrackingState;
            }
        }

        private void OnDestroy()
        {
            if (NativeHandle == IntPtr.Zero)
            {
                return;
            }

            if (NativeSession != null && !NativeSession.IsDestroyed)
            {
                NativeSession.AnchorApi.Detach(NativeHandle);
            }

            s_AnchorDict.Remove(NativeHandle);
            AnchorApi.Release(NativeHandle);
        }

        private bool _IsSessionDestroyed()
        {
            if (!m_IsSessionDestroyed)
            {
                var nativeSession = LifecycleManager.Instance.NativeSession;
                if (nativeSession != NativeSession)
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
