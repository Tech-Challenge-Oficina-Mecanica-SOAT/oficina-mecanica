namespace OficinaMecanica.Domain.Entities
{
    public class Servico
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public bool Ativo { get; private set; }

        public ICollection<OrdemServicoServico> OrdensServico { get; set; } = new List<OrdemServicoServico>();

        public Servico() { }

        public Servico(string nome, string descricao, decimal valor)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório");

            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero");

            Id = Guid.NewGuid();
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Valor = valor;
            CriadoEm = DateTime.UtcNow;
            Ativo = true;
        }

        public void Atualizar(string nome, string descricao, decimal valor)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório");

            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero");

            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Valor = valor;
        }

        public void Desativar() => Ativo = false;
        public void Ativar() => Ativo = true;
    }
}
