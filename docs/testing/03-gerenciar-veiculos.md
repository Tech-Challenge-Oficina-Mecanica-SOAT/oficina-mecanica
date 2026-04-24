# 03. Gerenciar Veículos

CRUD de veículos. Todo veículo precisa estar associado a um cliente existente.
**Todos os endpoints exigem perfil `Admin`.**

## Pré-condições

- Token JWT de Admin.
- `clienteId` de um cliente já cadastrado (ver [02](./02-gerenciar-clientes.md)).

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/Veiculos` | Lista todos |
| GET | `/api/Veiculos/{id}` | Busca por Id |
| GET | `/api/Veiculos/placa/{placa}` | Busca por placa |
| GET | `/api/Veiculos/cliente/{clienteId}` | Veículos de um cliente |
| POST | `/api/Veiculos` | Cria |
| PUT | `/api/Veiculos/{id}` | Atualiza |
| DELETE | `/api/Veiculos/{id}` | Remove |

---

## Caminho feliz

### 1. Criar: `POST /api/Veiculos`

```json
{
  "clienteId": "<id do cliente do passo 02>",
  "placa": "ABC1D23",
  "marca": "Volkswagen",
  "modelo": "Gol",
  "ano": 2020
}
```

`201 Created`. Guarde o `id`.

### 2. Consultar
- `GET /api/Veiculos/placa/ABC1D23` → `200 OK`.
- `GET /api/Veiculos/cliente/{clienteId}` → `200 OK` com a lista.

### 3. Atualizar: `PUT /api/Veiculos/{id}`

```json
{
  "placa": "ABC1D23",
  "marca": "Volkswagen",
  "modelo": "Gol G6",
  "ano": 2021
}
```

`200 OK`.

### 4. Remover
`DELETE /api/Veiculos/{id}` → `204 No Content`.

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| Cliente inexistente | `POST /api/Veiculos` com `clienteId` aleatório | `404 Not Found` |
| Placa duplicada | repetir `POST /api/Veiculos` com mesma placa | `400 Bad Request` |
| Ano inválido (ex.: 1800) | `POST /api/Veiculos` com `"ano": 1800` | `400 Bad Request` |
| Buscar placa inexistente | `GET /api/Veiculos/placa/XXX0X00` | `404 Not Found` |
| Atualizar id inexistente | `PUT /api/Veiculos/{guid-aleatório}` | `404 Not Found` |
| Sem token | `GET /api/Veiculos` sem `Authorization` | `401 Unauthorized` |

## Próximo passo

[04. Catálogo de serviços](./04-catalogo-servicos.md)
