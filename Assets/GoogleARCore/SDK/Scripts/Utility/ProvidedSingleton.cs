//-----------------------------------------------------------------------
// <copyright file="ProvidedSingleton.cs" company="Google">
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
    /// <summary>
    /// Wrapper for a singleton instance that is allocated using a supplied provider.
    /// </summary>
    /// <typeparam name="T1">The singleton instance type.</typeparam>
    /// <typeparam name="T2">An EnvironmentBasedProvider for T1.</typeparam>
    public partial class ProvidedSingleton<T1, T2>
        where T2 : EnvironmentBasedProvider<T1>, new()
    {
        /// <summary>
        /// The instance.
        /// </summary>
        private static T1 m_instance;

        /// <summary>
        /// Gets of sets the singleton instance, allocating it using the supplied provider if needed.
        /// </summary>
        public static T1 Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = (new T2()).ProvideInstance();
                }

                return m_instance;
            }

            set
            {
                m_instance = value;
            }
        }
    }
}
