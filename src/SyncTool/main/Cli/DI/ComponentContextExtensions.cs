using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Cli.DI
{
    static class ComponentContextExtensions 
    {
        public static IEnumerable<Type> GetImplementingTypes<T>(this IComponentContext scope)
        {
            return scope.ComponentRegistry
                .RegistrationsFor(new TypedService(typeof(T)))
                .Select(x => x.Activator)
                .Select(x => x.LimitType);
        }

    }
}
