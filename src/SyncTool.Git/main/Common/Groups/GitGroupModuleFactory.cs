using System;
using System.Collections.Generic;
using Autofac;
using SyncTool.Common.Groups;
using SyncTool.Git.DI;

namespace SyncTool.Git.Common.Groups
{
    class GitGroupModuleFactory : IGroupModuleFactory
    {
        static readonly HashSet<string> s_SupportedSchemes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "file",
            "http",
            "https"
        };

        public bool IsAddressSupported(string address)
        {
            try
            {
                var uri = new Uri(address);
                return s_SupportedSchemes.Contains(uri.Scheme);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public Module CreateModule() => new GitModule();
    }
}
