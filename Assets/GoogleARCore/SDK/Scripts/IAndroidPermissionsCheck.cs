//-----------------------------------------------------------------------
// <copyright file="IAndroidPermissionsCheck.cs" company="Google LLC">
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

namespace GoogleARCore
{
    using System;
    using GoogleARCore;

    /// <summary>
    /// Interface for checking Android permission. The interface is used for MOCK unity test.
    /// </summary>
    public interface IAndroidPermissionsCheck
    {
         /// <summary>
        /// Requests an Android permission from the user.
        /// </summary>
        /// <param name="permissionName">The permission to be requested (e.g.
        /// android.permission.CAMERA).</param>
        /// <returns>An asynchronous task that completes when the user has accepted or rejected the
        /// requested permission and yields a <see cref="AndroidPermissionsRequestResult"/> that
        /// summarizes the result. If this method is called when another permissions request is
        /// pending, <c>null</c> will be returned instead.</returns>
        AsyncTask<AndroidPermissionsRequestResult> RequestAndroidPermission(
            string permissionName);
    }
}
