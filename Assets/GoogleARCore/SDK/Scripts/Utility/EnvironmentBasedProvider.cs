//-----------------------------------------------------------------------
// <copyright file="EnvironmentBasedProvider.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
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
    /// <summary>
    /// Generic provider for runtime-dependant instances of a interface.
    /// </summary>
    /// <typeparam name="T">The type to be provided.</typeparam>
    public abstract class EnvironmentBasedProvider<T>
    {
        /// <summary>
        /// Provides an instance.
        /// </summary>
        /// <returns>An instance of type T.</returns>
        public abstract T ProvideInstance();
    }
}
