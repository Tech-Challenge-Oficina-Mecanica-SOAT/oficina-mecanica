using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OficinaMecanica.Application.DTOs;
using System.Text.Json.Nodes;

namespace OficinaMecanica.API.OpenApi;

internal sealed class ExampleSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly Dictionary<Type, Func<JsonNode>> Examples = new()
    {
        [typeof(LoginDto)] = () => new JsonObject
        {
            ["email"] = "admin@oficina.com",
            ["senha"] = "Senha@123",
        },
        [typeof(RegistrarUsuarioDto)] = () => new JsonObject
        {
            ["email"] = "admin@oficina.com",
            ["senha"] = "Senha@123",
            ["perfil"] = 0,
        },
        [typeof(CreateClienteDto)] = () => new JsonObject
        {
            ["nome"] = "João da Silva",
            ["documento"] = "52998224725",
            ["telefone"] = "11999998888",
            ["email"] = "joao@email.com",
        },
        [typeof(UpdateClienteDto)] = () => new JsonObject
        {
            ["nome"] = "João da Silva",
            ["telefone"] = "11988887777",
            ["email"] = "joao.novo@email.com",
        },
        [typeof(CreateVeiculoDto)] = () => new JsonObject
        {
            ["clienteId"] = "00000000-0000-0000-0000-000000000000",
            ["placa"] = "ABC1D23",
            ["marca"] = "Volkswagen",
            ["modelo"] = "Gol",
            ["ano"] = 2020,
        },
        [typeof(UpdateVeiculoDto)] = () => new JsonObject
        {
            ["placa"] = "ABC1D23",
            ["marca"] = "Volkswagen",
            ["modelo"] = "Gol G6",
            ["ano"] = 2021,
        },
        [typeof(CreateServicoDto)] = () => new JsonObject
        {
            ["nome"] = "Troca de óleo",
            ["descricao"] = "Troca de óleo do motor + filtro",
            ["valor"] = 150.00m,
        },
        [typeof(UpdateServicoDto)] = () => new JsonObject
        {
            ["nome"] = "Troca de óleo premium",
            ["descricao"] = "Troca de óleo sintético + filtro",
            ["valor"] = 220.00m,
        },
        [typeof(CreatePecaDto)] = () => new JsonObject
        {
            ["nome"] = "Filtro de óleo",
            ["codigo"] = "FO-001",
            ["precoUnitario"] = 45.90m,
            ["estoque"] = 50,
            ["descricao"] = "Filtro de óleo motor 1.0 a 2.0",
        },
        [typeof(UpdatePecaDto)] = () => new JsonObject
        {
            ["nome"] = "Filtro de óleo premium",
            ["precoUnitario"] = 59.90m,
            ["estoque"] = 75,
            ["descricao"] = "Filtro de óleo de alta performance",
        },
        [typeof(UpdateEstoqueDto)] = () => new JsonObject
        {
            ["quantidade"] = 10,
            ["tipoOperacao"] = "incrementar",
        },
    };

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (Examples.TryGetValue(context.JsonTypeInfo.Type, out var factory))
        {
            schema.Example = factory();
        }

        return Task.CompletedTask;
    }
}
