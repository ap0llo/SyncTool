
namespace SyncTool.Sql.Model
{
    static class StringExtensions
    {
        public static string NormalizeCaseInvariant(this string value)
        {
            return value.Trim().ToUpperInvariant();
        }
    }
}
