using SyncTool.Sql.Model;
using SyncTool.Sql.TestHelpers;
using System;
using System.Linq;
using Xunit;

namespace SyncTool.Sql.Test.Model
{
    public class FileSystemRepositoryTest : SqlTestBase
    {
        FileSystemRepository CreateRepository() => new FileSystemRepository(ContextFactory);


        [Fact]
        public void Files_is_initally_empty()
        {
            var instance = CreateRepository();
            Assert.Empty(instance.Files);
        }

        [Fact]
        public void Files_returns_expected_values()
        {
            var file1 = new FileDo() { Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var instance = CreateRepository();

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
            Assert.Throws<ArgumentException>(() => CreateRepository().AddFile(file));
        }

        [Fact]
        public void AddFile_throws_ArgumentException_if_NormalizedPath_is_null_or_empty()
        {
            var instance = CreateRepository();
            Assert.Throws<ArgumentException>(() => instance.AddFile(new FileDo() { Id = 0, Name = "file1", NormalizedPath = null }));
            Assert.Throws<ArgumentException>(() => instance.AddFile(new FileDo() { Id = 0, Name = "file1", NormalizedPath = "" }));
            Assert.Throws<ArgumentException>(() => instance.AddFile(new FileDo() { Id = 0, Name = "file1", NormalizedPath = "\t" }));
        }

        [Fact]
        public void AddFile_does_not_insert_duplicate_entries()
        {
            var instance = CreateRepository();
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var duplicate = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };

            instance.AddFile(file);
            instance.AddFile(duplicate);

            Assert.Equal(file.Id, duplicate.Id);
            Assert.Single(instance.Files);
        }

        [Fact]
        public void Directories_is_initally_empty()
        {
            var instance = CreateRepository();
            Assert.Empty(instance.Directories);
        }

        [Fact]
        public void Directories_returns_expected_values()
        {
            var dir = new DirectoryDo() { Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() };
            var instance = CreateRepository();

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
            Assert.Throws<ArgumentException>(() => CreateRepository().AddDirectory(dir));
        }

