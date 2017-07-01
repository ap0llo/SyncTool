using System;
using SyncTool.Git.RepositoryAccess;
using Xunit;

namespace SyncTool.Git.Test.RepositoryAccess
{
    /// <summary>
    /// Tests for <see cref="BranchName"/>
    /// </summary>
    public class BranchNameTest
    {        
        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData(null)]              
        public void Constructor_throws_ArgumentException_for_invalid_Names(string name)
        {
            Assert.Throws<ArgumentException>(() => new BranchName(null, name));
        }

        [Theory]
        [InlineData("name", "", "name")]        
        [InlineData("prefix/name", "prefix", "name")]                       
        [InlineData("prefix/morePrefix/name", "prefix/morePrefix", "name")]
        [InlineData("master", "", "master")]
        public void Parse_returns_expected_result(string toParse, string expectedPrefix, string expectedName)
        {
            var parsed = BranchName.Parse(toParse);

            Assert.Equal(expectedPrefix, parsed.Prefix);
            Assert.Equal(expectedName, parsed.Name);
        }


        [Fact]
        public void Parse_accepts_master_as_valid_branch_name()
        {
            var expected = BranchName.Master;
            var actual = BranchName.Parse("master");

            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData(null)]        
        public void Parse_throws_FormatException_for_invalid_values(string toParse)
        {
            Assert.Throws<FormatException>(() => BranchName.Parse(toParse));
        }


        [Theory]
        [InlineData("name", "", "name")]
        [InlineData("prefix/name", "prefix", "name")]        
        [InlineData("prefix/name", "prefix", "name")]
        [InlineData("name", "", "name")]        
        [InlineData("prefix/morePrefix/name", "prefix/morePrefix", "name")]
        public void ToString_returns_expected_result(string expectedResult, string prefix, string name)
        {
            var branchName = new BranchName(prefix, name);
            Assert.Equal(expectedResult, branchName.ToString());            
        }


        [Theory]
        [InlineData("name1", "name1", true)]
        [InlineData("name1", "nAMe1", true)]        
        [InlineData("prefix/name1", "prefix/name1", true)]
        [InlineData("prefix/name1", "PREFIX/name1", true)]        
        [InlineData("name1", "PREFIX/name1",false)]
        [InlineData("name1", "PREFIX/name1",false)]
        [InlineData("name2", "name1",false)]
        [InlineData("name2", "prefix/name2",false)]
        [InlineData("prefix/name2", "prefix2/name2",false)]
        public void Equals_compares_case_invariant(string name1, string name2, bool areEqual)
        {
            var branchName1 = BranchName.Parse(name1);
            var branchName2 = BranchName.Parse(name2);

            if (areEqual)
            {
                Assert.Equal(branchName1, branchName2);
                Assert.Equal(branchName1?.GetHashCode(), branchName2?.GetHashCode());                
            }
            else
            {
                Assert.NotEqual(branchName1, branchName2);
                Assert.NotEqual(branchName1?.GetHashCode(), branchName2?.GetHashCode());
            }

        }
    }
}