using System;
using LibGit2Sharp;

namespace SyncTool.Git.Common
{
    public class SignatureHelper
    {      
        //TODO
        public static Signature NewSignature() => new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);
    }
}