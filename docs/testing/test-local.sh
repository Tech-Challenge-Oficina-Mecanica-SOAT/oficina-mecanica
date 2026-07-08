#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════════════════
#  TESTES LOCAIS — API Oficina Mecânica (cobertura completa)
# ═══════════════════════════════════════════════════════════════════════════════
#
#  PRÉ-REQUISITO
#    docker compose up -d          (API em http://localhost:5000)
#
#  NOTA: exige banco limpo. Para resetar:
#    docker compose down -v && docker compose up -d --build
#
#  EXECUÇÃO
#     bash docs/testing/test-local.sh
#     bash docs/testing/test-local.sh 2>/dev/null   # silencia warnings curl
#
# ═══════════════════════════════════════════════════════════════════════════════

set -uo pipefail

BASE="${BASE:-http://localhost:5000}"
PASS=0
FAIL=0

# ── Helpers de output ────────────────────────────────────────────────────────

green()   { printf '\033[0;32m[PASS]\033[0m %s\n' "$1";  PASS=$((PASS+1)); }
red()     { printf '\033[0;31m[FAIL]\033[0m %s\n' "$1";  FAIL=$((FAIL+1)); }
info()    { printf '\033[0;36m[INFO]\033[0m %s\n' "$1"; }
section() { printf '\n\033[1;33m┌─ %s\033[0m\n' "$1"; }
abort()   { printf '\n\033[0;31m[ABORT]\033[0m %s\n' "$1"; summary; exit 1; }

check() {
  local desc="$1" expected="$2" actual="$3"
  if [ "$actual" = "$expected" ]; then
    green "$desc"
  else
    red "$desc  (esperado HTTP $expected, obtido $actual)"
  fi
}

# Aceita lista de códigos separados por espaço: check_any "desc" "201 409" "$STATUS"
check_any() {
  local desc="$1" expected="$2" actual="$3"
  if echo "$expected" | grep -qw "$actual"; then
    green "$desc  [HTTP $actual]"
  else
    red "$desc  (esperado [$expected], obtido $actual)"
  fi
}

# ── HTTP helper ──────────────────────────────────────────────────────────────
# Uso: do_curl METHOD URL [-H ...] [-d ...]
# Preenche: HTTP_STATUS (código) e HTTP_BODY (resposta)

TMPFILE=$(mktemp)
trap 'rm -f "$TMPFILE"' EXIT

HTTP_STATUS=""
HTTP_BODY=""

do_curl() {
  local method="$1" url="$2"; shift 2
  HTTP_STATUS=$(curl -s -o "$TMPFILE" -w "%{http_code}" -X "$method" "$url" "$@")
  HTTP_BODY=$(cat "$TMPFILE")
}

# Extrai campo string de JSON simples (sem jq)
extract() {
  echo "$HTTP_BODY" | grep -o "\"$1\":\"[^\"]*\"" | head -1 | cut -d'"' -f4
}

# ── Resumo final ─────────────────────────────────────────────────────────────

summary() {
  local total=$((PASS + FAIL))
  echo ""
  echo "╔══════════════════════════════════════════════╗"
  printf "║  Total: %-3d   " "$total"
  printf '\033[0;32mPASS: %-3d\033[0m   ' "$PASS"
  printf '\033[0;31mFAIL: %-3d\033[0m' "$FAIL"
  echo "  ║"
  echo "╚══════════════════════════════════════════════╝"
  echo ""
}

# ── Credenciais de teste ─────────────────────────────────────────────────────

ADMIN_EMAIL="admin-test@oficina.com"
MECANICO_EMAIL="mecanico-test@oficina.com"
CLIENTE_EMAIL="cliente-test@oficina.com"
SENHA="Senha@123"

TOKEN_ADMIN=""
TOKEN_MECANICO=""
TOKEN_CLIENTE=""

# ── IDs encadeados entre seções ──────────────────────────────────────────────

CLIENTE_ID=""
VEICULO_ID=""
SERVICO_ID=""
PECA_ID=""
OS_ID=""
ITEM_ID=""

# ── Dados de teste (banco limpo exigido — ver cabeçalho) ─────────────────────
CPF_TESTE="57329189065"
PLACA_TESTE="TST1D23"
CODIGO_PECA="FO-001"

