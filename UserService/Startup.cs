using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserService.DataAccess;
using UserService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.Geocoding;
using Prometheus;

namespace UserService
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityCore<User>();

            services.AddTransient<UserManager<User>>();

            services.AddTransient<IUserRepository, UserRepository>();
            
            services.AddSingleton<IHTTPClientFactory, HttpClientFactory>();
            services.AddSingleton<IReverseGeocodeRestAPIInvoker, GeocodeXYZReverseGeocoderRestInvoker>();

            RabbitMQHelper.RabbitServiceRegistration.RegisterConsumorService(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();

            app.UseRouting();
        }
    }
}
