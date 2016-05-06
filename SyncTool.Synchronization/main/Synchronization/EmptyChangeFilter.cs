// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    /// <summary>
    /// A <see cref="IChangeFilter"/> implementation, that dows not do any filtering 
    /// </summary>
    public sealed class EmptyChangeFilter: IChangeFilter
    {
        public override bool Equals(object obj) => Equals(obj as IChangeFilter);

        public override int GetHashCode() => 23;

        public bool Equals(IChangeFilter other) => other is EmptyChangeFilter;

        public bool IncludeInResult(IChange change) => true;
    }
}