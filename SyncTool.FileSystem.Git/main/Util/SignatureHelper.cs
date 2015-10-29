using System;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public class SignatureHelper
    {      
        //TODO
        public static Signature NewSignature() => new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);
    }
}