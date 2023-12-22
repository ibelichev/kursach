using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MoodTracker.Data;

namespace MoodTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<MoodTrackerContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("MoodTrackerContext")));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MoodTrackerContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Apply migrations in development environment
                ApplyMigrations(context);
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void ApplyMigrations(MoodTrackerContext context)
        {
            try
            {
                // Apply pending migrations
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                // Handle migration errors
                Console.WriteLine($"Error applying migrations: {ex.Message}");
            }
        }
    }
}