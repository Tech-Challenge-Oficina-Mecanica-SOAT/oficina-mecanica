using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OficinaMecanica.API.OpenApi;

internal sealed class JwtBearerDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Cole apenas o JWT (sem o prefixo 'Bearer ').",
        };

        document.Info.Title = "Oficina Mecânica API";
        document.Info.Version = "v1";
        document.Info.Description =
            "API do Tech Challenge SOAT/FIAP. Para fluxos de teste passo a passo, consulte docs/testing/.";

        return Task.CompletedTask;
    }
}
