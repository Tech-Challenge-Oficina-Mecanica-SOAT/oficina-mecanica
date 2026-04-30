# 06. Painel Público de Status de OS

Endpoint **anônimo** para o cliente final acompanhar o status da Ordem de Serviço pelo Id.

## Endpoint

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/Publico/os/{id}/status` | Status atual da OS |

## Pré-condição

Existir uma OS persistida. Crie uma via API seguindo o guia [07-ordens-servico.md](./07-ordens-servico.md) e guarde o `id` retornado.

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

1. Crie uma OS via `POST /api/ordens-servico` (ver [07](./07-ordens-servico.md)) → status inicial `Recebida`.
2. Chame o endpoint público → status `Recebida`.
3. Avance o status via `PATCH /api/ordens-servico/{id}/iniciar-diagnostico` (ver [08](./08-status-os.md)) → chame de novo → status `EmDiagnostico`.
4. Chame com GUID aleatório → `404`.
