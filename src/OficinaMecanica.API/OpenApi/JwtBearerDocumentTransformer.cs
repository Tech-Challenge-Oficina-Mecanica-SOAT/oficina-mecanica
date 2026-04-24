using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace OficinaMecanica.API.OpenApi;

internal sealed class JwtBearerDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Cole apenas o JWT (sem o prefixo 'Bearer ').",
        };

        var requirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    },
                },
                Array.Empty<string>()
            },
        };

        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        document.SecurityRequirements.Add(requirement);

        document.Info.Title = "Oficina Mecânica API";
        document.Info.Version = "v1";
        document.Info.Description =
            "API do Tech Challenge SOAT/FIAP. Para fluxos completos, consulte `docs/testing/`.";

        return Task.CompletedTask;
    }
}
