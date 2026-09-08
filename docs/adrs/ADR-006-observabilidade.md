# ADR-006 — Ferramenta de Observabilidade

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito, implementado e testado |
| **Data**     | 2026-09-08 (retroativa; decisão tomada e implementada em `oficina-infra-k8s` antes desta data, formalizada agora) |
| **Autores**  | grupo Tech Challenge Oficina Mecânica |
| **Contexto** | Fase 3 — Tech Challenge FIAP   |

---

## Contexto

A Fase 3 exige monitoramento cobrindo latência, CPU/memória, health checks, logs estruturados e visibilidade de traces da aplicação.

---

## Decisão

Usar **New Relic** (conta free tier), implementado em `oficina-infra-k8s` via Helm (métricas de infraestrutura) e via agente .NET (APM da API).

## Razão

- Tier gratuito cobre bem o volume deste projeto.
- Plataforma única para infraestrutura, APM e logs, em vez de ferramentas separadas por pilar.
- Agente .NET maduro, com auto-instrumentação (não exige alterar código da aplicação).
- Agente Kubernetes (Helm chart `nri-bundle`) coleta métricas de nodes e pods sem trabalho manual de configuração por serviço.

## Alternativas descartadas

- **Datadog**: trial gratuito de 14 dias, insustentável ao longo de uma fase de várias semanas.
- **Prometheus + Grafana self-hosted**: mais controle, mas exige manter e operar mais um serviço dentro do cluster, sem necessidade real no escopo deste projeto.
- **CloudWatch + AWS X-Ray**: sem custo adicional, mas dashboards mais limitados e sem o mesmo nível de correlação entre infraestrutura, APM e logs numa única tela.

---

## Implementação real

### Infraestrutura (Kubernetes)

Chart `newrelic/nri-bundle`, versão fixada em `8.0.20` (`oficina-infra-k8s/helm/values-newrelic.yaml`), com os componentes:
- `newrelic-infrastructure` (agente de infra, privileged) — métricas de node/pod.
- `nri-kube-events` — eventos do cluster (pods agendados, crashes, etc).
- `newrelic-logging` (Fluent Bit) — encaminha os logs estruturados (JSON, Serilog) de todos os pods.
- `nri-metadata-injection` **desabilitado** — ver "Descoberta real" abaixo.

### APM (.NET)

Instrumentado via **init container** no `Deployment` da API (`oficina-infra-k8s/k8s/services/api/deployment.yaml`), não por alteração no `Dockerfile` da aplicação:
- Init container `newrelic/newrelic-dotnet-init` (versão fixada, não `:latest`), copia os arquivos do agente para um volume `emptyDir` compartilhado com o container da API, ajustando o dono dos arquivos (`chown`) para o usuário não-root em que a API roda.
- Variáveis de ambiente do CLR Profiler (`CORECLR_ENABLE_PROFILING`, `CORECLR_PROFILER`, `CORECLR_PROFILER_PATH`, `CORECLR_NEWRELIC_HOME`) apontando para esse volume.
- `NEW_RELIC_LICENSE_KEY` e `NEW_RELIC_APP_NAME` via o mesmo Secret/ConfigMap Kubernetes que a API já usa para as demais configurações.

## Descoberta real durante a implementação (não prevista no planejamento)

**O node do cluster (`t3.small`) tem limite de 11 pods**, e o componente `nri-metadata-injection` do chart (que injeta o agente APM via webhook automático em runtime) estourava esse limite, travando a instalação com `Job ... not ready` — além de ser redundante, já que a instrumentação da API é feita manualmente via init container. Desabilitado (`enabled: false`), liberando pods suficientes para o restante do chart instalar corretamente.

## Investigação de conectividade (401 Unauthorized) — causa raiz e lição aprendida

Por um período, tanto o agente de infraestrutura quanto o agente APM rejeitavam a conexão com `401 Unauthorized ("Potential issue with license key")`, mesmo com uma license key do tipo correto ("Ingest - License") e sem corrupção do valor. **A causa não era nenhum bug de configuração: a license key usada durante toda a investigação simplesmente não existia na conta.** A tela de "API Keys" da conta tinha duas chaves "Ingest - License" reais, e nenhuma delas era a que estava sendo testada. O caminho que revelou a chave certa foi **"+ Add data" → guia de instalação ".NET"**, que mostra a license key completa embutida no comando de exemplo — diferente da tela de "API Keys", que só permite copiar o ID da chave, não o valor completo, pelo menu de contexto.

**Lição para não repetir:** se o New Relic rejeitar uma license key com 401 mesmo com o formato aparentemente correto, comparar com o guia de instalação de "Add data" antes de investigar qualquer outra hipótese (corrupção de valor, região do coletor, etc.) — é comum a conta ter mais de uma chave "Ingest - License" (histórico de tentativas de setup) e só uma delas ser a válida atual.

## Evidência de teste real

- **Infrastructure/Kubernetes**: agentes conectados sem erro, métricas de node/pod visíveis no dashboard.
- **APM**: log do agente confirma `Agent fully connected` e `Reporting to: https://one.newrelic.com/...`; transações reais aparecem em APM & Services geradas pelos próprios health checks (`livenessProbe`/`readinessProbe`, que batem na API a cada 10s) e por chamadas reais durante os testes de integração.
- **Logs**: logs estruturados de todos os pods, correlacionados por trace ID com o APM.

---

## Consequências

**Positivas:**
- Os três pilares (infraestrutura, APM, logs) confirmados funcionando com dados reais, não apenas configurados.
- A investigação do 401 fica documentada, evitando repetir a mesma investigação do zero se a key expirar ou a conta mudar.

**Negativas:**
- A license key ainda é passada manualmente como variável de ambiente (`install-newrelic.sh`/Secret do Kubernetes), não vem do Secrets Manager como os demais segredos do projeto — pendência já registrada em `docs/adrs/ADR-009-escolhas-academy.md`.
- A versão fixa da imagem do init container e do chart Helm precisa de atualização manual quando o New Relic lançar novas versões; sem isso, fica presa numa versão desatualizada indefinidamente (trade-off aceito por reprodutibilidade).

---

## Referências

- `oficina-infra-k8s/helm/values-newrelic.yaml`
- `oficina-infra-k8s/k8s/services/api/deployment.yaml`
- `oficina-infra-k8s/scripts/install-newrelic.sh`
- [ADR-009](./ADR-009-escolhas-academy.md) — pendência do secret da license key no Secrets Manager
