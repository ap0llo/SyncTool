using Microsoft.EntityFrameworkCore;

namespace SyncTool.Sql.Model
{
    public abstract class DatabaseContext : DbContext
    {
        public DbSet<SyncFolderDo> SyncFolders { get; set; }
        
        protected override abstract void OnConfiguring(DbContextOptionsBuilder optionsBuilder);        
    }
}
