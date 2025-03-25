using Microsoft.OpenApi.Models;
using PDFReader.SwaggerConfig;

namespace PDFReader.Extensions
{
    public static class SwaggerExtensions
    {
        public static void UseSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PDFReader API", Version = "v1" });

                // Add X-Device-Mac header parameter to all operations
                c.AddSecurityDefinition("DeviceMac", new OpenApiSecurityScheme
                {
                    Description = "Enter MAC address in the format XX:XX:XX:XX:XX:XX or any device identifier",
                    Name = "X-Device-Mac",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "DeviceMac"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                        {
                                            {
                                                new OpenApiSecurityScheme
                                                {
                                                    Reference = new OpenApiReference
                                                    {
                                                        Type = ReferenceType.SecurityScheme,
                                                        Id = "DeviceMac"
                                                    },
                                                    In = ParameterLocation.Header
                                                },
                                                new string[] {}
                                            }
                                        });
                
                // Add operation filters for documentation examples
                c.OperationFilter<SwaggerExampleFilter>();
                
                // Enable XML comments
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            });
        }
    }
}