//-----------------------------------------------------------------------
// <copyright file="RaycastManager.cs" company="Google">
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
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    public class RaycastManager
    {
        private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

        private List<Vector3> m_tmpPolygonPoints = new List<Vector3>();

        /// <summary>
        /// Hit test against all detected planes. If point cloud hit flag is enabled, it will return point
        /// cloud hit if there's no plane hit found.
        /// </summary>
        /// <param name="screenUV">Touch UV point in Unity screen space.
        ///     The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight).</param>
        /// <param name="hitTestType">Hit test method used.</param>
        /// <param name="hitPose">Output pose if hit test is successful.</param>
        /// <returns>Return true if hit test is successful, otherwise false.</returns>
        public bool Raycast(Ray ray, TrackableHitFlag filter, out TrackableHit hitResult)
        {
            TrackableHit? closestHit = null;
            Frame.GetAllPlanes(ref m_allPlanes);

            for (int i = 0; i < m_allPlanes.Count; i++)
            {
                TrackableHit? currentHit = null;

                if (!m_allPlanes[i].IsValid || m_allPlanes[i].SubsumedBy != null)
                {
                    continue;
                }

                TrackableHit hit;
                if (HitTestPlane(ray, filter, m_allPlanes[i], out hit))
                {
                    currentHit = hit;
                }

                if (currentHit != null && (closestHit == null || Vector3.Distance(ray.origin, currentHit.Value.Point)
                    < Vector3.Distance(ray.origin, closestHit.Value.Point)))
                {
                    closestHit = currentHit;
                }
            }

            if (closestHit != null)
            {
                hitResult = closestHit.Value;
                return true;
            }
            else
            {
                if (_HasFlag(filter, TrackableHitFlag.PointCloud))
                {
                    Vector3 hitpoint = new Vector3();
                    if (_IsRayIntersectingPoint(ray, Frame.PointCloud, ref hitpoint))
                    {
                        hitResult = new TrackableHit(hitpoint, Vector3.up, Vector3.Distance(hitpoint, ray.origin),
                            TrackableHitFlag.PointCloud, null);
                        return true;
                    }
                }
            }

            hitResult = new TrackableHit(Vector3.zero, Vector3.zero, 0.0f, 0, null);
            return false;
        }

        public bool HitTestPlane(Ray ray, TrackableHitFlag hitFilterFlags, TrackedPlane plane, out TrackableHit hit)
        {
            Plane unityPlane = new Plane(plane.Rotation * Vector3.up, plane.Position);
            float distance;

            if (unityPlane.Raycast(ray, out distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                TrackableHitFlag hitResultFlags = TrackableHitFlag.None;

                if (_HasFlag(hitFilterFlags, TrackableHitFlag.PlaneWithinBounds) &&
                    _IsPointInBoundingBox(hitPoint, plane))
                {
                    hitResultFlags = hitResultFlags | TrackableHitFlag.PlaneWithinBounds;
                }

                if (_HasFlag(hitFilterFlags, TrackableHitFlag.PlaneWithinPolygon) && _IsPointInPolygon(hitPoint, plane))
                {
                    hitResultFlags |= TrackableHitFlag.PlaneWithinPolygon;
                }

                if (_HasFlag(hitFilterFlags, TrackableHitFlag.PlaneWithinInfinity))
                {
                    hitResultFlags |= TrackableHitFlag.PlaneWithinInfinity;
                }

                if (hitResultFlags != TrackableHitFlag.None)
                {
                    hit = new TrackableHit(hitPoint, unityPlane.normal, distance, hitResultFlags, plane);
                    return true;
                }
            }

            hit = new TrackableHit(Vector3.zero, Vector3.zero, 0.0f, 0, null);
            return false;
        }

        /// <summary>
        /// Check if a point is within a TrackedPlane polygon's boundary. This function assume the polygon
        /// to be convex.
        /// </summary>
        /// <param name="point">3D point in world space.</param>
        /// <param name="plane">A TrackedPlane reference detected by ARCore.</param>
        private bool _IsPointInPolygon(Vector3 point, TrackedPlane plane)
        {
            List<Vector3> polygonPoints = m_tmpPolygonPoints;
            plane.GetBoundaryPolygon(ref m_tmpPolygonPoints);

            int count = polygonPoints.Count;
            if (count < 3)
            {
                return false;
            }

            Vector3 lastUp = Vector3.zero;

            for (int i = 0; i < count; ++i)
            {
                Vector3 v0 = point - polygonPoints[i];
                Vector3 v1;
                if (i == count - 1)
                {
                    v1 = polygonPoints[0] - polygonPoints[i];
                }
                else
                {
                    v1 = polygonPoints[i + 1] - polygonPoints[i];
                }

                Vector3 up = Vector3.Cross(v0, v1);
                if (i != 0)
                {
                    float sign = Vector3.Dot(up, lastUp);
                    if (sign < 0)
                    {
                        return false;
                    }
                }

                lastUp = up;
            }
            return true;
        }

        /// <summary>
        /// Check if a point is within a TrackedPlane bounding box.
        /// </summary>
        /// <param name="point">3D point in world space.</param>
        /// <param name="plane">A TrackedPlane reference detected by ARCore.</param>
        private bool _IsPointInBoundingBox(Vector3 point, TrackedPlane plane)
        {
            Matrix4x4 world_T_plane = Matrix4x4.TRS(plane.Position, plane.Rotation, Vector3.one);
            Vector3 pointInPlane = world_T_plane.inverse.MultiplyPoint3x4(point);

            if (-0.5f * plane.Bounds.x <= pointInPlane.x && pointInPlane.x <= 0.5f * plane.Bounds.x &&
                -0.5f * plane.Bounds.y <= pointInPlane.z && pointInPlane.z <= 0.5f * plane.Bounds.y)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a ray hit point with the point cloud.
        /// </summary>
        /// <param name="ray">A ray start from tracked position.</param>
        /// <param name="pointcloud">A point cloud to hit aginst.</param>
        /// <param name="hitPoint">The hit point's position.</param>
        private static bool _IsRayIntersectingPoint(Ray ray, PointCloud pointcloud, ref Vector3 hitPoint)
        {
            int pointcloudCount = pointcloud.PointCount;
            if (pointcloudCount == 0)
            {
                ARDebug.LogError("point cloud count is zero");
                return false;
            }

            float min = float.MaxValue;
            bool isPointFound = false;
            for(int i = 0; i < pointcloudCount; ++i)
            {
                Vector3 point = pointcloud.GetPoint(i);
                Vector3 v0 = point - ray.origin;
                Vector3 v1 = ray.direction;
                float angle = Vector3.Angle(v0, v1);

                const float TOUCH_SEARCH_ANGLE_DEGREE = 5.0f;

                if (angle < TOUCH_SEARCH_ANGLE_DEGREE && angle < min)
                {
                    isPointFound = true;
                    hitPoint = point;
                }
            }
            return isPointFound;
        }

        private bool _HasFlag(TrackableHitFlag filter, TrackableHitFlag flag)
        {
            return (filter & flag) != TrackableHitFlag.None;
        }
    }
}