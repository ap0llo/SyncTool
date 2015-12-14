// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Cli.Framework
{
    public struct CommandDescription
    {
        public Type ImplementationType { get; set; }

        public Type OptionType { get; set; }


        public override string ToString()
        {
            return $"{nameof(CommandDescription)}, {nameof(ImplementationType)} = {ImplementationType}, {nameof(OptionType)} = {OptionType}";
        }
    }
}