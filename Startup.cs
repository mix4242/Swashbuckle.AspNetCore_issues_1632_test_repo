using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApplication
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
            services.AddControllers();
            services.AddHttpContextAccessor();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description =
                        "Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme, Id = "ApiKey"
                            }
                        },
                        new string[] { }
                    }
                });
                
                c.DocumentFilter<SuppressContentDocumentFilter>();
            }).AddSwaggerGenNewtonsoftSupport();;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            app.UseSwagger(c => { c.RouteTemplate = "api-docs/{documentName}/docs.json"; });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api-docs";
                c.SwaggerEndpoint("/api-docs/v1/docs.json", "My API V1");
            });
        }
    }
    
    public class SuppressContentDocumentFilter : IDocumentFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SuppressContentDocumentFilter(IHttpContextAccessor httpContext)
        {
            this._httpContextAccessor = httpContext;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (this._httpContextAccessor.HttpContext.Request.QueryString.HasValue)
            {
                swaggerDoc.Paths.Clear();
                swaggerDoc.Components.Schemas.Clear();
                
                swaggerDoc.Components.SecuritySchemes.Clear();
                swaggerDoc.SecurityRequirements.Clear();
                return;
            }
        }
    }

}