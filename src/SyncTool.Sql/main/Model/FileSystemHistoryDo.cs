using JetBrains.Annotations;

namespace SyncTool.Sql.Model
{
    public class FileSystemHistoryDo
    {        
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string NormalizedName { get; set; }
        
        public int Version { get; set; }


        [UsedImplicitly]
        public FileSystemHistoryDo()
        {
        }

        public FileSystemHistoryDo(string name)
        {
            Name = name;
            NormalizedName = name.NormalizeCaseInvariant();
        }
    }
}
