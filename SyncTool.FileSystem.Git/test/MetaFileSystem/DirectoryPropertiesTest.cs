using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class DirectoryPropertiesTest
    {
        
        [Fact]
        public void Equals_Returns_true_if_name_is_equal()
        {
            var instance1 = new DirectoryProperties() {Name= "Directory" };
            var instance2 = new DirectoryProperties() {Name= "Directory" };

            Assert.Equal(instance1, instance2);
        }

        [Fact]
        public void Equals_treats_the_name_case_invariant()
        {
            var instance1 = new DirectoryProperties() { Name = "Directory" };
            var instance2 = new DirectoryProperties() { Name = "DiRecTory" };

            Assert.Equal(instance1, instance2);
        }

        [Fact]
        public void GetHashCode_returns_the_same_value_is_name_is_equal()
        {
            var instance1 = new DirectoryProperties() { Name = "Directory" };
            var instance2 = new DirectoryProperties() { Name = "DiRecTory" };
            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
        }
    }
}