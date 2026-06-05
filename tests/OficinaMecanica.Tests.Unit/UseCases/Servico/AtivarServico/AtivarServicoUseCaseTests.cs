using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.Servico.AtivarServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.AtivarServico;

public class AtivarServicoUseCaseTests
{
    private readonly Mock<IServicoRepository> _repo = new();
    private readonly AtivarServicoUseCase _sut;

    public AtivarServicoUseCaseTests() => _sut = new AtivarServicoUseCase(_repo.Object);

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Servico?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrado_Ativa()
    {
        var s = new Domain.Entities.Servico("Troca", "d", 10m);
        s.Desativar();
        _repo.Setup(r => r.GetByIdAsync(s.Id)).ReturnsAsync(s);

        var result = await _sut.ExecutarAsync(s.Id);

        result.IsSuccess.Should().BeTrue();
        s.Ativo.Should().BeTrue();
    }
}
