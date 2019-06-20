//-----------------------------------------------------------------------
// <copyright file="CloudAnchorsNetworkManager.cs" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
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
    using System;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// A NetworkManager that handles client connection and disconnection with customizable actions.
    /// </summary>
#pragma warning disable 618
    public class CloudAnchorsNetworkManager : NetworkManager
#pragma warning restore 618
    {
        /// <summary>
        /// Action which get called when the client connects to a server.
        /// </summary>
        public event Action OnClientConnected;

        /// <summary>
        /// Action which get called when the client disconnects from a server.
        /// </summary>
        public event Action OnClientDisconnected;

        /// <summary>
        /// Called on the client when connected to a server.
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
#pragma warning disable 618
        public override void OnClientConnect(NetworkConnection conn)
#pragma warning restore 618
        {
            base.OnClientConnect(conn);
            Debug.Log("Successfully connected to server: " + conn.lastError);
            if (OnClientConnected != null)
            {
                OnClientConnected();
            }
        }

        /// <summary>
        /// Called on the client when disconnected from a server.
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
#pragma warning disable 618
        public override void OnClientDisconnect(NetworkConnection conn)
#pragma warning restore 618
        {
            base.OnClientDisconnect(conn);
            Debug.Log("Disconnected from the server: " + conn.lastError);
            if (OnClientDisconnected != null)
            {
                OnClientDisconnected();
            }
        }
    }
}
