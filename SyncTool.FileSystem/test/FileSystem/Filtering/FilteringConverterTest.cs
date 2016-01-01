// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using Moq;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Filtering
{
    public class FilteringConverterTest
    {
        const string s_File1 = "file1";
        const string s_File2 = "file2";
           


        [Fact(DisplayName = nameof(FilteringConverter) + "Convert() removes files from result")]
        public void Convert_removes_files_from_result()
        {            
            var directory = new Directory(null, "root")
            {
                root => new File(root, s_File1),
                root => new File(root, s_File2)
            };

            var filter = new Mock<IFileSystemFilter>(MockBehavior.Strict);
            filter.Setup(f => f.Applies(It.Is<IFileSystemItem>(item => item.Name == s_File1))).Returns(true);
            filter.Setup(f => f.Applies(It.Is<IFileSystemItem>(item => item.Name == s_File2))).Returns(false);

            var converter = new FilteringConverter(filter.Object);

            var filteredDirectory = converter.Convert(directory);

            Assert.Single(filteredDirectory.Files);
            Assert.Empty(filteredDirectory.Directories);

            Assert.True(directory.FileExists(s_File2));            
        }



        [Fact(DisplayName = nameof(FilteringConverter) + "Convert() correctly sets references between parent and child directories")]
        public void Convert_correctly_sets_references_between_parent_and_child_directories()
        {
            var directory = new Directory(null, "root")
            {
                root => new File(root, s_File1),
                root => new Directory(root, "dir1")
            };

            // filter nothing 
            var filter = new Mock<IFileSystemFilter>(MockBehavior.Strict);
            filter.Setup(f => f.Applies(It.IsAny<IFileSystemItem>())).Returns(false);
            

            var converter = new FilteringConverter(filter.Object);

            var filteredDirectory = converter.Convert(directory);


            FileSystemAssert.DirectoryEqual(directory, filteredDirectory);

            Assert.Null(filteredDirectory.Parent);
            Assert.Equal(directory, directory.GetFile(s_File1).Parent);
            Assert.Equal(directory, directory.GetDirectory("dir1").Parent);
        }



    }
}