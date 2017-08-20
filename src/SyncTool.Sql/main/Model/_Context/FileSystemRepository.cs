using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static SyncTool.Sql.Model.TypeMapper;

namespace SyncTool.Sql.Model
{
    class FileSystemRepository
    {
        const string s_ParentId = "ParentId"; 
        const string s_ChildId = "ChildId";
        const string s_ContainsDirectory = "ContainsDirectory";
        const string s_ContainsFile = "ContainsFile";
        const string s_TmpId = "tmpId";
        const string s_FileId = "FileId";
        const string s_DirectoryId = "DirectoryId";

        readonly IDatabaseContextFactory m_ConnectionFactory;


        public IEnumerable<FileDo> Files
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<FileDo>($"SELECT * FROM {Table<FileDo>()}");
                } 
            }
        }

        public IEnumerable<DirectoryDo> Directories
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<DirectoryDo>($"SELECT * FROM {Table<DirectoryDo>()}");
                }
            }
        }

        public IEnumerable<FileInstanceDo> FileInstancess
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<FileInstanceDo>($"SELECT * FROM {Table<FileInstanceDo>()}");
                }
            }
        }

        public IEnumerable<DirectoryInstanceDo> DirectorieInstances
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<DirectoryInstanceDo>($"SELECT * FROM {Table<DirectoryInstanceDo>()}");
                }
            }
        }


        public FileSystemRepository(IDatabaseContextFactory connectionFactory)
        {
            m_ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            //TODO: Should happen on first access??
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                connection.ExecuteNonQuery($@"                

                    CREATE TABLE IF NOT EXISTS {Table<FileDo>()} (                
                        {nameof(FileDo.Id)} INTEGER PRIMARY KEY,
                        {nameof(FileDo.Name)} TEXT NOT NULL,
                        {nameof(FileDo.NormalizedPath)} TEXT UNIQUE NOT NULL);

                    CREATE TABLE IF NOT EXISTS {Table<DirectoryDo>()} (                
                        {nameof(DirectoryDo.Id)} INTEGER PRIMARY KEY,
                        {nameof(DirectoryDo.Name)} TEXT NOT NULL,
                        {nameof(DirectoryDo.NormalizedPath)} TEXT UNIQUE NOT NULL) ;
                
                    CREATE TABLE IF NOT EXISTS {Table <FileInstanceDo>()} (                
                        {nameof(FileInstanceDo.Id)} INTEGER PRIMARY KEY,
                        {s_FileId} INTEGER NOT NULL,
                        {nameof(FileInstanceDo.LastWriteTimeTicks)} INTEGER NOT NULL,
                        {nameof(FileInstanceDo.Length)} INTEGER NOT NULL,
                        FOREIGN KEY ({s_FileId}) REFERENCES {Table<FileDo>()}({nameof(FileDo.Id)}),
                        CONSTRAINT FileInstance_Unique UNIQUE (
                            {s_FileId}, 
                            {nameof(FileInstanceDo.LastWriteTimeTicks)}, 
                            {nameof(FileInstanceDo.Length)}) ); 

                    CREATE TABLE IF NOT EXISTS {Table<DirectoryInstanceDo>()} (
                        {nameof(DirectoryInstanceDo.Id)} INTEGER PRIMARY KEY,
                        {s_DirectoryId} INTEGER NOT NULL,    
                        {s_TmpId} TEXT UNIQUE,
                        FOREIGN KEY ({s_DirectoryId}) REFERENCES {Table<DirectoryDo>()}({nameof(DirectoryDo.Id)}));

                    CREATE TABLE IF NOT EXISTS {s_ContainsDirectory} (
                        {s_ParentId} INTEGER NOT NULL,
                        {s_ChildId} INTEGER NOT NULL,
                        FOREIGN KEY ({s_ParentId}) REFERENCES {Table<DirectoryInstanceDo>()}({nameof(DirectoryInstanceDo.Id)}),
                        FOREIGN KEY ({s_ChildId}) REFERENCES {Table<DirectoryInstanceDo>()}({nameof(DirectoryInstanceDo.Id)}),
                        CONSTRAINT {s_ContainsDirectory}_Unique UNIQUE({s_ParentId},{s_ChildId}) );
                    
                    CREATE TABLE IF NOT EXISTS {s_ContainsFile} (
                        {s_ParentId} INTEGER NOT NULL,
                        {s_ChildId} INTEGER NOT NULL,
                        FOREIGN KEY ({s_ParentId}) REFERENCES {Table<DirectoryInstanceDo>()}({nameof(DirectoryInstanceDo.Id)}),
                        FOREIGN KEY ({s_ChildId}) REFERENCES {Table<FileInstanceDo>()}({nameof(FileInstanceDo.Id)}),
                        CONSTRAINT {s_ContainsDirectory}_Unique UNIQUE({s_ParentId},{s_ChildId}) );
                ");
            }
        }


        public void AddFile(FileDo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (file.Id != 0)
                throw new ArgumentException("Cannot add file with id != 0", nameof(file));
            
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                Insert(connection, file);
            }
        }

        public void AddDirectory(DirectoryDo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            if (directory.Id != 0)
                throw new ArgumentException("Cannot add directory with id != 0", nameof(directory));

            if (String.IsNullOrWhiteSpace(directory.NormalizedPath))
                throw new ArgumentException($"{nameof(directory.NormalizedPath)} must not be empty");

            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                Insert(connection, directory);
            }
        }

        public void AddFileInstance(FileInstanceDo fileInstance)
        {
            if (fileInstance == null)
                throw new ArgumentNullException(nameof(fileInstance));

            if (fileInstance.Id != 0)
                throw new ArgumentException("Cannot add file instance with id != 0", nameof(fileInstance));

            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                Insert(connection, fileInstance);
            }
        }
        
        public void AddRecursively(DirectoryInstanceDo rootDirectory)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {                
                Insert(connection, rootDirectory);               
                transaction.Commit();
            }
        }

        public DirectoryInstanceDo GetDirectoryInstance(int id)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingle<DirectoryInstanceDo>($@"
                    SELECT * 
                    FROM {Table<DirectoryInstanceDo>()}
                    WHERE {nameof(DirectoryInstanceDo.Id)} = @id;
                ", 
                new { id = id });
            }
        }

        public void LoadDirectories(DirectoryInstanceDo parentDirectoryInstance)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                var directories = connection.Query<DirectoryInstanceDo>($@"    
                    SELECT * 
                    FROM {Table<DirectoryInstanceDo>()}
                    WHERE {nameof(DirectoryInstanceDo.Id)} IN (
                        SELECT {s_ChildId}
                        FROM {s_ContainsDirectory}
                        WHERE {s_ParentId} = @id        
                    );
                ",
                new { id = parentDirectoryInstance.Id });

                parentDirectoryInstance.Directories = directories.ToList();

            }
        }

        public void LoadDirectory(DirectoryInstanceDo directoryInstance)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                var dir = connection.QuerySingle<DirectoryDo>($@"
                    SELECT * 
                    FROM {Table<DirectoryDo>()}
                    WHERE {nameof(DirectoryDo.Id)} IN (
                        SELECT {s_DirectoryId}
                        FROM {Table<DirectoryInstanceDo>()}
                        WHERE {nameof(DirectoryInstanceDo.Id)} = @id
                    )
                ", 
                new {id = directoryInstance.Id});

                directoryInstance.Directory = dir;
            }
        }

        public void LoadFiles(DirectoryInstanceDo parentDirectoryInstance)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                var files = connection.Query<FileInstanceDo>($@"    
                    SELECT * 
                    FROM {Table<FileInstanceDo>()}
                    WHERE {nameof(FileInstanceDo.Id)} IN (
                        SELECT {s_ChildId}
                        FROM {s_ContainsFile}
                        WHERE {s_ParentId} = @id        
                    );
                ",
                    new { id = parentDirectoryInstance.Id });

                parentDirectoryInstance.Files = files.ToList();
            }
        }

        public void LoadFile(FileInstanceDo fileInstance)
        {        
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                var file = connection.QuerySingle<FileDo>($@"
                    SELECT * 
                    FROM {Table<FileDo>()}
                    WHERE {nameof(FileDo.Id)} IN (
                        SELECT {s_FileId} 
                        FROM {Table<FileInstanceDo>()}
                        WHERE {nameof(FileInstanceDo.Id)} = @id
                    )
                ",  
                new { id = fileInstance.Id });

                fileInstance.File = file;                    
            }                
        }


        void Insert(IDbConnection connection, DirectoryDo directory)
        {            
            if (directory.Id != 0)
                return;

            directory.Id = connection.ExecuteScalar<int>($@"

                    INSERT OR IGNORE INTO {Table<DirectoryDo>()} (
                        {nameof(DirectoryDo.Name)},
                        {nameof(DirectoryDo.NormalizedPath)} )
                    VALUES (@name, @path);

                    SELECT {nameof(DirectoryDo.Id)} FROM {Table<DirectoryDo>()}
                    WHERE {nameof(DirectoryDo.NormalizedPath)} = @path;
                ",
                ("name", directory.Name),
                ("path", directory.NormalizedPath)
            );
        }

        void Insert(IDbConnection connection, FileDo file)
        {
            if (String.IsNullOrWhiteSpace(file.NormalizedPath))
                throw new ArgumentException($"{nameof(FileDo.NormalizedPath)} must not be empty");

            if (file.Id != 0)
                return;

            file.Id = connection.ExecuteScalar<int>($@"
                    
                    INSERT OR IGNORE INTO {Table<FileDo>()} (
                        {nameof(FileDo.Name)},
                        {nameof(FileDo.NormalizedPath)} )
                    VALUES (@name, @path) ;
                    
                    SELECT {nameof(FileDo.Id)} FROM {Table<FileDo>()}
                    WHERE {nameof(FileDo.NormalizedPath)} = @path;",

                    ("name", file.Name),
                    ("path", file.NormalizedPath)
                );
        }

        void Insert(IDbConnection connection, DirectoryInstanceDo directoryInstance)
        {
            if (directoryInstance.Id != 0)
                return;

            foreach (var childDir in directoryInstance.Directories)
            {
                Insert(connection, childDir);
            }

            foreach(var file in directoryInstance.Files)
            {
                Insert(connection, file);
            }

            Insert(connection, directoryInstance.Directory);

            var tmpId = Guid.NewGuid().ToString();
            directoryInstance.Id = connection.ExecuteScalar<int>($@"
                    INSERT INTO {Table<DirectoryInstanceDo>()} ({s_DirectoryId}, {s_TmpId})
                    VALUES (@directoryId, @tmpId);

                    SELECT {nameof(DirectoryInstanceDo.Id)} 
                    FROM {Table<DirectoryInstanceDo>()}
                    WHERE {s_TmpId} = @tmpId",

                    ("directoryId", directoryInstance.Directory.Id),
                    ("tmpId", tmpId)
                );


            foreach(var file in directoryInstance.Files)
            {
                connection.ExecuteNonQuery($@"
                    INSERT INTO {s_ContainsFile} ({s_ParentId}, {s_ChildId}) VALUES (@parentId, @childId);",
                    ("parentId", directoryInstance.Id),
                    ("childId", file.Id)
                );
            }

            foreach (var childDir in directoryInstance.Directories)
            {
                connection.ExecuteNonQuery($@"
                    INSERT INTO {s_ContainsDirectory} ({s_ParentId}, {s_ChildId}) VALUES (@parentId, @childId);",
                    ("parentId", directoryInstance.Id),
                    ("childId", childDir.Id)
                );
            }

            connection.ExecuteNonQuery(
                $"UPDATE {Table<DirectoryInstanceDo>()} SET {s_TmpId} = NULL WHERE {s_TmpId} = @tmpId", 
                ("tmpId", tmpId)
            );

        }

        void Insert(IDbConnection connection, FileInstanceDo fileInstance)
        {        
            if (fileInstance.Id != 0)
                return;

            Insert(connection, fileInstance.File);

            fileInstance.Id = connection.ExecuteScalar<int>($@"
                    INSERT OR IGNORE INTO {Table<FileInstanceDo>()} (
                        {nameof(FileInstanceDo.File)}Id,
                        {nameof(FileInstanceDo.LastWriteTimeTicks)},
                        {nameof(FileInstanceDo.Length)} )
                    VALUES (@fileId, @ticks, @length );

                    SELECT * FROM {Table<FileInstanceDo>()}
                    WHERE {nameof(FileInstanceDo.File)}Id = @fileId AND
                            {nameof(FileInstanceDo.LastWriteTimeTicks)} = @ticks AND 
                            {nameof(FileInstanceDo.Length)} = @length ;
                ",
                ("fileId", fileInstance.File.Id),
                ("ticks", fileInstance.LastWriteTimeTicks),
                ("length", fileInstance.Length)
            );
        }
    }
}
