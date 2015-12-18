// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Cli.Output
{
    public interface IOutputWriter
    {
        void WriteLine(string line);

        void WriteErrorLine(string line);
    }
}