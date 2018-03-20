//-----------------------------------------------------------------------
// <copyright file="ApiDisplayUvCoordsExtensions.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
        Justification = "Internal")]
    internal static class ApiDisplayUvCoordsExtensions
    {
        public static DisplayUvCoords ToDisplayUvCoords(this ApiDisplayUvCoords apiCoords)
        {
            return new DisplayUvCoords(apiCoords.TopLeft, apiCoords.TopRight,
                apiCoords.BottomLeft, apiCoords.BottomRight);
        }
    }
}
