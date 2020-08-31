//-----------------------------------------------------------------------
// <copyright file="AugmentedFaceApi.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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
    using System.Runtime.InteropServices;
    using GoogleARCoreInternal;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class AugmentedFaceApi
    {
        private NativeSession _nativeSession;
        private float[] _tempVertices;
        private float[] _tempNormals;
        private float[] _tempUVs;
        private short[] _tempIndices;

        public AugmentedFaceApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public Pose GetCenterPose(IntPtr faceHandle)
        {
            var poseHandle = _nativeSession.PoseApi.Create();
            ExternApi.ArAugmentedFace_getCenterPose(
                _nativeSession.SessionHandle, faceHandle, poseHandle);
            Pose resultPose = _nativeSession.PoseApi.ExtractPoseValue(poseHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public Pose GetRegionPose(IntPtr faceHandle, ApiAugmentedFaceRegionType regionType)
        {
            var poseHandle = _nativeSession.PoseApi.Create();
            ExternApi.ArAugmentedFace_getRegionPose(
                _nativeSession.SessionHandle, faceHandle, regionType, poseHandle);
            Pose resultPose = _nativeSession.PoseApi.ExtractPoseValue(poseHandle);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public void GetVertices(IntPtr faceHandle, List<Vector3> vertices)
        {
            IntPtr verticesHandle = IntPtr.Zero;
            int verticesNum = 0;
            ExternApi.ArAugmentedFace_getMeshVertices(_nativeSession.SessionHandle, faceHandle,
                ref verticesHandle, ref verticesNum);
            int floatNum = verticesNum * 3;
            if (_tempVertices == null || _tempVertices.Length != floatNum)
            {
                _tempVertices = new float[floatNum];
            }

            Marshal.Copy(verticesHandle, _tempVertices, 0, floatNum);

            vertices.Clear();
            vertices.Capacity = verticesNum;
            for (int i = 0; i < floatNum; i += 3)
            {
                vertices.Add(
                    new Vector3(_tempVertices[i], _tempVertices[i + 1], -_tempVertices[i + 2]));
            }
        }

        public void GetNormals(IntPtr faceHandle, List<Vector3> normals)
        {
            IntPtr normalsHandle = IntPtr.Zero;
            int verticesNum = 0;
            ExternApi.ArAugmentedFace_getMeshNormals(_nativeSession.SessionHandle, faceHandle,
                ref normalsHandle, ref verticesNum);
            int floatNum = verticesNum * 3;
            if (_tempNormals == null || _tempNormals.Length != floatNum)
            {
                _tempNormals = new float[floatNum];
            }

            Marshal.Copy(normalsHandle, _tempNormals, 0, floatNum);

            normals.Clear();
            normals.Capacity = verticesNum;
            for (int i = 0; i < floatNum; i += 3)
            {
                normals.Add(
                    new Vector3(_tempNormals[i], _tempNormals[i + 1], -_tempNormals[i + 2]));
            }
        }

        public void GetTextureCoordinates(IntPtr faceHandle, List<Vector2> textureCoordinates)
        {
            IntPtr textureCoordinatesHandle = IntPtr.Zero;
            int uvNum = 0;
            ExternApi.ArAugmentedFace_getMeshTextureCoordinates(
                _nativeSession.SessionHandle, faceHandle, ref textureCoordinatesHandle, ref uvNum);
            int floatNum = uvNum * 2;
            if (_tempUVs == null || _tempUVs.Length != floatNum)
            {
                _tempUVs = new float[floatNum];
            }

            Marshal.Copy(textureCoordinatesHandle, _tempUVs, 0, floatNum);

            textureCoordinates.Clear();
            textureCoordinates.Capacity = uvNum;
            for (int i = 0; i < floatNum; i += 2)
            {
                textureCoordinates.Add(new Vector2(_tempUVs[i], _tempUVs[i + 1]));
            }
        }

        public void GetTriangleIndices(IntPtr faceHandle, List<int> indices)
        {
            IntPtr triangleIndicesHandle = IntPtr.Zero;
            int triangleNum = 0;
            ExternApi.ArAugmentedFace_getMeshTriangleIndices(
                _nativeSession.SessionHandle, faceHandle, ref triangleIndicesHandle,
                ref triangleNum);
            int indicesNum = triangleNum * 3;
            if (_tempIndices == null || _tempIndices.Length != indicesNum)
            {
                _tempIndices = new short[indicesNum];
            }

            Marshal.Copy(triangleIndicesHandle, _tempIndices, 0, indicesNum);

            indices.Clear();
            indices.Capacity = indicesNum;
            for (int i = 0; i < indicesNum; i += 3)
            {
                indices.Add(_tempIndices[i]);
                indices.Add(_tempIndices[i + 2]);
                indices.Add(_tempIndices[i + 1]);
            }
        }

        private struct ExternApi
        {
#pragma warning disable 626

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedFace_getCenterPose(
                IntPtr sessionHandle, IntPtr faceHandle, IntPtr poseHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedFace_getMeshVertices(
                IntPtr sessionHandle, IntPtr faceHandle, ref IntPtr vertices,
                ref Int32 numberOfVertices);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedFace_getMeshNormals(
                IntPtr sessionHandle, IntPtr faceHandle, ref IntPtr normals,
                ref Int32 numberOfVertices);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedFace_getMeshTextureCoordinates(
                IntPtr sessionHandle, IntPtr faceHandle, ref IntPtr uvs,
                ref Int32 numberOfVertices);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedFace_getMeshTriangleIndices(
                IntPtr sessionHandle, IntPtr faceHandle, ref IntPtr indices,
                ref Int32 numberOfTriangles);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArAugmentedFace_getRegionPose(
                IntPtr sessionHandle, IntPtr faceHandle, ApiAugmentedFaceRegionType regionType,
                IntPtr poseHandle);
#pragma warning restore 626
        }
    }
}
