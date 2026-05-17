using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Servico.AtualizarServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.AtualizarServico;

public class AtualizarServicoUseCaseTests
{
    private readonly Mock<IServicoRepository> _repo = new();
    private readonly AtualizarServicoUseCase _sut;

    public AtualizarServicoUseCaseTests() => _sut = new AtualizarServicoUseCase(_repo.Object, new ServicoMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Servico?)null);
        var result = await _sut.ExecutarAsync(new AtualizarServicoUseCaseRequest(Guid.NewGuid(), "n", "d", 10));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_AtualizaServico()
    {
        var s = new Domain.Entities.Servico("Troca", "d", 50m);
        _repo.Setup(r => r.GetByIdAsync(s.Id)).ReturnsAsync(s);

        var result = await _sut.ExecutarAsync(new AtualizarServicoUseCaseRequest(s.Id, "TrocaNova", "dn", 60m));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("TrocaNova");
    }
}
