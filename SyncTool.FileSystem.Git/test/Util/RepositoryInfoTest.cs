// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryInfoTest
    {

        [Fact(DisplayName = nameof(RepositoryInfo) + ".RepositoryName must not be null or empty")]
        public void RepositoryName_must_not_be_null_or_empty()
        {
            Assert.Throws<ArgumentNullException>(() => new RepositoryInfo(null));

            Assert.Throws<ArgumentException>(() => new RepositoryInfo(""));
            Assert.Throws<ArgumentException>(() => new RepositoryInfo("  "));
            Assert.Throws<ArgumentException>(() => new RepositoryInfo(" \t "));
        }



    }
}