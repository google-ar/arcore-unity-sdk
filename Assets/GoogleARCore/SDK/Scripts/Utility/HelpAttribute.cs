//-----------------------------------------------------------------------
// <copyright file="HelpAttribute.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using UnityEngine;

    /// <summary>
    /// Help attribute that displays the help message as a HelpBox below the property.
    /// When uses HelpAttribute and other inspector attributes, make sure that the HelpAttribute
    /// has the lowest order, otherwise it may not be drawn in the inspector.
    /// NOTE:
    /// When uses HelpAttribute and TextAreaAttribute together, the text area will not have
    /// a scrollbar.
    /// HelpAttribute is incompatible with a custom type.
    /// </summary>
    public class HelpAttribute : PropertyAttribute
    {
        /// <summary>
        /// The help message that displays in a help box.
        /// </summary>
        public readonly string HelpMessage = null;

        /// <summary>
        /// The type of the help message which controls the icon in help box.
        /// </summary>
        public readonly HelpMessageType MessageType = HelpMessageType.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GoogleARCoreInternal.HelpAttribute"/>
        /// class.
        /// </summary>
        /// <param name="helpMessage">Help message.</param>
        /// <param name="messageType">Message type.</param>
        public HelpAttribute(string helpMessage,
            HelpMessageType messageType = HelpMessageType.None)
        {
            HelpMessage = helpMessage;
            MessageType = messageType;
        }

        /// <summary>
        /// User message types.
        /// </summary>
        public enum HelpMessageType
        {
            /// <summary>
            /// Neutral message.
            /// </summary>
            None,

            /// <summary>
            /// Info message.
            /// </summary>
            Info,

            /// <summary>
            /// Warning message.
            /// </summary>
            Warning,

            /// <summary>
            /// Error message.
            /// </summary>
            Error,
        }
    }
}
