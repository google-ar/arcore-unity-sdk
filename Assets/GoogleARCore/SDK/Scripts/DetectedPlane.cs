//-----------------------------------------------------------------------
// <copyright file="DetectedPlane.cs" company="Google">
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

namespace GoogleARCore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// A planar surface in the real world detected and tracked by ARCore.
    /// </summary>
    public class DetectedPlane : Trackable
    {
        /// <summary>
        /// Construct DetectedPlane from a native handle.
        /// </summary>
        /// <param name="nativeHandle">A handle to the native ARCore API Trackable.</param>
        /// <param name="nativeApi">The ARCore native api.</param>
        internal DetectedPlane(IntPtr nativeHandle, NativeSession nativeApi)
            : base(nativeHandle, nativeApi)
        {
            m_TrackableNativeHandle = nativeHandle;
            m_NativeSession = nativeApi;
        }

        /// <summary>
        /// Gets a reference to the plane subsuming this plane, if any. If not null, only the
        /// subsuming plane should be considered valid for rendering.
        /// </summary>
        public DetectedPlane SubsumedBy
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    Debug.LogError(
                        "SubsumedBy:: Trying to access a session that has already been destroyed.");
                    return null;
                }

                return m_NativeSession.PlaneApi.GetSubsumedBy(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the position and orientation of the plane's center in Unity world space.
        /// </summary>
        public Pose CenterPose
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    Debug.LogError(
                        "CenterPose:: Trying to access a session that has already been destroyed.");
                    return new Pose();
                }

                return m_NativeSession.PlaneApi.GetCenterPose(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the extent of the plane in the X dimension, centered on the plane position.
        /// </summary>
        public float ExtentX
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    Debug.LogError(
                        "ExtentX:: Trying to access a session that has already been destroyed.");
                    return 0f;
                }

                return m_NativeSession.PlaneApi.GetExtentX(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the extent of the plane in the Z dimension, centered on the plane position.
        /// </summary>
        public float ExtentZ
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    Debug.LogError(
                        "ExtentZ:: Trying to access a session that has already been destroyed.");
                    return 0f;
                }

                return m_NativeSession.PlaneApi.GetExtentZ(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the type of the plane.
        /// </summary>
        public DetectedPlaneType PlaneType
        {
            get
            {
                if (_IsSessionDestroyed())
                {
                    Debug.LogError(
                        "PlaneType:: Trying to access a session that has already been destroyed.");
                    return DetectedPlaneType.HorizontalUpwardFacing;
                }

                return m_NativeSession.PlaneApi.GetPlaneType(m_TrackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets a list of points (in clockwise order) in Unity world space representing a boundary
        /// polygon for the plane.
        /// </summary>
        /// <param name="boundaryPolygonPoints">A list of <b>Vector3</b> to be filled by the method
        /// call.</param>
        [SuppressMemoryAllocationError(Reason = "List could be resized.")]
        public void GetBoundaryPolygon(List<Vector3> boundaryPolygonPoints)
        {
            if (_IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetBoundaryPolygon:: Trying to access a session that has already been " +
                    "destroyed.");
                return;
            }

            m_NativeSession.PlaneApi.GetPolygon(m_TrackableNativeHandle, boundaryPolygonPoints);
        }
    }
}
