//-----------------------------------------------------------------------
// <copyright file="InvalidSessionAccessException.cs" company="Google">
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

namespace GoogleARCore
{
    using System;

    /// <summary>
    /// An exception indicating that the ARCore session resources have been accessed while in an invalid state.  This
    /// will happen when the session is not connected but the developer has accessed the Session or Frame.
    /// </summary>
    public class InvalidSessionAccessException : Exception
    {
        public InvalidSessionAccessException(string message) : base(message) {}

        public InvalidSessionAccessException(string message, Exception inner) : base(message, inner) {}
    }
}