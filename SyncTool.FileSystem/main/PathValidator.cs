// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using SyncTool.FileSystem;

namespace SyncTool
{
    internal static class PathValidator
    {
        internal static void EnsurePathIsValid(string path)
        {
            // path must not be null
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // path must not be empty or whitespace
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new FormatException($"'{nameof(path)}' must not be null or empty");
            }

            // path must not start with a slash
            if (path[0] == Constants.DirectorySeparatorChar)
            {
                throw new FormatException($"'{nameof(path)}' must not start with '{Constants.DirectorySeparatorChar}'");
            }

            // path must not end with a slash
            if (path[path.Length - 1] == Constants.DirectorySeparatorChar)
            {
                throw new FormatException($"'{nameof(path)}' must not end with '{Constants.DirectorySeparatorChar}'");
            }

            // path must not contain any forbidden characters
            if (Constants.InvalidPathCharacters.Any(path.Contains))
            {
                throw new FormatException("The path contains invalid characters");
            }

        }
    }
}