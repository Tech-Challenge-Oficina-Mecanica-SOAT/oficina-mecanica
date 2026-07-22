Resumo
------
Este documento resume a verificação da Linguagem Ubíqua fornecida e faz um comparativo com a implementação encontrada no projeto "oficina-mecanica". A seguir estão os principais alinhamentos, divergências e recomendações práticas tanto para o código quanto para a documentação do projeto.

Fontes utilizadas
- Planilha de Linguagem Ubíqua (conteúdo fornecido via Google Sheets)
- Código do repositório: arquivos de domínio e controllers (ex.: src/OficinaMecanica.Domain/Entities, src/OficinaMecanica.API/Controllers)

Principais termos canônicos (extraídos da planilha)
- Cliente — entidade identificada por documento (CPF/CNPJ)
- Veículo — entidade identificada por placa
- Ficha cadastral — modelo de leitura (readonly)
- Ordem de Serviço (OS) — agregado raiz central
- Serviço — atividade (mão de obra)
- Peça — insumo material
- Orçamento — objeto de valor gerado automaticamente
- Catálogo de serviços — sistema externo referência
- API pública da OS — somente leitura (painel de status)
- Notificação / Gateway de notificação — sistema externo para enviar mensagens

Mapeamento rápido entre planilha e código existente
- Cliente: src/OficinaMecanica.Domain/Entities/Cliente.cs, DTO: src/OficinaMecanica.Application/DTOs/ClienteDto.cs
- Veículo: src/OficinaMecanica.Domain/Entities/Veiculo.cs, DTO: src/OficinaMecanica.Application/DTOs/VeiculoDto.cs
- Ordem de Serviço: src/OficinaMecanica.Domain/Entities/OrdemServico.cs, Controller: src/OficinaMecanica.API/Controllers/OrdemServicosController.cs
- Serviço: src/OficinaMecanica.Domain/Entities/Servico.cs, DTO: src/OficinaMecanica.Application/DTOs/ServicoDto.cs
- Peça: src/OficinaMecanica.Domain/Entities/Peca.cs, DTO: src/OficinaMecanica.Application/DTOs/PecaDto.cs

Conformidades observadas
- A presença de entidades nomeadas "Cliente", "Veiculo", "OrdemServico", "Servico" e "Peca" demonstra boa aderência à linguagem canônica.
- Existe separação entre entidades de domínio e DTOs, e controllers REST para cada agregado, o que facilita manter a linguagem ubíqua nas fronteiras do sistema.

Gaps e pontos a corrigir (documento + código)
1) Termos sinônimos e anti-padrões
   - Verificar se em todo o código e documentação não há sinônimos (ex.: "Carro", "Automóvel", "Pedido", "Ticket") — padronizar para "Veículo" e "Ordem de Serviço".
   - Ação: procurar comentários, mensagens de log, nomes de endpoints e testes que ainda usam sinônimos e renomear.

2) DTOs e APIs públicas
   - A planilha recomenda que a API pública da OS seja somente leitura e não exponha dados sensíveis (CPF, endereço, telefone).
   - Ação: garantir existência de um DTO público (ex.: OrdemServicoStatusDto) que contenha apenas: id da OS, status, serviços previstos e previsão de conclusão. Revisar controllers em src/OficinaMecanica.API para usar esse DTO em endpoints públicos.

3) Estados da OS e transições auditáveis
   - A planilha define status possíveis (Recebida, Em diagnóstico, Aguardando aprovação, Em execução, Finalizada, Entregue, Cancelada) e indica que transições devem ser por eventos rastreáveis.
   - Ação: assegurar que exista um enum e / ou máquina de estados centralizada que represente exatamente esses valores e que todas as mudanças de status disparem eventos/entradas no histórico (audit trail). Se não houver enum correspondente, criar um: OrdemServicoStatus.

