//-----------------------------------------------------------------------
// <copyright file="AugmentedFaceApi.cs" company="Google">
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
        private NativeSession m_NativeSession;
        private float[] m_TempVertices;
        private float[] m_TempNormals;
        private float[] m_TempUVs;
        private short[] m_TempIndices;

        public AugmentedFaceApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public Pose GetCenterPose(IntPtr faceHandle)
        {
            var poseHandle = m_NativeSession.PoseApi.Create();
            ExternApi.ArAugmentedFace_getCenterPose(
                m_NativeSession.SessionHandle, faceHandle, poseHandle);
            Pose resultPose = m_NativeSession.PoseApi.ExtractPoseValue(poseHandle);
            m_NativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public Pose GetRegionPose(IntPtr faceHandle, ApiAugmentedFaceRegionType regionType)
        {
            var poseHandle = m_NativeSession.PoseApi.Create();
            ExternApi.ArAugmentedFace_getRegionPose(
                m_NativeSession.SessionHandle, faceHandle, regionType, poseHandle);
            Pose resultPose = m_NativeSession.PoseApi.ExtractPoseValue(poseHandle);
            m_NativeSession.PoseApi.Destroy(poseHandle);
            return resultPose;
        }

        public void GetVertices(IntPtr faceHandle, List<Vector3> vertices)
        {
            IntPtr verticesHandle = IntPtr.Zero;
            int verticesNum = 0;
            ExternApi.ArAugmentedFace_getMeshVertices(m_NativeSession.SessionHandle, faceHandle,
                ref verticesHandle, ref verticesNum);
            int floatNum = verticesNum * 3;
            if (m_TempVertices == null || m_TempVertices.Length != floatNum)
            {
                m_TempVertices = new float[floatNum];
            }

            Marshal.Copy(verticesHandle, m_TempVertices, 0, floatNum);

            vertices.Clear();
            vertices.Capacity = verticesNum;
            for (int i = 0; i < floatNum; i += 3)
            {
                vertices.Add(
                    new Vector3(m_TempVertices[i], m_TempVertices[i + 1], -m_TempVertices[i + 2]));
            }
        }

        public void GetNormals(IntPtr faceHandle, List<Vector3> normals)
        {
            IntPtr normalsHandle = IntPtr.Zero;
            int verticesNum = 0;
            ExternApi.ArAugmentedFace_getMeshNormals(m_NativeSession.SessionHandle, faceHandle,
                ref normalsHandle, ref verticesNum);
            int floatNum = verticesNum * 3;
            if (m_TempNormals == null || m_TempNormals.Length != floatNum)
            {
                m_TempNormals = new float[floatNum];
            }

            Marshal.Copy(normalsHandle, m_TempNormals, 0, floatNum);

            normals.Clear();
            normals.Capacity = verticesNum;
            for (int i = 0; i < floatNum; i += 3)
            {
                normals.Add(
                    new Vector3(m_TempNormals[i], m_TempNormals[i + 1], -m_TempNormals[i + 2]));
            }
        }

        public void GetTextureCoordinates(IntPtr faceHandle, List<Vector2> textureCoordinates)
        {
            IntPtr textureCoordinatesHandle = IntPtr.Zero;
            int uvNum = 0;
            ExternApi.ArAugmentedFace_getMeshTextureCoordinates(
                m_NativeSession.SessionHandle, faceHandle, ref textureCoordinatesHandle, ref uvNum);
            int floatNum = uvNum * 2;
            if (m_TempUVs == null || m_TempUVs.Length != floatNum)
            {
                m_TempUVs = new float[floatNum];
            }

            Marshal.Copy(textureCoordinatesHandle, m_TempUVs, 0, floatNum);

            textureCoordinates.Clear();
            textureCoordinates.Capacity = uvNum;
            for (int i = 0; i < floatNum; i += 2)
            {
                textureCoordinates.Add(new Vector2(m_TempUVs[i], m_TempUVs[i + 1]));
            }
        }

        public void GetTriangleIndices(IntPtr faceHandle, List<int> indices)
        {
            IntPtr triangleIndicesHandle = IntPtr.Zero;
            int triangleNum = 0;
            ExternApi.ArAugmentedFace_getMeshTriangleIndices(
                m_NativeSession.SessionHandle, faceHandle, ref triangleIndicesHandle,
                ref triangleNum);
            int indicesNum = triangleNum * 3;
            if (m_TempIndices == null || m_TempIndices.Length != indicesNum)
            {
                m_TempIndices = new short[indicesNum];
            }

            Marshal.Copy(triangleIndicesHandle, m_TempIndices, 0, indicesNum);

            indices.Clear();
            indices.Capacity = indicesNum;
            for (int i = 0; i < indicesNum; i += 3)
            {
                indices.Add(m_TempIndices[i]);
                indices.Add(m_TempIndices[i + 2]);
                indices.Add(m_TempIndices[i + 1]);
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
