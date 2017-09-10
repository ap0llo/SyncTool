using System;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using SyncTool.Utilities;

namespace SyncTool.Sql.Model
{
    sealed class PathFilterTable : IDisposable
    {
        public enum Column
        {
            Path
        }

        readonly string m_Id;
        readonly IDbConnection m_Connection;
        readonly DatabaseLimits m_Limits;
        readonly string[] m_PathFilter;


        public string Name => $"Table_PathFilter_{m_Id}";


        private PathFilterTable([NotNull] IDbConnection connection, [NotNull] DatabaseLimits limits, [NotNull] string[] pathFilter)
        {
            m_Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            m_Limits = limits ?? throw new ArgumentNullException(nameof(limits));
            m_PathFilter = pathFilter ?? throw new ArgumentNullException(nameof(pathFilter));
            m_Id = Guid.NewGuid().ToString().Replace("-", "");
        }


        public void Dispose() => m_Connection.ExecuteNonQuery($"DROP TABLE {Name};");


        void Create()
        {
            m_Connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} 
                (
                    {Column.Path} VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci PRIMARY KEY
                );
            ");
            
            foreach (var segment in m_PathFilter.GetSegments(m_Limits.MaxParameterCount))
            {
                var query = $@"
                    INSERT INTO {Name} ({Column.Path}) 
                    VALUES 
                    {
                        segment
                            .Select((_, index) => $"(@path{index})")
                            .JoinToString(",")
                    };";
                
                var parameters = segment.Select((path, index) => ($"path{index}", (object) path));
                
                m_Connection.ExecuteNonQuery(query, parameters.ToArray());
            }
        }


        public static PathFilterTable CreateTemporary(IDbConnection connection, DatabaseLimits limits, string[] pathFilter)
        {
            var table = new PathFilterTable(connection, limits, pathFilter);
            table.Create();
            return table;
        }
    }
}
