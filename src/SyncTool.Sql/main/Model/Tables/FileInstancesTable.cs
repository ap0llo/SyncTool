using JetBrains.Annotations;
using System;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class FileInstancesTable
    {
        public const string Name = "FileInstances";

        public enum Column
        {
            Id,
            FileId,
            LastWriteTimeTicks,
            Length
        }

        public class Record
        {
            public int Id { get; set; }

            public FilesTable.Record File { get; set; }

            public long LastWriteTimeTicks { get; set; }

            public long Length { get; set; }


            [UsedImplicitly]
            public Record() { }

            public Record(FilesTable.Record file, DateTime lastWriteTime, long length)
            {
                File = file;
                LastWriteTimeTicks = lastWriteTime.Ticks;
                Length = length;
            }
        }

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}                 INTEGER PRIMARY KEY,
                    {Column.FileId}             INTEGER NOT NULL,
                    {Column.LastWriteTimeTicks} INTEGER NOT NULL,
                    {Column.Length}             INTEGER NOT NULL,
                    FOREIGN KEY ({Column.FileId}) REFERENCES {FilesTable.Name}({FilesTable.Column.Id}),
                    CONSTRAINT FileInstance_Unique UNIQUE (
                        {Column.FileId}, 
                        {Column.LastWriteTimeTicks}, 
                        {Column.Length}
                    )); 
            ");
        }
    }
}
