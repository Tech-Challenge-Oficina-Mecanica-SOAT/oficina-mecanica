# Detecta SO e configura o interpretador bash (para as receitas do make e para o Terraform).
# No Windows, o make (build nativa, ex.: choco) não localiza 'bash' sozinho e cai para
# cmd.exe, então precisamos apontar o caminho absoluto do Git Bash explicitamente.
ifeq ($(OS),Windows_NT)
  SHELL := C:/Program Files/Git/bin/bash.exe
  export TF_VAR_bash_interpreter := C:/Program Files/Git/bin/bash.exe
else
  SHELL := bash
  export TF_VAR_bash_interpreter := bash
endif
.SHELLFLAGS := -ec

CLUSTER_CONTEXT := kind-oficina-mecanica
PIDS_FILE       := .pids

.PHONY: setup oficina-up oficina-down oficina-reset

## Verifica pré-requisitos e gera k8s/secret.yaml com credenciais de desenvolvimento
setup:
	@echo "=== Verificando pré-requisitos ==="
	@command -v docker    >/dev/null 2>&1 || { echo "ERRO: docker não encontrado."; exit 1; }
	@docker info          >/dev/null 2>&1 || { echo "ERRO: Docker Desktop não está rodando. Inicie-o e tente novamente."; exit 1; }
	@command -v kind      >/dev/null 2>&1 || { echo "ERRO: kind não encontrado. Instale em: https://kind.sigs.k8s.io/docs/user/quick-start/#installation"; exit 1; }
	@command -v terraform >/dev/null 2>&1 || { echo "ERRO: terraform não encontrado. Instale em: https://developer.hashicorp.com/terraform/install"; exit 1; }
	@command -v kubectl   >/dev/null 2>&1 || { echo "ERRO: kubectl não encontrado."; exit 1; }
	@echo "Pré-requisitos OK."
	@echo "=== Gerando k8s/secret.yaml com credenciais de desenvolvimento ==="
	@printf '%s\n' \
		'apiVersion: v1' \
		'kind: Secret' \
		'metadata:' \
		'  name: oficina-mecanica-secret' \
		'type: Opaque' \
		'stringData:' \
		'  Jwt__SecretKey: "oficina-mecanica-jwt-secret-key-fase2-2025!!"' \
		'  Seguranca__PasswordKey: "oficina-password-key-fase2"' \
		'  PostgresPassword: "oficina123"' \
		'  ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=OficinaDB;Username=postgres;Password=oficina123"' \
		> k8s/secret.yaml
	@echo "k8s/secret.yaml gerado."
	@echo ""
	@echo "=== Setup concluído. Próximo passo: make oficina-up ==="

## Build da imagem Docker + cluster K8s via Terraform + port-forwards em background
oficina-up:
	@echo "=== Build da imagem da API ==="
	docker build -t oficina-mecanica-api:local .
	@echo "=== Provisionando infraestrutura (Terraform) ==="
	cd infra/local && terraform init -input=false && terraform apply -auto-approve
	@echo "=== Iniciando port-forwards ==="
	@kubectl --context $(CLUSTER_CONTEXT) port-forward svc/oficina-mecanica-api 5000:80 >/dev/null 2>&1 & echo $$! > $(PIDS_FILE)
	@kubectl --context $(CLUSTER_CONTEXT) port-forward svc/mailhog 8025:8025 >/dev/null 2>&1 & echo $$! >> $(PIDS_FILE)
	@sleep 3
	@echo ""
	@echo "==========================================="
	@echo "  API:     http://localhost:5000/scalar"
	@echo "  MailHog: http://localhost:8025"
	@echo "==========================================="
	@echo "Para encerrar: make oficina-down"

## Encerra port-forwards e destrói o cluster Kind (apaga todos os dados)
oficina-down:
	@echo "=== Encerrando port-forwards ==="
	@if [ -f $(PIDS_FILE) ]; then \
		while IFS= read -r pid; do kill "$$pid" 2>/dev/null || true; done < $(PIDS_FILE); \
		rm -f $(PIDS_FILE); \
		echo "Port-forwards encerrados."; \
	else \
		echo "Nenhum port-forward ativo."; \
	fi
	@echo "=== Destruindo infraestrutura (Terraform) ==="
	@cd infra/local && terraform destroy -auto-approve || true
	@echo "=== Ambiente encerrado ==="

## Reinicia do zero — resolve dados duplicados ou erros em segunda rodada
oficina-reset:
	@echo "=== Reset: destruindo e recriando o ambiente do zero ==="
	$(MAKE) oficina-down
	$(MAKE) oficina-up
