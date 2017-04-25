namespace SyncTool.Cli.Framework
{
    public interface ICommand<T> where T : OptionsBase, new()
    {
        int Run(T opts);
    }
}