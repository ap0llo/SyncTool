namespace SyncTool.Cli.Framework
{
    public interface ICommand<T> where T : new ()
    {
        int Run(T opts);
    }
}