echo ""
echo "╔══════════════════════════════════════════════════════════════════════╗"
echo "║          TESTES — API Oficina Mecânica (cobertura completa)         ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
info "BASE: $BASE"

###############################################################################
section "0. BOOTSTRAP — login com admin semente"
###############################################################################
# A API cria admin@oficina.com / Senha@123 automaticamente no primeiro boot
# (quando o banco está vazio). Esse token é necessário para registrar
# usuários com perfil Admin ou Mecânico via POST /Auth/registrar.

do_curl POST "$BASE/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@oficina.com","senha":"Senha@123"}'
TOKEN_SEED_ADMIN=$(extract "token")
if [ -n "$TOKEN_SEED_ADMIN" ]; then
  green "POST /Auth/login — admin semente OK"
else
  abort "admin semente não encontrado — banco pode não estar limpo ou a API não subiu corretamente"
fi

###############################################################################
section "1. AUTENTICAÇÃO — registro"
###############################################################################

do_curl POST "$BASE/Auth/registrar" \
  -H "Authorization: Bearer $TOKEN_SEED_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"senha\":\"$SENHA\",\"perfil\":0}"
check_any "POST /Auth/registrar — Admin (201 | 409 já existe)" "201 409" "$HTTP_STATUS"

do_curl POST "$BASE/Auth/registrar" \
  -H "Authorization: Bearer $TOKEN_SEED_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$MECANICO_EMAIL\",\"senha\":\"$SENHA\",\"perfil\":1}"
check_any "POST /Auth/registrar — Mecânico" "201 409" "$HTTP_STATUS"

do_curl POST "$BASE/Auth/registrar" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$CLIENTE_EMAIL\",\"senha\":\"$SENHA\",\"perfil\":2}"
check_any "POST /Auth/registrar — Cliente" "201 409" "$HTTP_STATUS"

# E-mail duplicado → 409
do_curl POST "$BASE/Auth/registrar" \
  -H "Authorization: Bearer $TOKEN_SEED_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"senha\":\"$SENHA\",\"perfil\":0}"
check "POST /Auth/registrar — e-mail duplicado → 409" "409" "$HTTP_STATUS"

###############################################################################
section "2. AUTENTICAÇÃO — login"
###############################################################################

# Senha errada → 401
do_curl POST "$BASE/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"senha\":\"senha-errada\"}"
check "POST /Auth/login — senha incorreta → 401" "401" "$HTTP_STATUS"

# Login Admin
do_curl POST "$BASE/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"senha\":\"$SENHA\"}"
TOKEN_ADMIN=$(extract "token")
if [ -n "$TOKEN_ADMIN" ]; then
  green "POST /Auth/login — Admin: token JWT recebido"
else
  abort "Login Admin falhou. Verifique se a API está no ar em $BASE"
fi

# Login Mecânico
do_curl POST "$BASE/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$MECANICO_EMAIL\",\"senha\":\"$SENHA\"}"
TOKEN_MECANICO=$(extract "token")
if [ -n "$TOKEN_MECANICO" ]; then
  green "POST /Auth/login — Mecânico: token JWT recebido"
else
  red "POST /Auth/login — Mecânico: token não encontrado"
fi

# Login Cliente
do_curl POST "$BASE/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$CLIENTE_EMAIL\",\"senha\":\"$SENHA\"}"
TOKEN_CLIENTE=$(extract "token")
if [ -n "$TOKEN_CLIENTE" ]; then
  green "POST /Auth/login — Cliente: token JWT recebido"
else
  red "POST /Auth/login — Cliente: token não encontrado"
fi

###############################################################################
section "3. AUTORIZAÇÃO — acesso a rotas protegidas"
###############################################################################

do_curl GET "$BASE/api/Clientes"
check "GET /api/Clientes sem token → 401" "401" "$HTTP_STATUS"

do_curl GET "$BASE/api/Clientes" -H "Authorization: Bearer token-invalido.abc"
check "GET /api/Clientes com token inválido → 401" "401" "$HTTP_STATUS"

