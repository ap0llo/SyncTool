namespace SyncTool.Utilities
{
    public interface IObjectMapper<TSource, TTarget>
    {
        TTarget MapObject(TSource item);      
    }
}