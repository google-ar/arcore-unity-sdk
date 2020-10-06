//-----------------------------------------------------------------------
// <copyright file="AugmentedFace.cs" company="Google LLC">
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

namespace GoogleARCore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCore;
    using GoogleARCoreInternal;
    using UnityEngine;
    using UnityEngine.Profiling;

    /// <summary>
    /// A face detected and trackable by ARCore.
    /// </summary>
    public partial class AugmentedFace : Trackable
    {
        /// <summary>
        /// Construct AugmentedFace from a native handle.
        /// </summary>
        /// <param name="nativeHandle">A handle to the native ARCore API Trackable.</param>
        /// <param name="nativeApi">The ARCore native API.</param>
        internal AugmentedFace(IntPtr nativeHandle, NativeSession nativeApi)
            : base(nativeHandle, nativeApi)
        {
            _trackableNativeHandle = nativeHandle;
            _nativeSession = nativeApi;
        }

        /// <summary>
        /// Gets the position and orientation of the face's center in world space.
        /// </summary>
        /// <remarks>
        /// The face center is defined to have the origin located behind the nose and between
        /// the two cheek bones.
        /// The forward vector of the pose (+Z) points out the back of the person's head.
        /// The up vector of the pose (+Y) points out the top of the person's head.
        /// The right vector of the pose (+X) points to the left side of the person's face.
        /// </remarks>
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

                return _nativeSession.AugmentedFaceApi.GetCenterPose(_trackableNativeHandle);
            }
        }

        /// <summary>
        /// Gets the position and orientation of a face region in world space.
        /// </summary>
        /// <param name="region">The face region to query the pose.</param>
        /// <returns>The position and orientation of a face region in world space.</returns>
        public Pose GetRegionPose(AugmentedFaceRegion region)
        {
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetRegionPose: Trying to access a session that has already been destroyed.");
                return new Pose();
            }

            return _nativeSession.AugmentedFaceApi.GetRegionPose(
                _trackableNativeHandle, (ApiAugmentedFaceRegionType)region);
        }

        /// <summary>
        /// Gets the vertices of the face mesh.
        /// </summary>
        /// <remarks>
        /// Vector -Z is points forward out of the face.
        /// </remarks>
        /// <param name="vertices">
        /// A list to be filled with the vertices of the face mesh.
        /// The list will be empty when the motion tracking state is TrackingState.Paused or
        /// TackingState.Stopped.
        /// </param>
        public void GetVertices(List<Vector3> vertices)
        {
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetVertices:: Trying to access a session that has already been destroyed.");
                return;
            }

            _nativeSession.AugmentedFaceApi.GetVertices(_trackableNativeHandle, vertices);
        }

        /// <summary>
        /// Gets a list of texture coordinates of the face mesh.
        /// </summary>
        /// <param name="textureCoordinates">
        /// A list of texture coordinates.
        /// The list will be empty when the motion tracking state is TrackingState.Paused or
        /// TackingState.Stopped.
        /// </param>
        public void GetTextureCoordinates(List<Vector2> textureCoordinates)
        {
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetTextureCoordinates:: Trying to access a session that has already been " +
                    "destroyed.");
                return;
            }

            _nativeSession.AugmentedFaceApi.GetTextureCoordinates(
                _trackableNativeHandle, textureCoordinates);
        }

        /// <summary>
        /// Gets a list of normals of the face mesh.
        /// </summary>
        /// <param name="normals">
        /// A list of vertex normals.
        /// The list will be empty when the motion tracking state is TrackingState.Paused or
        /// TackingState.Stopped.
        /// </param>
        public void GetNormals(List<Vector3> normals)
        {
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetNormals:: Trying to access a session that has already been destroyed.");
                return;
            }

            _nativeSession.AugmentedFaceApi.GetNormals(_trackableNativeHandle, normals);
        }

        /// <summary>
        /// Gets a list of triangle indices of the face mesh.
        /// </summary>
        /// <param name="indices">
        /// A list of indices.
        /// The list will be empty when the motion tracking state is TrackingState.Paused or
        /// TackingState.Stopped.
        /// </param>
        public void GetTriangleIndices(List<int> indices)
        {
            if (IsSessionDestroyed())
            {
                Debug.LogError(
                    "GetTriangleIndices:: Trying to access a session that has already been " +
                    "destroyed.");
                return;
            }

            _nativeSession.AugmentedFaceApi.GetTriangleIndices(_trackableNativeHandle, indices);
      }
    }
}
