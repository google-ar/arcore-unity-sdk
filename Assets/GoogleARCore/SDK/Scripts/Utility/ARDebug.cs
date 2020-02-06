//-----------------------------------------------------------------------
// <copyright file="ARDebug.cs" company="Google">
//
// Copyright 2016 Google LLC. All Rights Reserved.
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
    using System;
    using System.Diagnostics;
    using UnityEngine;

    /// <summary>
    /// A custom class similar to Unity's Debug.
    /// </summary>
    public class ARDebug
    {
        /// <summary>
        /// Logs an error with a stack trace.
        /// </summary>
        /// <param name="message">The error message.</param>
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogErrorFormat(message + "\n{0}", new StackTrace(1));
        }

        /// <summary>
        /// Logs an error with a stack trace.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="args">The output arguments.</param>
        public static void LogErrorFormat(string format, params object[] args)
        {
            object[] newArgs = new object[args.Length + 1];
            Array.Copy(args, newArgs, args.Length);
            newArgs[args.Length] = new StackTrace(1);
            UnityEngine.Debug.LogErrorFormat(format + "\n{" + args.Length + "}", newArgs);
        }
    }
}
