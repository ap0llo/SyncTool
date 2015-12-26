// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace SyncTool.FileSystem
{
    public static class FileExtensions
    {
        static readonly char[] s_TrimChars = " .".ToCharArray();


        public static string GetExtension(this IFile file) => Path.GetExtension(file.Name);

        public static bool HasExtensions(this IFile file, string extension)
        {
            return file.GetExtension().TrimStart(s_TrimChars).Equals(extension.TrimStart(s_TrimChars), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}