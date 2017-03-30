// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using Microscope;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    public class MicroscopeChangeFilter : IChangeFilter, IEquatable<MicroscopeChangeFilter>
    {
        readonly string m_Query;
        readonly QueryEvaluator m_Evaluator;


        public MicroscopeChangeFilter(string query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            m_Query = query;
            m_Evaluator = new QueryEvaluator(m_Query);
        }


        public override int GetHashCode() => StringComparer.InvariantCulture.GetHashCode(m_Query);

        public override bool Equals(object obj) => Equals(obj as IChangeFilter);

        public bool Equals(IChangeFilter other) => Equals(other as MicroscopeChangeFilter);

        public bool Equals(MicroscopeChangeFilter other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.InvariantCulture.Equals(m_Query, other.m_Query);
        }

        public bool IncludeInResult(IChange change) => m_Evaluator.Evaluate(change.Path);

        public bool IncludeInResult(IChangeList changeList) => m_Evaluator.Evaluate(changeList.Path);
    }
}