// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Configuration.Model;
using Xunit;

namespace SyncTool.Configuration.Configuration.Model
{
    /// <summary>
    /// Tests for <see cref="SyncFolder"/>
    /// </summary>
    public class SyncFolderTest
    {         
        [Fact]
        public void Filter_returns_the_default_filter_configuration_instead_of_null()
        {            
            var syncFolder = new SyncFolder("folder1");
            Assert.Equal(FilterConfiguration.Empty, syncFolder.Filter);            

            var syncFolder2 = new SyncFolder("folder2") { Filter =  null };
            Assert.Equal(FilterConfiguration.Empty, syncFolder2.Filter);
        }

    }
}