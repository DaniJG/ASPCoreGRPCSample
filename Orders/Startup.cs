using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shippings;
using System.Net.Http;

namespace Orders
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            var shippings_url = Environment.GetEnvironmentVariable("SHIPPINGS_URL") ?? "https://localhost:5003";
            services
                .AddGrpcClient<ProductShipment.ProductShipmentClient>(opts =>
                {
                    opts.BaseAddress = new Uri(shippings_url);
                }).ConfigurePrimaryHttpMessageHandler(() => {
                    var handler = new HttpClientHandler();
                    if (!shippings_url.Contains("localhost"))
                    {
                        // NOTE: This is for development purposes only!
                        // When running in dev with docker-compose, the address of the server wont be localhost, and so the dev HTTPS cert is invalid
                        // This will disable the validation of the HTTPS certificates
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    }
                    return handler;
                });
;           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<OrderPlacementService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
