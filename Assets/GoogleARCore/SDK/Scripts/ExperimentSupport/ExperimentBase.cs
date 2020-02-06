//-----------------------------------------------------------------------
// <copyright file="ExperimentBase.cs" company="Google">
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
    using GoogleARCore;

    /// <summary>
    /// A base class allowing an experiment to subscribe to internal SDK events by implementing
    /// a subclass that will be automatically found via reflection.
    /// </summary>
    internal abstract class ExperimentBase
    {
        /// <summary>
        /// Called to get a experiment's experimental feature flags.
        /// </summary>
        /// <returns>The experiment's experimental feature flags.</returns>
        public virtual int GetExperimentalFeatureFlags()
        {
            return 0;
        }

        /// <summary>
        /// Called before lifecycle manager call ArPresto_update to update session features
        /// that requre session recreation.
        /// </summary>
        public virtual void OnUpdateSessionFeatures()
        {
        }

        /// <summary>
        /// Called at the beginnging of the frame.
        /// </summary>
        public virtual void OnEarlyUpdate()
        {
        }

        /// <summary>
        /// Called to check if the experiment configuration is dirty.
        /// </summary>
        /// <returns><c>true</c> if the experiment configuration is dirty, otherwise
        /// <c>false</c>.</returns>
        public virtual bool IsConfigurationDirty()
        {
            return false;
        }

        /// <summary>
        /// Called before a native configuration is set.
        /// </summary>
        /// <param name="sessionHandle">The session handle the config is being set on.</param>
        /// <param name="configHandle">The handle to the native configuration.</param>
        public virtual void OnSetConfiguration(IntPtr sessionHandle, IntPtr configHandle)
        {
        }

        /// <summary>
        /// Called to check if an unknown trackable type is managed by an experiment.
        /// </summary>
        /// <param name="trackableType">The unknown trackable type.</param>
        /// <returns><c>true</c> if the subsriber manages the type, otherwise
        /// <c>false</c>.</returns>
        public virtual bool IsManagingTrackableType(int trackableType)
        {
            return false;
        }

        /// <summary>
        /// Called to retrieve TrackableHitFlags associated with the trackable managed by the
        /// experiment.
        /// </summary>
        /// <param name="trackableType">The trackable type.</param>
        /// <returns>TrackableHitFlags value for the associated trackable.</returns>
        public virtual TrackableHitFlags GetTrackableHitFlags(int trackableType)
        {
            return TrackableHitFlags.None;
        }

        /// <summary>
        /// Factory for creating or accessing a trackable managed by the experiment.
        /// </summary>
        /// <param name="trackableType">The trackable type to create.</param>
        /// <param name="trackableHandle">The handle for the trackable to create.</param>
        /// <returns>The managed Trackable associated with trackableHandle.</returns>
        public virtual Trackable TrackableFactory(int trackableType, IntPtr trackableHandle)
        {
            return null;
        }
    }
}
