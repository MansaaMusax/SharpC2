using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using NSwag.Generation.Processors.Security;

using System.Text;

using TeamServer.Hubs;

namespace TeamServer
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddControllers().AddNewtonsoftJson(j =>
            {
                j.SerializerSettings.ContractResolver = new DefaultContractResolver();
                j.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services.AddSwaggerDocument(c =>
            {
                c.PostProcess = d =>
                {
                    d.Info.Version = "v1";
                    d.Info.Title = "SharpC2 API";
                    d.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "Daniel Duggan, Adam Chester",
                        Url = "https://github.com/SharpC2/SharpC2"
                    };
                };
                c.DocumentProcessors.Add(new SecurityDefinitionAppender("Bearer", new NSwag.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                    In = NSwag.OpenApiSecurityApiKeyLocation.Header
                }));
                c.OperationProcessors.Add(new OperationSecurityScopeProcessor("Bearer"));
            });

            services.AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(j =>
            {
                j.RequireHttpsMetadata = false;
                j.SaveToken = true;
                j.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gvcznxrbobuzvvzytfynvvzrpqaaihrvkrgbgnfqdzdwojjzbiymzcfeuywidvjuwdnlvplwlzcwjbyfaveegnvxnvfcbjdwgggywzngsoxxyroaiogmcmvisdmogfge")),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/MessageHub");
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}
