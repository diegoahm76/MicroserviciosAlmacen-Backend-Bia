using AccountingEntry.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AccountingEntry.Domain.Services;
using AccountingEntry.Domain.Services.Interfaces;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using AccountingEntry.Repository.Interfaces;
using AccountingEntry.Repository;
using AutoMapper;
using AccountingEntry.API;

namespace AccountingEntry
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			//Add Context
			services.AddDbContext<PimisysContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnectionString")));

			services.AddScoped(typeof(DbContext), typeof(PimisysContext));

			//Add Repository
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddTransient<IGenericUnitOfWork, GenericUnitOfWork>();

			//add Services
			services.AddTransient<IAccountingEntryService, AccountingEntryService>();

			//JSON serializer
			services.AddControllersWithViews().
				AddNewtonsoftJson(options =>
				options.SerializerSettings.ReferenceLoopHandling = Newtonsoft
				.Json.ReferenceLoopHandling.Ignore)
				.AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver
				= new DefaultContractResolver());

			//Mapper
			var mapperConfig = new MapperConfiguration(m => {
				m.AddProfile(new MappingProfile());
			});

			IMapper mapper = mapperConfig.CreateMapper();
			services.AddSingleton(mapper);

			services.AddControllers();

			//Register the Swagger Generator
			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Accounting Entry Microservice",
					Version = "v1"
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger();
			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("./swagger/v1/swagger.json", "Accounting Entry Microservice V1");
			});

			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
