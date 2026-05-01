# 08. Ciclo de Vida e Status da OS

Transições de status da Ordem de Serviço e consulta do histórico de alterações.

## Status disponíveis (`EnumStatusOS`)

| Valor | Nome | Descrição |
|-------|------|-----------|
| 1 | `Recebida` | OS aberta, aguardando diagnóstico |
| 2 | `EmDiagnostico` | Mecânico inspecionando o veículo |
| 3 | `AguardandoAprovacao` | Orçamento enviado ao cliente |
| 4 | `EmExecucao` | Cliente aprovou; serviço em andamento |
| 5 | `Finalizada` | Serviço concluído, aguardando retirada |
| 6 | `Entregue` | Veículo entregue ao cliente |
| 7 | `Rejeitada` | Cliente recusou o orçamento |

## Fluxo normal

```
Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue
                                               ↘ Rejeitada
```

## Endpoints

| Método | Rota | Transição | Perfil |
|--------|------|-----------|--------|
| PATCH | `/api/ordens-servico/{id}/iniciar-diagnostico` | `Recebida → EmDiagnostico` | Admin, Mecânico |
| PATCH | `/api/ordens-servico/{id}/aprovar` | `AguardandoAprovacao → EmExecucao` | Admin, Cliente |
| PATCH | `/api/ordens-servico/{id}/rejeitar` | `AguardandoAprovacao → Rejeitada` | Admin, Cliente |
| PATCH | `/api/ordens-servico/{id}/notificar-conclusao` | `EmExecucao → Finalizada` | Admin, Mecânico |
| PATCH | `/api/ordens-servico/{id}/entregar` | `Finalizada → Entregue` | Admin |
| PATCH | `/api/ordens-servico/{id}/status` | Qualquer transição forçada | Admin |
| GET | `/api/ordens-servico/{id}/historico` | — | Admin, Mecânico, Cliente |

> **Nota:** a transição `EmDiagnostico → AguardandoAprovacao` não possui endpoint dedicado. Use `PATCH /api/ordens-servico/{id}/status` com `novoStatus: 3` para avançar manualmente.

---

## Pré-condições

- Token JWT com o perfil indicado em cada endpoint.
- OS existente (ver [07](./07-ordens-servico.md)). Guarde o `id`.

---

## Caminho feliz — fluxo completo

### 1. Iniciar diagnóstico (Admin ou Mecânico)

`PATCH /api/ordens-servico/{id}/iniciar-diagnostico` — sem body.

**`204 No Content`**. Status: `Recebida → EmDiagnostico`.

### 2. Avançar para AguardandoAprovacao (Admin)

`PATCH /api/ordens-servico/{id}/status`

```json
{
  "novoStatus": 3,
  "motivo": "Diagnóstico concluído, orçamento enviado ao cliente"
}
```

**`204 No Content`**. Status: `EmDiagnostico → AguardandoAprovacao`.

### 3a. Aprovar orçamento (Admin ou Cliente)

`PATCH /api/ordens-servico/{id}/aprovar` — sem body.

**`204 No Content`**. Status: `AguardandoAprovacao → EmExecucao`.

### 3b. Rejeitar orçamento (Admin ou Cliente)

`PATCH /api/ordens-servico/{id}/rejeitar`

```json
{ "motivo": "Valor fora do orçamento do cliente" }
```

**`204 No Content`**. Status: `AguardandoAprovacao → Rejeitada`.

### 4. Notificar conclusão (Admin ou Mecânico)

`PATCH /api/ordens-servico/{id}/notificar-conclusao` — sem body.

**`204 No Content`**. Status: `EmExecucao → Finalizada`.

### 5. Entregar veículo (Admin)

`PATCH /api/ordens-servico/{id}/entregar` — sem body.

**`204 No Content`**. Status: `Finalizada → Entregue`.

### 6. Consultar histórico (Admin, Mecânico ou Cliente)

`GET /api/ordens-servico/{id}/historico`

**`200 OK`**:

```json
[
  {
    "id": "...",
    "ordemServicoId": "...",
    "statusAnterior": null,
    "statusNovo": "Recebida",
    "alteradoEm": "2026-04-30T17:00:00Z",
    "alteradoPor": "sistema",
    "motivo": "Criação inicial"
  },
  {
    "statusAnterior": "Recebida",
    "statusNovo": "EmDiagnostico",
    "alteradoPor": "mecanico@oficina.com",
    "motivo": "Diagnóstico iniciado"
  }
]
```

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| Transição inválida | `PATCH .../aprovar` em OS com status `Recebida` | `400 Bad Request` com mensagem da transição inválida |
| OS inexistente | qualquer PATCH com GUID aleatório | `404 Not Found` |
| Perfil sem permissão | `PATCH .../entregar` com token de Mecânico | `403 Forbidden` |
| Sem token | qualquer endpoint | `401 Unauthorized` |
| Rejeitar sem motivo | `PATCH .../rejeitar` com body vazio | `400 Bad Request` |
| `novoStatus` igual ao atual | `PATCH .../status` com o mesmo status | `400 Bad Request` |

---

## Cenário de teste sugerido (ponta a ponta)

1. Criar OS → status `Recebida`.
2. Adicionar serviços e peças (ver [07](./07-ordens-servico.md)).
3. `iniciar-diagnostico` → `EmDiagnostico`.
4. Forçar `AguardandoAprovacao` via `PATCH /status` com `novoStatus: 3`.
5. `aprovar` → `EmExecucao`.
6. `notificar-conclusao` → `Finalizada`.
7. `entregar` → `Entregue`.
8. `GET /historico` → verificar todas as 7 entradas.
9. `GET /Publico/os/{id}/status` (sem token) → confirmar status público `Entregue`.
