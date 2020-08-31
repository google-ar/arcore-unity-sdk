//-----------------------------------------------------------------------
// <copyright file="ExperimentManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Reflection;
    using GoogleARCore;

    internal class ExperimentManager
    {
        private static ExperimentManager _instance;
        private List<ExperimentBase> _experiments;

        public ExperimentManager()
        {
            // Experiments all derive from ExperimentBase to get hooks to the internal
            // state. Find and hook them up.
            _experiments = new List<ExperimentBase>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> allTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes();
                    allTypes.AddRange(assemblyTypes);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    UnityEngine.Debug.Log(
                        "Unable to load types from assembly:: " + assembly.ToString() + ":: " +
                        ex.Message);
                }
            }

            foreach (var type in allTypes)
            {
                if (!type.IsClass ||
                    type.IsAbstract ||
                    !typeof(ExperimentBase).IsAssignableFrom(type))
                {
                    continue;
                }

                _experiments.Add(Activator.CreateInstance(type) as ExperimentBase);
            }
        }

        public static ExperimentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExperimentManager();
                }

                return _instance;
            }
        }

        public bool IsSessionExperimental { get; private set; }

        public bool IsConfigurationDirty
        {
            get
            {
                bool result = false;

                foreach (var experiment in _experiments)
                {
                    result = result || experiment.IsConfigurationDirty();
                }

                return result;
            }
        }

        public void Initialize()
        {
            LifecycleManager.Instance.EarlyUpdate += _instance.OnEarlyUpdate;
            LifecycleManager.Instance.UpdateSessionFeatures +=
                _instance.OnUpdateSessionFeatures;
            LifecycleManager.Instance.OnSetConfiguration +=
                        _instance.SetConfiguration;
        }

        public bool IsManagingTrackableType(int trackableType)
        {
            return GetTrackableTypeManager(trackableType) != null;
        }

        public TrackableHitFlags GetTrackableHitFlags(int trackableType)
        {
            ExperimentBase trackableManager = GetTrackableTypeManager(trackableType);
            if (trackableManager != null)
            {
                return trackableManager.GetTrackableHitFlags(trackableType);
            }

            return TrackableHitFlags.None;
        }

        public Trackable TrackableFactory(int trackableType, IntPtr trackableHandle)
        {
            ExperimentBase trackableManager = GetTrackableTypeManager(trackableType);
            if (trackableManager != null)
            {
                return trackableManager.TrackableFactory(trackableType, trackableHandle);
            }

            return null;
        }

        public void OnUpdateSessionFeatures()
        {
            foreach (var experiment in _experiments)
            {
                experiment.OnUpdateSessionFeatures();
            }
        }

        private void OnEarlyUpdate()
        {
            foreach (var experiment in _experiments)
            {
                experiment.OnEarlyUpdate();
            }
        }

        private void SetConfiguration(IntPtr sessionHandle, IntPtr configHandle)
        {
            foreach (var experiment in _experiments)
            {
                experiment.OnSetConfiguration(sessionHandle, configHandle);
            }
        }

        private ExperimentBase GetTrackableTypeManager(int trackableType)
        {
            foreach (var experiment in _experiments)
            {
                if (experiment.IsManagingTrackableType(trackableType))
                {
                    return experiment;
                }
            }

            return null;
        }
    }
}
