//-----------------------------------------------------------------------
// <copyright file="DetectedPlane.cs" company="Google LLC">
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
            _trackableNativeHandle = nativeHandle;
            _nativeSession = nativeApi;
        }

        /// <summary>
        /// Gets a reference to the plane subsuming this plane, if any. If not null, only the
        /// subsuming plane should be considered valid for rendering.
        /// </summary>
        public DetectedPlane SubsumedBy
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "SubsumedBy:: Trying to access a session that has already been destroyed.");
                    return null;
                }

                return _nativeSession.PlaneApi.GetSubsumedBy(_trackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the position and orientation of the plane's center in Unity world space.
        /// </summary>
        public Pose CenterPose
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "CenterPose:: Trying to access a session that has already been destroyed.");
                    return new Pose();
                }

                return _nativeSession.PlaneApi.GetCenterPose(_trackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the extent of the plane in the X dimension, centered on the plane position.
        /// </summary>
        public float ExtentX
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "ExtentX:: Trying to access a session that has already been destroyed.");
                    return 0f;
                }

                return _nativeSession.PlaneApi.GetExtentX(_trackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the extent of the plane in the Z dimension, centered on the plane position.
        /// </summary>
        public float ExtentZ
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "ExtentZ:: Trying to access a session that has already been destroyed.");
                    return 0f;
                }

                return _nativeSession.PlaneApi.GetExtentZ(_trackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the type of the plane.
        /// </summary>
        public DetectedPlaneType PlaneType
        {
            get
            {
                if (IsSessionDestroyed())
                {
                    Debug.LogError(
                        "PlaneType:: Trying to access a session that has already been destroyed.");
                    return DetectedPlaneType.HorizontalUpwardFacing;
                }

                return _nativeSession.PlaneApi.GetPlaneType(_trackableNativeHandle);
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
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetBoundaryPolygon:: Trying to access a session that has already been " +
                    "destroyed.");
                return;
            }

            _nativeSession.PlaneApi.GetPolygon(_trackableNativeHandle, boundaryPolygonPoints);
        }
    }
}
