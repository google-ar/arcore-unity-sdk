//-----------------------------------------------------------------------
// <copyright file="HitTestApi.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using GoogleARCore;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class HitTestApi
    {
        private NativeSession _nativeSession;

        public HitTestApi(NativeSession nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public bool Raycast(
            IntPtr frameHandle, float x, float y, TrackableHitFlags filter,
            List<TrackableHit> outHitList)
        {
            outHitList.Clear();

            IntPtr hitResultListHandle = IntPtr.Zero;
            ExternApi.ArHitResultList_create(
                _nativeSession.SessionHandle, ref hitResultListHandle);
            ExternApi.ArFrame_hitTest(
                _nativeSession.SessionHandle, frameHandle, x, y, hitResultListHandle);
            FilterTrackableHits(hitResultListHandle, Mathf.Infinity, filter, outHitList);
            ExternApi.ArHitResultList_destroy(hitResultListHandle);
            return outHitList.Count != 0;
        }

        public bool Raycast(IntPtr frameHandle, float x, float y, float approximateDistanceMeters,
            List<TrackableHit> outHitList)
        {
            outHitList.Clear();

            IntPtr hitResultListHandle = IntPtr.Zero;
            ExternApi.ArHitResultList_create(_nativeSession.SessionHandle, ref hitResultListHandle);
            ExternApi.ArFrame_hitTestInstantPlacement(_nativeSession.SessionHandle, frameHandle,
                x, y, approximateDistanceMeters, hitResultListHandle);
            FilterTrackableHits(
                hitResultListHandle, Mathf.Infinity, TrackableHitFlags.None, outHitList);
            ExternApi.ArHitResultList_destroy(hitResultListHandle);
            return outHitList.Count != 0;
        }

        public bool Raycast(
            IntPtr frameHandle, Vector3 origin, Vector3 direction, float maxDistance,
            TrackableHitFlags filter, List<TrackableHit> outHitList)
        {
            outHitList.Clear();

            IntPtr hitResultListHandle = IntPtr.Zero;
            ExternApi.ArHitResultList_create(
                _nativeSession.SessionHandle, ref hitResultListHandle);

            // Invert z to match ARCore coordinate system.
            origin.z = -origin.z;
            direction.z = -direction.z;
            ExternApi.ArFrame_hitTestRay(
                _nativeSession.SessionHandle, frameHandle, ref origin, ref direction,
                hitResultListHandle);
            FilterTrackableHits(hitResultListHandle, maxDistance, filter, outHitList);
            ExternApi.ArHitResultList_destroy(hitResultListHandle);
            return outHitList.Count != 0;
        }

        private void FilterTrackableHits(
            IntPtr hitResultListHandle, float maxDistance, TrackableHitFlags filter,
            List<TrackableHit> outHitList)
        {
            int hitListSize = 0;
            ExternApi.ArHitResultList_getSize(
                _nativeSession.SessionHandle, hitResultListHandle, ref hitListSize);

            for (int i = 0; i < hitListSize; i++)
            {
                TrackableHit trackableHit;
                if (HitResultListGetItemAt(hitResultListHandle, i, out trackableHit))
                {
                    if ((filter & trackableHit.Flags) != TrackableHitFlags.None &&
                        trackableHit.Distance <= maxDistance)
                    {
                        outHitList.Add(trackableHit);
                    }
                    // InstantPlacementPoint is not controlled by TrackableHitFlags.
                    else if (trackableHit.Trackable is InstantPlacementPoint &&
                        trackableHit.Distance <= maxDistance)
                    {
                        outHitList.Add(trackableHit);
                    }
                }
            }
        }

        private bool HitResultListGetItemAt(
            IntPtr hitResultListHandle, int index, out TrackableHit outTrackableHit)
        {
            outTrackableHit = new TrackableHit();

            // Query the hit result.
            IntPtr hitResultHandle = IntPtr.Zero;
            ExternApi.ArHitResult_create(_nativeSession.SessionHandle, ref hitResultHandle);
            ExternApi.ArHitResultList_getItem(
                _nativeSession.SessionHandle, hitResultListHandle, index, hitResultHandle);
            if (hitResultHandle == IntPtr.Zero)
            {
                ExternApi.ArHitResult_destroy(hitResultHandle);
                return false;
            }

            // Query the pose from hit result.
            IntPtr poseHandle = _nativeSession.PoseApi.Create();
            ExternApi.ArHitResult_getHitPose(
                _nativeSession.SessionHandle, hitResultHandle, poseHandle);
            Pose hitPose = _nativeSession.PoseApi.ExtractPoseValue(poseHandle);

            // Query the distance from hit result.
            float hitDistance = 0.0f;
            ExternApi.ArHitResult_getDistance(
                _nativeSession.SessionHandle, hitResultHandle, ref hitDistance);

            // Query the trackable from hit result.
            IntPtr trackableHandle = IntPtr.Zero;
            ExternApi.ArHitResult_acquireTrackable(
                _nativeSession.SessionHandle, hitResultHandle, ref trackableHandle);
            Trackable trackable = _nativeSession.TrackableFactory(trackableHandle);
            _nativeSession.TrackableApi.Release(trackableHandle);

            // Calculate trackable hit flags.
            TrackableHitFlags flag = TrackableHitFlags.None;
            if (trackable == null)
            {
                Debug.Log("Could not create trackable from hit result.");
                _nativeSession.PoseApi.Destroy(poseHandle);
                return false;
            }
            else if (trackable is DetectedPlane)
            {
                if (_nativeSession.PlaneApi.IsPoseInPolygon(trackableHandle, poseHandle))
                {
                    flag |= TrackableHitFlags.PlaneWithinPolygon;
                }

                if (_nativeSession.PlaneApi.IsPoseInExtents(trackableHandle, poseHandle))
                {
                    flag |= TrackableHitFlags.PlaneWithinBounds;
                }

                flag |= TrackableHitFlags.PlaneWithinInfinity;
            }
            else if (trackable is FeaturePoint)
            {
                var point = trackable as FeaturePoint;
                flag |= TrackableHitFlags.FeaturePoint;
                if (point.OrientationMode == FeaturePointOrientationMode.SurfaceNormal)
                {
                    flag |= TrackableHitFlags.FeaturePointWithSurfaceNormal;
                }
            }
            else if (trackable is InstantPlacementPoint)
            {
                // No flag update for InstantPlacementPoint Trackable Type.
            }
            else if (trackable is DepthPoint)
            {
                flag |= TrackableHitFlags.Depth;
            }
            else
            {
                ApiTrackableType trackableType =
                    _nativeSession.TrackableApi.GetType(trackableHandle);
                if (!ExperimentManager.Instance.IsManagingTrackableType((int)trackableType))
                {
                    _nativeSession.PoseApi.Destroy(poseHandle);
                    return false;
                }

                flag |= ExperimentManager.Instance.GetTrackableHitFlags((int)trackableType);
            }

            outTrackableHit = new TrackableHit(hitPose, hitDistance, flag, trackable);
            _nativeSession.PoseApi.Destroy(poseHandle);
            return true;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_hitTest(
                IntPtr session, IntPtr frame, float pixel_x, float pixel_y, IntPtr hit_result_list);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_hitTestRay(
                IntPtr session, IntPtr frame, ref Vector3 origin, ref Vector3 direction,
                IntPtr hit_result_list);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_hitTestInstantPlacement(
                IntPtr session, IntPtr frame, float pixel_x, float pixel_y,
                float guessed_distance_meters, IntPtr hit_result_list);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_create(
                IntPtr session, ref IntPtr out_hit_result_list);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_destroy(
                IntPtr hit_result_list);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_getSize(
                IntPtr session, IntPtr hit_result_list, ref int out_size);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResultList_getItem(
                IntPtr session, IntPtr hit_result_list, int index, IntPtr out_hit_result);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_create(IntPtr session, ref IntPtr out_hit_result);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_destroy(IntPtr hit_result);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_getDistance(IntPtr session, IntPtr hit_result,
                ref float out_distance);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_getHitPose(
                IntPtr session, IntPtr hit_result, IntPtr out_pose);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArHitResult_acquireTrackable(
                IntPtr session, IntPtr hit_result, ref IntPtr out_trackable);
#pragma warning restore 626
        }
    }
}
