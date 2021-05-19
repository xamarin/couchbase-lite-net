﻿// 
//  DatabaseConfiguration.cs
//
//  Copyright (c) 2021 Couchbase, Inc All rights reserved.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

using Couchbase.Lite.DI;

using JetBrains.Annotations;

namespace Couchbase.Lite
{
    /// <summary>
    /// A struct containing configuration for creating or opening database data
    /// </summary>
    public readonly struct DatabaseConfiguration
    {
        #region Constants

        private const string Tag = nameof(DatabaseConfiguration);

        #endregion

        #region Variables

        private readonly string _directory;
      
        #endregion

        #region Properties

        /// <summary>
        /// Gets the directory to use when creating or opening the data. 
        /// </summary>
        [CanBeNull]
        public readonly string Directory { get { return _directory ?? Service.GetRequiredInstance<IDefaultDirectoryResolver>().DefaultDirectory(); } }

#if COUCHBASE_ENTERPRISE
        /// <summary>
        /// Gets the encryption key to use on the database
        /// </summary>
        [CanBeNull]
        public readonly EncryptionKey EncryptionKey { get; }
#endif

        /// <summary>
        /// Experiment API. Enable version vector.
        /// </summary>
        /// <remarks>
        /// If the enableVersionVector is set to true, the database will use version vector instead of
        /// using revision tree.When enabling version vector on an existing database, the database
        /// will be upgraded to use the revision tree while the database is opened.
        /// NOTE:
        /// 1. The database that uses version vector cannot be downgraded back to use revision tree.
        /// 2. The current version of Sync Gateway doesn't support version vector so the syncronization
        /// with Sync Gateway will not be working.
        /// </remarks>
        internal bool EnableVersionVector => false;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="directory">
        /// Default directory is <see cref="Service.GetRequiredInstance<IDefaultDirectoryResolver>().DefaultDirectory()" /> if directory set to null.
        /// </param>
        public DatabaseConfiguration(
            string directory = null
#if COUCHBASE_ENTERPRISE
            , EncryptionKey encryptionKey = null
#endif
            )
        {
            _directory = directory ?? Service.GetRequiredInstance<IDefaultDirectoryResolver>().DefaultDirectory();
#if COUCHBASE_ENTERPRISE
            EncryptionKey = encryptionKey;
#endif
        }

        #endregion
    }
}


