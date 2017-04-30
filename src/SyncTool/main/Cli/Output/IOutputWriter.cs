using System;
using System.Collections.Generic;

namespace SyncTool.Cli.Output
{
    public interface IOutputWriter
    {
        void WriteLine();

        void WriteLine(string line);

        void WriteLine(string line, string prefix);        

        void WriteErrorLine(string line);        

        void WriteHorizontalLine(char c = '-');

        /// <summary>
        /// Writes the specified table to the output
        /// </summary>
        /// <param name="columnNames">The column headers of the table</param>
        /// <param name="columns">The table encoded as a list of column values (e.g. first item in the <paramref name="columns"/> cref="columns"/> enumerable contains all values of the first column)</param> 
        /// <exception cref="ArgumentException">The number of columns in <paramref name="columnNames"/> does not match the number of columns in <paramref name="columns"/></exception>
        /// <exception cref="ArgumentException">The number of elements differs between rows</exception>
        /// <exception cref="ArgumentNullException"><paramref name="columnNames"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramred name="columns"/> is null</exception>
        void WriteTable(IEnumerable<string> columnNames, IEnumerable<IEnumerable<string>> columns);
    }
}