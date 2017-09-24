using System;
using System.IO;

namespace SyncTool.Git.Options
{
    public sealed class GitOptions
    {
        string m_TempPath;


        public string TempPath
        {
            get => m_TempPath;
            set => m_TempPath = Environment.ExpandEnvironmentVariables(value);
        } 


        public GitOptions() => TempPath = Path.GetTempPath(); 
    }
}