//-----------------------------------------------------------------------
// <copyright file="DisplayUvCoords.cs" company="Google LLC">
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

namespace GoogleARCore
{
    using UnityEngine;

    /// <summary>
    /// Stores UV display coordinates for mapping the four corners of the display.
    /// </summary>
    public struct DisplayUvCoords
    {
        /// <summary>
        /// Gets full screen uv coordinates.
        /// </summary>
        public static readonly DisplayUvCoords FullScreenUvCoords = new DisplayUvCoords(
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0));

        /// <summary>
        /// The top-left UV coordinates for the display.
        /// </summary>
        public Vector2 TopLeft;

        /// <summary>
        /// The top-right UV coordinates for the display.
        /// </summary>
        public Vector2 TopRight;

        /// <summary>
        /// The bottom-left UV coordinates for the display.
        /// </summary>
        public Vector2 BottomLeft;

        /// <summary>
        /// The bottom-right UV coordinates for the display.
        /// </summary>
        public Vector2 BottomRight;

        /// <summary>
        /// Initializes the DisplayUvCoords structure.
        /// </summary>
        /// <param name="topLeft">The top-left UV coordinates for the display.</param>
        /// <param name="topRight">The top-right UV coordinates for the display.</param>
        /// <param name="bottomLeft">The bottom-left UV coordinates for the display.</param>
        /// <param name="bottomRight">The bottom-right UV coordinates for the display.</param>
        public DisplayUvCoords(
            Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        //// @cond EXCLUDE_FROM_DOXYGEN

        [System.Obsolete(
            "The type ApiDisplayCoords is deprecated.  Please use GoogleARCore.DisplayUvCoords.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1600:ElementsMustBeDocumented",
            Justification = "Deprecated")]
        public static implicit operator GoogleARCoreInternal.ApiDisplayUvCoords(
            DisplayUvCoords coords)
        {
            return new GoogleARCoreInternal.ApiDisplayUvCoords(
                coords.TopLeft, coords.TopRight, coords.BottomLeft, coords.BottomRight);
        }

        //// @endcond
    }
}
