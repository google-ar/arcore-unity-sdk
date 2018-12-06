//-----------------------------------------------------------------------
// <copyright file="ApiCloudAnchorStateExtensions.cs" company="Google">
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

namespace GoogleARCoreInternal.CrossPlatform
{
    using GoogleARCore.CrossPlatform;

    internal static class ApiCloudAnchorStateExtensions
    {
        public static CloudServiceResponse ToCloudServiceResponse(this ApiCloudAnchorState anchorState)
        {
            switch (anchorState)
            {
                case ApiCloudAnchorState.Success:
                    return CloudServiceResponse.Success;
                case ApiCloudAnchorState.ErrorServiceUnavailable:
                    return CloudServiceResponse.ErrorServiceUnreachable;
                case ApiCloudAnchorState.ErrorNotAuthorized:
                    return CloudServiceResponse.ErrorNotAuthorized;
                case ApiCloudAnchorState.ErrorResourceExhausted:
                    return CloudServiceResponse.ErrorApiQuotaExceeded;
                case ApiCloudAnchorState.ErrorHostingDatasetProcessingFailed:
                    return CloudServiceResponse.ErrorDatasetInadequate;
                case ApiCloudAnchorState.ErrorResolveingCloudIdNotFound:
                    return CloudServiceResponse.ErrorCloudIdNotFound;
                case ApiCloudAnchorState.ErrorResolvingLocalizationNoMatch:
                    return CloudServiceResponse.ErrorLocalizationFailed;
                case ApiCloudAnchorState.ErrorResolvingSDKTooOld:
                    return CloudServiceResponse.ErrorSDKTooOld;
                case ApiCloudAnchorState.ErrorResolvingSDKTooNew:
                    return CloudServiceResponse.ErrorSDKTooNew;
                case ApiCloudAnchorState.None:
                case ApiCloudAnchorState.TaskInProgress:
                case ApiCloudAnchorState.ErrorInternal:
                default:
                    return CloudServiceResponse.ErrorInternal;
            }
        }
    }
}
