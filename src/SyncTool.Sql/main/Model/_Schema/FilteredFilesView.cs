using System;
using System.Data;
using JetBrains.Annotations;

namespace SyncTool.Sql.Model
{
    sealed class FilteredFilesView : IDisposable
    {
        public enum Column
        {
            Id,
            Name,
            NormalizedPath,
            Path
        }


        readonly string m_Id;
        readonly IDbConnection m_Connection;
        readonly DatabaseLimits m_Limits;
        readonly string[] m_PathFilter;
        PathFilterTable m_PathFilterTable;


        public string Name => $"View_FilteredFiles_{m_Id}";


        private FilteredFilesView([NotNull] IDbConnection connection, [NotNull] DatabaseLimits limits, [CanBeNull] string[] pathFilter)
        {
            m_Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            m_Limits = limits ?? throw new ArgumentNullException(nameof(limits));
            m_PathFilter = pathFilter;
            m_Id = Guid.NewGuid().ToString().Replace("-", "");
        }
        

        public void Dispose()
        {
            m_Connection.ExecuteNonQuery($"DROP VIEW IF EXISTS {Name};");
            m_PathFilterTable?.Dispose();
        }


        void Create()
        {
            var whereClause = "";
            if (m_PathFilter != null)
            {
                m_PathFilterTable = PathFilterTable.CreateTemporary(m_Connection, m_Limits, m_PathFilter);                
                whereClause = $@"
                    WHERE lower({FilesTable.Column.Path}) IN 
                    (
                        SELECT lower({PathFilterTable.Column.Path}) FROM {m_PathFilterTable.Name}
                    );
                ";
            }

            m_Connection.ExecuteNonQuery($@"
                CREATE VIEW {Name} AS 
                    SELECT 
                        {FilesTable.Column.Id} AS {Column.Id},         
                        {FilesTable.Column.Name} AS {Column.Name},            
                        {FilesTable.Column.Path} AS {Column.Path}
                    FROM {FilesTable.Name}
                    {whereClause}
            ");
        }
        

        public static FilteredFilesView CreateTemporary(IDbConnection connection, DatabaseLimits limits, string[] pathFilter)
        {
            var view = new FilteredFilesView(connection, limits, pathFilter);
            view.Create();
            return view;
        }
    }
}
