//-----------------------------------------------------------------------
// <copyright file="RoomSharingServer.cs" company="Google">
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

namespace GoogleARCore.Examples.CloudAnchor
{
    using System;
    using System.Collections.Generic;
    using GoogleARCore.CrossPlatform;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Server to share rooms to other devices.
    /// </summary>
    public class RoomSharingServer : MonoBehaviour
    {
        /// <summary>
        /// The dictionary that maps local rooms to hosted Anchors.
        /// </summary>
        private Dictionary<int, XPAnchor> m_RoomAnchorsDict = new Dictionary<int, XPAnchor>();

        /// <summary>
        /// Initialize the server.
        /// </summary>
        public void Start()
        {
            NetworkServer.Listen(8888);
            NetworkServer.RegisterHandler(RoomSharingMsgType.AnchorIdFromRoomRequest, OnGetAnchorIdFromRoomRequest);
        }

        /// <summary>
        /// Saves the cloud anchor to room.
        /// </summary>
        /// <param name="room">The room to save the anchor to.</param>
        /// <param name="anchor">The Anchor to save.</param>
        public void SaveCloudAnchorToRoom(int room, XPAnchor anchor)
        {
            m_RoomAnchorsDict.Add(room, anchor);
        }

        /// <summary>
        /// Resolves a room request.
        /// </summary>
        /// <param name="netMsg">The resolve room request.</param>
        private void OnGetAnchorIdFromRoomRequest(NetworkMessage netMsg)
        {
            var roomMessage = netMsg.ReadMessage<AnchorIdFromRoomRequestMessage>();
            XPAnchor anchor;
            bool found = m_RoomAnchorsDict.TryGetValue(roomMessage.RoomId, out anchor);
            AnchorIdFromRoomResponseMessage response = new AnchorIdFromRoomResponseMessage
            {
                Found = found,
            };

            if (found)
            {
                response.AnchorId = anchor.CloudId;
            }

            NetworkServer.SendToClient(netMsg.conn.connectionId, RoomSharingMsgType.AnchorIdFromRoomResponse, response);
        }
    }
}