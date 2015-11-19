// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
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
        


        public GitDirectory(string name, Commit commit) : this(name, commit.Author.When.DateTime, commit.Tree)
        {            
        }

        public GitDirectory(string name, DateTime commitTime, Tree tree) : base(name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
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
                            var file = new GitFile(treeEntry.Name, m_CommitTime, blob);                        
                            Add(file);
                            break;

                        case TreeEntryTargetType.Tree:
                            var subTree = (Tree)treeEntry.Target;                        
                            var subDirectory = new GitDirectory(treeEntry.Name, m_CommitTime, subTree);

                            Add(subDirectory);                        
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