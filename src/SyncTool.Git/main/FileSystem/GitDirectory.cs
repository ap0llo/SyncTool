using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;

namespace SyncTool.Git.FileSystem
{
    /// <summary>
    /// Wraps a git commit and makes it accessable as <see cref="IDirectory"/> 
    /// </summary>
    public class GitDirectory : InMemoryDirectory
    {
        readonly string m_Name;
        readonly DateTime m_CommitTime;
        readonly Tree m_Tree;

        bool m_Loaded = false;
        


        public GitDirectory(IDirectory parent, string name, Commit commit) : this(parent, name, commit.Author.When.DateTime, commit.Tree)
        {            
        }

        public GitDirectory(IDirectory parent, string name, DateTime commitTime, Tree tree) : base(parent, name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
            m_Name = name;
            m_CommitTime = commitTime;
            m_Tree = tree;                    
            
        }


        void LoadTree()
        {
            lock (this)
            {
                if (m_Loaded)
                {
                    return;
                }                

                foreach (var treeEntry in m_Tree)
                {
                    switch (treeEntry.TargetType)
                    {
                        case TreeEntryTargetType.Blob:

                            var blob = (Blob) treeEntry.Target;                            
                            Add(d => new GitFile(d, treeEntry.Name, m_CommitTime, blob));
                            break;

                        case TreeEntryTargetType.Tree:
                            var subTree = (Tree)treeEntry.Target;                                                    

                            Add(d => new GitDirectory(d, treeEntry.Name, m_CommitTime, subTree));                        
                            break;
                    }
                }

                m_Loaded = true;

            }


                    
        }
        

        public override IEnumerable<IDirectory> Directories
        {
            get
            {
                LoadTree();
                return base.Directories;
            }
        }

        public override IEnumerable<IFile> Files
        {
            get
            {
                LoadTree();
                return base.Files;
            }
        }

        public override IDirectory GetDirectory(string path)
        {
            LoadTree();
            return base.GetDirectory(path);
        }

        public override IFile GetFile(string path)
        {
            LoadTree();
            return base.GetFile(path);
        }

        public override bool FileExists(string path)
        {
            LoadTree();
            return base.FileExists(path);
        }

        public override bool DirectoryExists(string path)
        {
            LoadTree();
            return base.DirectoryExists(path);
        }
    }
}