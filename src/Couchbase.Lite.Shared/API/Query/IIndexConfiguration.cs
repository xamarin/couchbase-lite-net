﻿// 
// IIndexConfiguration.cs
// 
// Copyright (c) 2021 Couchbase, Inc All rights reserved.
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

using JetBrains.Annotations;

namespace Couchbase.Lite.Query
{
    public interface IIndexConfiguration
    {
    }

    /// <summary>
    /// An interface for an index based on a simple property value
    /// </summary>
    public interface IValueIndexConfiguration : IIndexConfiguration
    {

    }

    /// <summary>
    /// An interface for an index based on full text searching
    /// </summary>
    public interface IFullTextIndexConfiguration : IIndexConfiguration
    {
        /// <summary>
        /// Sets whether or not to ignore accents when performing 
        /// the full text search
        /// </summary>
        /// <param name="ignoreAccents">Whether or not to ignore accents</param>
        /// <returns>The index for further processing</returns>
        [NotNull]
        IFullTextIndexConfiguration IgnoreAccents(bool ignoreAccents);

        /// <summary>
        /// Sets the locale to use when performing full text searching
        /// </summary>
        /// <param name="language">The language code in the form of ISO-639 language code</param>
        /// <returns>The index for further processing</returns>
        [NotNull]
        IFullTextIndexConfiguration SetLanguage(string language);
    }
}
