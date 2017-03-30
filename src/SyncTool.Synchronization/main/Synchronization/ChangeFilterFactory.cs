// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.Configuration.Model;

namespace SyncTool.Synchronization
{
    public class ChangeFilterFactory : IChangeFilterFactory
    {
        EmptyChangeFilter m_EmptyChangeFilter;


        public IChangeFilter GetFilter(FilterConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            switch (configuration.Type)
            {
                case FilterType.None:
                    if (m_EmptyChangeFilter == null)
                    {
                        m_EmptyChangeFilter = new EmptyChangeFilter();
                    }
                    return m_EmptyChangeFilter;                    

                case FilterType.MicroscopeQuery:
                    return new MicroscopeChangeFilter(configuration.CustomData);                    

                default:
                    throw new NotImplementedException();
            }
        }
    }
}