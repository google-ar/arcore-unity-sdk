//-----------------------------------------------------------------------
// <copyright file="AndroidPermissionsRequestResult.cs" company="Google">
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

namespace GoogleARCore
{
    using GoogleARCoreInternal;
    using UnityEngine;

    /// <summary>
    /// Structure holding data summarizing the result of an Android permissions request.
    /// </summary>
    public struct AndroidPermissionsRequestResult
    {
        /// <summary>
        /// Constructs a new AndroidPermissionsRequestResult.
        /// </summary>
        /// <param name="permissionNames">The value for PermissionNames.</param>
        /// <param name="grantResults">The value for GrantResults.</param>
        public AndroidPermissionsRequestResult(string[] permissionNames,  bool[] grantResults) : this()
        {
            PermissionNames = permissionNames;
            GrantResults = grantResults;
        }

        /// <summary>
        /// Gets a collection of permissions requested.
        /// </summary>
        public string[] PermissionNames { get; private set; }

        /// <summary>
        /// Gets a collection of results corresponding to {@link PermissionNames}.
        /// </summary>
        public bool[] GrantResults { get; private set; }

        /// <summary>
        /// Gets a value indicating whether all permissions are granted.
        /// </summary>
        public bool IsAllGranted
        {
            [SuppressMemoryAllocationError(IsWarning = true, Reason = "Requires further investigation.")]
            get
            {
                if (PermissionNames == null || GrantResults == null)
                {
                    return false;
                }

                for (int i = 0; i < GrantResults.Length; i++)
                {
                    if (!GrantResults[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
