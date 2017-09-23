using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SyncTool.Git.DI;
using SyncTool.Common;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using SyncTool.Common.DI;
using SyncTool.FileSystem.DI;
using SyncTool.Git;
using SyncTool.Sql.DI;

namespace SyncTool.WebUI
{
    public class Startup
    {

        public IContainer ApplicationContainer { get; private set; }

        public ILifetimeScope ApplicationLifetimeScope { get; private set; }
		

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            
            // Create the container builder.
            var builder = new ContainerBuilder();
            builder.RegisterModule<CommonApplicationScopeModule>();
            builder.RegisterModule<FileSystemModule>();
            builder.RegisterModule<GitModuleFactoryModule>();
            builder.RegisterModule<SqlModuleFactoryModule>();
            builder.RegisterInstance(new GitOptions()).AsSelf();
            builder.Populate(services);

            ApplicationContainer = builder.Build();
            ApplicationLifetimeScope = ApplicationContainer.BeginLifetimeScope(Scope.Application);

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(ApplicationLifetimeScope);            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