4) Orçamento e regras de cálculo
   - Orçamento deve ser cálculo automático (soma de serviços + peças) e não editável manualmente.
   - Ação: revisar a geração de orçamento no domínio (OrdemServico) garantindo método que recalcule valor total e tornar setter do campo de valor privado ou calculado no getter.

5) Integração com catálogo e API de estoque
   - A planilha descreve Catálogo de serviços e API de estoque como sistemas externos; a OS deve referenciar serviços (não duplicar) e checar disponibilidade de peças no momento da adição.
   - Ação: garantir abstrações de repositório/serviço para catálogo e estoque. Implementar consulta síncrona/assíncrona ao adicionar peça à OS e validar disponibilidade.

6) Aprovações e timeouts
   - Aprovação do cliente é evento pivotal que desbloqueia execução. Timeouts (ex.: cancelar após X horas) devem ser tratados.
   - Ação: implementar estado "Aguardando aprovação" com timestamp e job/cron/worker que aplica políticas de timeout e gera alertas.

7) Notificações
   - As notificações devem passar por um gateway abstrato (não integrar diretamente com canais na lógica da OS).
   - Ação: adicionar interface INotificationGateway na camada de aplicação/infra e garantir que controllers/serviços chamem essa abstração.

Checklist de mudanças no código (sugestões concretas)
- Criar/confirmar enum OrdemServicoStatus com os valores canônicos da planilha.
- Garantir DTO público OrdemServicoStatusDto usado pelo endpoint GET /os/{id}/status.
- Reforçar validação de unicidade do Cliente pelo documento (CPF/CNPJ) no repositório (IClienteRepository / ClienteRepository).
- Transformar Orçamento em propriedade calculada (método RecalcularOrcamento) em OrdemServico.cs.
- Adicionar INotificationGateway e implementar adaptação para envio real (ex.: GatewayTwilio, GatewaySendGrid) na infraestrutura.
- Implementar consulta à API de estoque via interface IEstoqueApi quando adicionar Peca à OS; rejeitar adição se indisponível.
- Revisar nomenclatura de endpoints e rotas para usar termos canônicos: /clientes, /veiculos, /os, /servicos, /pecas.

Checklist de mudanças na documentação
- Atualizar README / DOCUMENTATION.md com trechos da Linguagem Ubíqua (term list) e mapping entre termos e arquivos/capabilities do sistema.
- Criar guia curto para desenvolvedores: "Como aplicar a Linguagem Ubíqua no código" (naming, DTOs, endpoints, logs, mensagens de erro).
- Atualizar especificação da API pública para garantir que não haja exposição de dados sensíveis e que a API seja read-only para status da OS.

Plano de ação sugerido (prioridade)
1) Aplicar enum OrdemServicoStatus e revisar usos (alta)
2) Garantir DTO público de status e ajustar controller público da OS (alta)
3) Implementar RecalcularOrcamento e bloquear edição manual (média)
4) Adicionar abstração de Notification Gateway e IEstoqueApi (média)
5) Revisão de nomes e anti-padrões em todo o repositório (baixa — pode ser gradual)
	
Observação sobre o arquivo local solicitado
- Não tenho acesso direto ao arquivo local "C:\\Users\\rafae\\OneDrive\\Documentos\\FIAP\\Linguagem Ubíqua.xlsx" presente fora do repositório. Usei o conteúdo disponibilizado anteriormente via Google Sheets para esta análise. Se quiser que eu use o arquivo .xlsx local, copie-o para a raiz do repositório (por exemplo: ./docs/LinguagemUbiqua.xlsx) ou cole seu conteúdo/aba relevante aqui.

Conclusão
-------
O projeto já apresenta boa aderência aos termos centrais da Linguagem Ubíqua. As recomendações acima visam eliminar ambiguidade, proteger dados sensíveis, garantir regras de negócio (orçamento, aprovação, estoque) e alinhar APIs públicas ao contrato definido pela linguagem. Posso aplicar as mudanças de código sugeridas; indique quais itens deseja que eu implemente automaticamente primeiro.
