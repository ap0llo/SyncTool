namespace SyncTool.Sql.Model
{
    public interface IDatabaseContextFactory
    {
        DatabaseContext CreateContext();
    }
}
