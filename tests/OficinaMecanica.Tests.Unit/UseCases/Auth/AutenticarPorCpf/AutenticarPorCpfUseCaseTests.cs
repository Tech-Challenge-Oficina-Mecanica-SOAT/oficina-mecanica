using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.Auth.AutenticarPorCpf;
using OficinaMecanica.Domain.Enums;
using OficinaMecanica.Domain.Interfaces;
using DomainUsuario = OficinaMecanica.Domain.Entities.Usuario;

namespace OficinaMecanica.Tests.Unit.UseCases.Auth.AutenticarPorCpf;

public class AutenticarPorCpfUseCaseTests
{
    private const string CpfValido = "52998224725";

    private readonly Mock<IAutenticacaoClienteQuery> _autenticacaoClienteQuery = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<ITokenGenerator> _token = new();
    private readonly AutenticarPorCpfUseCase _sut;

    public AutenticarPorCpfUseCaseTests() =>
        _sut = new AutenticarPorCpfUseCase(_autenticacaoClienteQuery.Object, _usuarioRepo.Object, _token.Object);

    [Fact]
    public async Task ExecutarAsync_CpfInvalido_RetornaValidation()
    {
        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest("123"));
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteInexistente_RetornaNotFound()
    {
        _autenticacaoClienteQuery.Setup(r => r.ObterPorDocumentoAsync(CpfValido))
            .ReturnsAsync((DadosClienteAutenticacao?)null);

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteInativo_RetornaNotFound()
    {
        var clienteId = Guid.NewGuid();
        _autenticacaoClienteQuery.Setup(r => r.ObterPorDocumentoAsync(CpfValido))
            .ReturnsAsync(new DadosClienteAutenticacao(clienteId, false));

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteSemUsuarioVinculado_RetornaNotFound()
    {
        var clienteId = Guid.NewGuid();
        _autenticacaoClienteQuery.Setup(r => r.ObterPorDocumentoAsync(CpfValido))
            .ReturnsAsync(new DadosClienteAutenticacao(clienteId, true));
        _usuarioRepo.Setup(r => r.ObterPorClienteIdAsync(clienteId)).ReturnsAsync((DomainUsuario?)null);

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_RetornaToken()
    {
        var clienteId = Guid.NewGuid();
        var usuario = new DomainUsuario("cliente@teste.com", "hash", Perfil.Cliente, clienteId);
        _autenticacaoClienteQuery.Setup(r => r.ObterPorDocumentoAsync(CpfValido))
            .ReturnsAsync(new DadosClienteAutenticacao(clienteId, true));
        _usuarioRepo.Setup(r => r.ObterPorClienteIdAsync(clienteId)).ReturnsAsync(usuario);
        var tok = new TokenResponse("token", DateTime.UtcNow.AddHours(1));
        _token.Setup(t => t.GerarParaUsuario(usuario)).Returns(tok);

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token");
    }
}