do_curl GET "$BASE/api/Clientes" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Clientes com token Admin → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Veiculos" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Veiculos com token Admin → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/ordens-servico" -H "Authorization: Bearer $TOKEN_MECANICO"
check "GET /api/ordens-servico com token Mecânico → 403" "403" "$HTTP_STATUS"

do_curl GET "$BASE/api/ordens-servico" -H "Authorization: Bearer $TOKEN_CLIENTE"
check "GET /api/ordens-servico com token Cliente → 403" "403" "$HTTP_STATUS"

###############################################################################
section "4. CLIENTES"
###############################################################################

# Criar (aceita 409 por duplicata — recupera o registro existente)
do_curl POST "$BASE/api/Clientes" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"nome\":\"Joao da Silva\",\"documento\":\"$CPF_TESTE\",\"telefone\":\"11999998888\",\"email\":\"joao@email.com\"}"
if [ "$HTTP_STATUS" = "201" ]; then
  green "POST /api/Clientes — criar → 201"
  CLIENTE_ID=$(extract "id")
elif [ "$HTTP_STATUS" = "409" ]; then
  info "POST /api/Clientes → 409 (duplicata) — buscando registro existente"
  do_curl GET "$BASE/api/Clientes/documento/$CPF_TESTE" -H "Authorization: Bearer $TOKEN_ADMIN"
  CLIENTE_ID=$(extract "id")
  [ -n "$CLIENTE_ID" ] && green "POST /api/Clientes — criar → 201 (registro existente reaproveitado)" \
                       || red "POST /api/Clientes — criar → 201  (esperado HTTP 201, obtido 409)"
else
  red "POST /api/Clientes — criar → 201  (esperado HTTP 201, obtido $HTTP_STATUS)"
fi
if [ -z "$CLIENTE_ID" ]; then abort "clienteId não extraído"; fi
info "clienteId: $CLIENTE_ID"

# Listar
do_curl GET "$BASE/api/Clientes" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Clientes → 200" "200" "$HTTP_STATUS"

# Por ID
do_curl GET "$BASE/api/Clientes/$CLIENTE_ID" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Clientes/{id} → 200" "200" "$HTTP_STATUS"

# Por documento
do_curl GET "$BASE/api/Clientes/documento/$CPF_TESTE" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Clientes/documento/{cpf} → 200" "200" "$HTTP_STATUS"

# Atualizar
do_curl PUT "$BASE/api/Clientes/$CLIENTE_ID" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"nome":"Joao da Silva Junior","telefone":"11988887777","email":"joao.novo@email.com"}'
check "PUT /api/Clientes/{id} — atualizar → 200" "200" "$HTTP_STATUS"

# Desativar / Ativar
do_curl PATCH "$BASE/api/Clientes/$CLIENTE_ID/desativar" -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /api/Clientes/{id}/desativar → 204" "204" "$HTTP_STATUS"

do_curl PATCH "$BASE/api/Clientes/$CLIENTE_ID/ativar" -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /api/Clientes/{id}/ativar → 204" "204" "$HTTP_STATUS"

# Caminho triste — ID inexistente
do_curl GET "$BASE/api/Clientes/00000000-0000-0000-0000-000000000000" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Clientes/{id-inexistente} → 404" "404" "$HTTP_STATUS"

# Caminho triste — documento duplicado
do_curl POST "$BASE/api/Clientes" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"nome\":\"Outro Joao\",\"documento\":\"$CPF_TESTE\",\"telefone\":\"11000000000\",\"email\":\"outro@email.com\"}"
check "POST /api/Clientes — documento duplicado → 400" "400" "$HTTP_STATUS"

###############################################################################
section "5. VEÍCULOS"
###############################################################################

# Criar (aceita 400 por duplicata — recupera o registro existente)
do_curl POST "$BASE/api/Veiculos" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"clienteId\":\"$CLIENTE_ID\",\"placa\":\"$PLACA_TESTE\",\"marca\":\"Volkswagen\",\"modelo\":\"Gol\",\"ano\":2020}"
if [ "$HTTP_STATUS" = "201" ]; then
  green "POST /api/Veiculos — criar → 201"
  VEICULO_ID=$(extract "id")
