using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Tests.Unit.DTOs
{
    public class PainelStatusOSDtoTests
    {
        [Fact]
        public void Constructor_CreateValidInstance_Success()
        {
            // Arrange
            var osId = Guid.NewGuid();
            var status = "Aberto";
            var atualizadoEm = DateTime.Now;

            // Act
            var dto = new PainelStatusOSDto(osId, status, atualizadoEm);

            // Assert
            Assert.Equal(osId, dto.OsId);
            Assert.Equal(status, dto.Status);
            Assert.Equal(atualizadoEm, dto.AtualizadoEm);
        }

        [Fact]
        public void Equality_TwoInstancesWithSameValues_AreEqual()
        {
            // Arrange
            var osId = Guid.NewGuid();
            var status = "Fechado";
            var atualizadoEm = DateTime.Now;

            // Act
            var dto1 = new PainelStatusOSDto(osId, status, atualizadoEm);
            var dto2 = new PainelStatusOSDto(osId, status, atualizadoEm);

            // Assert
            Assert.Equal(dto1, dto2);
        }

        [Fact]
        public void Inequality_TwoInstancesWithDifferentValues_AreNotEqual()
        {
            // Arrange
            var osId1 = Guid.NewGuid();
            var osId2 = Guid.NewGuid();

            // Act
            var dto1 = new PainelStatusOSDto(osId1, "Aberto", DateTime.Now);
            var dto2 = new PainelStatusOSDto(osId2, "Fechado", DateTime.Now);

            // Assert
            Assert.NotEqual(dto1, dto2);
        }

        [Theory]
        [InlineData("Aberto")]
        [InlineData("Fechado")]
        [InlineData("Em Andamento")]
        [InlineData("Cancelado")]
        public void Constructor_WithDifferentStatuses_CreatesInstanceCorrectly(string status)
        {
            // Arrange
            var osId = Guid.NewGuid();
            var atualizadoEm = DateTime.Now;

            // Act
            var dto = new PainelStatusOSDto(osId, status, atualizadoEm);

            // Assert
            Assert.Equal(status, dto.Status);
        }
    }
}
