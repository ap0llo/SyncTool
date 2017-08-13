using SyncTool.Sql.Model;
using SyncTool.Sql.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SyncTool.Sql.Test.Model
{
    public class FileSystemRepositoryTest : SqlTestBase
    {
        FileSystemRepository CreateInstance() => new FileSystemRepository(ContextFactory);


        [Fact]
        public void Files_is_initally_empty()
        {
            var instance = CreateInstance();
            Assert.Empty(instance.Files);
        }

        [Fact]
        public void Files_returns_expected_values()
        {
            var file1 = new FileDo() { Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var instance = CreateInstance();

            instance.AddFile(file1);

            var files = instance.Files.ToArray();
            Assert.Single(files);
            Assert.EndsWith(file1.Name, files.Single().Name);
            Assert.EndsWith(file1.NormalizedPath, files.Single().NormalizedPath);
        }

        [Fact]
        public void AddFile_throws_ArgumentException_if_id_is_not_0()
        {
            var file = new FileDo() { Id = 1, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            Assert.Throws<ArgumentException>(() => CreateInstance().AddFile(file));
        }

        [Fact]
        public void AddFile_throws_ArgumentException_if_NormalizedPath_is_null_or_empty()
        {
            var instance = CreateInstance();
            Assert.Throws<ArgumentException>(() => instance.AddFile(new FileDo() { Id = 0, Name = "file1", NormalizedPath = null }));
            Assert.Throws<ArgumentException>(() => instance.AddFile(new FileDo() { Id = 0, Name = "file1", NormalizedPath = "" }));
            Assert.Throws<ArgumentException>(() => instance.AddFile(new FileDo() { Id = 0, Name = "file1", NormalizedPath = "\t" }));
        }

        [Fact]
        public void AddFile_does_not_insert_duplicate_entries()
        {
            var instance = CreateInstance();
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var inserted = instance.AddFile(file);
            var inserted2 = instance.AddFile(file);

            Assert.Equal(inserted.Id, inserted2.Id);
        }

        [Fact]
        public void Directories_is_initally_empty()
        {
            var instance = CreateInstance();
            Assert.Empty(instance.Directories);
        }

        [Fact]
        public void Directories_returns_expected_values()
        {
            var dir = new DirectoryDo() { Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() };
            var instance = CreateInstance();

            instance.AddDirectory(dir);

            var directories = instance.Directories.ToArray();
            Assert.Single(directories);
            Assert.EndsWith(dir.Name, directories.Single().Name);
            Assert.EndsWith(dir.NormalizedPath, directories.Single().NormalizedPath);
        }

        [Fact]
        public void AddDirectory_throws_ArgumentException_if_id_is_not_0()
        {
            var dir = new DirectoryDo() { Id = 1, Name = "dir1", NormalizedPath = "/dir".NormalizeCaseInvariant() };
            Assert.Throws<ArgumentException>(() => CreateInstance().AddDirectory(dir));
        }

        [Fact]
        public void AddDirectory_throws_ArgumentException_if_NormalizedPath_is_null_or_empty()
        {
            var instance = CreateInstance();
            Assert.Throws<ArgumentException>(() => instance.AddDirectory(new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = null }));
            Assert.Throws<ArgumentException>(() => instance.AddDirectory(new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = "" }));
            Assert.Throws<ArgumentException>(() => instance.AddDirectory(new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = "\t" }));
        }

        [Fact]
        public void AddDirectory_does_not_insert_duplicate_entries()
        {
            var instance = CreateInstance();
            var dir = new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() };
            var inserted = instance.AddDirectory(dir);
            var inserted2 = instance.AddDirectory(dir);

            Assert.Equal(inserted.Id, inserted2.Id);
        }


        [Fact]
        public void AddFileInstance_throws_ArgumentException_if_id_is_not_0()
        {
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);
            fileInstance.Id = 1;
            Assert.Throws<ArgumentException>(() => CreateInstance().AddFileInstance(file, fileInstance));
        }

        [Fact]
        public void AddFileInstance_implicitly_add_a_file_if_it_does_not_exist()
        {
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);
            
            var instance = CreateInstance();
            Assert.Empty(instance.Files);
            instance.AddFileInstance(file, fileInstance);
            Assert.Single(instance.Files);
        }

        [Fact]
        public void AddFileInstance_assigns_file_id_of_existing_file_if_it_exists()
        {
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);

            var instance = CreateInstance();
            var insertedFile = instance.AddFile(file);            
            var insertedFileInstance = instance.AddFileInstance(file, fileInstance);

            Assert.Equal(insertedFile.Id, insertedFileInstance.FileId);
        }
        [Fact]
        public void AddFileInstance_does_not_insert_duplicate_entries()
        {
            var instance = CreateInstance();

            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);

            var inserted = instance.AddFileInstance(file, fileInstance);
            var inserted2 = instance.AddFileInstance(file, fileInstance);

            Assert.Equal(inserted.Id, inserted2.Id);
        }

    }
}
