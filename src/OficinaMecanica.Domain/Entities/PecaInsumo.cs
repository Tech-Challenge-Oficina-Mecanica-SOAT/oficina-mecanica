using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaMecanica.Domain.Entities
{
    public class PecaInsumo
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }

        public ICollection<OrdemServicoPeca> OrdensServico { get; set; } = new List<OrdemServicoPeca>();
    }
}