        [Fact]
        public void AddDirectory_throws_ArgumentException_if_NormalizedPath_is_null_or_empty()
        {
            var instance = CreateRepository();
            Assert.Throws<ArgumentException>(() => instance.AddDirectory(new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = null }));
            Assert.Throws<ArgumentException>(() => instance.AddDirectory(new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = "" }));
            Assert.Throws<ArgumentException>(() => instance.AddDirectory(new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = "\t" }));
        }

        [Fact]
        public void AddDirectory_does_not_insert_duplicate_entries()
        {
            var instance = CreateRepository();
            var dir = new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() };
            var duplicate = new DirectoryDo() { Id = 0, Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() };

            instance.AddDirectory(dir);
            instance.AddDirectory(duplicate);

            Assert.Equal(dir.Id, duplicate.Id);
        }


        [Fact]
        public void AddFileInstance_throws_ArgumentException_if_id_is_not_0()
        {
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);
            fileInstance.Id = 1;
            Assert.Throws<ArgumentException>(() => CreateRepository().AddFileInstance(fileInstance));
        }

        [Fact]
        public void AddFileInstance_implicitly_add_a_file_if_it_does_not_exist()
        {
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);
            
            var instance = CreateRepository();
            Assert.Empty(instance.Files);
            instance.AddFileInstance(fileInstance);
            Assert.Single(instance.Files);
        }

        [Fact]
        public void AddFileInstance_assigns_file_id_of_existing_file_if_it_exists()
        {
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);

            var instance = CreateRepository();

            instance.AddFile(file);            
            instance.AddFileInstance(fileInstance);

            Assert.Equal(file.Id, fileInstance.File.Id);
            Assert.Single(instance.Files);
        }

        [Fact]
        public void AddFileInstance_does_not_insert_duplicate_entries()
        {
            var time = DateTime.UtcNow;
            var instance = CreateRepository();

            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, time, 23);
            var duplicate = new FileInstanceDo(file, time, 23);

            instance.AddFileInstance(fileInstance);
            instance.AddFileInstance(duplicate);

            Assert.Equal(fileInstance.Id, duplicate.Id);
        }

        [Fact]
        public void AddRecursively_adds_all_files_and_directories()
        {
            // ARRANGE
            var root = new DirectoryInstanceDo(new DirectoryDo() { Name = "Root", NormalizedPath = "/" });
            var dir1 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() });
            var dir11 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir11", NormalizedPath = "/dir1/dir11".NormalizeCaseInvariant() });
            var dir2 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir2", NormalizedPath = "/dir2".NormalizeCaseInvariant() });
            dir1.Directories.Add(dir11);
            root.Directories.Add(dir1);
            root.Directories.Add(dir2);
            var file1 = new FileInstanceDo(new FileDo() { Name = "file1", NormalizedPath = "/dir1/file1".NormalizeCaseInvariant() }, DateTime.Now, 42);
            dir1.Files.Add(file1);


            //ACT
            var repository = CreateRepository();
            repository.AddRecursively(root);

            //ASSERT
            Assert.Single(repository.Files);
            Assert.Single(repository.FileInstancess);
            Assert.Equal(4, repository.Directories.Count());
            Assert.Equal(4, repository.DirectorieInstances.Count());

            Assert.NotEqual(0, root.Id);
            Assert.NotEqual(0, root.Directory.Id);
            Assert.NotEqual(0, dir1.Id);
            Assert.NotEqual(0, dir1.Directory.Id);
            Assert.NotEqual(0, dir11.Id);
            Assert.NotEqual(0, dir11.Directory.Id);
            Assert.NotEqual(0, dir2.Id);
            Assert.NotEqual(0, dir2.Directory.Id);
            Assert.NotEqual(0, file1.Id);
            Assert.NotEqual(0, file1.File.Id);
        }

        [Fact]
        public void LoadFile_loads_a_file_instances_file()
        {
            var file = new FileDo() { Id = 0, Name = "file1", NormalizedPath = "/file1".NormalizeCaseInvariant() };
            var fileInstance = new FileInstanceDo(file, DateTime.UtcNow, 23);

            var repo = CreateRepository();
            repo.AddFileInstance(fileInstance);

            var loadedInstance = repo.FileInstancess.Single();
            Assert.Null(loadedInstance.File);

            repo.LoadFile(loadedInstance);

            Assert.NotNull(loadedInstance.File);
            Assert.Equal(file.Name, loadedInstance.File.Name);
            Assert.Equal(file.NormalizedPath, loadedInstance.File.NormalizedPath);

        }

        [Fact]
        public void LoadDirectory_loads_a_directory_instances_directory()
        {
            var directory = new DirectoryDo() { Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() };
            var directoryInstance = new DirectoryInstanceDo(directory);

            var repo = CreateRepository();
            repo.AddRecursively(directoryInstance);


            var loadedInstance = repo.DirectorieInstances.Single();
            Assert.Null(loadedInstance.Directory);

            repo.LoadDirectory(loadedInstance);

            Assert.NotNull(loadedInstance.Directory);
            Assert.Equal(directory.Name, loadedInstance.Directory.Name);
            Assert.Equal(directory.NormalizedPath, loadedInstance.Directory.NormalizedPath);
        }

        [Fact]
        public void LoadDirectories_loads_a_directorys_child_directories()
        {
            // ARRANGE
            var root = new DirectoryInstanceDo(new DirectoryDo() { Name = "Root", NormalizedPath = "/" });
            var dir1 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() });
            var dir11 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir11", NormalizedPath = "/dir1/dir11".NormalizeCaseInvariant() });
            var dir2 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir2", NormalizedPath = "/dir2".NormalizeCaseInvariant() });
            dir1.Directories.Add(dir11);
            root.Directories.Add(dir1);
            root.Directories.Add(dir2);
            var file1 = new FileInstanceDo(new FileDo() { Name = "file1", NormalizedPath = "/dir1/file1".NormalizeCaseInvariant() }, DateTime.Now, 42);
            dir1.Files.Add(file1);

            var repo = CreateRepository();
            repo.AddRecursively(root);

            var loadedRootInstance = repo.GetDirectoryInstance(root.Id);
            Assert.Null(loadedRootInstance.Directories);
            Assert.Null(loadedRootInstance.Files);

            repo.LoadDirectories(loadedRootInstance);
            Assert.NotNull(loadedRootInstance.Directories);
            Assert.Null(loadedRootInstance.Files);

            Assert.Equal(2, loadedRootInstance.Directories.Count);
            Assert.Single(loadedRootInstance.Directories.Where(x => x.Id == dir1.Id));
            Assert.Single(loadedRootInstance.Directories.Where(x => x.Id == dir2.Id));
        }

        [Fact]
        public void LoadFiles_loads_a_directorys_child_directories()
        {
            // ARRANGE
            var root = new DirectoryInstanceDo(new DirectoryDo() { Name = "Root", NormalizedPath = "/" });
            var dir1 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() });
            var dir11 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir11", NormalizedPath = "/dir1/dir11".NormalizeCaseInvariant() });
            var dir2 = new DirectoryInstanceDo(new DirectoryDo() { Name = "dir2", NormalizedPath = "/dir2".NormalizeCaseInvariant() });
            dir1.Directories.Add(dir11);
            root.Directories.Add(dir1);
            root.Directories.Add(dir2);
            var file1 = new FileInstanceDo(new FileDo() { Name = "file1", NormalizedPath = "/dir1/file1".NormalizeCaseInvariant() }, DateTime.Now, 42);
            dir1.Files.Add(file1);

            var repo = CreateRepository();
            repo.AddRecursively(root);

            var loadedInstance = repo.GetDirectoryInstance(dir1.Id);
            Assert.Null(loadedInstance.Directories);
            Assert.Null(loadedInstance.Files);

            repo.LoadFiles(loadedInstance);
            Assert.Null(loadedInstance.Directories);
            Assert.NotNull(loadedInstance.Files);

            Assert.Equal(1, loadedInstance.Files.Count);
            Assert.Single(loadedInstance.Files.Where(x => x.Id == file1.Id));
        }

        [Fact]
        public void GetDirectoryInstance_loads_the_instance_from_the_database()
        {
            var directory = new DirectoryDo() { Name = "dir1", NormalizedPath = "/dir1".NormalizeCaseInvariant() };
            var directoryInstance = new DirectoryInstanceDo(directory);

            var repo = CreateRepository();
            repo.AddRecursively(directoryInstance);

            var loadedInstance = repo.GetDirectoryInstance(directoryInstance.Id);
            Assert.Equal(directoryInstance.Id, loadedInstance.Id);
            Assert.Null(loadedInstance.Directory);
            Assert.Null(loadedInstance.Directories);
            Assert.Null(loadedInstance.Files);
        }        
    }
}
