//-----------------------------------------------------------------------
// <copyright file="ThrottledLogMessage.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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
    /// Helper class that rate limits messages logged to at most one per specified interval.
    /// </summary>
    internal class ThrottledLogMessage
    {
        private float _lastMessageTime;
        private float _minLogIntervalSeconds;

        public ThrottledLogMessage(float minLogIntervalSeconds)
        {
            _minLogIntervalSeconds = minLogIntervalSeconds;
            _lastMessageTime = -minLogIntervalSeconds - 1f;
        }

        /// <summary>
        /// If the minimum logging interval has passed, log a formatted warning message to the Unity console.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        public void ThrottledLogWarningFormat(string format, params object[] args)
        {
            if (ShouldLog())
            {
                Debug.LogWarningFormat(format, args);
            }
        }

        private bool ShouldLog()
        {
            float now = Time.realtimeSinceStartup;
            if (now - _lastMessageTime > _minLogIntervalSeconds)
            {
                _lastMessageTime = now;
                return true;
            }

            return false;
        }
    }
}
