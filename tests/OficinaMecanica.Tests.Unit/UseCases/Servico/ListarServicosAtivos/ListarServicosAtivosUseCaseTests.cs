using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Servico.ListarServicosAtivos;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.ListarServicosAtivos;

public class ListarServicosAtivosUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaApenasAtivos()
    {
        var repo = new Mock<IServicoRepository>();
        repo.Setup(r => r.GetAtivosAsync()).ReturnsAsync(new[] { new Domain.Entities.Servico("a", "b", 10m) });
        var sut = new ListarServicosAtivosUseCase(repo.Object, new ServicoMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
