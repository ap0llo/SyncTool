using System;
using SyncTool.FileSystem;
using NodaTime;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Services
{
    sealed class SqlFileReference : IFileReference
    {
        readonly FileReferenceDo m_FileReferenceDo;


        public string Path => m_FileReferenceDo.Path;

        public Instant? LastWriteTime =>
            m_FileReferenceDo.LastWriteUnixTimeTicks.HasValue
                ? (Instant?) Instant.FromUnixTimeTicks(m_FileReferenceDo.LastWriteUnixTimeTicks.Value)
                : null;

        public long? Length => m_FileReferenceDo.Length;


        public SqlFileReference(FileReferenceDo fileReferenceDo)
        {
            m_FileReferenceDo = fileReferenceDo ?? throw new ArgumentNullException(nameof(fileReferenceDo));
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as IFileReference);

        public bool Equals(IFileReference other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.InvariantCultureIgnoreCase.Equals(Path, other.Path) &&
                   LastWriteTime == other.LastWriteTime &&
                   Length == other.Length;
        }
    }


    internal static class FileReferenceDoExtensions
    {
        public static SqlFileReference ToSqlFileReference(this FileReferenceDo fileReferenceDo) =>
            new SqlFileReference(fileReferenceDo);
    }
}
