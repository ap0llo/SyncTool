using Microsoft.EntityFrameworkCore;

namespace SyncTool.Sql.Model
{
    public abstract class DatabaseContext : DbContext
    {        
        public DbSet<FileSystemHistoryDo> FileSystemHistories { get; set; }

        public DbSet<FileSystemSnapshotDo> FileSystemSnapshots { get; set; }

        public DbSet<FileDo> Files { get; set; }

        public DbSet<FileInstanceDo> FileInstances { get; set; }

        public DbSet<DirectoryDo> Directories { get; set; }

        public DbSet<DirectoryInstanceDo> DirectoryInstances { get; set; }
        

        protected override abstract void OnConfiguring(DbContextOptionsBuilder optionsBuilder);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileSystemHistoryDo>().HasAlternateKey(c => c.NormalizedName);
            modelBuilder.Entity<FileSystemHistoryDo>().Property(e => e.Version).IsConcurrencyToken();
            
            modelBuilder.Entity<FileInstanceDo>().HasOne(e => e.File).WithMany(e => e.Instances);
            modelBuilder.Entity<FileInstanceDo>().Property(e => e.LastWriteTimeTicks).IsRequired();
            modelBuilder.Entity<FileInstanceDo>().Property(e => e.Length).IsRequired();

            modelBuilder.Entity<FileDo>().HasAlternateKey(c => c.NormalizedPath);

            modelBuilder.Entity<DirectoryDo>().HasAlternateKey(c => c.NormalizedPath);

            modelBuilder.Entity<FileSystemSnapshotDo>().HasOne(x => x.History);
            modelBuilder.Entity<FileSystemSnapshotDo>().HasAlternateKey(x => x.SequenceNumber);
        }
    }
}
