using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SyncTool.Cli.Output
{
    public class ConsoleOutputWriter : IOutputWriter
    {
        protected int LineLength
        {
            get
            {
                try
                {
                    return Console.WindowWidth - 1;
                }
                catch (IOException)
                {
                    return 80;
                }
            }
        }


        public void WriteLine() => Console.WriteLine();

        public void WriteLine(string line, string prefix)
        {
            WriteLines(FormatLine(line, LineLength - prefix.Length).Select(x => prefix + x));
        }

        public void WriteLine(string line) => WriteLine(line, "");

        public void WriteErrorLine(string line)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(line);
            Console.ForegroundColor = color;
        }

        public void WriteHorizontalLine(char c = '-')
        {
            var line = "";
            for (var i = 0; i < LineLength; i++)
            {
                line += c;
            }
            WriteLine(line);
        }

        public void WriteTable(IEnumerable<string> columnNames, IEnumerable<IEnumerable<string>> columns)
        {
            if (columnNames == null)
            {
                throw new ArgumentNullException(nameof(columnNames));
            }
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            WriteTable(columnNames.ToArray(), columns.Select(x => x.ToArray()).ToArray(), LineLength);
        }



        IEnumerable<string> FormatLine(string line, int maxLength)
        {
            if (line.Contains("\r") == false && line.Contains("\n") == false && line.Length <= maxLength)
            {
                return new[] {line};
            }

            var result = new LinkedList<string>();
            var lines = line.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in lines)
            {
                if (entry.Length > maxLength)
                {
                    foreach (var item  in WrapLine(line, maxLength))
                    {
                        result.AddLast(item);
                    }
                }
                else
                {
                    result.AddLast(entry);
                }
            }
            return result;
        }

        IEnumerable<string> WrapLine(string line, int maxLength)
        {
            var lines = new LinkedList<string>();
            var words = line.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var currentLine = "";
            foreach (var word in words)
            {
                if (word.Length > maxLength)
                {
                    lines.AddLast(currentLine);
                    currentLine = "";
                    lines.AddLast(word.Substring(0, maxLength));
                }

                if ((currentLine + " " + word).Length < maxLength)
                {
                    currentLine += " " + word;
                }
                else
                {
                    lines.AddLast(currentLine);
                    currentLine = "";
                }
            }

            return lines;
        }

        void WriteLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }


        void WriteTable(string[] columnNames, string[][] columns, int lineLength)
        {
            // check that the number of colum names matches the number of columns
            if (columnNames.Count() != columns.Count())
            {
                throw new ArgumentNullException($"The number of columens in {nameof(columnNames)} does not match the number of columns in {nameof(columns)}");
            }

            // check that the number of rows is equal for all columns
            if (columns.Any() && columns.GroupBy(col => col.Length).Count() != 1)
            {
                throw new ArgumentException($"The number of rows must be the same for all columns");
            }

            var columnCount = columnNames.Length;

            // no columns => nothing to do
            if (columnCount == 0)
            {
                return;
            }

            var rowCount = columns[0].Length;

            // determine widths of columns
            var columnWidths = GetColumnWidths(columnNames, columns, lineLength);

            // draw header row
            DrawRow(columnNames, columnWidths);
            WriteHorizontalLine('=');

            // convert columns to rows so we can iterate over them
            var rows = Enumerable.Range(0, rowCount)
                .Select(i => columns.Select(col => col[i]));
            // draw table contents
            foreach (var row in rows)
            {
                DrawRow(row.ToArray(), columnWidths);
                WriteHorizontalLine();
            }
        }

        void DrawRow(string[] row, int[] columnWidths)
        {
            // split each column values into multiple lines if the content is wider than the colum    
            var cells = new string[row.Length][];
            for (var i = 0; i < cells.Length; i++)
            {
                cells[i] = FormatLine(row[i], columnWidths[i]).ToArray();
            }


            //build a format string for the line in the format " {0,5} | {1,10} ..."            
            var formatString = Enumerable.Range(0, row.Length)
                .Select(j => $" {{{j},-{columnWidths[j]}}} ")
                .Aggregate((a, b) => $"{a}|{b}");

            // draw each ot the lines that makes up this column
            for (var lineIndex = 0; lineIndex < cells.Max(x => x.Length); lineIndex++)
            {
                var values = new string[row.Length];
                for (var columnIndex = 0; columnIndex < values.Length; columnIndex++)
                {
                    if (lineIndex >= cells[columnIndex].Length)
                    {
                        // this column does not have as many lines => fill with empty string
                        values[columnIndex] = "";
                    }
                    else
                    {
                        values[columnIndex] = cells[columnIndex][lineIndex];
                    }
                }

                WriteLine(String.Format(formatString, values));
            }
        }

        int[] GetColumnWidths(string[] columnNames, string[][] columns, int maxLineLength)
        {
            var columnWidths = new int[columnNames.Length];
            // get the columns width based on their content
            for (var i = 0; i < columnNames.Length; i++)
            {
                columnWidths[i] = Math.Max(columnNames[i].Length, columns[i].Max(x => x.Length));
            }

            // scale the widths so they fit into the specified line length
            return ScaleColumnWidths(columnWidths, maxLineLength);
        }

        int[] ScaleColumnWidths(int[] columnWidths, int maxLineLength)
        {
            // adjust the line length we're working with :
            // there is a separator char between each column
            // and a space before and after the content of a cell
            var adjustedLineLength = maxLineLength - (columnWidths.Length - 1) - (columnWidths.Length*2);

            // get the complete length an unscaled row would need
            var sum = (double) columnWidths.Sum();

            // columns fit into the required length => no need to scale
            if (sum < adjustedLineLength)
            {
                return columnWidths;
            }

            // calculate scaled widths
            var result = new int[columnWidths.Length];
            for (var i = 0; i < columnWidths.Length; i++)
            {
                var percentage = columnWidths[i]/sum;
                result[i] = (int) (percentage*adjustedLineLength);
            }

            return result;
        }
    }
}