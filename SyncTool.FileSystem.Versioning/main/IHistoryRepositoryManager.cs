// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryRepositoryManager : IDisposable
    {

        /// <summary>
        /// Gets the specified history repository
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="HistoryRepositoryNotFoundException"></exception>
        IHistoryRepository GetHistoryRepository(string name);

    }
}