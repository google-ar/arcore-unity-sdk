//-----------------------------------------------------------------------
// <copyright file="SuppressMemoryAllocationErrorAttribute.cs" company="Google LLC">
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
    using System;
    using UnityEngine;

    /// <summary>
    /// A custom Attribute class to annotate functions that are allowed to allocate memory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SuppressMemoryAllocationErrorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="GoogleARCoreInternal.SuppressMemoryAllocationErrorAttribute"/> class.
        /// </summary>
        public SuppressMemoryAllocationErrorAttribute()
        {
            this.IsWarning = false;
            this.Reason = "Unknown";
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show a warning instead of an error.
        /// </summary>
        public bool IsWarning { get; set; }

        /// <summary>
        /// Gets or sets the reason for suppressing the memory allocation error.
        /// </summary>
        public string Reason { get; set; }
    }
}
