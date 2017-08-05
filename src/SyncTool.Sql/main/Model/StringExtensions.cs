
namespace SyncTool.Sql.Model
{
    internal static class StringExtensions
    {
        public static string NormalizeCaseInvariant(this string value)
        {
            return value.Trim().ToUpperInvariant();
        }
    }
}
