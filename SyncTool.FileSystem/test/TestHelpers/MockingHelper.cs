// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using Moq;

namespace SyncTool.FileSystem.TestHelpers
{
    public static class MockingHelper
    {
        public static IFile GetMockedFile() => GetFileMock().Object;

        public static IFile GetMockedFile(string path) => GetFileMock(path).Object;

        public static IFile GetMockedFile(string path, DateTime lastWriteTime) => GetFileMock(path, lastWriteTime).Object;

        public static IFile GetMockedFile(string path, DateTime lastWriteTime, long length) => GetFileMock(path, lastWriteTime, length).Object;


        public static Mock<IFile> GetFileMock()
        {
            var mock = new Mock<IFile>(MockBehavior.Strict);
            mock.Setup(m => m.WithParent(It.IsAny<IDirectory>())).Returns(mock.Object);
            return mock;
        }

        public static Mock<IFile> GetFileMock(string path)
        {
            var mock = GetFileMock();
            mock.Setup(m => m.Path).Returns(path);            
            return mock;
        }

        public static Mock<IFile> GetFileMock(string path, DateTime lastWriteTime)
        {
            var mock = GetFileMock(path);            
            mock.Setup(m => m.LastWriteTime).Returns(lastWriteTime);
            return mock;
        }

        public static Mock<IFile> GetFileMock(string path, DateTime lastWriteTime, long length)
        {
            var mock = GetFileMock(path, lastWriteTime);
            mock.Setup(m => m.Length).Returns(length);
            return mock;
        }

        public static Mock<IFile> Named(this Mock<IFile> mock, string name)
        {
            mock.Setup(m => m.Name).Returns(name);
            return mock;
        }

        public static Mock<IFile> WithLength(this Mock<IFile> mock, long length)
        {
            mock.Setup(m => m.Length).Returns(length);
            return mock;
        }

        public static Mock<IFile> WithLastWriteTime(this Mock<IFile> mock, DateTime lastWriteTime)
        {
            mock.Setup(m => m.LastWriteTime).Returns(lastWriteTime);
            return mock;
        }

        public static Mock<IFile> WithParentNamed(this Mock<IFile> mock, string name)
        {
            var dirMock = new Mock<IDirectory>();
            dirMock.Setup(m => m.Name).Returns(name);
            mock.Setup(m => m.Parent).Returns(dirMock.Object);
            mock.Setup(m => m.Path).Returns(name + "/" + mock.Object.Name);
            return mock;
        }

    }



    
}