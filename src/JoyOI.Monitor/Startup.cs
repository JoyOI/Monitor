using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using JoyOI.Monitor.Lib;
using Microsoft.Extensions.Configuration;

namespace JoyOI.Monitor
{
    public class Startup
    {
        public static IConfiguration Config;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out Config);
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddJoyOIUserCenter();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSession();
            app.UseStaticFiles();
            app.UseAuthorize();
            app.UseMvcWithDefaultRoute();
        }
    }
}
