using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using WebServices.Application;
using WebServices.Application.Contracts;
using WebServices.DataAccess;
using WebServices.DataAccess.Contracts;
using WebServices.Repository;
using WebServices.Repository.Contracts;

namespace WebServices.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration().WriteTo.File(Configuration["pathLogs"]).CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebServices.API", Version = "v1" });
            });

            //Generate the connection to the BBDD and create the migrations of the objects in the BBDD
            services.AddDbContext<ConnectionContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
            x=> x.MigrationsAssembly("WebServices.API")));

            //When you require an object of type IRateApplication, an instance of the object of type RateApplication will be created
            services.AddScoped(typeof(IRateApplication), typeof(RateAplication));

            //When you require an object of type ITransactionApplication, an instance of the object of type TransactionApplication will be created
            services.AddScoped(typeof(ITransactionApplication), typeof(TransactionApplication));

            //When you require an object of type IGenericRepository, an instance of the object of type GenericRepository will be created
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //When you require an object of type IDBContext, an instance of the object of type DBContext will be created
            services.AddScoped(typeof(IDBContext<>), typeof(DBContext<>));


        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebServices.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
