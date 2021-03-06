﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VueCliMiddleware;
using Whiteboard.Hubs;

namespace Whiteboard
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");

            services.AddSignalR();

            string connectionString = Configuration["ConnectionString:DefaultConnection"];
            services.AddDbContext<AppContext>(options => options.UseSqlServer(connectionString));

            var builder = new DbContextOptionsBuilder<AppContext>();
            builder.UseSqlServer(connectionString);

            services.AddSingleton<DbContextOptions>(builder.Options);

            services.AddSingleton<IConnectionStorage, ConnectionStorage>();
            services.AddSingleton<IActiveRoomStorage, ActiveRoomStorage>();

            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(configuration =>
            {
                configuration.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials();
            });

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseSignalR(configuration =>
            {
                configuration.MapHub<RoomHub>("/ws/rooms");
            });
            app.UseMvc();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseVueCli(npmScript: "serve", port: 3000);
                }
            });
        }
    }
}
