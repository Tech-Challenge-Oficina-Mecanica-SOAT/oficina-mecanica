namespace OficinaMecanica.Domain.Entities
{
    public class OrdemServicoServico
    {
        public Guid OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;

        public Guid ServicoId { get; set; }
        public Servico Servico { get; set; } = null!;
    }
}
