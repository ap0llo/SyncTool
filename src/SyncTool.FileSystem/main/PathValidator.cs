﻿using System;
using System.Linq;
using SyncTool.Utilities;

namespace SyncTool.FileSystem
{
    //TODO: Rename "Ensure.." methods to "Assert..."
    public static class PathValidator
    {
        public static void EnsureIsValidDirectoryPath(string path) => EnsureIsValidPath(path);

        public static void EnsureIsValidFilePath(string path)
        {
            EnsureIsValidPath(path);

            // "/" is allowed for directory paths, but not for file paths
            if(path.TrimStart(Constants.DirectorySeparatorChar) == "")
            {
                throw new FormatException($"'{path}' is not a valid file path");
            }
        }

        public static void EnsureIsRootedPath(string path)
        {
            if (!path.StartsWith(Constants.DirectorySeparatorChar))
            {
                throw new FormatException($"'{path}' is not rooted");
            }
        }


        static void EnsureIsValidPath(string path)
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

            // path must not end with a slash (but may start with one)
            if (path.TrimStart(Constants.DirectorySeparatorChar) != "" && path.EndsWith(Constants.DirectorySeparatorChar))
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