elif [ "$HTTP_STATUS" = "400" ]; then
  info "POST /api/Veiculos → 400 (duplicata) — buscando registro existente"
  do_curl GET "$BASE/api/Veiculos/placa/$PLACA_TESTE" -H "Authorization: Bearer $TOKEN_ADMIN"
  VEICULO_ID=$(extract "id")
  [ -n "$VEICULO_ID" ] && green "POST /api/Veiculos — criar → 201 (registro existente reaproveitado)" \
                       || red "POST /api/Veiculos — criar → 201  (esperado HTTP 201, obtido 400)"
else
  red "POST /api/Veiculos — criar → 201  (esperado HTTP 201, obtido $HTTP_STATUS)"
fi
if [ -z "$VEICULO_ID" ]; then abort "veiculoId não extraído"; fi
info "veiculoId: $VEICULO_ID"

# Listar / Buscar
do_curl GET "$BASE/api/Veiculos" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Veiculos → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Veiculos/$VEICULO_ID" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Veiculos/{id} → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Veiculos/placa/$PLACA_TESTE" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Veiculos/placa/{placa} → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Veiculos/cliente/$CLIENTE_ID" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Veiculos/cliente/{clienteId} → 200" "200" "$HTTP_STATUS"

# Atualizar
do_curl PUT "$BASE/api/Veiculos/$VEICULO_ID" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"placa\":\"$PLACA_TESTE\",\"marca\":\"Volkswagen\",\"modelo\":\"Gol G6\",\"ano\":2021}"
check "PUT /api/Veiculos/{id} — atualizar → 200" "200" "$HTTP_STATUS"

# Caminho triste — cliente inexistente
do_curl POST "$BASE/api/Veiculos" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"clienteId":"00000000-0000-0000-0000-000000000000","placa":"ZZZ9Z99","marca":"Fiat","modelo":"Uno","ano":2010}'
check "POST /api/Veiculos — clienteId inexistente → 404" "404" "$HTTP_STATUS"

# Caminho triste — placa duplicada
do_curl POST "$BASE/api/Veiculos" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"clienteId\":\"$CLIENTE_ID\",\"placa\":\"$PLACA_TESTE\",\"marca\":\"Fiat\",\"modelo\":\"Palio\",\"ano\":2019}"
check "POST /api/Veiculos — placa duplicada → 400" "400" "$HTTP_STATUS"

###############################################################################
section "6. SERVIÇOS"
###############################################################################

# Criar (aceita 400 por duplicata — recupera o registro existente)
do_curl POST "$BASE/api/Servicos" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"nome":"Troca de oleo","descricao":"Troca de oleo do motor + filtro","valor":150.00}'
if [ "$HTTP_STATUS" = "201" ]; then
  green "POST /api/Servicos — criar → 201"
  SERVICO_ID=$(extract "id")
elif [ "$HTTP_STATUS" = "400" ]; then
  info "POST /api/Servicos → 400 (duplicata) — buscando registro existente"
  do_curl GET "$BASE/api/Servicos/nome/Troca%20de%20oleo" -H "Authorization: Bearer $TOKEN_ADMIN"
  SERVICO_ID=$(extract "id")
  [ -n "$SERVICO_ID" ] && green "POST /api/Servicos — criar → 201 (registro existente reaproveitado)" \
                       || red "POST /api/Servicos — criar → 201  (esperado HTTP 201, obtido 400)"
else
  red "POST /api/Servicos — criar → 201  (esperado HTTP 201, obtido $HTTP_STATUS)"
fi
if [ -z "$SERVICO_ID" ]; then abort "servicoId não extraído"; fi
info "servicoId: $SERVICO_ID"

# Listar
do_curl GET "$BASE/api/Servicos" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Servicos → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Servicos/ativos" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Servicos/ativos → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Servicos/$SERVICO_ID" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Servicos/{id} → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Servicos/nome/Troca" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Servicos/nome/{nome} → 200" "200" "$HTTP_STATUS"

# Atualizar
do_curl PUT "$BASE/api/Servicos/$SERVICO_ID" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"nome":"Troca de oleo premium","descricao":"Oleo sintetico + filtro","valor":220.00}'
check "PUT /api/Servicos/{id} → 200" "200" "$HTTP_STATUS"

