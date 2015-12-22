// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using Moq;

namespace SyncTool.FileSystem.TestHelpers
{
    public static class MockingHelper
    {
        public static Mock<IFile> GetFileMock() => new Mock<IFile>();        

        public static IFile GetMockedFile() => GetFileMock().Object;

        public static IFile GetMockedFile(string path) => GetFileMock(path).Object;

        public static Mock<IFile> GetFileMock(string path)
        {
            var mock = new Mock<IFile>(MockBehavior.Strict);
            mock.Setup(m => m.Path).Returns(path);
            return mock;
        }
    }
}