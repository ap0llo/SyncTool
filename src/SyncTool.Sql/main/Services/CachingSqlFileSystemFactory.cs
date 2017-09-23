using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Services
{
    /// <summary>
    /// Implementation of <see cref="T:SyncTool.Sql.Services.ISqlFileSystemFactory" /> that caches the created 
    /// instances of <see cref="T:SyncTool.Sql.Services.SqlFile" /> and <see cref="T:SyncTool.Sql.Services.SqlDirectory" />
    /// and reuses them for subseqent requests.  
    /// </summary>
    /// <remarks>
    /// Caching is possible because directories and files stored in the database are immutable.
    /// Becuase every dile and directory instance holds a reference to the parent directory instance
    /// both databse id and parent instance are used as keys in the cache
    /// </remarks>
    class CachingSqlFileSystemFactory : ISqlFileSystemFactory
    {
        readonly Func<IDirectory, FileInstanceDo, SqlFile> m_FileFactoryFunction;
        readonly Func<IDirectory, DirectoryInstanceDo, SqlDirectory> m_DirectoryFactoryFunction;
        readonly ConcurrentDictionary<(int, IDirectory), SqlDirectory> m_DirectoryCache = new ConcurrentDictionary<(int, IDirectory), SqlDirectory>();
        readonly ConcurrentDictionary<(int, IDirectory), SqlFile> m_FileCache = new ConcurrentDictionary<(int, IDirectory), SqlFile>();


        //Factory functions are provided automatically by Autofac
        public CachingSqlFileSystemFactory(
            [NotNull] Func<IDirectory, DirectoryInstanceDo, SqlDirectory> directoryFactoryFunction,
            [NotNull] Func<IDirectory, FileInstanceDo, SqlFile> fileFactoryFunction)
        {
            m_FileFactoryFunction = fileFactoryFunction ?? throw new ArgumentNullException(nameof(fileFactoryFunction));
            m_DirectoryFactoryFunction = directoryFactoryFunction ?? throw new ArgumentNullException(nameof(directoryFactoryFunction));
        }


        public SqlDirectory CreateSqlDirectory(IDirectory parent, DirectoryInstanceDo directoryDo) 
            => m_DirectoryCache.GetOrAdd(
                (directoryDo.Id, parent), 
                _ => m_DirectoryFactoryFunction.Invoke(parent, directoryDo)
            );

        public SqlFile CreateSqlFile(IDirectory parent, FileInstanceDo fileInstance) 
            => m_FileCache.GetOrAdd(
                (fileInstance.Id, parent),
                _ => m_FileFactoryFunction.Invoke(parent, fileInstance)
            );
    }
}