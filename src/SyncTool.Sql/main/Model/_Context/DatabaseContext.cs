using Microsoft.EntityFrameworkCore;

namespace SyncTool.Sql.Model
{
    public abstract class DatabaseContext : DbContext
    {
        public DbSet<SyncFolderDo> SyncFolders { get; set; }
        
        public DbSet<FileSystemHistoryDo> FileSystemHistories { get; set; }

        protected override abstract void OnConfiguring(DbContextOptionsBuilder optionsBuilder);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileSystemHistoryDo>().HasAlternateKey(c => c.Name);               
        }
    }
}
