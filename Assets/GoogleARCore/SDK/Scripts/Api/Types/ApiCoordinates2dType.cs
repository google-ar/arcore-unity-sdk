//-----------------------------------------------------------------------
// <copyright file="ApiCoordinates2dType.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    internal enum ApiCoordinates2dType
    {
        TexturePixels = 0,
        TextureNormalized = 1,
        ImagePixels = 2,
        ImageNormalized = 3,
        FeatureTrackingImage = 4,
        FeatureTrackingImageNormalized = 5,
        OpenGLDeviceNormalized = 6,
        View = 7,
        ViewNormalized = 8,
    }
}
