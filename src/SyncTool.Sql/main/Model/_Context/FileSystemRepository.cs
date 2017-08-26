using Dapper;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SyncTool.Sql.Model
{
    class FileSystemRepository
    {
        readonly IDatabase m_Database;

        public IEnumerable<FileDo> Files
        {
            get
            {
                using (var connection = m_Database.OpenConnection())
                {
                    return connection.Query<FileDo>($"SELECT * FROM {FilesTable.Name}");
                } 
            }
        }

        public IEnumerable<DirectoryDo> Directories
        {
            get
            {
                using (var connection = m_Database.OpenConnection())
                {
                    return connection.Query<DirectoryDo>($"SELECT * FROM {DirectoriesTable.Name}");
                }
            }
        }

        public IEnumerable<FileInstanceDo> FileInstancess
        {
            get
            {
                using (var connection = m_Database.OpenConnection())
                {
                    return connection.Query<FileInstanceDo>($"SELECT * FROM {FileInstancesTable.Name}");
                }
            }
        }

        public IEnumerable<DirectoryInstanceDo> DirectorieInstances
        {
            get
            {
                using (var connection = m_Database.OpenConnection())
                {
                    return connection.Query<DirectoryInstanceDo>($"SELECT * FROM {DirectoryInstancesTable.Name}");
                }
            }
        }


        public FileSystemRepository(IDatabase database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));

            //TODO: Should happen on first access??
            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                FilesTable.Create(connection);
                FileInstancesTable.Create(connection);
                DirectoriesTable.Create(connection);
                DirectoryInstancesTable.Create(connection);
                ContainsDirectoryTable.Create(connection);
                ContainsFileTable.Create(connection);                

                transaction.Commit();
            }
        }


        public void AddFile(FileDo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (file.Id != 0)
                throw new ArgumentException("Cannot add file with id != 0", nameof(file));
            
            using (var connection = m_Database.OpenConnection())
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

            using (var connection = m_Database.OpenConnection())
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

            using (var connection = m_Database.OpenConnection())
            {
                Insert(connection, fileInstance);
            }
        }
        
        public void AddRecursively(DirectoryInstanceDo rootDirectory)
        {
            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {                
                Insert(connection, rootDirectory);               
                transaction.Commit();
            }
        }

        public DirectoryInstanceDo GetDirectoryInstance(int id)
        {
            using (var connection = m_Database.OpenConnection())
            {
                return connection.QuerySingle<DirectoryInstanceDo>($@"
                    SELECT * 
                    FROM {DirectoryInstancesTable.Name}
                    WHERE {DirectoryInstancesTable.Column.Id} = @id;
                ", 
                new { id = id });
            }
        }

        public void LoadDirectories(DirectoryInstanceDo parentDirectoryInstance)
        {
            using (var connection = m_Database.OpenConnection())
            {
                var directories = connection.Query<DirectoryInstanceDo>($@"    
                    SELECT * 
                    FROM {DirectoryInstancesTable.Name}
                    WHERE {DirectoryInstancesTable.Column.Id} IN (
                        SELECT {ContainsDirectoryTable.Column.ChildId}
                        FROM {ContainsDirectoryTable.Name}
                        WHERE {ContainsDirectoryTable.Column.ParentId} = @id        
                    );
                ",
                new { id = parentDirectoryInstance.Id });

                parentDirectoryInstance.Directories = directories.ToList();
            }
        }

        public void LoadDirectory(DirectoryInstanceDo directoryInstance)
        {
            using (var connection = m_Database.OpenConnection())
            {
                var dir = connection.QuerySingle<DirectoryDo>($@"
                    SELECT * 
                    FROM {DirectoriesTable.Name}
                    WHERE {DirectoriesTable.Column.Id} IN (
                        SELECT {DirectoryInstancesTable.Column.DirectoryId}
                        FROM {DirectoryInstancesTable.Name}
                        WHERE {DirectoryInstancesTable.Column.Id} = @id
                    )
                ", 
                new {id = directoryInstance.Id});

                directoryInstance.Directory = dir;
            }
        }

        public void LoadFiles(DirectoryInstanceDo parentDirectoryInstance)
        {
            using (var connection = m_Database.OpenConnection())
            {
                var files = connection.Query<FileInstanceDo>($@"    
                    SELECT * 
                    FROM {FileInstancesTable.Name}
                    WHERE {FileInstancesTable.Column.Id} IN (
                        SELECT {ContainsFileTable.Column.ChildId}
                        FROM {ContainsFileTable.Name}
                        WHERE {ContainsFileTable.Column.ParentId} = @id        
                    );
                ",
                    new { id = parentDirectoryInstance.Id });

                parentDirectoryInstance.Files = files.ToList();
            }
        }

        public void LoadFile(FileInstanceDo fileInstance)
        {        
            using (var connection = m_Database.OpenConnection())
            {
                var file = connection.QuerySingle<FileDo>($@"
                    SELECT * 
                    FROM {FilesTable.Name}
                    WHERE {FilesTable.Column.Id} IN (
                        SELECT {FileInstancesTable.Column.FileId} 
                        FROM {FileInstancesTable.Name}
                        WHERE {FileInstancesTable.Column.Id} = @id
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

                    INSERT OR IGNORE INTO {DirectoriesTable.Name} 
                    (
                        {DirectoriesTable.Column.Name},
                        {DirectoriesTable.Column.NormalizedPath} 
                    )
                    VALUES (@name, @path);

                    SELECT {DirectoriesTable.Column.Id} FROM {DirectoriesTable.Name}
                    WHERE {DirectoriesTable.Column.NormalizedPath} = @path;
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
                    
                    INSERT OR IGNORE INTO {FilesTable.Name} 
                    (
                        {FilesTable.Column.Name},
                        {FilesTable.Column.NormalizedPath},
                        {FilesTable.Column.Path} 
                    )
                    VALUES (@name, @normalizedPath, @path) ;
                    
                    SELECT {FilesTable.Column.Id} FROM {FilesTable.Name}
                    WHERE {FilesTable.Column.NormalizedPath} = @normalizedPath;",

                    ("name", file.Name),          
                    ("path", file.Path),
                    ("normalizedPath", file.NormalizedPath)
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
                    INSERT INTO {DirectoryInstancesTable.Name} 
                    (
                        {DirectoryInstancesTable.Column.DirectoryId}, 
                        {DirectoryInstancesTable.Column.TmpId}
                    )
                    VALUES (@directoryId, @tmpId);

                    SELECT {DirectoryInstancesTable.Column.Id} 
                    FROM {DirectoryInstancesTable.Name}
                    WHERE {DirectoryInstancesTable.Column.TmpId} = @tmpId",

                    ("directoryId", directoryInstance.Directory.Id),
                    ("tmpId", tmpId)
                );
        
            foreach(var file in directoryInstance.Files)
            {
                connection.ExecuteNonQuery($@"
                    INSERT INTO {ContainsFileTable.Name} 
                    (
                        {ContainsFileTable.Column.ParentId}, 
                        {ContainsFileTable.Column.ChildId}
                    ) 
                    VALUES (@parentId, @childId);",

                    ("parentId", directoryInstance.Id),
                    ("childId", file.Id)
                );
            }

            foreach (var childDir in directoryInstance.Directories)
            {
                connection.ExecuteNonQuery($@"
                    INSERT INTO {ContainsDirectoryTable.Name} 
                    (
                        {ContainsDirectoryTable.Column.ParentId}, 
                        {ContainsDirectoryTable.Column.ChildId}
                    ) 
                    VALUES (@parentId, @childId);",

                    ("parentId", directoryInstance.Id),
                    ("childId", childDir.Id)
                );
            }

            connection.ExecuteNonQuery($@"
                UPDATE {DirectoryInstancesTable.Name} 
                SET {DirectoryInstancesTable.Column.TmpId} = NULL 
                WHERE {DirectoryInstancesTable.Column.TmpId} = @tmpId", 

                ("tmpId", tmpId)
            );

        }

        void Insert(IDbConnection connection, FileInstanceDo fileInstance)
        {        
            if (fileInstance.Id != 0)
                return;

            Insert(connection, fileInstance.File);

            fileInstance.Id = connection.ExecuteScalar<int>($@"
                    INSERT OR IGNORE INTO {FileInstancesTable.Name} 
                    (
                        {FileInstancesTable.Column.FileId},
                        {FileInstancesTable.Column.LastWriteTimeTicks},
                        {FileInstancesTable.Column.Length} 
                    )
                    VALUES (@fileId, @ticks, @length );

                    SELECT * FROM {FileInstancesTable.Name}
                    WHERE {FileInstancesTable.Column.FileId} = @fileId  AND
                          {FileInstancesTable.Column.LastWriteTimeTicks} = @ticks AND 
                          {FileInstancesTable.Column.Length} = @length ;
                ",
                ("fileId", fileInstance.File.Id),
                ("ticks", fileInstance.LastWriteTimeTicks),
                ("length", fileInstance.Length)
            );
        }
    }
}
