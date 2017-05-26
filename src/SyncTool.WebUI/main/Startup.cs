using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SyncTool.Git.DI;
using SyncTool.Synchronization.DI;
using SyncTool.Common;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using SyncTool.FileSystem.FileSystem.DI;
using SyncTool.Common.DI;

namespace SyncTool.WebUI
{
    public class Startup
    {

        public IContainer ApplicationContainer { get; private set; }

        public ILifetimeScope ApplicationLifetimeScope { get; private set; }


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            
            // Create the container builder.
            var builder = new ContainerBuilder();
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule<FileSystemModule>();
            builder.RegisterModule<GitModuleFactoryModule>();
            builder.Populate(services);

            ApplicationContainer = builder.Build();
            ApplicationLifetimeScope = ApplicationContainer.BeginLifetimeScope(Scope.Application);

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(ApplicationLifetimeScope);            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            appLifetime.ApplicationStopped.Register(() =>
            {
                ApplicationLifetimeScope.Dispose();
                ApplicationContainer.Dispose();
            });
        }
    }
}
