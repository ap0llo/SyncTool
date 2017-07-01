using System;
using Xunit;
using SyncTool.FileSystem.Versioning.MetaFileSystem;

namespace SyncTool.FileSystem.Versioning.Test.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="FileProperties"/>
    /// </summary>
    public class FilePropertiesTest
    {
        readonly FileProperties m_Instance1;
        readonly FileProperties m_Instance2;
        readonly FileProperties m_Instance3;


        public FilePropertiesTest()
        {
            var name = "file";
            var lastWriteTime = DateTime.Now;
            var length = 1234;

            m_Instance1 = new FileProperties() { Name = name, LastWriteTime = lastWriteTime, Length = length };
            m_Instance2 = new FileProperties() { Name = name, LastWriteTime = lastWriteTime, Length = length };
            m_Instance3 = new FileProperties() { Name = name.ToUpper(), LastWriteTime = lastWriteTime, Length = length };
        }


        [Fact]
        public void Equals_Returns_true_if_all_properties_are_equal()
        {            
            Assert.Equal(m_Instance1, m_Instance2);
        }

        [Fact]
        public void Equals_treats_the_name_case_invariant()
        {
            Assert.Equal(m_Instance2, m_Instance3);
        }

        [Fact]
        public void GetHashCode_returns_the_same_value_is_name_is_equal()
        {
            var file1 = new FileProperties() {Name = "file", LastWriteTime = DateTime.Now, Length = 1234};
            var file2 = new FileProperties() { Name = "fILe", LastWriteTime = DateTime.MinValue, Length = 5678 };
            Assert.Equal(file1.GetHashCode(), file2.GetHashCode());
        }
    }
}