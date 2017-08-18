//-----------------------------------------------------------------------
// <copyright file="OrientationManager.cs" company="Google">
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
    using UnityEngine;

    /// <summary>
    /// Provides a singleton Monobehaviour of type <c>T</c>.
    /// </summary>
    /// <typeparam name="T">The type of the component to be provided as a singleton.</typeparam>
    public class SingletonComponentProvider<T> where T : MonoBehaviour
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private T m_instance;

        /// <summary>
        /// Provides an instance of the singleton.  An instance will be created if none exists in the current scene.
        /// </summary>
        /// <param name="gameObjectName">The name of the created gameobject if the component needs to be
        /// generated. </param>
        public T Provide(string gameObjectName = "ARCore Singleton")
        {
            if (m_instance == null && (m_instance = GameObject.FindObjectOfType<T>()) != null)
            {
                m_instance = GameObject.FindObjectOfType<T>();
            }
            else if (m_instance == null)
            {
                GameObject go = new GameObject();
                m_instance = go.AddComponent<T>();
            }

            return m_instance;
        }
    }
}
