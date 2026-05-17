using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Cliente.ListarClientes;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.ListarClientes;

public class ListarClientesUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaListaMapeada()
    {
        var repo = new Mock<IClienteRepository>();
        var c = new Domain.Entities.Cliente("Joao", new Documento("12345678909"), "111", new Email("a@b.com"));
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { c });
        var sut = new ListarClientesUseCase(repo.Object, new ClienteMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecutarAsync_ListaVazia_RetornaSuccessVazio()
    {
        var repo = new Mock<IClienteRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Domain.Entities.Cliente>());
        var sut = new ListarClientesUseCase(repo.Object, new ClienteMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
