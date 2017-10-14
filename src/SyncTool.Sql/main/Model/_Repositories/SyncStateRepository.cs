using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SyncTool.Utilities;

namespace SyncTool.Sql.Model
{
    public class SyncStateRepository
    {
        readonly Database m_Database;


        public SyncStateRepository(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public SyncStateDo GetSyncState() => m_Database.QuerySingle<SyncStateDo>($"SELECT * FROM {SyncStateTable.Name}");

        public void LoadActions(SyncStateDo syncStateDo)
        {
            if (syncStateDo == null)
                throw new ArgumentNullException(nameof(syncStateDo));

            lock (syncStateDo)
            {
                if (syncStateDo.Actions != null)
                    return;

                syncStateDo.Actions = m_Database.Query<SyncActionDo>($"SELECT * FROM {SyncActionsTable.Name}").ToList();                
            }            
        }

        public void LoadConflicts(SyncStateDo syncStateDo)
        {
            if (syncStateDo == null)
                throw new ArgumentNullException(nameof(syncStateDo));

            lock (syncStateDo)
            {
                if (syncStateDo.Conflicts != null)
                    return;

                syncStateDo.Conflicts = m_Database.Query<SyncConflictDo>($"SELECT * FROM {SyncConflictsTable.Name}").ToList();
            }
        }

        public void LoadConflictingVersions(SyncConflictDo syncConflictDo)
        {
            if (syncConflictDo == null)
            {
                throw new ArgumentNullException(nameof(syncConflictDo));
            }

            lock(syncConflictDo)
            {
                if (syncConflictDo.ConflictingVersions != null)
                    return;

                syncConflictDo.ConflictingVersions = m_Database.Query<FileReferenceDo>($@"
                    SELECT * FROM {FileReferencesTable.Name}
                    WHERE {FileReferencesTable.Column.Id} IN
                    (
                        SELECT {ContainsConflictingVersionsTable.Column.FileReferenceId}
                        FROM {ContainsConflictingVersionsTable.Name}
                        WHERE
                            {ContainsConflictingVersionsTable.Column.SyncConflictId} = @conflictId
                        AND
                            {ContainsConflictingVersionsTable.Column.FileReferenceId} IS NOT NULL
                    );",
                    new { conflictId = syncConflictDo.Id } )
                .ToList();

                var containsNull = m_Database.ExecuteScalar<int>($@"
                        SELECT count(*)
                        FROM {ContainsConflictingVersionsTable.Name}
                        WHERE
                            {ContainsConflictingVersionsTable.Column.SyncConflictId} = @conflictId
                        AND
                            {ContainsConflictingVersionsTable.Column.FileReferenceId} IS NULL;",
                        ("conflictId", syncConflictDo.Id)) > 0;

                if (containsNull)
                    syncConflictDo.ConflictingVersions.Add(null);
            }
        }

        public void LoadVersions(SyncActionDo syncActionDo)
        {
            if (syncActionDo == null)
                throw new ArgumentNullException(nameof(syncActionDo));

            lock(syncActionDo)
            {
                syncActionDo.FromVersion = m_Database.QuerySingleOrDefault<FileReferenceDo>($@"
                    SELECT * FROM {FileReferencesTable.Name}
                    WHERE {FileReferencesTable.Column.Id} IN
                    (
                        SELECT {SyncActionsTable.Column.FromVersionFileReferenceId}
                        FROM {SyncActionsTable.Name}
                        WHERE {SyncActionsTable.Column.Id} = @actionId
                    )",
                    new {  actionId = syncActionDo.Id}
                );

                syncActionDo.ToVersion = m_Database.QuerySingleOrDefault<FileReferenceDo>($@"
                    SELECT * FROM {FileReferencesTable.Name}
                    WHERE {FileReferencesTable.Column.Id} IN
                    (
                        SELECT {SyncActionsTable.Column.ToVersionFileReferenceId}
                        FROM {SyncActionsTable.Name}
                        WHERE {SyncActionsTable.Column.Id} = @actionId
                    )",
                    new { actionId = syncActionDo.Id }
                );

            }

        }

        public void UpdateSyncState(SyncStateDo syncStateDo)
        {
            if (syncStateDo == null)
                throw new ArgumentNullException(nameof(syncStateDo));

            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                connection.ExecuteNonQuery($"DELETE FROM {SyncActionsTable.Name};");
                connection.ExecuteNonQuery($"DELETE FROM {ContainsConflictingVersionsTable.Name};");
                connection.ExecuteNonQuery($"DELETE FROM {SyncConflictsTable.Name};");
                connection.ExecuteNonQuery($"DELETE FROM {FileReferencesTable.Name};");

                // save file references
                var fileReferences = GetFileReferences(syncStateDo).ToArray();
                InsertFileReferences(connection, fileReferences);

                // save sync conflicts
                var conflicts = syncStateDo.Conflicts.ToArray();
                InsertConflicts(connection, conflicts);

                // save sync actions
                var actions = syncStateDo.Actions.ToArray();
                InsertActions(connection, actions);

                // update sync state
                var changedRows = connection.ExecuteNonQuery($@"
                    UPDATE {SyncStateTable.Name}                    
                    SET {SyncStateTable.Column.SnapshotId} = @snapshotId,
                        {SyncStateTable.Column.Version}    = @newVersion
                    WHERE {SyncStateTable.Column.Version} = @oldVersion ",                          

                   ("snapshotId", syncStateDo.SnapshotId),                   
                   ("oldVersion", syncStateDo.Version - 1),
                   ("newVersion", syncStateDo.Version)
               );

                if (changedRows == 0)
                {
                    transaction.Rollback();
                    throw new DatabaseUpdateException("No rows affected by update");
                }

                if (changedRows > 1)
                {
                    transaction.Rollback();
                    throw new DatabaseUpdateException("More than one row affected by update");
                }

                transaction.Commit();
            }            
        }


