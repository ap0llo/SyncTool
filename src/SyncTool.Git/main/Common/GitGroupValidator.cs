using SyncTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Git.Common
{
    class GitGroupValidator : IGroupValidator
    {
        readonly GroupStorage m_GroupStorage;

        public GitGroupValidator(GroupStorage groupStorage)
        {
            this.m_GroupStorage = groupStorage;
        }

        public void EnsureGroupIsValid(string groupName, string address)
        {            
            // create a transaction for the repository (this will clone the repository)
            var transaction = new GitTransaction(address, m_GroupStorage.Path);
            try
            {
                transaction.Begin();
            }
            catch (TransactionCloneException ex) when (ex is TransactionCloneException || ex is GitTransactionException)
            {
                throw new ValidationException($"Error cloning repository for group '{groupName}'", ex);
            }

            // cloning succeeded, now verify that the local directory is actually a repository for a group           
            if (!RepositoryVerifier.IsValid(transaction.LocalPath))
            {
                throw new ValidationException($"Repository for group '{groupName}' is not valid");
            }

            transaction.Commit();
        }
    }
}
