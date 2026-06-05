using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Servico.ListarServicos;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.ListarServicos;

public class ListarServicosUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaLista()
    {
        var repo = new Mock<IServicoRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { new Domain.Entities.Servico("a", "b", 10m) });
        var sut = new ListarServicosUseCase(repo.Object, new ServicoMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
