using Xunit;

namespace SyncTool.Git.FileSystem.Versioning.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="DirectoryProperties"/>
    /// </summary>
    public class DirectoryPropertiesTest
    {
        
        [Fact(DisplayName = "DirectoryProperties.Equals() Returns true if name is equal")]
        public void Equals_Returns_true_if_name_is_equal()
        {
            var instance1 = new DirectoryProperties() {Name= "Directory" };
            var instance2 = new DirectoryProperties() {Name= "Directory" };

            Assert.Equal(instance1, instance2);
        }

        [Fact(DisplayName = "DirectoryProperties.Equals() treats the name case invariant")]
        public void Equals_treats_the_name_case_invariant()
        {
            var instance1 = new DirectoryProperties() { Name = "Directory" };
            var instance2 = new DirectoryProperties() { Name = "DiRecTory" };

            Assert.Equal(instance1, instance2);
        }

        [Fact(DisplayName = "DirectoryProperties.GetHashCode() returns the same value is name is equal")]
        public void GetHashCode_returns_the_same_value_is_name_is_equal()
        {
            var instance1 = new DirectoryProperties() { Name = "Directory" };
            var instance2 = new DirectoryProperties() { Name = "DiRecTory" };
            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
        }

    }
}