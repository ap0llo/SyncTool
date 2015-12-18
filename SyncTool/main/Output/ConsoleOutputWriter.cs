// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Cli.Output
{
    public class ConsoleOutputWriter : IOutputWriter
    {
        public void WriteLine(string line) => Console.WriteLine(line);

        public void WriteErrorLine(string line)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(line);
            Console.ForegroundColor = color;
        }
    }
}