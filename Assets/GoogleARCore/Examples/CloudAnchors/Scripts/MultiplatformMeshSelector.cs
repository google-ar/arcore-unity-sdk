//-----------------------------------------------------------------------
// <copyright file="MultiplatformMeshSelector.cs" company="Google">
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

namespace GoogleARCore.Examples.CloudAnchors
{
    using UnityEngine;

    /// <summary>
    /// Selects the mesh corresponding to the runtime platform by its name. To use Unity's Networking API, the same
    /// prefab will be spawn across all clients. To allow different meshes for different platforms (which might be
    /// useful for light estimation for example), this script selects the corresponding mesh for the runtime platform.
    /// </summary>
    public class MultiplatformMeshSelector : MonoBehaviour
    {
        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                transform.Find("ARCoreMesh").gameObject.SetActive(true);
                transform.Find("ARKitMesh").gameObject.SetActive(false);
            }
            else
            {
                transform.Find("ARCoreMesh").gameObject.SetActive(false);
                transform.Find("ARKitMesh").gameObject.SetActive(true);
            }
        }
    }
}
