using System;
using System.Collections.Generic;

namespace OficinaMecanica.Domain.Entities
{
    public class PecaInsumo
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Codigo { get; private set; } = string.Empty;
        public decimal Preco { get; private set; }
        public int Quantidade { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public bool Ativo { get; private set; }

        public ICollection<OrdemServicoPeca> OrdensServico { get; set; } = new List<OrdemServicoPeca>();

        private PecaInsumo() { }

        public PecaInsumo(string nome, string codigo, decimal preco, int quantidade)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório");
            
            if (preco <= 0)
                throw new ArgumentException("Preço deve ser maior que zero");
            
            if (quantidade < 0)
                throw new ArgumentException("Quantidade não pode ser negativa");
            
            Id = Guid.NewGuid();
            Nome = nome;
            Codigo = codigo ?? string.Empty;
            Preco = preco;
            Quantidade = quantidade;
            CriadoEm = DateTime.UtcNow;
            Ativo = true;
        }

        public void Atualizar(string nome, decimal preco, int quantidade)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório");
            
            if (preco <= 0)
                throw new ArgumentException("Preço deve ser maior que zero");
            
            if (quantidade < 0)
                throw new ArgumentException("Quantidade não pode ser negativa");
            
            Nome = nome;
            Preco = preco;
            Quantidade = quantidade;
        }

        public void IncrementarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");
            Quantidade += quantidade;
        }

        public void DecrementarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");
            if (Quantidade < quantidade)
                throw new InvalidOperationException("Estoque insuficiente");
            Quantidade -= quantidade;
        }

        public void Desativar() => Ativo = false;
        public void Ativar() => Ativo = true;
    }
}
