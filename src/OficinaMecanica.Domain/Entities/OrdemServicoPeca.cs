namespace OficinaMecanica.Domain.Entities
{
    public class OrdemServicoPeca
    {
        public Guid OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;

        public Guid PecaInsumoId { get; set; }
        public PecaInsumo PecaInsumo { get; set; } = null!;

        public int Quantidade { get; set; }
    }
}
