using System;
using System.Linq;
using System.Text;

namespace SyncTool.Git.Common
{
    public sealed class BranchName : IEquatable<BranchName>
    {
        

        public static readonly BranchName Master = new BranchName("master");


        public string Prefix { get; }

        public string Name { get; }
        

        public BranchName(string name) : this("", name)
        {
            
        }
        

        public BranchName(string prefix, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty", nameof(name));
            }            

            Prefix = prefix?.Trim() ?? "";
            Name = name.Trim();
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
                    return new BranchName(fragments[0]);

                default:                                   
                    return new BranchName(fragments.Take(fragments.Length - 1).Aggregate((a, b) => $"{a}/{b}") , fragments.Last());
                    
            }
        }



        public bool Equals(BranchName other)
        {
            return other != null &&
                   StringComparer.InvariantCultureIgnoreCase.Equals(Name, other.Name) &&
                   StringComparer.InvariantCultureIgnoreCase.Equals(Prefix, other.Prefix);
        }


        public override bool Equals(object obj) => Equals(obj as BranchName);

        public override int GetHashCode()
        {          
            var hashCode = (Prefix != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Prefix) : 0);
            hashCode = (hashCode * 397) ^ StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name);           
            return hashCode;            
        }       
    }
}