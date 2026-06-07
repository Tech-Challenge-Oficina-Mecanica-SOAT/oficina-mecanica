using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOrcamentoPorEmail;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.RejeitarOS;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServicoStatus.AprovarOrcamentoPorEmail;

public class AprovarOrcamentoPorEmailUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly Mock<IAprovarOSUseCase> _aprovarUseCase = new();
    private readonly Mock<IRejeitarOSUseCase> _rejeitarUseCase = new();
    private readonly AprovarOrcamentoPorEmailUseCase _sut;

    private const string TokenValido = "token-valido-123";

    public AprovarOrcamentoPorEmailUseCaseTests() =>
        _sut = new AprovarOrcamentoPorEmailUseCase(_repo.Object, _aprovarUseCase.Object, _rejeitarUseCase.Object);

    private Domain.Entities.OrdemServico CriarOsAguardandoAprovacao()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        os.ForcarStatus(EnumStatusOS.AguardandoAprovacao, "admin", "setup");
        os.TokenAprovacao = TokenValido;
        os.TokenUsado = false;
        return os;
    }

    [Fact]
    public async Task ExecutarAsync_TokenNaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorTokenAsync(It.IsAny<string>()))
             .ReturnsAsync((Domain.Entities.OrdemServico?)null);

        var result = await _sut.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(TokenValido, true));

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_TokenJaUsado_RetornaValidation()
    {
        var os = CriarOsAguardandoAprovacao();
        os.TokenUsado = true;
        _repo.Setup(r => r.ObterPorTokenAsync(TokenValido)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(TokenValido, true));

        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error.Should().Be("Link já foi utilizado");
    }

    [Fact]
    public async Task ExecutarAsync_StatusIncorreto_RetornaValidation()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs"); // status = Recebida
        os.TokenAprovacao = TokenValido;
        _repo.Setup(r => r.ObterPorTokenAsync(TokenValido)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(TokenValido, true));

        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error.Should().Contain("Recebida");
    }

    [Fact]
    public async Task ExecutarAsync_Aprovado_RetornaAprovadoEMarcaTokenUsado()
    {
        var os = CriarOsAguardandoAprovacao();
        _repo.Setup(r => r.ObterPorTokenAsync(TokenValido)).ReturnsAsync(os);
        _aprovarUseCase.Setup(u => u.ExecutarAsync(It.IsAny<AprovarOSRequest>()))
                       .ReturnsAsync(Result<bool>.Success(true));

        var result = await _sut.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(TokenValido, true));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("aprovado");
        os.TokenUsado.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_Recusado_RetornaRecusadoEMarcaTokenUsado()
    {
        var os = CriarOsAguardandoAprovacao();
        _repo.Setup(r => r.ObterPorTokenAsync(TokenValido)).ReturnsAsync(os);
        _rejeitarUseCase.Setup(u => u.ExecutarAsync(It.IsAny<RejeitarOSUseCaseRequest>()))
                        .ReturnsAsync(Result<bool>.Success(true));

        var result = await _sut.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(TokenValido, false));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("recusado");
        os.TokenUsado.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_AprovarFalha_RetornaValidation()
    {
        var os = CriarOsAguardandoAprovacao();
        _repo.Setup(r => r.ObterPorTokenAsync(TokenValido)).ReturnsAsync(os);
        _aprovarUseCase.Setup(u => u.ExecutarAsync(It.IsAny<AprovarOSRequest>()))
                       .ReturnsAsync(Result<bool>.Validation("Transição inválida"));

        var result = await _sut.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(TokenValido, true));

        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error.Should().Be("Transição inválida");
    }

    [Fact]
    public async Task ExecutarAsync_RejeitarFalha_RetornaValidation()
    {
        var os = CriarOsAguardandoAprovacao();
        _repo.Setup(r => r.ObterPorTokenAsync(TokenValido)).ReturnsAsync(os);
        _rejeitarUseCase.Setup(u => u.ExecutarAsync(It.IsAny<RejeitarOSUseCaseRequest>()))
                        .ReturnsAsync(Result<bool>.Validation("Transição inválida"));

        var result = await _sut.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(TokenValido, false));

        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error.Should().Be("Transição inválida");
    }
}
