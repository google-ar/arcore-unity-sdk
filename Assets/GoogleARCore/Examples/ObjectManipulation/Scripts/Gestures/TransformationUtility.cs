//-----------------------------------------------------------------------
// <copyright file="TransformationUtility.cs" company="Google">
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

namespace GoogleARCore.Examples.ObjectManipulationInternal
{
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Provides helper functions for common functionality for transforming objects in AR.
    /// </summary>
    public static class TransformationUtility
    {
        /// <summary>
        /// Slight offset of the down ray used in GetBestPlacementPosition to ensure that the
        /// current groundingPlane is included in the hit results.
        /// </summary>
        private const float k_DownRayOffset = 0.01f;

        /// <summary>
        /// Max amount (inches) to offset the screen touch in GetBestPlacementPosition.
        /// The actual amount if dependent on the angle of the camera relative.
        /// The further downward the camera is angled, the more the screen touch is offset.
        /// </summary>
        private const float k_MaxScreenTouchOffset = 0.4f;

        /// <summary>
        /// In GetBestPlacementPosition, when the camera is closer than this value to the object,
        /// reduce how much the object hovers.
        /// </summary>
        private const float k_HoverDistanceThreshold = 1.0f;

        /// <summary>
        /// Translation mode.
        /// </summary>
        public enum TranslationMode
        {
            /// <summary>
            /// Horizontal translation mode.
            /// </summary>
            Horizontal,

            /// <summary>
            /// Vertical translation mode.
            /// </summary>
            Vertical,

            /// <summary>
            /// Horizontal or vertical translation mode.
            /// </summary>
            Any,
        }

        /// <summary>
        /// Calculates the best position to place an object in AR based on screen position.
        /// Could be used for tapping a location on the screen, dragging an object, or using a fixed
        /// cursor in the center of the screen for placing and moving objects.
        ///
        /// Objects are placed along the x/z of the grounding plane. When placed on an AR plane
        /// below the grounding plane, the object will drop straight down onto it in world space.
        /// This prevents the object from being pushed deeper into the scene when moving from a
        /// higher plane to a lower plane. When moving from a lower plane to a higher plane, this
        /// function returns a new groundingPlane to replace the old one.
        /// </summary>
        /// <returns>The best placement position.</returns>
        /// <param name="currentAnchorPosition">Position of the parent anchor, i.e., where the
        /// object is before translation starts.</param>
        /// <param name="screenPos">Location on the screen in pixels to place the object at.</param>
        /// <param name="groundingPlaneHeight">The starting height of the plane that the object is
        /// being placed along.</param>
        /// <param name="hoverOffset">How much should the object hover above the groundingPlane
        /// before it has been placed.</param>
        /// <param name="maxTranslationDistance">The maximum distance that the object can be
        /// translated.</param>
        /// <param name="translationMode">The translation mode, indicating the plane types allowed.
        /// </param>
        public static Placement GetBestPlacementPosition(
            Vector3 currentAnchorPosition,
            Vector2 screenPos,
            float groundingPlaneHeight,
            float hoverOffset,
            float maxTranslationDistance,
            TranslationMode translationMode)
        {
            Placement result = new Placement();
            result.UpdatedGroundingPlaneHeight = groundingPlaneHeight;

            // Get the angle between the camera and the object's down direction.
            float angle = Vector3.Angle(Camera.main.transform.forward, Vector3.down);

            angle = 90.0f - angle;

            float touchOffsetRatio = Mathf.Clamp01(angle / 90.0f);
            float screenTouchOffset = touchOffsetRatio * k_MaxScreenTouchOffset;
            screenPos.y += GestureTouchesUtility.InchesToPixels(screenTouchOffset);

            float hoverRatio = Mathf.Clamp01(angle / 45.0f);
            hoverOffset *= hoverRatio;

            float distance = (Camera.main.transform.position - currentAnchorPosition).magnitude;
            float distanceHoverRatio = Mathf.Clamp01(distance / k_HoverDistanceThreshold);
            hoverOffset *= distanceHoverRatio;

            // The best estimate of the point in the plane where the object will be placed:
            Vector3 groundingPoint;

            // Get the ray to cast into the scene from the perspective of the camera.
            TrackableHit hit;
            if (Frame.Raycast(
                screenPos.x, screenPos.y, TrackableHitFlags.PlaneWithinBounds, out hit))
            {
                if (hit.Trackable is DetectedPlane)
                {
                    DetectedPlane plane = hit.Trackable as DetectedPlane;
                    if (IsPlaneTypeAllowed(translationMode, plane.PlaneType))
                    {
                        // Avoid detecting the back of existing planes.
                        if ((hit.Trackable is DetectedPlane) &&
                            Vector3.Dot(Camera.main.transform.position - hit.Pose.position,
                                        hit.Pose.rotation * Vector3.up) < 0)
                        {
                            Debug.Log("Hit at back of the current DetectedPlane");
                            return result;
                        }

                        // Don't allow hovering for vertical or horizontal downward facing planes.
                        if (plane.PlaneType == DetectedPlaneType.Vertical ||
                            plane.PlaneType == DetectedPlaneType.HorizontalDownwardFacing)
                        {
                            // Limit the translation to maxTranslationDistance.
                            groundingPoint = LimitTranslation(
                                hit.Pose.position, currentAnchorPosition, maxTranslationDistance);

                            result.PlacementPlane = hit;
                            result.PlacementPosition = groundingPoint;
                            result.HoveringPosition = groundingPoint;
                            result.UpdatedGroundingPlaneHeight = groundingPoint.y;
                            result.PlacementRotation = hit.Pose.rotation;
                            return result;
                        }

                        // Allow hovering for horizontal upward facing planes.
                        if (plane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing)
                        {
                            // Return early if the camera is pointing upwards.
                            if (angle < 0f)
                            {
                                return result;
                            }

                            // Limit the translation to maxTranslationDistance.
                            groundingPoint = LimitTranslation(
                                hit.Pose.position, currentAnchorPosition, maxTranslationDistance);

                            // Find the hovering position by casting from the camera onto the
                            // grounding plane and offsetting the result by the hover offset.
                            result.HoveringPosition = groundingPoint + (Vector3.up * hoverOffset);

                            // If the AR Plane is above the grounding plane, then the hit plane's
                            // position is used to replace the current groundingPlane. Otherwise,
                            // the hit is ignored because hits are only detected on lower planes by
                            // casting straight downwards in world space.
                            if (groundingPoint.y > groundingPlaneHeight)
                            {
                                result.PlacementPlane = hit;
                                result.PlacementPosition = groundingPoint;
                                result.UpdatedGroundingPlaneHeight = hit.Pose.position.y;
                                result.PlacementRotation = hit.Pose.rotation;
                                return result;
                            }
                        }
                        else
                        {
                            // Not supported plane type.
                            return result;
                        }
                    }
                    else
                    {
                        // Plane type not allowed.
                        return result;
                    }
                }
                else
                {
                    // Hit is not a plane.
                    return result;
                }
            }

            // Return early if the camera is pointing upwards.
            if (angle < 0f)
            {
                return result;
            }

            // If the grounding point is lower than the current gorunding plane height, or if the
            // raycast did not return a hit, then we extend the grounding plane to infinity, and do
            // a new raycast into the scene from the perspective of the camera.
            Ray cameraRay = Camera.main.ScreenPointToRay(screenPos);
            Plane groundingPlane =
                new Plane(Vector3.up, new Vector3(0.0f, groundingPlaneHeight, 0.0f));

            // Find the hovering position by casting from the camera onto the grounding plane
            // and offsetting the result by the hover offset.
            float enter;
            if (groundingPlane.Raycast(cameraRay, out enter))
            {
                groundingPoint = LimitTranslation(
                    cameraRay.GetPoint(enter), currentAnchorPosition, maxTranslationDistance);

                result.HoveringPosition = groundingPoint + (Vector3.up * hoverOffset);
            }
            else
            {
                // If we can't successfully cast onto the groundingPlane, just return early.
                return result;
            }

            // Cast straight down onto AR planes that are lower than the current grounding plane.
            if (Frame.Raycast(
                    groundingPoint + (Vector3.up * k_DownRayOffset), Vector3.down,
                    out hit, Mathf.Infinity, TrackableHitFlags.PlaneWithinBounds))
            {
                result.PlacementPosition = hit.Pose.position;
                result.PlacementPlane = hit;
                result.PlacementRotation = hit.Pose.rotation;
                return result;
            }

            return result;
        }

