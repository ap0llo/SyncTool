// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using Xunit;

namespace SyncTool.Git.Common
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
        [InlineData("v0")]
        [InlineData("v1256")]        
        public void Constructor_throws_ArgumentException_for_invalid_Names(string name)
        {
            Assert.Throws<ArgumentException>(() => new BranchName(null, name, 0));
        }

        [Theory]
        [InlineData("name/v0", "", "name", 0)]        
        [InlineData("prefix/name/v0", "prefix", "name", 0)]
        [InlineData("prefix/name/v23", "prefix", "name", 23)]
        [InlineData("name/v42", "", "name", 42)]
        [InlineData("name/V42", "", "name", 42)]
        [InlineData("prefix/morePrefix/name/v23", "prefix/morePrefix", "name", 23)]
        [InlineData("master", "", "master", 0)]
        public void Parse_returns_expected_result(string toParse, string expectedPrefix, string expectedName, int expectedVersion)
        {
            var parsed = BranchName.Parse(toParse);

            Assert.Equal(expectedPrefix, parsed.Prefix);
            Assert.Equal(expectedName, parsed.Name);
            Assert.Equal(expectedVersion, parsed.Version);
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
        [InlineData("v0")]
        [InlineData("v1256")]
        [InlineData("name")]
        [InlineData("prefix/name")]
        public void Parse_throws_FormatException_for_invalid_values(string toParse)
        {
            Assert.Throws<FormatException>(() => BranchName.Parse(toParse));
        }


        [Theory]
        [InlineData("name/v0", "", "name", 0)]
        [InlineData("prefix/name/v0", "prefix", "name", 0)]        
        [InlineData("prefix/name/v23", "prefix", "name", 23)]
        [InlineData("name/v42", "", "name", 42)]        
        [InlineData("prefix/morePrefix/name/v23", "prefix/morePrefix", "name", 23)]
        public void ToString_returns_expected_result(string expectedResult, string prefix, string name, int version)
        {
            var branchName = new BranchName(prefix, name, version);
            Assert.Equal(expectedResult, branchName.ToString());            
        }


        [Theory]
        [InlineData("name1/v0", "name1/v0", true)]
        [InlineData("name1/v0", "nAMe1/v0", true)]
        [InlineData("name1/v0", "nAMe1/V0", true)]
        [InlineData("prefix/name1/v0", "prefix/name1/v0", true)]
        [InlineData("prefix/name1/v0", "PREFIX/name1/v0", true)]        
        [InlineData("name1/v0", "PREFIX/name1/v0",false)]
        [InlineData("name1/v0", "PREFIX/name1/v0",false)]
        [InlineData("name2/v0", "name1/v0",false)]
        [InlineData("name2/v0", "prefix/name2/v0",false)]
        [InlineData("prefix/name2/v0", "prefix2/name2/v0",false)]
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