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
        readonly Database m_Database;


        public IEnumerable<FilesTable.Record> Files => m_Database.Query<FilesTable.Record>($"SELECT * FROM {FilesTable.Name}");

        public IEnumerable<DirectoriesTable.Record> Directories => m_Database.Query<DirectoriesTable.Record>($"SELECT * FROM {DirectoriesTable.Name}");

        public IEnumerable<FileInstancesTable.Record> FileInstancess => m_Database.Query<FileInstancesTable.Record>($"SELECT * FROM {FileInstancesTable.Name}");

        public IEnumerable<DirectoryInstancesTable.Record> DirectorieInstances => m_Database.Query<DirectoryInstancesTable.Record>($"SELECT * FROM {DirectoryInstancesTable.Name}");


        public FileSystemRepository(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public void AddFile(FilesTable.Record file)
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

        public void AddDirectory(DirectoriesTable.Record directory)
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

        public void AddFileInstance(FileInstancesTable.Record fileInstance)
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
        
        public void AddRecursively(DirectoryInstancesTable.Record rootDirectory)
        {
            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {                
                Insert(connection, rootDirectory);               
                transaction.Commit();
            }
        }

        public DirectoryInstancesTable.Record GetDirectoryInstance(int id)
        {
            return m_Database.QuerySingle<DirectoryInstancesTable.Record>($@"
                SELECT * 
                FROM {DirectoryInstancesTable.Name}
                WHERE {DirectoryInstancesTable.Column.Id} = @id;
            ", 
            new { id = id });
        }

        public void LoadDirectories(DirectoryInstancesTable.Record parentDirectoryInstance)
        {            
            var directories = m_Database.Query<DirectoryInstancesTable.Record>($@"    
                SELECT * 
                FROM {DirectoryInstancesTable.Name}
                WHERE {DirectoryInstancesTable.Column.Id} IN 
                (
                    SELECT {ContainsDirectoryTable.Column.ChildId}
                    FROM {ContainsDirectoryTable.Name}
                    WHERE {ContainsDirectoryTable.Column.ParentId} = @id        
                );
            ",
            new { id = parentDirectoryInstance.Id });

            parentDirectoryInstance.Directories = directories.ToList();           
        }

        public void LoadDirectory(DirectoryInstancesTable.Record directoryInstance)
        {
            directoryInstance.Directory = m_Database.QuerySingle<DirectoriesTable.Record>($@"
                SELECT * 
                FROM {DirectoriesTable.Name}
                WHERE {DirectoriesTable.Column.Id} IN 
                (
                    SELECT {DirectoryInstancesTable.Column.DirectoryId}
                    FROM {DirectoryInstancesTable.Name}
                    WHERE {DirectoryInstancesTable.Column.Id} = @id
                )", 
                new {id = directoryInstance.Id});
        }

        public void LoadFiles(DirectoryInstancesTable.Record parentDirectoryInstance)
        {
            var files = m_Database.Query<FileInstancesTable.Record>($@"    
                SELECT * 
                FROM {FileInstancesTable.Name}
                WHERE {FileInstancesTable.Column.Id} IN 
                (
                    SELECT {ContainsFileTable.Column.ChildId}
                    FROM {ContainsFileTable.Name}
                    WHERE {ContainsFileTable.Column.ParentId} = @id        
                );",
                new { id = parentDirectoryInstance.Id });

            parentDirectoryInstance.Files = files.ToList();
            
        }

        public void LoadFile(FileInstancesTable.Record fileInstance)
        {
            fileInstance.File = m_Database.QuerySingle<FilesTable.Record>($@"
                SELECT * 
                FROM {FilesTable.Name}
                WHERE {FilesTable.Column.Id} IN 
                (
                    SELECT {FileInstancesTable.Column.FileId} 
                    FROM {FileInstancesTable.Name}
                    WHERE {FileInstancesTable.Column.Id} = @id
                )
            ",  
            new { id = fileInstance.Id });
        }


        void Insert(IDbConnection connection, DirectoriesTable.Record directory)
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

        void Insert(IDbConnection connection, FilesTable.Record file)
        {
            if (String.IsNullOrWhiteSpace(file.NormalizedPath))
                throw new ArgumentException($"{nameof(FilesTable.Record.NormalizedPath)} must not be empty");

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

        void Insert(IDbConnection connection, DirectoryInstancesTable.Record directoryInstance)
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

        void Insert(IDbConnection connection, FileInstancesTable.Record fileInstance)
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
