using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    /// <summary>
    /// Wraps a git commit and makes it accessable as <see cref="IDirectory"/> 
    /// </summary>
    public class GitDirectory : AbstractDirectory
    {
        readonly DateTime m_CommitTime;
        readonly Tree m_Tree;
        
        

        public GitDirectory(string rootDirectoryName, Commit commit) : this(rootDirectoryName, commit.Author.When.DateTime, commit.Tree)
        {            
        }

        public GitDirectory(string name, DateTime commitTime, Tree tree) : base(name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
            m_CommitTime = commitTime;
            m_Tree = tree;                    
            LoadTree();
        }


        void LoadTree()
        {            
            foreach (var treeEntry in m_Tree)
            {
                switch (treeEntry.TargetType)
                {
                    case TreeEntryTargetType.Blob:

                        var blob = (Blob) treeEntry.Target;
                        var file = new GitFile(treeEntry.Name, m_CommitTime, blob);                        
                        m_Files.Add(file.Name, file);
                        break;

                    case TreeEntryTargetType.Tree:
                        var subTree = (Tree)treeEntry.Target;                        
                        var subDirectory = new GitDirectory(treeEntry.Name, m_CommitTime, subTree);

                        m_Directories.Add(subDirectory.Name, subDirectory);                        
                        break;
                }
            }



                    
        }

    }
}