// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using Ninject;
using SyncTool.Cli.Framework;

namespace SyncTool.Cli.DI
{
    /// <summary>
    /// Implementation of <see cref="ICommandFactory"/> that uses Ninject to create command instances
    /// </summary>
    public class NinjectCommandFactory : ICommandFactory
    {
        readonly IKernel m_Kernel;


        public NinjectCommandFactory(IKernel kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }
            m_Kernel = kernel;
        }


        public object CreateCommandInstance(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException(nameof(commandType));
            }

            return m_Kernel.Get(commandType);
        }
    }
}