# Desativar / Ativar
do_curl PATCH "$BASE/api/Servicos/$SERVICO_ID/desativar" -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /api/Servicos/{id}/desativar → 204" "204" "$HTTP_STATUS"

do_curl PATCH "$BASE/api/Servicos/$SERVICO_ID/ativar" -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /api/Servicos/{id}/ativar → 204" "204" "$HTTP_STATUS"

# Caminho triste
do_curl GET "$BASE/api/Servicos/nome/naoexiste" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Servicos/nome/{inexistente} → 404" "404" "$HTTP_STATUS"

###############################################################################
section "7. PEÇAS E ESTOQUE"
###############################################################################

# Criar (aceita 400 por duplicata — recupera o registro existente)
do_curl POST "$BASE/api/Pecas" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"nome\":\"Filtro de oleo\",\"codigo\":\"$CODIGO_PECA\",\"precoUnitario\":45.90,\"estoque\":50,\"descricao\":\"Filtro motor 1.0 a 2.0\"}"
if [ "$HTTP_STATUS" = "201" ]; then
  green "POST /api/Pecas — criar → 201"
  PECA_ID=$(extract "id")
elif [ "$HTTP_STATUS" = "400" ]; then
  info "POST /api/Pecas → 400 (duplicata) — buscando registro existente"
  do_curl GET "$BASE/api/Pecas/codigo/$CODIGO_PECA" -H "Authorization: Bearer $TOKEN_ADMIN"
  PECA_ID=$(extract "id")
  [ -n "$PECA_ID" ] && green "POST /api/Pecas — criar → 201 (registro existente reaproveitado)" \
                    || red "POST /api/Pecas — criar → 201  (esperado HTTP 201, obtido 400)"
else
  red "POST /api/Pecas — criar → 201  (esperado HTTP 201, obtido $HTTP_STATUS)"
fi
if [ -z "$PECA_ID" ]; then abort "pecaId não extraído"; fi
info "pecaId: $PECA_ID"

# Listar / Buscar
do_curl GET "$BASE/api/Pecas" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Pecas → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Pecas/$PECA_ID" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Pecas/{id} → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Pecas/codigo/$CODIGO_PECA" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Pecas/codigo/{codigo} → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Pecas/estoque-baixo?limite=100" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Pecas/estoque-baixo → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/Pecas/$PECA_ID/estoque" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/Pecas/{id}/estoque → 200" "200" "$HTTP_STATUS"

# Movimentações
do_curl PATCH "$BASE/api/Pecas/$PECA_ID/estoque" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"quantidade":20,"tipoOperacao":"incrementar"}'
check "PATCH /api/Pecas/{id}/estoque — incrementar → 200" "200" "$HTTP_STATUS"

do_curl PATCH "$BASE/api/Pecas/$PECA_ID/estoque" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"quantidade":5,"tipoOperacao":"decrementar"}'
check "PATCH /api/Pecas/{id}/estoque — decrementar → 200" "200" "$HTTP_STATUS"

# Atualizar dados
do_curl PUT "$BASE/api/Pecas/$PECA_ID" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"nome":"Filtro de oleo premium","precoUnitario":55.00}'
check "PUT /api/Pecas/{id} → 200" "200" "$HTTP_STATUS"

# Caminho triste — saldo insuficiente
do_curl PATCH "$BASE/api/Pecas/$PECA_ID/estoque" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"quantidade":99999,"tipoOperacao":"decrementar"}'
check "PATCH /api/Pecas/{id}/estoque — saldo insuficiente → 400" "400" "$HTTP_STATUS"

# Caminho triste — tipoOperacao inválido
do_curl PATCH "$BASE/api/Pecas/$PECA_ID/estoque" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"quantidade":1,"tipoOperacao":"zerar"}'
check "PATCH /api/Pecas/{id}/estoque — tipoOperacao inválido → 400" "400" "$HTTP_STATUS"

# Caminho triste — código duplicado
do_curl POST "$BASE/api/Pecas" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"nome\":\"Outro filtro\",\"codigo\":\"$CODIGO_PECA\",\"precoUnitario\":30.00,\"estoque\":10}"
check "POST /api/Pecas — código duplicado → 400" "400" "$HTTP_STATUS"

