# 05. Controle de Peças e Estoque

Cadastro do catálogo de peças/insumos e movimentações de estoque.

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/Pecas` | Lista todas |
| GET | `/api/Pecas/{id}` | Busca por Id |
| GET | `/api/Pecas/codigo/{codigo}` | Busca por código |
| GET | `/api/Pecas/estoque-baixo?limite=10` | Estoque ≤ limite |
| GET | `/api/Pecas/{id}/estoque` | Estoque atual |
| POST | `/api/Pecas` | Cria |
| PUT | `/api/Pecas/{id}` | Atualiza dados |
| PATCH | `/api/Pecas/{id}/estoque` | Movimenta estoque |
| DELETE | `/api/Pecas/{id}` | Remove |

---

## Caminho feliz

### 1. Criar peça: `POST /api/Pecas`

```json
{
  "nome": "Filtro de óleo",
  "codigo": "FO-001",
  "precoUnitario": 45.90,
  "estoque": 50,
  "descricao": "Filtro motor 1.0 a 2.0"
}
```

`201 Created`. Guarde o `id`.

### 2. Entrada de estoque: `PATCH /api/Pecas/{id}/estoque`

```json
{ "quantidade": 20, "tipoOperacao": "incrementar" }
```

`200 OK` com `novoEstoque: 70`.

### 3. Saída de estoque (uso em OS)

```json
{ "quantidade": 5, "tipoOperacao": "decrementar" }
```

`200 OK` com `novoEstoque: 65`.

### 4. Cenário ponta a ponta sugerido

1. Criar peça com estoque = 50.
2. Decrementar 45 → estoque = 5.
3. `GET /api/Pecas/estoque-baixo?limite=10` → a peça aparece.
4. Incrementar 100 → estoque = 105.
5. `GET /api/Pecas/estoque-baixo?limite=10` → não aparece mais.

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| Código duplicado | repetir `POST /api/Pecas` com mesmo `codigo` | `400 Bad Request` |
| Estoque insuficiente | `PATCH .../estoque` decrementando mais que o saldo | `400 Bad Request` |
| `tipoOperacao` inválido | enviar `"tipoOperacao": "zerar"` | `400 Bad Request` |
| Quantidade negativa | enviar `"quantidade": -5` | `400 Bad Request` |
| Movimentar peça inexistente | `PATCH /api/Pecas/{guid-aleatório}/estoque` | `404 Not Found` |
| Buscar código inexistente | `GET /api/Pecas/codigo/XX-999` | `404 Not Found` |

## Próximo passo

[06. Painel público de OS](./06-painel-publico-os.md)
