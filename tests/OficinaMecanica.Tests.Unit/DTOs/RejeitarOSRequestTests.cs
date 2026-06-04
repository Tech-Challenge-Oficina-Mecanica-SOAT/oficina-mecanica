using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Tests.Unit.DTOs
{
    public class RejeitarOSRequestTests
    {
        [Fact]
        public void DeveCriarRejeitarOSDto_ComMotivoValido()
        {
            // Arrange
            var motivo = "Peça indisponível";

            // Act
            var dto = new RejeitarOSRequest(motivo);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(motivo, dto.Motivo);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Motivo com caracteres especiais: !@#$%")]
        [InlineData("Motivo muito longo para demonstração de casos de uso")]
        public void DeveCriarRejeitarOSDto_ComVariosMotivos(string motivo)
        {
            // Act
            var dto = new RejeitarOSRequest(motivo);

            // Assert
            Assert.Equal(motivo, dto.Motivo);
        }

        [Fact]
        public void DeveCompararDoisRejeitarOSDto_ComMesmoMotivo()
        {
            // Arrange
            var motivo = "Orçamento recusado pelo cliente";
            var dto1 = new RejeitarOSRequest(motivo);
            var dto2 = new RejeitarOSRequest(motivo);

            // Act & Assert
            Assert.Equal(dto1, dto2);
        }

        [Fact]
        public void NaoDeveCompararDoisRejeitarOSDto_ComMotivoDiferente()
        {
            // Arrange
            var dto1 = new RejeitarOSRequest("Motivo 1");
            var dto2 = new RejeitarOSRequest("Motivo 2");

            // Act & Assert
            Assert.NotEqual(dto1, dto2);
        }

        [Fact]
        public void DeveDesconstruirRejeitarOSRequest()
        {
            // Arrange
            var motivo = "Falha na inspeção";
            var dto = new RejeitarOSRequest(motivo);

            // Act
            var motivoObtido = dto.Motivo;

            // Assert
            Assert.Equal(motivo, motivoObtido);
        }

        [Fact]
        public void DeveRetornarStringRepresentacao()
        {
            // Arrange
            var motivo = "Teste de representação";
            var dto = new RejeitarOSRequest(motivo);

            // Act
            var toString = dto.ToString();

            // Assert
            Assert.Contains("RejeitarOSRequest", toString);
            Assert.Contains(motivo, toString);
        }
    }
}
