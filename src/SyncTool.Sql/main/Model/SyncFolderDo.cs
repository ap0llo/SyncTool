using SyncTool.Configuration;

namespace SyncTool.Sql.Model
{
    public class SyncFolderDo
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public int Version { get; set; }


        public SyncFolder ToSyncFolder() => new SyncFolder(Name) { Path = Path };

        public static SyncFolderDo FromSyncFolder(SyncFolder folder)
        {
            return new SyncFolderDo()
            {
                Name = folder.Name,
                Path = folder.Path
            };
        }
    }
}
