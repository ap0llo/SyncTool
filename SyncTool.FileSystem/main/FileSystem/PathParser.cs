// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Linq;

namespace SyncTool.FileSystem
{
    public static class PathParser
    {

        public static string GetFileName(string path)
        {
            PathValidator.EnsureIsValidDirectoryPath(path);
            
            if (path.Contains(Constants.DirectorySeparatorChar))
            {
                // remove the first name from the path (that's the name of this directory's child directory -> 'localName')
                var index = path.LastIndexOf(Constants.DirectorySeparatorChar);
                return path.Substring(index + 1);
            }
            else
            {
                return path;
            }
        }


        public static string GetDirectoryName(string path)
        {
            PathValidator.EnsureIsValidDirectoryPath(path);

            if (path.Contains(Constants.DirectorySeparatorChar))
            {                
                var index = path.LastIndexOf(Constants.DirectorySeparatorChar);
                
                if (index == 0)
                {
                    return path.Substring(0, index + 1);
                }
                else
                {
                    return path.Substring(0, index);
                }
                
            }
            else
            {
                return "";
            }
        }




    }
}