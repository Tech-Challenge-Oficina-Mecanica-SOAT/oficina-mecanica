using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Cliente.ConsultarClientePorDocumento;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.ConsultarClientePorDocumento;

public class ConsultarClientePorDocumentoUseCaseTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly ConsultarClientePorDocumentoUseCase _sut;

    public ConsultarClientePorDocumentoUseCaseTests()
    {
        _sut = new ConsultarClientePorDocumentoUseCase(_repo.Object, new ClienteMapper());
    }

    [Fact]
    public async Task ExecutarAsync_DocumentoInvalido_RetornaValidation()
    {
        var result = await _sut.ExecutarAsync("abc");
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByDocumentoAsync(It.IsAny<string>())).ReturnsAsync((Domain.Entities.Cliente?)null);
        var result = await _sut.ExecutarAsync("12345678909");
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrado_RetornaSuccess()
    {
        var cliente = new Domain.Entities.Cliente("Joao", new Documento("12345678909"), new Telefone("11911223344"), new Email("a@b.com"));
        _repo.Setup(r => r.GetByDocumentoAsync("12345678909")).ReturnsAsync(cliente);

        var result = await _sut.ExecutarAsync("12345678909");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Joao");
    }
}
