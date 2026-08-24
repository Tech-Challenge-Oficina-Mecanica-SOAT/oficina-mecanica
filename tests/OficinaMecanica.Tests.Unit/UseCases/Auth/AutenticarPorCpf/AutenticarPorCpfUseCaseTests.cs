using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.Auth.AutenticarPorCpf;
using OficinaMecanica.Domain.Enums;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;
using DomainCliente = OficinaMecanica.Domain.Entities.Cliente;
using DomainUsuario = OficinaMecanica.Domain.Entities.Usuario;

namespace OficinaMecanica.Tests.Unit.UseCases.Auth.AutenticarPorCpf;

public class AutenticarPorCpfUseCaseTests
{
    private const string CpfValido = "52998224725";

    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<ITokenGenerator> _token = new();
    private readonly AutenticarPorCpfUseCase _sut;

    public AutenticarPorCpfUseCaseTests() =>
        _sut = new AutenticarPorCpfUseCase(_clienteRepo.Object, _usuarioRepo.Object, _token.Object);

    private static DomainCliente CriarCliente() =>
        new("Cliente Teste", new Documento(CpfValido), new Telefone("11999999999"), new Email("cliente@teste.com"));

    [Fact]
    public async Task ExecutarAsync_CpfInvalido_RetornaValidation()
    {
        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest("123"));
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteInexistente_RetornaNotFound()
    {
        _clienteRepo.Setup(r => r.GetByDocumentoAsync(CpfValido)).ReturnsAsync((DomainCliente?)null);

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteInativo_RetornaNotFound()
    {
        var cliente = CriarCliente();
        cliente.Desativar();
        _clienteRepo.Setup(r => r.GetByDocumentoAsync(CpfValido)).ReturnsAsync(cliente);

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteSemUsuarioVinculado_RetornaNotFound()
    {
        var cliente = CriarCliente();
        _clienteRepo.Setup(r => r.GetByDocumentoAsync(CpfValido)).ReturnsAsync(cliente);
        _usuarioRepo.Setup(r => r.ObterPorClienteIdAsync(cliente.Id)).ReturnsAsync((DomainUsuario?)null);

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_RetornaToken()
    {
        var cliente = CriarCliente();
        var usuario = new DomainUsuario("cliente@teste.com", "hash", Perfil.Cliente, cliente.Id);
        _clienteRepo.Setup(r => r.GetByDocumentoAsync(CpfValido)).ReturnsAsync(cliente);
        _usuarioRepo.Setup(r => r.ObterPorClienteIdAsync(cliente.Id)).ReturnsAsync(usuario);
        var tok = new TokenResponse("token", DateTime.UtcNow.AddHours(1));
        _token.Setup(t => t.GerarParaUsuario(usuario)).Returns(tok);

        var result = await _sut.ExecutarAsync(new AutenticarPorCpfRequest(CpfValido));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token");
    }
}
