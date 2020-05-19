//-----------------------------------------------------------------------
// <copyright file="ILifecycleManager.cs" company="Google LLC">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using GoogleARCore;

    [SuppressMessage("UnityRules.UnityStyleRules", "US1101:NonPublicFieldsMustHavePrefixM",
     Justification = "This is an interface so fields are already public.")]
    internal interface ILifecycleManager
    {
        event Action UpdateSessionFeatures;

        event Action EarlyUpdate;

        event Action<bool> OnSessionSetEnabled;

        event Action<IntPtr, IntPtr> OnSetConfiguration;

        event Action OnResetInstance;

        SessionStatus SessionStatus { get; }

        LostTrackingReason LostTrackingReason { get; }

        ARCoreSession SessionComponent { get; }

        NativeSession NativeSession { get; }

        bool IsSessionChangedThisFrame { get; }

        AsyncTask<ApkAvailabilityStatus> CheckApkAvailability();

        AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested);

        void CreateSession(ARCoreSession session);

        void EnableSession();

        void DisableSession();

        void ResetSession();
    }
}
