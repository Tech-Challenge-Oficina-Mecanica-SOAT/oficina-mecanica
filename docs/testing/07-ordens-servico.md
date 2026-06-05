# 07. Ordens de Serviço

Criação e gestão de OS: abertura, adição/remoção de itens (serviços e peças) e consulta do orçamento consolidado.

**Todos os endpoints exigem autenticação com perfil `Admin`.**

## Pré-condições

- Token JWT de Admin (ver [01](./01-autenticacao.md)).
- `clienteId` de um cliente existente (ver [02](./02-gerenciar-clientes.md)).
- `veiculoId` de um veículo existente, associado ao cliente (ver [03](./03-gerenciar-veiculos.md)).
- `servicoId` de um serviço existente (ver [04](./04-catalogo-servicos.md)).
- `pecaId` de uma peça existente (ver [05](./05-controle-pecas-estoque.md)).

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/ordens-servico` | Lista todas as OS (resumo) |
| GET | `/api/ordens-servico/{id}` | Detalhe da OS com todos os itens |
| POST | `/api/ordens-servico` | Abre uma nova OS |
| POST | `/api/ordens-servico/{id}/itens` | Adiciona item à OS |
| DELETE | `/api/ordens-servico/{id}/itens/{itemId}` | Remove item da OS |
| GET | `/api/ordens-servico/tempo-medio-execucao` | Tempo médio das OS finalizadas (horas) |

---

## Caminho feliz

### 1. Abrir OS: `POST /api/ordens-servico`

```json
{
  "clienteId": "<id do cliente>",
  "veiculoId": "<id do veículo>",
  "observacoes": "Barulho ao frear no lado esquerdo"
}
```

**`201 Created`**. Guarde o `id`. Status inicial: `Recebida`.

### 2. Consultar a OS: `GET /api/ordens-servico/{id}`

**`200 OK`** com a OS completa:

```json
{
  "id": "...",
  "clienteNome": "João da Silva",
  "veiculoDescricao": "Volkswagen Gol 2020 - ABC1D23",
  "status": "Recebida",
  "observacoes": "Barulho ao frear no lado esquerdo",
  "total": 0.00,
  "dataAbertura": "2026-04-30T17:00:00Z",
  "dataFechamento": null,
  "itens": []
}
```

### 3. Adicionar serviço: `POST /api/ordens-servico/{id}/itens`

```json
{
  "tipo": "servico",
  "referenciaId": "<id do serviço>",
  "quantidade": 1
}
```

**`201 Created`**. O campo `total` da OS é atualizado automaticamente.

### 4. Adicionar peça: `POST /api/ordens-servico/{id}/itens`

```json
{
  "tipo": "peca",
  "referenciaId": "<id da peça>",
  "quantidade": 2
}
```

**`201 Created`**. O `total` agora reflete serviço + peça.

> O campo `tipo` aceita: `servico`, `peca` ou `insumo`.

### 5. Consultar orçamento

`GET /api/ordens-servico/{id}` → o campo `total` é o valor do orçamento consolidado; `itens` lista cada linha com seu subtotal.

### 6. Remover item: `DELETE /api/ordens-servico/{id}/itens/{itemId}`

**`204 No Content`**. O `total` é recalculado.

### 7. Listar todas as OS

`GET /api/ordens-servico` → **`200 OK`** com array resumido (sem itens).

### 8. Tempo médio de execução

`GET /api/ordens-servico/tempo-medio-execucao` → **`200 OK`**:

```json
{ "tempoMedioHoras": 4.5 }
```

> Retorna `0` enquanto não houver OS com status `Finalizada`.

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| Cliente inexistente | `POST /api/ordens-servico` com `clienteId` inválido | `400 Bad Request` |
| Veículo inexistente | `POST /api/ordens-servico` com `veiculoId` inválido | `400 Bad Request` |
| OS inexistente | `GET /api/ordens-servico/{guid-aleatório}` | `404 Not Found` |
| Adicionar item em OS inexistente | `POST /api/ordens-servico/{guid-aleatório}/itens` | `404 Not Found` |
| Remover item inexistente | `DELETE /api/ordens-servico/{id}/itens/{guid-aleatório}` | `404 Not Found` |
| `tipo` inválido | `POST .../itens` com `"tipo": "outros"` | `400 Bad Request` |
| Sem token | qualquer endpoint | `401 Unauthorized` |
| Token sem role Admin (Mecanico/Cliente) | qualquer endpoint | `403 Forbidden` |

## Próximo passo

[08. Ciclo de vida e status da OS](./08-status-os.md)
