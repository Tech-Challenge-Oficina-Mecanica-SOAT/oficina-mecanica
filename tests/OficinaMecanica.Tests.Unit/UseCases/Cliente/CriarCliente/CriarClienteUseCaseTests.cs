using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Cliente.CriarCliente;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.CriarCliente;

public class CriarClienteUseCaseTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly CriarClienteUseCase _sut;

    public CriarClienteUseCaseTests()
    {
        _sut = new CriarClienteUseCase(_repo.Object, new ClienteMapper());
    }

    private static CriarClienteRequest ValidRequest() => new()
    {
        Nome = "Joao",
        Documento = "12345678909",
        Telefone = "11999999999",
        Email = "joao@email.com"
    };

    [Fact]
    public async Task ExecutarAsync_DocumentoInvalido_RetornaValidation()
    {
        var req = ValidRequest();
        req.Documento = "abc";
        var result = await _sut.ExecutarAsync(req);
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_DocumentoJaExistente_RetornaConflict()
    {
        _repo.Setup(r => r.ExistsByDocumentoAsync(It.IsAny<string>())).ReturnsAsync(true);
        var result = await _sut.ExecutarAsync(ValidRequest());
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_CriaCliente()
    {
        _repo.Setup(r => r.ExistsByDocumentoAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Cliente>()))
            .ReturnsAsync((Domain.Entities.Cliente c) => c);

        var result = await _sut.ExecutarAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Joao");
    }
}
