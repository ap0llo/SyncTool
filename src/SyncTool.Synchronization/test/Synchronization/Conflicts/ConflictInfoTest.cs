// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using Xunit;

namespace SyncTool.Synchronization.Conflicts
{
    public class ConflictInfoTest
    {


        [Fact]
        public void T01_Constructor_validates_file_path()
        {
            Assert.Throws<ArgumentNullException>(() => new ConflictInfo(null, null));
            Assert.Throws<FormatException>(() => new ConflictInfo("", null));
            Assert.Throws<FormatException>(() => new ConflictInfo(" ", null));
            Assert.Throws<FormatException>(() => new ConflictInfo("/", null));
            Assert.Throws<FormatException>(() => new ConflictInfo("fileName", null));
            Assert.Throws<FormatException>(() => new ConflictInfo("relative/path", null));
        }

    }
}