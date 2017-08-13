using Dapper;
using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    class FileSystemRepository
    {
        private readonly IDatabaseContextFactory m_ConnectionFactory;


        public IEnumerable<FileDo> Files
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<FileDo>($"SELECT * FROM {FileDo.TableName}");
                } 
            }
        }

        public IEnumerable<DirectoryDo> Directories
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<DirectoryDo>($"SELECT * FROM {DirectoryDo.TableName}");
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

                    CREATE TABLE IF NOT EXISTS {FileDo.TableName} (                
                        {nameof(FileDo.Id)} INTEGER PRIMARY KEY,
                        {nameof(FileDo.Name)} TEXT NOT NULL,
                        {nameof(FileDo.NormalizedPath)} TEXT UNIQUE NOT NULL);

                    CREATE TABLE IF NOT EXISTS {DirectoryDo.TableName} (                
                        {nameof(DirectoryDo.Id)} INTEGER PRIMARY KEY,
                        {nameof(DirectoryDo.Name)} TEXT NOT NULL,
                        {nameof(DirectoryDo.NormalizedPath)} TEXT UNIQUE NOT NULL) ;
                
                    CREATE TABLE IF NOT EXISTS {FileInstanceDo.TableName} (                
                        {nameof(FileInstanceDo.Id)} INTEGER PRIMARY KEY,
                        {nameof(FileInstanceDo.FileId)} INTEGER NOT NULL,
                        {nameof(FileInstanceDo.LastWriteTimeTicks)} INTEGER NOT NULL,
                        {nameof(FileInstanceDo.Length)} INTEGER NOT NULL,
                        FOREIGN KEY ({nameof(FileInstanceDo.FileId)}) REFERENCES {FileDo.TableName}({nameof(FileDo.Id)}),
                        CONSTRAINT FileInstance_Unique UNIQUE (
                            {nameof(FileInstanceDo.FileId)}, 
                            {nameof(FileInstanceDo.LastWriteTimeTicks)}, 
                            {nameof(FileInstanceDo.Length)} )
                        ) ; "
                    );
            }
        }
        

        public FileDo AddFile(FileDo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (file.Id != 0)
                throw new ArgumentException("Cannot add file with id != 0", nameof(file));

            if (String.IsNullOrWhiteSpace(file.NormalizedPath))
                throw new ArgumentException($"{nameof(FileDo.NormalizedPath)} must not be empty");

            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingle<FileDo>($@"
                    
                    INSERT OR IGNORE INTO {FileDo.TableName} (
                        {nameof(FileDo.Name)},
                        {nameof(FileDo.NormalizedPath)} )
                    VALUES (
                        @{nameof(FileDo.Name)},
                        @{nameof(FileDo.NormalizedPath)}) ;
                    
                    SELECT * FROM {FileDo.TableName}
                    WHERE {nameof(FileDo.NormalizedPath)} = {nameof(FileDo.NormalizedPath)};
                ",
                file);
            }        
        }        

        public DirectoryDo AddDirectory(DirectoryDo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            if (directory.Id != 0)
                throw new ArgumentException("Cannot add directory with id != 0", nameof(directory));

            if (String.IsNullOrWhiteSpace(directory.NormalizedPath))
                throw new ArgumentException($"{nameof(directory.NormalizedPath)} must not be empty");

            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingle<DirectoryDo>($@"
                    
                    INSERT OR IGNORE INTO {DirectoryDo.TableName} (
                        {nameof(DirectoryDo.Name)},
                        {nameof(DirectoryDo.NormalizedPath)} )
                    VALUES (
                        @{nameof(DirectoryDo.Name)},
                        @{nameof(DirectoryDo.NormalizedPath)}) ;
                    
                    SELECT * FROM {DirectoryDo.TableName}
                    WHERE {nameof(DirectoryDo.NormalizedPath)} = {nameof(DirectoryDo.NormalizedPath)};
                ",
                directory);
            }
        }

        public FileInstanceDo AddFileInstance(FileDo file, FileInstanceDo fileInstance)
        {
            if (fileInstance == null)
                throw new ArgumentNullException(nameof(fileInstance));

            if(fileInstance.Id != 0)
                throw new ArgumentException("Cannot add file instance with id != 0", nameof(fileInstance));

            file = AddFile(file);
            fileInstance.FileId = file.Id;

            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingle<FileInstanceDo>($@"
                    INSERT OR IGNORE INTO {FileInstanceDo.TableName} (
                        {nameof(FileInstanceDo.FileId)},
                        {nameof(FileInstanceDo.LastWriteTimeTicks)},
                        {nameof(FileInstanceDo.Length)} )
                    VALUES (
                        @{nameof(FileInstanceDo.FileId)},
                        @{nameof(FileInstanceDo.LastWriteTimeTicks)},
                        @{nameof(FileInstanceDo.Length)} );

                    SELECT * FROM {FileInstanceDo.TableName}
                    WHERE {nameof(FileInstanceDo.FileId)} = @{nameof(FileInstanceDo.FileId)} AND
                          {nameof(FileInstanceDo.LastWriteTimeTicks)} = @{nameof(FileInstanceDo.LastWriteTimeTicks)} AND 
                          {nameof(FileInstanceDo.Length)} = @{nameof(FileInstanceDo.Length)} ;
                ",
                fileInstance);                
            }
        }
    }
}
