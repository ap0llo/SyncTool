namespace SyncTool.Synchronization.Conflicts
{
    public static class ConflictServiceExtensions
    {
        public static void AddItems(this IConflictService service, params ConflictInfo[] syncActions) => service.AddItems(syncActions);

        public static void RemoveItems(this IConflictService service, params ConflictInfo[] syncActions) => service.RemoveItems(syncActions);
    }
}