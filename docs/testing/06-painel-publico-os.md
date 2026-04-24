# 06. Painel Público de Status de OS

Endpoint **anônimo** para o cliente final acompanhar o status da Ordem de Serviço pelo Id.

## Endpoint

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/Publico/os/{id}/status` | Status atual da OS |

## Pré-condição

Existir uma OS persistida. A API hoje **não expõe endpoint de criação de OS**, então para testar é preciso inserir uma OS direto no banco. Exemplo via `psql`:

```bash
docker exec -it oficina_postgres psql -U postgres -d OficinaDB
```

```sql
INSERT INTO "OrdensServico" ("Id", "VeiculoId", "StatusOS", "DataAbertura")
VALUES ('11111111-1111-1111-1111-111111111111',
        '<id de um veículo existente>',
        1,                               -- 1 = EmExecucao (consulte o enum StatusOS)
        NOW());
```

> Nomes de tabelas/colunas podem variar conforme a migration. Se diferir, rode `\dt` para listar as tabelas e `\d "OrdensServico"` para ver as colunas.

---

## Caminho feliz

`GET /Publico/os/11111111-1111-1111-1111-111111111111/status` *(sem header `Authorization`)*

`200 OK`

```json
{
  "osId": "11111111-1111-1111-1111-111111111111",
  "status": "EmExecucao",
  "atualizadoEm": "2026-04-24T15:42:00Z"
}
```

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| OS inexistente | `GET /Publico/os/{guid-aleatório}/status` | `404 Not Found` com `{ "mensagem": "Ordem de serviço {id} não encontrada." }` |
| Id em formato inválido | `GET /Publico/os/abc/status` | `400 Bad Request` (rota não casa com `:guid`) |

## Cenário de teste sugerido

1. Insira a OS no banco com `StatusOS = 0` (Aberta).
2. Chame o endpoint → status `Aberta`.
3. Atualize no banco para `StatusOS = 1` (EmExecucao) → chame de novo → status atualizado.
4. Chame com GUID aleatório → `404`.
