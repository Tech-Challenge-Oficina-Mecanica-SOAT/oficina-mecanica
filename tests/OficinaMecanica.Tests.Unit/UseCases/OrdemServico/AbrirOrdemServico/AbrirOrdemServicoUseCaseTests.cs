using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServico.AbrirOrdemServico;

public class AbrirOrdemServicoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly Mock<IOrdemServicoMetrics> _metrics = new();
    private readonly AbrirOrdemServicoUseCase _sut;

    public AbrirOrdemServicoUseCaseTests()
    {
        _sut = new AbrirOrdemServicoUseCase(_repo.Object, new OrdemServicoMapper(), _metrics.Object);
    }

    [Fact]
    public async Task ExecutarAsync_ComClienteIdVazio_RetornaValidation()
    {
        var request = new AbrirOrdemServicoRequest { ClienteId = Guid.Empty, VeiculoId = Guid.NewGuid() };
        var result = await _sut.ExecutarAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_ComVeiculoIdVazio_RetornaValidation()
    {
        var request = new AbrirOrdemServicoRequest { ClienteId = Guid.NewGuid(), VeiculoId = Guid.Empty };
        var result = await _sut.ExecutarAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_CriaOS()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var osId = Guid.NewGuid();
        var criada = new Domain.Entities.OrdemServico(clienteId, veiculoId, "obs");

        _repo.Setup(r => r.CriarAsync(It.IsAny<Domain.Entities.OrdemServico>())).ReturnsAsync(osId);
        _repo.Setup(r => r.ObterPorIdComItensAsync(osId)).ReturnsAsync(criada);

        var request = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Observacoes = "obs"
        };

        var result = await _sut.ExecutarAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.Value!.ClienteId.Should().Be(clienteId);
    }
}
