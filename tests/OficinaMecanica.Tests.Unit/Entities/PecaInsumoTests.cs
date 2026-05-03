using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities
{

    public class PecaInsumoTests
    {
        private const string NomeValido = "Óleo Motor";
        private const string CodigoValido = "OM-001";
        private const string DescricaoValida = "Óleo motor 5W30";
        private const decimal PrecoValido = 50m;
        private const int QuantidadeValida = 10;

        [Fact]
        public void Construtor_ComParametrosValidos_DeveAtualizarPropriedades()
        {
            // Arrange & Act
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Assert
            Assert.NotEqual(Guid.Empty, peca.Id);
            Assert.Equal(NomeValido, peca.Nome);
            Assert.Equal(CodigoValido, peca.Codigo);
            Assert.Equal(DescricaoValida, peca.Descricao);
            Assert.Equal(PrecoValido, peca.Preco);
            Assert.Equal(QuantidadeValida, peca.Quantidade);
            Assert.True(peca.Ativo);
            Assert.NotEqual(DateTime.MinValue, peca.CriadoEm);
        }

        [Fact]
        public void Construtor_ComNomeNulo_DeveLancarArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new PecaInsumo(null!, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida));
        }

        [Fact]
        public void Construtor_ComNomeVazio_DeveLancarArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new PecaInsumo(string.Empty, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida));
        }

        [Fact]
        public void Construtor_ComPrecoZero_DeveLancarArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, 0, QuantidadeValida));
        }

        [Fact]
        public void Construtor_ComPrecoNegativo_DeveLancarArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, -10m, QuantidadeValida));
        }

        [Fact]
        public void Construtor_ComQuantidadeNegativa_DeveLancarArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, -5));
        }

        [Fact]
        public void Construtor_ComCodigoNulo_DeveDefinirStringVazia()
        {
            // Arrange & Act
            var peca = new PecaInsumo(NomeValido, null!, DescricaoValida, PrecoValido, QuantidadeValida);

            // Assert
            Assert.Equal(string.Empty, peca.Codigo);
        }

        [Fact]
        public void Construtor_ComDescricaoNula_DeveDefinirStringVazia()
        {
            // Arrange & Act
            var peca = new PecaInsumo(NomeValido, CodigoValido, null!, PrecoValido, QuantidadeValida);

            // Assert
            Assert.Equal(string.Empty, peca.Descricao);
        }

        [Fact]
        public void Atualizar_ComParametrosValidos_DeveAtualizarPropriedades()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);
            var novoNome = "Óleo Premium";
            var novaDescricao = "Óleo motor 10W40";
            var novoPreco = 75m;
            var novaQuantidade = 20;

            // Act
            peca.Atualizar(novoNome, novaDescricao, novoPreco, novaQuantidade);

            // Assert
            Assert.Equal(novoNome, peca.Nome);
            Assert.Equal(novaDescricao, peca.Descricao);
            Assert.Equal(novoPreco, peca.Preco);
            Assert.Equal(novaQuantidade, peca.Quantidade);
        }

        [Fact]
        public void Atualizar_ComNomeNulo_DeveLancarArgumentException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                peca.Atualizar(null!, DescricaoValida, PrecoValido, QuantidadeValida));
        }

        [Fact]
        public void Atualizar_ComPrecoInvalido_DeveLancarArgumentException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                peca.Atualizar(NomeValido, DescricaoValida, 0, QuantidadeValida));
        }

        [Fact]
        public void Atualizar_ComQuantidadeNegativa_DeveLancarArgumentException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                peca.Atualizar(NomeValido, DescricaoValida, PrecoValido, -1));
        }

        [Fact]
        public void IncrementarEstoque_ComQuantidadeValida_DeveAumentarQuantidade()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);
            var incremento = 5;

            // Act
            peca.IncrementarEstoque(incremento);

            // Assert
            Assert.Equal(QuantidadeValida + incremento, peca.Quantidade);
        }

        [Fact]
        public void IncrementarEstoque_ComQuantidadeZero_DeveLancarArgumentException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peca.IncrementarEstoque(0));
        }

        [Fact]
        public void IncrementarEstoque_ComQuantidadeNegativa_DeveLancarArgumentException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peca.IncrementarEstoque(-5));
        }

        [Fact]
        public void DecrementarEstoque_ComQuantidadeValida_DeveReduzirQuantidade()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);
            var decremento = 3;

            // Act
            peca.DecrementarEstoque(decremento);

            // Assert
            Assert.Equal(QuantidadeValida - decremento, peca.Quantidade);
        }

        [Fact]
        public void DecrementarEstoque_ComQuantidadeZero_DeveLancarArgumentException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peca.DecrementarEstoque(0));
        }

        [Fact]
        public void DecrementarEstoque_ComQuantidadeNegativa_DeveLancarArgumentException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peca.DecrementarEstoque(-5));
        }

        [Fact]
        public void DecrementarEstoque_ComQuantidadeInsuficiente_DeveLancarInvalidOperationException()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, 5);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => peca.DecrementarEstoque(10));
        }

        [Fact]
        public void Desativar_DeveDefinirAtivoComoFalso()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Act
            peca.Desativar();

            // Assert
            Assert.False(peca.Ativo);
        }

        [Fact]
        public void Ativar_DeveDefinirAtivoComoVerdadeiro()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);
            peca.Desativar();

            // Act
            peca.Ativar();

            // Assert
            Assert.True(peca.Ativo);
        }

        [Fact]
        public void OrdensServico_DeveSerInicializadoComoListaVazia()
        {
            // Arrange & Act
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, QuantidadeValida);

            // Assert
            Assert.NotNull(peca.OrdensServico);
            Assert.Empty(peca.OrdensServico);
        }

        [Fact]
        public void DecrementarEstoque_ComQuantidadeIgualAoTotal_DeveZerar()
        {
            // Arrange
            var peca = new PecaInsumo(NomeValido, CodigoValido, DescricaoValida, PrecoValido, 10);

            // Act
            peca.DecrementarEstoque(10);

            // Assert
            Assert.Equal(0, peca.Quantidade);
        }
    }
}
