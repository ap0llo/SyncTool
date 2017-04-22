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