using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities
{
    public class PecaTests
    {
        [Fact]
        public void DeveCriarPecaComValoresValidos()
        {
            // Arrange & Act
            var peca = new Peca
            {
                Nome = "Filtro de Óleo",
                Codigo = "FO-001",
                PrecoUnitario = 45.50m,
                Estoque = 100,
                Descricao = "Filtro de óleo para motor"
            };

            // Assert
            Assert.NotEqual(Guid.Empty, peca.Id);
            Assert.Equal("Filtro de Óleo", peca.Nome);
            Assert.Equal("FO-001", peca.Codigo);
            Assert.Equal(45.50m, peca.PrecoUnitario);
            Assert.Equal(100, peca.Estoque);
            Assert.Equal("Filtro de óleo para motor", peca.Descricao);
        }

        [Fact]
        public void DeveGerarIdUnicoAutomaticamente()
        {
            // Arrange & Act
            var peca1 = new Peca { Nome = "Peca 1", Codigo = "P1", PrecoUnitario = 10, Estoque = 5 };
            var peca2 = new Peca { Nome = "Peca 2", Codigo = "P2", PrecoUnitario = 20, Estoque = 10 };

            // Assert
            Assert.NotEqual(peca1.Id, peca2.Id);
        }

        [Fact]
        public void DeveTerCriadoEmComDataUtcNow()
        {
            // Arrange
            var agora = DateTime.UtcNow;

            // Act
            var peca = new Peca
            {
                Nome = "Peca Teste",
                Codigo = "PT-001",
                PrecoUnitario = 50,
                Estoque = 10
            };

            // Assert
            Assert.True(peca.CriadoEm >= agora);
            Assert.Null(peca.AtualizadoEm);
        }

        [Fact]
        public void DevePermitirDescricaoNula()
        {
            // Arrange & Act
            var peca = new Peca
            {
                Nome = "Peca Sem Descricao",
                Codigo = "PSD-001",
                PrecoUnitario = 30,
                Estoque = 5,
                Descricao = null
            };

            // Assert
            Assert.Null(peca.Descricao);
        }

        [Fact]
        public void DevePermitirAtualizarAtualizadoEm()
        {
            // Arrange
            var peca = new Peca
            {
                Nome = "Peca Atualizavel",
                Codigo = "PA-001",
                PrecoUnitario = 60,
                Estoque = 8
            };
            var dataAtualizacao = DateTime.UtcNow.AddHours(1);

            // Act
            peca.AtualizadoEm = dataAtualizacao;

            // Assert
            Assert.Equal(dataAtualizacao, peca.AtualizadoEm);
        }

        [Fact]
        public void DevePermitirValoresDecimaisComDuasCasas()
        {
            // Arrange & Act
            var peca = new Peca
            {
                Nome = "Peca Decimal",
                Codigo = "PD-001",
                PrecoUnitario = 123.45m,
                Estoque = 15
            };

            // Assert
            Assert.Equal(123.45m, peca.PrecoUnitario);
        }
    }
}