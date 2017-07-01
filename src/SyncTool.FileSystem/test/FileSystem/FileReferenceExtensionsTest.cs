using System;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem
{
    public class FileReferenceExtensionsTest
    {
        readonly IFile m_File1;
        readonly IFile m_File2;
        readonly IFile m_File3;
        private readonly DateTime m_LastWriteTime2;
        private long m_Length3 = 42;

        public FileReferenceExtensionsTest()
        {
            m_LastWriteTime2 = DateTime.Now.AddHours(1);

            m_File1 = FileMockingHelper.GetFileMock()
                .Named("file1")
                .WithParentNamed("dir1")
                .Object;

            m_File2 = FileMockingHelper.GetFileMock()
                .Named("file1")
                .WithParentNamed("dir1")
                .WithLastWriteTime(m_LastWriteTime2)
                .Object;
            m_File3 = FileMockingHelper.GetFileMock()
                .Named("file1")
                .WithParentNamed("dir1")
                .WithLength(m_Length3)
                .Object;
        }



        [Fact]
        public void Matches_matches_file_only_by_path()
        {
            var reference = new FileReference("/dir1/file1");
            Assert.True(reference.Matches(m_File1));
        }

        [Fact]
        public void Matches_matches_path_case_invariant()
        {
            var reference = new FileReference("/dIR1/fiLe1");
            Assert.True(reference.Matches(m_File1));
        }

        [Fact]
        public void Matches_does_not_match_file_when_only_a_path_is_specified()
        {
            var reference = new FileReference("/dir2/file1");
            Assert.False(reference.Matches(m_File1));
        }


        [Fact]
        public void Matches_checks_LastWriteTime_if_specified_in_reference()
        {
            var reference1 = new FileReference("/dir1/file1", m_LastWriteTime2);
            var reference2 = new FileReference("/dir1/file1", m_LastWriteTime2.AddMinutes(1));
            Assert.True(reference1.Matches(m_File2));
            Assert.False(reference2.Matches(m_File2));
        }


        [Fact]
        public void Matches_checks_Length_if_specified_in_reference()
        {
            var reference1 = new FileReference("/dir1/file1", null, m_Length3);
            var reference2 = new FileReference("/dir1/file1", null, m_Length3 + 1);
            Assert.True(reference1.Matches(m_File3));
            Assert.False(reference2.Matches(m_File3));
        }
    }
}