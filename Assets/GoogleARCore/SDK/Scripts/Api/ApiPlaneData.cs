//-----------------------------------------------------------------------
// <copyright file="ApiPlaneData.cs" company="Google">
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
    using System;
    using System.Runtime.InteropServices;

    /// The coordinate system of the plane frame is defined as:
    /// The Z axis is vertical with the positive direction pointing up.
    /// The X axis and Y axis are horizontal in the plane.
    /// The positive direction of X axis points to the projection point of camera
    /// on the XY plane.
    [StructLayout(LayoutKind.Sequential)]
    public struct ApiPlaneData
    {
        public int id;
        public ApiPoseData pose;
        public IntPtr boundaryPolygon;
        public int boundaryPointNum;
        public double centerX;
        public double centerY;
        public double width;
        public double height;
        public double yaw;
        public double timestamp;
        public int subsumedBy;
        public bool isValid;
    }
}