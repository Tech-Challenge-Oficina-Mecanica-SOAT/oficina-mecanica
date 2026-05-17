using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Tests.Unit.DTOs;

public class LoginRequestTests
{
    [Fact]
    public void LoginDto_DeveCriarComEmailESenha()
    {
        // Arrange & Act
        var loginDto = new LoginRequest("usuario@email.com", "senha123");

        // Assert
        Assert.Equal("usuario@email.com", loginDto.Email);
        Assert.Equal("senha123", loginDto.Senha);
    }

    [Fact]
    public void LoginDto_DeveSerIgualQuandoValoresSaoIguais()
    {
        // Arrange
        var login1 = new LoginRequest("usuario@email.com", "senha123");
        var login2 = new LoginRequest("usuario@email.com", "senha123");

        // Act & Assert
        Assert.Equal(login1, login2);
    }

    [Fact]
    public void LoginDto_DeveSerDiferenteQuandoEmailsDiferem()
    {
        // Arrange
        var login1 = new LoginRequest("usuario1@email.com", "senha123");
        var login2 = new LoginRequest("usuario2@email.com", "senha123");

        // Act & Assert
        Assert.NotEqual(login1, login2);
    }

    [Fact]
    public void LoginDto_DeveSerDiferenteQuandoSenhasDiferem()
    {
        // Arrange
        var login1 = new LoginRequest("usuario@email.com", "senha123");
        var login2 = new LoginRequest("usuario@email.com", "senha456");

        // Act & Assert
        Assert.NotEqual(login1, login2);
    }

    [Fact]
    public void LoginDto_DeveGerarToStringCorreto()
    {
        // Arrange
        var login = new LoginRequest("usuario@email.com", "senha123");

        // Act
        var result = login.ToString();

        // Assert
        Assert.Contains("Email", result);
        Assert.Contains("usuario@email.com", result);
        Assert.Contains("Senha", result);
    }

    [Fact]
    public void LoginDto_DeveDesconstruirCorretamente()
    {
        // Arrange
        var login = new LoginRequest("usuario@email.com", "senha123");

        // Act
        var (email, senha) = login;

        // Assert
        Assert.Equal("usuario@email.com", email);
        Assert.Equal("senha123", senha);
    }
}