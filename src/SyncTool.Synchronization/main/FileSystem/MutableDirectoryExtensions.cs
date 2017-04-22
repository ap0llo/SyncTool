using System;

namespace SyncTool.FileSystem
{
    internal static class MutableDirectoryExtensions
    {

         public static void EnsureDirectoryExists(this MutableDirectory directory, string path)
         {
             if (directory.DirectoryExists(path))
             {
                 return;               
             }

             var parts = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
             foreach (var part in parts)
             {
                 if (!directory.DirectoryExists(part))
                 {
                     directory.Add(d => new MutableDirectory(d, part));
                 }
                 directory = directory.GetDirectory(part);
             }            
         }
 

    }
}