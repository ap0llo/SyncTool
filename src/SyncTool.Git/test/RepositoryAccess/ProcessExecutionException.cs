// ReSharper disable PossibleNullReferenceException

using System;

namespace SyncTool.Git.Test.RepositoryAccess
{
    public class ProcessExecutionException : Exception
    {
        public ProcessExecutionException(string message) : base(message)
        {
        }
    }
}