namespace SyncTool.Cli.Framework
{
    public interface ICommand
    {

    }

    public interface ICommand<T> : ICommand where T : OptionsBase, new()
    {
        int Run(T opts);
    }
}