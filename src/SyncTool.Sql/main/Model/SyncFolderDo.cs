
using SyncTool.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncTool.Sql.Model
{
    public class SyncFolderDo
    {        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]        
        public string Name { get; set; }

        public string Path { get; set; }


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
