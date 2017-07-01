using SyncTool.Common.Groups;
using SyncTool.Git.RepositoryAccess;
using SyncTool.Git.RepositoryAccess.Transactions;

namespace SyncTool.Git.Common.Groups
{
    class GitGroupValidator : IGroupValidator
    {
        readonly GroupStorage m_GroupStorage;

        public GitGroupValidator(GroupStorage groupStorage)
        {
            m_GroupStorage = groupStorage;
        }

        public void EnsureGroupIsValid(string groupName, string address)
        {            
            // create a transaction for the repository (this will clone the repository)
            var transaction = new GitTransaction(address, m_GroupStorage.Path);
            try
            {
                transaction.Begin();
            }
            catch (TransactionCloneException ex)
            {
                throw new GroupValidationException($"Error cloning repository for group '{groupName}'", ex);
            }

            // cloning succeeded, now verify that the local directory is actually a repository for a group           
            if (!RepositoryVerifier.IsValid(transaction.LocalPath))
            {
                throw new GroupValidationException($"Repository for group '{groupName}' is not valid");
            }

            transaction.Commit();
        }
    }
}