        IEnumerable<FileReferenceDo> GetFileReferences(SyncStateDo syncState)
        {
            foreach(var action in syncState.Actions)
            {
                if (action.FromVersion != null)
                    yield return action.FromVersion;

                if (action.ToVersion != null)
                    yield return action.ToVersion;
            }

            foreach(var fileReference in syncState.Conflicts.SelectMany(c => c.ConflictingVersions).Where(f => f != null))
            {
                if (fileReference != null)
                    yield return fileReference;                
            }
        }

        void InsertFileReferences(IDbConnection connection, FileReferenceDo[] fileReferences)
        {           
            // assign ids
            for(int i = 0; i < fileReferences.Length; i++)
            {
                fileReferences[i].Id = i + 1;
            }

            // insert
            foreach(var segment in fileReferences.GetSegments(m_Database.Limits.MaxParameterCount / 4))
            {
                InsertFileReferences(connection, segment);
            }            
        }

        void InsertFileReferences(IDbConnection connection, ArraySegment<FileReferenceDo> fileReferences)
        {
            var query = $@"
                INSERT INTO {FileReferencesTable.Name}
                (
                    {FileReferencesTable.Column.Id},
                    {FileReferencesTable.Column.Path},
                    {FileReferencesTable.Column.LastWriteUnixTimeTicks},
                    {FileReferencesTable.Column.Length}
                )
                VALUES
                {
                    fileReferences
                        .Select( (reference, index) => $"(@id{index}, @path{index}, @ticks{index}, @length{index})")
                        .JoinToString(" , ")                    
                }
            ";

            var parameters = fileReferences
                .SelectMany((reference, index) =>
                    new(string, object)[]
                    {
                        ($"id{index}", reference.Id),
                        ($"path{index}", reference.Path),
                        ($"ticks{index}", reference.LastWriteUnixTimeTicks),
                        ($"length{index}", reference.Length)
                    })
                .ToArray();

            connection.ExecuteNonQuery(query, parameters);            
        }

