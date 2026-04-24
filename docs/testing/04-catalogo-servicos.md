# 04. Catálogo de Serviços

Cadastro dos serviços oferecidos pela oficina (troca de óleo, alinhamento, etc.).

> O `ServicosController` atualmente está sem `[Authorize]`, então responde sem token. Mesmo assim, recomenda-se enviar o JWT para simular o uso em produção.

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/Servicos` | Lista todos |
| GET | `/api/Servicos/ativos` | Apenas ativos |
| GET | `/api/Servicos/{id}` | Busca por Id |
| GET | `/api/Servicos/nome/{nome}` | Busca por nome |
| POST | `/api/Servicos` | Cria |
| PUT | `/api/Servicos/{id}` | Atualiza |
| DELETE | `/api/Servicos/{id}` | Remove |
| PATCH | `/api/Servicos/{id}/ativar` | Ativa |
| PATCH | `/api/Servicos/{id}/desativar` | Desativa |

---

## Caminho feliz

### 1. Criar: `POST /api/Servicos`

```json
{
  "nome": "Troca de óleo",
  "descricao": "Troca de óleo do motor + filtro",
  "valor": 150.00
}
```

`201 Created`. Guarde o `id`.

### 2. Listar ativos
`GET /api/Servicos/ativos` → `200 OK`.

### 3. Atualizar: `PUT /api/Servicos/{id}`

```json
{
  "nome": "Troca de óleo premium",
  "descricao": "Óleo sintético + filtro",
  "valor": 220.00
}
```

`200 OK`.

### 4. Ativar / desativar / remover

```http
PATCH /api/Servicos/{id}/desativar
PATCH /api/Servicos/{id}/ativar
DELETE /api/Servicos/{id}
```

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| Valor negativo | `POST /api/Servicos` com `"valor": -10` | `400 Bad Request` |
| Buscar por nome inexistente | `GET /api/Servicos/nome/inexistente` | `404 Not Found` |
| Atualizar id inexistente | `PUT /api/Servicos/{guid-aleatório}` | `404 Not Found` |
| Deletar id inexistente | `DELETE /api/Servicos/{guid-aleatório}` | `404 Not Found` |
| Ativar id inexistente | `PATCH /api/Servicos/{guid-aleatório}/ativar` | `404 Not Found` |

## Próximo passo

[05. Controle de peças e estoque](./05-controle-pecas-estoque.md)
