using System;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git.Test
{
    public class SignatureHelper
    {
      
        public static Signature NewSignature() => new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);
    }
}