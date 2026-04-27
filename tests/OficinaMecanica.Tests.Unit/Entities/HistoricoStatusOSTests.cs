using FluentAssertions;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities;

public class HistoricoStatusOSTests
{
    [Fact]
    public void Construtor_ComDadosValidos_CriaHistorico()
    {
        var osId = Guid.NewGuid();
        var historico = new HistoricoStatusOS(
            osId,
            statusAnterior: null,
            statusNovo: EnumStatusOS.Recebida,
            alteradoPor: "sistema",
            motivo: "Criação inicial"
        );

        historico.Id.Should().NotBe(Guid.Empty);
        historico.OrdemServicoId.Should().Be(osId);
        historico.StatusAnterior.Should().BeNull();
        historico.StatusNovo.Should().Be(EnumStatusOS.Recebida);
        historico.AlteradoPor.Should().Be("sistema");
        historico.Motivo.Should().Be("Criação inicial");
        historico.AlteradoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Construtor_ComOsIdVazio_LancaExcecao()
    {
        Action act = () => new HistoricoStatusOS(
            Guid.Empty, null, EnumStatusOS.Recebida, "sistema", "Criação inicial");

        act.Should().Throw<ArgumentException>().WithMessage("*OrdemServicoId*");
    }

    [Fact]
    public void Construtor_ComAlteradoPorVazio_LancaExcecao()
    {
        Action act = () => new HistoricoStatusOS(
            Guid.NewGuid(), null, EnumStatusOS.Recebida, " ", "Criação inicial");

        act.Should().Throw<ArgumentException>().WithMessage("*AlteradoPor*");
    }

    [Fact]
    public void Construtor_ComStatusAnteriorIgualAoNovo_LancaExcecao()
    {
        Action act = () => new HistoricoStatusOS(
            Guid.NewGuid(), EnumStatusOS.Recebida, EnumStatusOS.Recebida, "user", "x");

        act.Should().Throw<ArgumentException>().WithMessage("*mesmo*");
    }
}
