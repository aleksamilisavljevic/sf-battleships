using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Fabric.Description;
using System.Collections.ObjectModel;

namespace API
{
  /// <summary>
  /// The FabricRuntime creates an instance of this class for each service type instance.
  /// </summary>
  internal sealed class API : StatelessService
  {

    public API(StatelessServiceContext context)
        : base(context)
    { }

    /// <summary>
    /// Optional override to create listeners (like tcp, http) for this service instance.
    /// </summary>
    /// <returns>The collection of listeners.</returns>
    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
    {
      return new ServiceInstanceListener[]
      {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        var  MyAllowSpecificOrigins = "CORSPolicy";

                        var configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                        var jwtSection = configPackage.Settings.Sections["APIJwtSettings"];
                        var secretKey = File.ReadAllText(Path.Join(Environment.GetEnvironmentVariable("C:\\Users\\aleksam\\Documents\\battleships\\battleships\\battleships\\secrets"), "APISecretKey"));
                        var issuer = jwtSection.Parameters["Issuer"].Value;
                        var audience = jwtSection.Parameters["Audience"].Value;

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy(name: MyAllowSpecificOrigins,
                                              policy  =>
                                              {
                                                  policy.WithOrigins("http://localhost:4200")
                                                        .AllowAnyMethod()
                                                        .AllowAnyHeader()
                                                        .AllowCredentials();
                                              });
                        });
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);
                        builder.Services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters()
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = issuer,
                                ValidAudience = audience,
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
                            };
                        });
                        builder.Services.AddAuthorization();
                        builder.Services.AddControllers();
                        builder.Services.AddEndpointsApiExplorer();

                        var app = builder.Build();

                        app.UseCors();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.MapControllers();

                        return app;

                    }))
      };
    }
   
  }
}
