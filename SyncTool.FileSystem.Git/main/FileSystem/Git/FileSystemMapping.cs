using System.Collections.Generic;
using SyncTool.Common.Utilities;

namespace SyncTool.FileSystem.Git
{
    public class FileSystemMapping : IFileSystemMapping
    {
        readonly IReversibleDictionary<IDirectory, IDirectory> m_MappedDirectories = new ReversibleDictionary<IDirectory, IDirectory>(); 
        readonly IReversibleDictionary<IFile, IFile> m_MappedFiles = new ReversibleDictionary<IFile, IFile>();


        public IFile GetMappedFile(IFile originalFile) => m_MappedFiles[originalFile];
        
        public IFile GetOriginalFile(IFile mappedFile) => m_MappedFiles.ReversedDictionary[mappedFile];

        public IDirectory GetMappedDirectory(IDirectory originalDirectory) => m_MappedDirectories[originalDirectory];

        public IDirectory GetOriginalDirectory(IDirectory mappedDirectory) => m_MappedDirectories.ReversedDictionary[mappedDirectory];



        public void AddMapping(IDirectory originalValue, IDirectory mappedValue) => m_MappedDirectories.Add(originalValue, mappedValue);

        public void AddMapping(IFile originalValue, IFile mappedValue) => m_MappedFiles.Add(originalValue, mappedValue);
    }
}