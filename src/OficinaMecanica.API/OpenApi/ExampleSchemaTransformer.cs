using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.API.OpenApi;

internal sealed class ExampleSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly Dictionary<Type, IOpenApiAny> Examples = new()
    {
        [typeof(LoginDto)] = new OpenApiObject
        {
            ["email"] = new OpenApiString("admin@oficina.com"),
            ["senha"] = new OpenApiString("Senha@123"),
        },
        [typeof(RegistrarUsuarioDto)] = new OpenApiObject
        {
            ["email"] = new OpenApiString("admin@oficina.com"),
            ["senha"] = new OpenApiString("Senha@123"),
            ["perfil"] = new OpenApiInteger(0),
        },
        [typeof(CreateClienteDto)] = new OpenApiObject
        {
            ["nome"] = new OpenApiString("João da Silva"),
            ["documento"] = new OpenApiString("12345678901"),
            ["telefone"] = new OpenApiString("11999998888"),
            ["email"] = new OpenApiString("joao@email.com"),
        },
        [typeof(UpdateClienteDto)] = new OpenApiObject
        {
            ["nome"] = new OpenApiString("João da Silva"),
            ["telefone"] = new OpenApiString("11988887777"),
            ["email"] = new OpenApiString("joao.novo@email.com"),
        },
        [typeof(CreateVeiculoDto)] = new OpenApiObject
        {
            ["clienteId"] = new OpenApiString("00000000-0000-0000-0000-000000000000"),
            ["placa"] = new OpenApiString("ABC1D23"),
            ["marca"] = new OpenApiString("Volkswagen"),
            ["modelo"] = new OpenApiString("Gol"),
            ["ano"] = new OpenApiInteger(2020),
        },
        [typeof(UpdateVeiculoDto)] = new OpenApiObject
        {
            ["placa"] = new OpenApiString("ABC1D23"),
            ["marca"] = new OpenApiString("Volkswagen"),
            ["modelo"] = new OpenApiString("Gol G6"),
            ["ano"] = new OpenApiInteger(2021),
        },
        [typeof(CreateServicoDto)] = new OpenApiObject
        {
            ["nome"] = new OpenApiString("Troca de óleo"),
            ["descricao"] = new OpenApiString("Troca de óleo do motor + filtro"),
            ["valor"] = new OpenApiDouble(150.00),
        },
        [typeof(UpdateServicoDto)] = new OpenApiObject
        {
            ["nome"] = new OpenApiString("Troca de óleo premium"),
            ["descricao"] = new OpenApiString("Troca de óleo sintético + filtro"),
            ["valor"] = new OpenApiDouble(220.00),
        },
        [typeof(CreatePecaDto)] = new OpenApiObject
        {
            ["nome"] = new OpenApiString("Filtro de óleo"),
            ["codigo"] = new OpenApiString("FO-001"),
            ["precoUnitario"] = new OpenApiDouble(45.90),
            ["estoque"] = new OpenApiInteger(50),
            ["descricao"] = new OpenApiString("Filtro de óleo motor 1.0 a 2.0"),
        },
        [typeof(UpdatePecaDto)] = new OpenApiObject
        {
            ["nome"] = new OpenApiString("Filtro de óleo premium"),
            ["precoUnitario"] = new OpenApiDouble(59.90),
            ["estoque"] = new OpenApiInteger(75),
            ["descricao"] = new OpenApiString("Filtro de óleo de alta performance"),
        },
        [typeof(UpdateEstoqueDto)] = new OpenApiObject
        {
            ["quantidade"] = new OpenApiInteger(10),
            ["tipoOperacao"] = new OpenApiString("incrementar"),
        },
    };

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (Examples.TryGetValue(context.JsonTypeInfo.Type, out var example))
        {
            schema.Example = example;
        }

        return Task.CompletedTask;
    }
}
