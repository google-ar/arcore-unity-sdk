//-----------------------------------------------------------------------
// <copyright file="ApiPoseData.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityTango = GoogleAR.UnityNative;

    /// <summary>
    /// The TangoPoseData struct contains 6DOF pose information.
    ///
    /// The device pose is given using Android conventions.  See the Android
    /// <a href ="http://developer.android.com/guide/topics/sensors/sensors_overview.html#sensors-coords">Sensor
    /// Overview</a> page for more information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ApiPoseData
    {
        /// <summary>
        /// An integer denoting the version of the structure.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        /// <summary>
        /// Timestamp of the time that this pose estimate corresponds to.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double timestamp;

        /// <summary>
        /// Orientation, as a quaternion, of the pose of the target frame
        /// with reference to the base frame.
        /// Specified as (x,y,z,w) where RotationAngle is in radians:
        /// <code>
        ///   x = RotationAxis.x * sin(RotationAngle / 2)
        ///   y = RotationAxis.y * sin(RotationAngle / 2)
        ///   z = RotationAxis.z * sin(RotationAngle / 2)
        ///   w = cos(RotationAngle / 2)
        /// </code>
        /// </summary>
        public DVector4 orientation;

        /// <summary>
        /// Translation, ordered x, y, z, of the pose of the target frame
        /// with reference to the base frame.
        /// </summary>
        public DVector3 translation;

        /// <summary>
        /// The status of the pose, according to the pose lifecycle.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public ApiPoseStatusType statusCode;

        /// <summary>
        /// The pair of coordinate frames for this pose.
        ///
        /// We retrieve a pose for a target coordinate frame (such as the Tango device) against a base
        /// coordinate frame (such as a learned area).
        /// </summary>
        [MarshalAs(UnmanagedType.Struct)]
        public ApiCoordinateFramePair framePair;

        /// <summary>
        /// Unused.  Integer levels are determined by service.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int confidence;

        /// <summary>
        /// Unused.  Reserved for metric accuracy.
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float accuracy;

        /// <summary>
        /// Constructs an ApiPoseData equivalent to a unity PoseData.
        /// </summary>
        public ApiPoseData(UnityTango.PoseData unityPoseData)
        {
            // TODO (mtsmall): Tell unity version and accuracy should be uint.
            version = (int)unityPoseData.version;
            timestamp = unityPoseData.timestamp;
            orientation = new DVector4(unityPoseData.orientation_x, unityPoseData.orientation_y,
                unityPoseData.orientation_z, unityPoseData.orientation_w);
            translation = new DVector3(unityPoseData.orientation_x, unityPoseData.orientation_y,
                unityPoseData.orientation_z);
            statusCode = unityPoseData.statusCode.ToApiType();
            framePair = new ApiCoordinateFramePair(unityPoseData.frame);
            confidence = (int)unityPoseData.confidence;
            accuracy = unityPoseData.accuracy;
        }

        /// <summary>
        /// Gets a unity PoseData equivalent to the ApiPoseData.
        /// </summary>
        /// <returns>A unity PoseData equivalent to the ApiPoseData.</returns>
        public UnityTango.PoseData ToUnityType()
        {
            UnityTango.PoseData unityPose = new UnityTango.PoseData();

            unityPose.version = (uint)version;
            unityPose.timestamp = timestamp;
            unityPose.statusCode = statusCode.ToUnityType();
            unityPose.frame = framePair.ToUnityType();
            unityPose.confidence = (uint)confidence;
            unityPose.accuracy = accuracy;

            if (framePair.baseFrame != ApiCoordinateFrameType.StartOfService) {
                ARDebug.LogErrorFormat("apiPlaneData's base frame is not supported.");
                unityPose.translation_x = translation.x;
                unityPose.translation_y = translation.y;
                unityPose.translation_z = translation.z;

                unityPose.orientation_x = orientation.x;
                unityPose.orientation_y = orientation.y;
                unityPose.orientation_z = orientation.z;
                unityPose.orientation_w = orientation.w;
                return unityPose;
            }


            Matrix4x4 startService_T_plane = Matrix4x4.TRS(translation.ToVector3(),
                orientation.ToQuaternion(), Vector3.one);

            Matrix4x4 unityTransform = Constants.UNITY_WORLD_T_START_SERVICE *
                startService_T_plane *
                Constants.UNITY_WORLD_T_START_SERVICE.inverse;

            Vector3 position = unityTransform.GetColumn(3);
            unityPose.translation_x = position.x;
            unityPose.translation_y = position.y;
            unityPose.translation_z = position.z;

            Quaternion rotation = Quaternion.LookRotation(unityTransform.GetColumn(2),
                unityTransform.GetColumn(1));
            unityPose.orientation_x = rotation.x;
            unityPose.orientation_y = rotation.y;
            unityPose.orientation_z = rotation.z;
            unityPose.orientation_w = rotation.w;
            return unityPose;
        }
    }
}