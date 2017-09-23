using System;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace SyncTool.Common.Options
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterOptions<T>(this ContainerBuilder containerBuilder, IConfiguration configuration, string sectionName = null) where T : class, new()
        {
            if(sectionName == null)
            {
                var optionsTypeName = typeof(T).Name;

                if (!optionsTypeName.EndsWith("Options", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Could not determine configuration section name." +
                        "Use provide type which's name ends with 'Options' or explicitly specify the section name");

                sectionName = optionsTypeName.Substring(0, optionsTypeName.Length - "Options".Length);
            }

            var section = configuration.GetSection(sectionName);
            var options = section.Get<T>();
            options = options ?? new T();

            containerBuilder.RegisterInstance(options).AsSelf();
        }        
    }
}