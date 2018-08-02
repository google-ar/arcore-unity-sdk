//-----------------------------------------------------------------------
// <copyright file="RoomSharingClient.cs" company="Google">
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
    using System;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Network Client that resolves rooms to anchor id on other devices.
    /// </summary>
    public class RoomSharingClient : NetworkClient
    {
        /// <summary>
        /// The callback to call after the anchor id is received.
        /// </summary>
        private GetAnchorIdFromRoomDelegate m_GetAnchorIdFromRoomCallback;

        /// <summary>
        /// The room id to resolve.
        /// </summary>
        private int m_RoomId;

        /// <summary>
        /// Get anchor identifier from room delegate.
        /// </summary>
        /// <param name="found">Tells if the Anchor id was found or not.</param>
        /// <param name="anchorId">The anchor id of the room.</param>
        public delegate void GetAnchorIdFromRoomDelegate(bool found, string anchorId);

        /// <summary>
        /// Gets the anchor id of a room.
        /// </summary>
        /// <param name="roomId">Room identifier to resolve.</param>
        /// <param name="ipAddress">The Ip address of the device where the room belongs to.</param>
        /// <param name="GetAnchorIdFromRoomCallback">The callback to be called after the room was resolved.</param>
        public void GetAnchorIdFromRoom(Int32 roomId, string ipAddress, GetAnchorIdFromRoomDelegate GetAnchorIdFromRoomCallback)
        {
            m_GetAnchorIdFromRoomCallback = GetAnchorIdFromRoomCallback;
            m_RoomId = roomId;
            RegisterHandler(MsgType.Connect, OnConnected);
            RegisterHandler(RoomSharingMsgType.AnchorIdFromRoomResponse, OnGetAnchorIdFromRoomResponse);
            RegisterHandler(MsgType.Disconnect, OnDisconnected);
            RegisterHandler(MsgType.Error, OnError);
            Connect(ipAddress, 8888);
        }

        /// <summary>
        /// Handles connected event.
        /// </summary>
        /// <param name="networkMessage">The Connect response.</param>
        private void OnConnected(NetworkMessage networkMessage)
        {
            AnchorIdFromRoomRequestMessage anchorIdRequestMessage = new AnchorIdFromRoomRequestMessage
            {
                RoomId = m_RoomId
            };

            Send(RoomSharingMsgType.AnchorIdFromRoomRequest, anchorIdRequestMessage);
        }

        /// <summary>
        /// Handles when there is an error connecting to the server.
        /// </summary>
        /// <param name="networkMessage">Error message.</param>
        private void OnError(NetworkMessage networkMessage)
        {
            Debug.Log("Error connecting to Room Sharing Server");
            if (m_GetAnchorIdFromRoomCallback != null)
            {
                m_GetAnchorIdFromRoomCallback(false, null);
            }
        }

        /// <summary>
        /// Handles when there is disconnection from the server.
        /// </summary>
        /// <param name="networkMessage">Disconnection message.</param>
        private void OnDisconnected(NetworkMessage networkMessage)
        {
            Debug.Log("Disconnected from Room Sharing Server");
            if (m_GetAnchorIdFromRoomCallback != null)
            {
                m_GetAnchorIdFromRoomCallback(false, null);
            }
        }

        /// <summary>
        /// Handles the resolve room response from server.
        /// </summary>
        /// <param name="networkMessage">The resolve room response message.</param>
        private void OnGetAnchorIdFromRoomResponse(NetworkMessage networkMessage)
        {
            var response = networkMessage.ReadMessage<AnchorIdFromRoomResponseMessage>();
            if (m_GetAnchorIdFromRoomCallback != null)
            {
                m_GetAnchorIdFromRoomCallback(response.Found, response.AnchorId);
            }

            m_GetAnchorIdFromRoomCallback = null;
        }
    }
}