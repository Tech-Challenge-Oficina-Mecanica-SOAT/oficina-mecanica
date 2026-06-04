using FluentAssertions;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.Tests.Unit.Common;

public class ResultTests
{
    [Fact]
    public void Success_RetornaIsSuccessTrueComValor()
    {
        var result = Result<string>.Success("ok");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        result.ErrorType.Should().Be(ResultErrorType.None);
    }

    [Fact]
    public void Validation_RetornaIsSuccessFalseComMensagem()
    {
        var result = Result<string>.Validation("campo obrigatório");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("campo obrigatório");
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public void NotFound_DefineErrorTypeNotFound()
    {
        var result = Result<int>.NotFound("recurso ausente");
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public void Conflict_DefineErrorTypeConflict()
    {
        var result = Result<int>.Conflict("já existe");
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public void Unauthorized_DefineErrorTypeUnauthorized()
    {
        var result = Result<int>.Unauthorized("sem acesso");
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
    }
}