        /// <summary>
        /// Limits the translation to the maximum distance allowed.
        /// </summary>
        /// <returns>The new target position, limited so that the object does not tranlsate more
        /// than the maximum allowed distance.</returns>
        /// <param name="desiredPosition">Desired position.</param>
        /// <param name="currentPosition">Current position.</param>
        /// <param name="maxTranslationDistance">Max translation distance.</param>
        private static Vector3 LimitTranslation(Vector3 desiredPosition, Vector3 currentPosition,
                                                float maxTranslationDistance)
        {
            if ((desiredPosition - currentPosition).magnitude > maxTranslationDistance)
            {
                return currentPosition + (
                    (desiredPosition - currentPosition).normalized * maxTranslationDistance);
            }

            return desiredPosition;
        }

        private static bool IsPlaneTypeAllowed(
            TranslationMode translationMode, DetectedPlaneType planeType)
        {
            if (translationMode == TranslationMode.Any)
            {
                return true;
            }

            if (translationMode == TranslationMode.Horizontal &&
               (planeType == DetectedPlaneType.HorizontalDownwardFacing ||
                planeType == DetectedPlaneType.HorizontalUpwardFacing))
            {
                return true;
            }

            if (translationMode == TranslationMode.Vertical &&
               planeType == DetectedPlaneType.Vertical)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Result of the function GetBestPlacementPosition that indicates if a placementPosition
        /// was found and information about the placement position.
        /// </summary>
        public struct Placement
        {
            /// <summary>
            /// The position that the object should be displayed at before the placement has been
            /// confirmed.
            /// </summary>
            public Vector3? HoveringPosition;

            /// <summary>
            /// The resulting position that the object should be placed at.
            /// </summary>
            public Vector3? PlacementPosition;

            /// <summary>
            /// The resulting rotation that the object should have.
            /// </summary>
            public Quaternion? PlacementRotation;

            /// <summary>
            /// The AR Plane that the object is being placed on.
            /// </summary>
            public TrackableHit? PlacementPlane;

            /// <summary>
            /// This is the updated groundingPlaneHeight resulting from this hit detection.
            /// </summary>
            public float UpdatedGroundingPlaneHeight;
        }
    }
}
