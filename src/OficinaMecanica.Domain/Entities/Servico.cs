using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaMecanica.Domain.Entities
{
    public class Servico
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }

        public ICollection<OrdemServicoServico> OrdensServico { get; set; } = new List<OrdemServicoServico>();
    }
}