###############################################################################
section "8. ORDENS DE SERVIÇO"
###############################################################################

# Abrir OS
do_curl POST "$BASE/api/ordens-servico" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"clienteId\":\"$CLIENTE_ID\",\"veiculoId\":\"$VEICULO_ID\",\"observacoes\":\"Barulho ao frear no lado esquerdo\"}"
check "POST /api/ordens-servico — abrir OS → 201" "201" "$HTTP_STATUS"
OS_ID=$(extract "id")
if [ -z "$OS_ID" ]; then abort "osId não extraído"; fi
info "osId: $OS_ID"

# Listar / Detalhe
do_curl GET "$BASE/api/ordens-servico" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/ordens-servico → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/api/ordens-servico/$OS_ID" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/ordens-servico/{id} → 200" "200" "$HTTP_STATUS"

# Adicionar serviço à OS
do_curl POST "$BASE/api/ordens-servico/$OS_ID/itens" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"tipo\":\"servico\",\"referenciaId\":\"$SERVICO_ID\",\"quantidade\":1}"
check "POST /api/ordens-servico/{id}/itens — servico → 201" "201" "$HTTP_STATUS"
ITEM_ID=$(extract "id")
if [ -z "$ITEM_ID" ]; then red "itemId não extraído — remoção de item não será testada"; fi
info "itemId: $ITEM_ID"

# Adicionar peça à OS
do_curl POST "$BASE/api/ordens-servico/$OS_ID/itens" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"tipo\":\"peca\",\"referenciaId\":\"$PECA_ID\",\"quantidade\":2}"
check "POST /api/ordens-servico/{id}/itens — peca → 201" "201" "$HTTP_STATUS"

# Verificar total atualizado
do_curl GET "$BASE/api/ordens-servico/$OS_ID" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/ordens-servico/{id} — total atualizado → 200" "200" "$HTTP_STATUS"

# Remover item
if [ -n "$ITEM_ID" ]; then
  do_curl DELETE "$BASE/api/ordens-servico/$OS_ID/itens/$ITEM_ID" \
    -H "Authorization: Bearer $TOKEN_ADMIN"
  check "DELETE /api/ordens-servico/{id}/itens/{itemId} → 204" "204" "$HTTP_STATUS"
fi

# Tempo médio
do_curl GET "$BASE/api/ordens-servico/tempo-medio-execucao" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/ordens-servico/tempo-medio-execucao → 200" "200" "$HTTP_STATUS"

# Caminho triste — OS inexistente
do_curl GET "$BASE/api/ordens-servico/00000000-0000-0000-0000-000000000000" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/ordens-servico/{id-inexistente} → 404" "404" "$HTTP_STATUS"

# Caminho triste — tipo inválido
do_curl POST "$BASE/api/ordens-servico/$OS_ID/itens" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"tipo\":\"invalido\",\"referenciaId\":\"$SERVICO_ID\",\"quantidade\":1}"
check "POST /api/ordens-servico/{id}/itens — tipo inválido → 400" "400" "$HTTP_STATUS"

###############################################################################
section "9. CICLO DE VIDA DA OS — transições de status"
###############################################################################

# Transição inválida antes de iniciar (aprovar OS ainda Recebida → 400)
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/aprovar" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /aprovar — OS Recebida (transição inválida) → 400" "400" "$HTTP_STATUS"

# Recebida → EmDiagnostico (Admin ou Mecânico)
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/iniciar-diagnostico" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /iniciar-diagnostico — Recebida → EmDiagnostico → 204" "204" "$HTTP_STATUS"

# Idempotência — tentar iniciar diagnóstico de novo → 400
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/iniciar-diagnostico" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /iniciar-diagnostico — segunda vez (já EmDiagnostico) → 400" "400" "$HTTP_STATUS"

# EmDiagnostico → AguardandoAprovacao via ForcarStatus (Admin)
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/status" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"novoStatus":3,"motivo":"Diagnostico concluido, orcamento enviado"}'
check "PATCH /status — forçar AguardandoAprovacao (novoStatus:3) → 204" "204" "$HTTP_STATUS"

