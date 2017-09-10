
namespace SyncTool.Sql.Model
{
    static class StringExtensions
    {
        //TODO: Remove once all usages have been removed
        public static string NormalizeCaseInvariant(this string value)
        {
            return value.Trim().ToUpperInvariant();
        }
    }
}
