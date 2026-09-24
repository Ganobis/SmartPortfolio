using Microsoft.OpenApi.Models;

namespace SmartPortfolio.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "JWT",
                        Description = "Paste your JWT"
                    }
                };

                foreach (var path in document.Paths.Values)
                {
                    foreach (var operations in path.Operations.Values)
                    {
                        operations.Security.Add(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Id = "Bearer",
                                        Type = ReferenceType.SecurityScheme
                                    }
                                },
                                Array.Empty<string>()
                            }
                        });
                    }
                }
                return Task.CompletedTask;
            });
        });

        return services;
    }
}