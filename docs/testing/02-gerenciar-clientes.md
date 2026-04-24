# 02. Gerenciar Clientes

CRUD de clientes + ativação/desativação. **Todos os endpoints exigem perfil `Admin`.**

## Pré-condições

- Token JWT de Admin (ver [01](./01-autenticacao.md)).

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/Clientes` | Lista todos |
| GET | `/api/Clientes/{id}` | Busca por Id |
| GET | `/api/Clientes/documento/{documento}` | Busca por CPF/CNPJ |
| POST | `/api/Clientes` | Cria |
| PUT | `/api/Clientes/{id}` | Atualiza |
| DELETE | `/api/Clientes/{id}` | Remove |
| PATCH | `/api/Clientes/{id}/ativar` | Reativa |
| PATCH | `/api/Clientes/{id}/desativar` | Desativa |

---

## Caminho feliz

### 1. Criar: `POST /api/Clientes`

```json
{
  "nome": "João da Silva",
  "documento": "12345678901",
  "telefone": "11999998888",
  "email": "joao@email.com"
}
```

**`201 Created`**. Guarde o `id` retornado.

### 2. Consultar
- `GET /api/Clientes` → `200 OK` com a lista.
- `GET /api/Clientes/{id}` → `200 OK` com o cliente criado.
- `GET /api/Clientes/documento/12345678901` → `200 OK`.

### 3. Atualizar: `PUT /api/Clientes/{id}`

```json
{
  "nome": "João da Silva Junior",
  "telefone": "11988887777",
  "email": "joao.novo@email.com"
}
```

`200 OK`.

### 4. Desativar / reativar
- `PATCH /api/Clientes/{id}/desativar` → `204 No Content`.
- `PATCH /api/Clientes/{id}/ativar` → `204 No Content`.

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| Sem token | `GET /api/Clientes` sem `Authorization` | `401 Unauthorized` |
| Documento duplicado | `POST /api/Clientes` repetindo `documento` | `400 Bad Request` |
| Buscar por id inexistente | `GET /api/Clientes/00000000-0000-0000-0000-000000000000` | `404 Not Found` |
| Atualizar id inexistente | `PUT /api/Clientes/{guid-aleatório}` | `404 Not Found` |
| Deletar id inexistente | `DELETE /api/Clientes/{guid-aleatório}` | `404 Not Found` |

## Próximo passo

[03. Gerenciar veículos](./03-gerenciar-veiculos.md)