        void InsertConflicts(IDbConnection connection, SyncConflictDo[] conflicts)
        {
            // assign ids
            for (int i = 0; i < conflicts.Length; i++)
            {
                conflicts[i].Id = i + 1;
            }
            
            // insert new values
            foreach(var segment in conflicts.GetSegments(m_Database.Limits.MaxParameterCount / 2))
            {
                InsertConflicts(connection, segment);
            }

            // insert into table linking conflicts to file references
            InsertConflictingVersions(connection, conflicts);
        }

        void InsertConflicts(IDbConnection connection, ArraySegment<SyncConflictDo> conflicts)
        {
            var query = $@"
                INSERT INTO {SyncConflictsTable.Name}
                (
                    {SyncConflictsTable.Column.Id},
                    {SyncConflictsTable.Column.SnapshotId}
                )
                VALUES
                {
                    conflicts
                        .Select( (conflict, index) => $" (@id{index}, @snapshotId{index}) ")
                        .JoinToString(" , ")
                }
            ";

            var parameters = conflicts
                .SelectMany( (conflict, index) =>
                    new (string, object)[]
                    {
                        ($"id{index}", conflict.Id),
                        ($"snapshotId{index}", conflict.SnapshotId)
                    })
                .ToArray();

            connection.ExecuteNonQuery(query, parameters);
        }

        void InsertConflictingVersions(IDbConnection connection, SyncConflictDo[] conflicts)
        {
            (int conflictId, int? fileReferenceId)[] relations = conflicts
                 .SelectMany(conflict =>
                     conflict.ConflictingVersions
                                .Select(fileReference => (conflict.Id, fileReference?.Id)) 
                 )
                 .ToArray();


            foreach (var segement in relations.GetSegments(m_Database.Limits.MaxParameterCount / 2))
            {
                var query = $@"
                    INSERT INTO {ContainsConflictingVersionsTable.Name}
                    (
                        {ContainsConflictingVersionsTable.Column.SyncConflictId},
                        {ContainsConflictingVersionsTable.Column.FileReferenceId}
                    )
                    VALUES
                    {
                        relations
                            .Select((relation, index) => $"(@conflictId{index}, @fileReferenceId{index})")
                            .JoinToString(" , ")
                    }
                ";

                var parameters = relations
                    .SelectMany((relation, index) =>
                       new(string, object)[]
                       {
                           ($"conflictId{index}", relation.conflictId),
                           ($"fileReferenceId{index}", relation.fileReferenceId)
                       })
                    .ToArray();

                connection.ExecuteNonQuery(query, parameters);
            }
        }

        void InsertActions(IDbConnection connection, SyncActionDo[] actions)
        {
            // assign ids
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Id = i + 1;
            }

            // insert items
            foreach (var segment in actions.GetSegments(m_Database.Limits.MaxParameterCount / 4))
            {
                InsertActions(connection, segment);
            }
        }

        void InsertActions(IDbConnection connection, ArraySegment<SyncActionDo> actions)
        {
            var query = $@"
                INSERT INTO {SyncActionsTable.Name}
                (
                    {SyncActionsTable.Column.Id},
                    {SyncActionsTable.Column.SnapshotId},
                    {SyncActionsTable.Column.FromVersionFileReferenceId},
                    {SyncActionsTable.Column.ToVersionFileReferenceId}
                )
                VALUES
                {
                    actions
                        .Select((_, index) => $"(@id{index}, @snapshotId{index}, @fromVersion{index}, @toVersion{index})")
                        .JoinToString(" , ")
                }
            ";

            var parameters = actions
                .SelectMany((action, index) =>
                    new(string, object)[]
                    {
                        ($"id{index}", action.Id),
                        ($"snapshotId{index}", action.SnapshotId),
                        ($"fromVersion{index}", action.FromVersion?.Id),
                        ($"toVersion{index}", action.ToVersion?.Id)
                    })
                .ToArray();

            connection.ExecuteNonQuery(query, parameters);
        }
    }

}
