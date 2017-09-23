using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SyncTool.Git.Options;

namespace SyncTool.Git.RepositoryAccess
{
    public class WorkingDirectoryFactory
    {
        readonly GitOptions m_GitOptions;

        public WorkingDirectoryFactory([NotNull] GitOptions gitOptions)
        {
            m_GitOptions = gitOptions ?? throw new ArgumentNullException(nameof(gitOptions));
        }

        public TemporaryWorkingDirectory CreateTemporaryWorkingDirectory(string sourceUrl, string branchName) 
            => new TemporaryWorkingDirectory(m_GitOptions, sourceUrl, branchName);
    }
}
