using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Autofac;
using NLog.Config;
using NLog.Extensions.Logging;
using System.IO;
using SyncTool.Cli.Installation;
using SyncTool.Cli.Options;

namespace SyncTool.Cli.Logging
{
    sealed class LoggingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(LogggerProxy<>)).As(typeof(ILogger<>));
            builder.Register(ctx => ctx.Resolve<ILoggerFactory>().CreateLogger("UnknownCategory")).As<ILogger>();

            builder.Register(ctx =>
            {                                    
                var loggerFactory = new LoggerFactory();

                var enable = ctx.Resolve<IConfiguration>().GetValue("Logging:Enable", true);
                if(enable)
                {                 
                    var nLogConfigPath = Path.Combine(ApplicationInfo.RootDirectory, ConfigFileNames.NLogConfigFile);
                    var nLogConfig = new XmlLoggingConfiguration(nLogConfigPath);

                    var loggingConfig = loggerFactory.AddNLog().ConfigureNLog(nLogConfig);                    
                    loggingConfig.Variables["ApplicationDirectory"] = ApplicationInfo.RootDirectory;                    
                }

                return loggerFactory;
            })
            .As<ILoggerFactory>()
            .SingleInstance();
        }
    }
}