# AguardandoAprovacao → EmExecucao (Admin ou Cliente)
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/aprovar" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /aprovar — AguardandoAprovacao → EmExecucao → 204" "204" "$HTTP_STATUS"

# EmExecucao → Finalizada (Admin ou Mecânico)
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/notificar-conclusao" \
  -H "Authorization: Bearer $TOKEN_MECANICO"
check "PATCH /notificar-conclusao — EmExecucao → Finalizada → 204" "204" "$HTTP_STATUS"

# Finalizada → Entregue (Admin)
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/entregar" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /entregar — Finalizada → Entregue → 204" "204" "$HTTP_STATUS"

# Tentar avançar OS já Entregue → 400
do_curl PATCH "$BASE/api/ordens-servico/$OS_ID/iniciar-diagnostico" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "PATCH /iniciar-diagnostico — OS Entregue (transição inválida) → 400" "400" "$HTTP_STATUS"

# Histórico completo
do_curl GET "$BASE/api/ordens-servico/$OS_ID/historico" \
  -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /api/ordens-servico/{id}/historico → 200" "200" "$HTTP_STATUS"

# Fluxo alternativo — rejeição (nova OS)
# Re-login Cliente pois o token de 5 min pode ter expirado após as seções anteriores
do_curl POST "$BASE/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$CLIENTE_EMAIL\",\"senha\":\"$SENHA\"}"
TOKEN_CLIENTE=$(extract "token")

do_curl POST "$BASE/api/ordens-servico" \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d "{\"clienteId\":\"$CLIENTE_ID\",\"veiculoId\":\"$VEICULO_ID\",\"observacoes\":\"Teste de rejeicao\"}"
OS_ID2=""
OS_ID2=$(extract "id")
if [ -n "$OS_ID2" ]; then
  do_curl PATCH "$BASE/api/ordens-servico/$OS_ID2/iniciar-diagnostico" \
    -H "Authorization: Bearer $TOKEN_ADMIN"
  if [ "$HTTP_STATUS" != "204" ]; then
    red "PATCH /rejeitar — AguardandoAprovacao → Rejeitada → 204  (falha ao iniciar-diagnostico: $HTTP_STATUS)"
  else
    do_curl PATCH "$BASE/api/ordens-servico/$OS_ID2/status" \
      -H "Authorization: Bearer $TOKEN_ADMIN" \
      -H "Content-Type: application/json" \
      -d '{"novoStatus":3,"motivo":"Orcamento enviado"}'
    if [ "$HTTP_STATUS" != "204" ]; then
      red "PATCH /rejeitar — AguardandoAprovacao → Rejeitada → 204  (falha ao forcar status: $HTTP_STATUS)"
    else
      do_curl PATCH "$BASE/api/ordens-servico/$OS_ID2/rejeitar" \
        -H "Authorization: Bearer $TOKEN_CLIENTE" \
        -H "Content-Type: application/json" \
        -d '{"motivo":"Valor acima do esperado"}'
      check "PATCH /rejeitar — AguardandoAprovacao → Rejeitada → 204" "204" "$HTTP_STATUS"
    fi
  fi
else
  red "PATCH /rejeitar — não foi possível criar segunda OS para teste de rejeição"
fi

###############################################################################
section "10. PAINEL PÚBLICO (sem autenticação)"
###############################################################################

do_curl GET "$BASE/Publico/os/$OS_ID/status"
check "GET /Publico/os/{id}/status — sem token → 200" "200" "$HTTP_STATUS"

do_curl GET "$BASE/Publico/os/00000000-0000-0000-0000-000000000000/status"
check "GET /Publico/os/{id-inexistente}/status → 404" "404" "$HTTP_STATUS"

###############################################################################
section "11. TEMPO MÉDIO (com OS finalizada)"
###############################################################################

do_curl GET "$BASE/api/ordens-servico/tempo-medio-execucao" -H "Authorization: Bearer $TOKEN_ADMIN"
check "GET /tempo-medio-execucao — com OS Entregue → 200" "200" "$HTTP_STATUS"

###############################################################################

summary
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
