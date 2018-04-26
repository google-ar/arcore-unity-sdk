// <copyright file="AnchorIdFromRoomRequestMessage.cs" company="Google">
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
    using UnityEngine.Networking;

    /// <summary>
    /// Anchor identifier from room request message.
    /// </summary>
    public class AnchorIdFromRoomRequestMessage : MessageBase
    {
        /// <summary>
        /// The room identifier to get the Anchor id from.
        /// </summary>
        public Int32 RoomId;

        /// <summary>
        /// Serialize the message.
        /// </summary>
        /// <param name="writer">Writer to write the message to.</param>
        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(RoomId);
        }

        /// <summary>
        /// Deserialize the message.
        /// </summary>
        /// <param name="reader">Reader to read the message from.</param>
        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            RoomId = reader.ReadInt32();
        }
    }
}