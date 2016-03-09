// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SyncTool.Git.Common
{
    public sealed class BranchName : IEquatable<BranchName>
    {
        static readonly Regex s_VersionRegex = new Regex("^v[0-9]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);


        public static readonly BranchName Master = new BranchName("master");


        public string Prefix { get; }

        public string Name { get; }

        public int Version { get; }


        public BranchName(string name) : this("", name, 0)
        {
            
        }

        public BranchName(string prefix, string name) : this(prefix, name, 0)
        {

        }

        public BranchName(string prefix, string name, int version)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty", nameof(name));
            }

            if (s_VersionRegex.IsMatch(name))
            {
                throw new ArgumentException($"'{name}' cannot be used as name because it could conflict with a version name", nameof(name));
            }

            Prefix = prefix?.Trim() ?? "";
            Name = name.Trim();
            Version = version;
        }



        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (!String.IsNullOrEmpty(Prefix))
            {
                stringBuilder.Append(Prefix);
                stringBuilder.Append("/");
            }

            stringBuilder.Append(Name);

            stringBuilder.Append("/v");
            stringBuilder.Append(Version);
            return stringBuilder.ToString();
        }

        public static BranchName Parse(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new FormatException(nameof(name) + " cannot be empty");
            }

            if (StringComparer.InvariantCultureIgnoreCase.Equals(name, "master"))
            {
                return Master;
            }


            var fragments = name.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            switch (fragments.Length)
            {
                case 0:
                    // should not be possible
                    throw new InvalidOperationException();

                case 1:
                    if (s_VersionRegex.IsMatch(fragments.Single()))
                    {
                        throw new FormatException($"Invalid branch name '{name}'. Name is missing");
                    }
                    else
                    {
                        throw new FormatException($"Invalid branch name '{name}'. Version is missing");
                    }

                case 2:
                    if (!s_VersionRegex.IsMatch(fragments.Last()))
                    {
                        throw new FormatException($"Invalid branch name '{name}'. Version is missing");
                    }
                    else
                    {
                        return new BranchName("", fragments.First(), int.Parse(fragments.Last().Substring(1)));
                    }

                default:
                    if (!s_VersionRegex.IsMatch(fragments.Last()))
                    {
                        throw new FormatException($"Invalid branch name '{name}'. Version is missing");
                    }
                    else
                    {
                        return new BranchName(fragments.Take(fragments.Length - 2).Aggregate((a, b) => $"{a}/{b}"), fragments.Skip(fragments.Length - 2).First(), int.Parse(fragments.Last().Substring(1)));
                    }
            }
        }



        public bool Equals(BranchName other)
        {
            return other != null &&
                   StringComparer.InvariantCultureIgnoreCase.Equals(Name, other.Name) &&
                   StringComparer.InvariantCultureIgnoreCase.Equals(Prefix, other.Prefix) &&
                   Version == other.Version;
        }


        public override bool Equals(object obj) => Equals(obj as BranchName);

        public override int GetHashCode()
        {          
            var hashCode = (Prefix != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Prefix) : 0);
            hashCode = (hashCode * 397) ^ StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name);
            hashCode = (hashCode * 397) ^ Version;
            return hashCode;            
        }       
    }
}