using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServico.AdicionarItensOS;

public class AdicionarItensOSUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly Mock<IMarcarAguardandoAprovacaoUseCase> _marcar = new();
    private readonly AdicionarItensOSUseCase _sut;

    public AdicionarItensOSUseCaseTests() =>
        _sut = new AdicionarItensOSUseCase(_repo.Object, new OrdemServicoMapper(), _marcar.Object);

    private static AdicionarItensOSRequest Req(Guid osId, string tipo = "servico") => new()
    {
        OrdemServicoId = osId,
        Itens = new List<AdicionarOSItemRequest>
        {
            new() { Tipo = tipo, ReferenciaId = Guid.NewGuid(), Descricao = "x", Quantidade = 1, PrecoUnitario = 10m }
        }
    };

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdComItensAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(Req(Guid.NewGuid()));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_TipoInvalido_RetornaValidation()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdComItensAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(Req(os.Id, "tipoXYZ"));

        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_AdicionaItensETransiciona()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        os.ForcarStatus(EnumStatusOS.EmDiagnostico, "admin", "setup");
        _repo.Setup(r => r.ObterPorIdComItensAsync(os.Id)).ReturnsAsync(os);
        _repo.Setup(r => r.AdicionarItensAsync(It.IsAny<IEnumerable<OrdemServicoItem>>()))
            .ReturnsAsync((IEnumerable<OrdemServicoItem> i) => i.ToList());
        _marcar.Setup(m => m.ExecutarAsync(It.IsAny<MarcarAguardandoAprovacaoRequest>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await _sut.ExecutarAsync(Req(os.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
