// ReSharper disable PossibleNullReferenceException

using System;

namespace SyncTool.Git.Common
{
    public class ProcessExecutionException : Exception
    {
        public ProcessExecutionException(string message) : base(message)
        {
        }
    }
}