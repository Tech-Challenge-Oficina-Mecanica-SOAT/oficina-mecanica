# Guia de Testes da API Oficina Mecânica

Este diretório contém roteiros para testar os fluxos da API, organizados por **caso de uso**, com **caminho feliz** e **caminho triste** (erros esperados).

A documentação interativa do projeto é gerada via **Scalar** (em `/scalar`). Não há Swagger UI.

## TL;DR: Quickstart (5 minutos)

> Recomendado para uma primeira avaliação rápida do projeto.

1. **Suba a aplicação:**
   ```bash
   docker compose up -d
   # API: http://localhost:5000
   ```
2. **Abra o Scalar:** <http://localhost:5000/scalar>
3. **Registre um Admin** no endpoint `POST /Auth/registrar`. O body já vem pré-preenchido com um exemplo válido. Clique em *Send*.
4. **Faça login** no endpoint `POST /Auth/login`, mesmo body. Copie o `token` da resposta.
5. **Autentique-se na UI:** botão *Authentication* (canto superior direito do Scalar) → escolha **Bearer** → cole o token.
6. **Execute o fluxo completo** seguindo os arquivos numerados (`01` ao `06`) deste diretório.

Alternativa para quem prefere terminal/IDE: abra [`oficina.http`](./oficina.http) no VS Code (extensão *REST Client*) ou Rider. Ele já encadeia as variáveis (token, clienteId, etc.) entre as requisições. Basta clicar em "Send Request" de cima para baixo.

## Como autenticar

| Forma de uso | Como fazer |
|----|-----------|
| **Scalar** (`/scalar`) | Botão *Authentication* → esquema *Bearer* → cole apenas o JWT |
| **`oficina.http`** | Token capturado automaticamente em `@token` após o `POST /Auth/login` |

## Casos de uso

| # | Caso de uso | Perfil exigido | Documento |
|---|-------------|---------------|-----------|
| 1 | Autenticação (registrar Admin + login) | Anônimo | [01-autenticacao.md](./01-autenticacao.md) |
| 2 | Gerenciar clientes | Admin | [02-gerenciar-clientes.md](./02-gerenciar-clientes.md) |
| 3 | Gerenciar veículos | Admin | [03-gerenciar-veiculos.md](./03-gerenciar-veiculos.md) |
| 4 | Catálogo de serviços | Aberto | [04-catalogo-servicos.md](./04-catalogo-servicos.md) |
| 5 | Controle de peças e estoque | Aberto | [05-controle-pecas-estoque.md](./05-controle-pecas-estoque.md) |
| 6 | Painel público de OS | Anônimo | [06-painel-publico-os.md](./06-painel-publico-os.md) |

## Convenção neste guia

Cada arquivo tem duas seções principais:

- **Caminho feliz**: sequência que termina com sucesso (`2xx`).
- **Caminho triste**: entradas inválidas ou pré-condições não atendidas, com o status HTTP esperado (`4xx`).

## Fluxo recomendado para uma demo ponta a ponta

1. `01`: registrar Admin e logar.
2. `02`: criar cliente, atualizar, desativar/ativar.
3. `03`: cadastrar veículo associado ao cliente.
4. `04`: cadastrar/atualizar serviço.
5. `05`: cadastrar peça e movimentar estoque (incrementar e decrementar).
6. `06`: consultar status público de uma OS pelo Id.
