//-----------------------------------------------------------------------
// <copyright file="ExperimentManager.cs" company="Google">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using GoogleARCore;

    internal class ExperimentManager
    {
        private static ExperimentManager s_Instance;
        private List<ExperimentBase> m_Experiments;

        public ExperimentManager()
        {
            // Experiments all derive from ExperimentBase to get hooks to the internal
            // state. Find and hook them up.
            m_Experiments = new List<ExperimentBase>();

            var implementingTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ExperimentBase).IsAssignableFrom(p));

            foreach (var type in implementingTypes)
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                m_Experiments.Add(Activator.CreateInstance(type) as ExperimentBase);
            }
        }

        private delegate void OnBeforeSetConfigurationCallback(IntPtr sessionHandhle, IntPtr configHandle);

        public static ExperimentManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ExperimentManager();
                    LifecycleManager.Instance.EarlyUpdateEvent += s_Instance.OnEarlyUpdate;
                }

                return s_Instance;
            }
        }

        public bool IsSessionExperimental { get; private set; }

        public bool IsConfigurationDirty
        {
            get
            {
                bool result = false;

                foreach (var experiment in m_Experiments)
                {
                    result = result || experiment.IsConfigurationDirty();
                }

                return result;
            }
        }

        public void OnEarlyUpdate()
        {
            foreach (var experiment in m_Experiments)
            {
                experiment.OnEarlyUpdate();
            }
        }

        public void OnBeforeSetConfiguration(IntPtr sessionHandle, IntPtr configHandle)
        {
            foreach (var experiment in m_Experiments)
            {
                experiment.OnBeforeSetConfiguration(sessionHandle, configHandle);
            }
        }

        public bool IsManagingTrackableType(int trackableType)
        {
            return _GetTrackableTypeManager(trackableType) != null;
        }

        public Trackable TrackableFactory(int trackableType, IntPtr trackableHandle)
        {
            ExperimentBase trackableManager = _GetTrackableTypeManager(trackableType);
            if (trackableManager != null)
            {
                return trackableManager.TrackableFactory(trackableType, trackableHandle);
            }

            throw new NotImplementedException(
                    "ExperimentManager.TrackableFactory::No constructor for requested trackable type.");
        }

        private ExperimentBase _GetTrackableTypeManager(int trackableType)
        {
            foreach (var experiment in m_Experiments)
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
