-- =============================================================================
-- AUDITORIA DE DADOS LEGADOS — rodar ANTES do deploy com Value Objects
-- Banco: PostgreSQL
-- Objetivo: identificar linhas que causariam ArgumentException ao serem
--           materializadas pelo EF Core via ValueConverters dos VOs.
-- =============================================================================
-- Uso:
--   psql -h <host> -U <user> -d <database> -f auditoria-dados-legados.sql
--   Se a consulta final "RESUMO" retornar 0 em todos os campos, o deploy é seguro.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Email inválido (Clientes)
--    Regra do VO: não-nulo, não-vazio, passa em MailAddress.TryCreate
--    Aproximação SQL: exatamente um @, algo antes e depois, ponto no domínio
-- -----------------------------------------------------------------------------
SELECT
    "Id",
    "Nome",
    "Email",
    'EMAIL_INVALIDO' AS motivo
FROM "Clientes"
WHERE
    "Email" IS NULL
    OR trim("Email") = ''
    OR "Email" ~ '\s'
    OR length("Email") - length(replace("Email", '@', '')) <> 1
    OR "Email" NOT LIKE '%@%.%'
    OR "Email" LIKE '@%'
    OR "Email" LIKE '%@'
    OR split_part("Email", '@', 2) NOT LIKE '%.%';

-- -----------------------------------------------------------------------------
-- 2. Documento inválido (Clientes)
--    Regra do VO: após remover não-alfanuméricos e upper, deve ter 11 dígitos
--    (CPF) ou 14 alfanuméricos (CNPJ).
--    Nota: a validação de dígitos verificadores não é possível em SQL puro sem
--    funções customizadas; este script detecta o problema de formato/tamanho.
-- -----------------------------------------------------------------------------
WITH doc_limpo AS (
    SELECT
        "Id",
        "Nome",
        "Documento",
        upper(regexp_replace("Documento", '[^A-Za-z0-9]', '', 'g')) AS limpo
    FROM "Clientes"
)
SELECT
    "Id",
    "Nome",
    "Documento",
    'DOCUMENTO_FORMATO_INVALIDO' AS motivo
FROM doc_limpo
WHERE
    limpo !~ '^[0-9]{11}$'       -- CPF: 11 dígitos
    AND limpo !~ '^[A-Z0-9]{14}$'; -- CNPJ: 14 alfanuméricos (inclui Mercosul)

-- -----------------------------------------------------------------------------
-- 3. Telefone inválido (Clientes)
--    Regra do VO: após extrair apenas dígitos, deve ter entre 10 e 13 dígitos.
-- -----------------------------------------------------------------------------
WITH tel_limpo AS (
    SELECT
        "Id",
        "Nome",
        "Telefone",
        regexp_replace("Telefone", '[^0-9]', '', 'g') AS apenas_digitos
    FROM "Clientes"
)
SELECT
    "Id",
    "Nome",
    "Telefone",
    'TELEFONE_INVALIDO' AS motivo
FROM tel_limpo
WHERE
    "Telefone" IS NULL
    OR apenas_digitos = ''
    OR length(apenas_digitos) < 10
    OR length(apenas_digitos) > 13;

-- -----------------------------------------------------------------------------
-- 4. Placa inválida (Veiculos)
--    Regra do VO: após remover não-alfanuméricos e upper, deve corresponder ao
--    formato antigo (AAA9999) ou Mercosul (AAA9A99).
-- -----------------------------------------------------------------------------
WITH placa_limpa AS (
    SELECT
        "Id",
        "Placa",
        upper(regexp_replace("Placa", '[^A-Za-z0-9]', '', 'g')) AS normalizada
    FROM "Veiculos"
)
SELECT
    "Id",
    "Placa",
    'PLACA_INVALIDA' AS motivo
FROM placa_limpa
WHERE
    "Placa" IS NULL
    OR normalizada !~ '^[A-Z]{3}[0-9]{4}$'       -- Formato antigo: ABC1234
    AND normalizada !~ '^[A-Z]{3}[0-9][A-Z][0-9]{2}$'; -- Mercosul: ABC1D23

-- -----------------------------------------------------------------------------
-- RESUMO — execute este bloco por último para ter um panorama rápido
-- -----------------------------------------------------------------------------
WITH emails AS (
    SELECT count(*) AS n
    FROM "Clientes"
    WHERE "Email" IS NULL OR trim("Email") = '' OR "Email" ~ '\s'
       OR length("Email") - length(replace("Email", '@', '')) <> 1
       OR "Email" NOT LIKE '%@%.%'
),
documentos AS (
    SELECT count(*) AS n
    FROM "Clientes"
    WHERE upper(regexp_replace("Documento", '[^A-Za-z0-9]', '', 'g')) !~ '^[0-9]{11}$'
      AND upper(regexp_replace("Documento", '[^A-Za-z0-9]', '', 'g')) !~ '^[A-Z0-9]{14}$'
),
telefones AS (
    SELECT count(*) AS n
    FROM "Clientes"
    WHERE length(regexp_replace("Telefone", '[^0-9]', '', 'g')) < 10
       OR length(regexp_replace("Telefone", '[^0-9]', '', 'g')) > 13
),
placas AS (
    SELECT count(*) AS n
    FROM "Veiculos"
    WHERE upper(regexp_replace("Placa", '[^A-Za-z0-9]', '', 'g')) !~ '^[A-Z]{3}[0-9]{4}$'
      AND upper(regexp_replace("Placa", '[^A-Za-z0-9]', '', 'g')) !~ '^[A-Z]{3}[0-9][A-Z][0-9]{2}$'
)
SELECT
    (SELECT n FROM emails)     AS emails_invalidos,
    (SELECT n FROM documentos) AS documentos_invalidos,
    (SELECT n FROM telefones)  AS telefones_invalidos,
    (SELECT n FROM placas)     AS placas_invalidas,
    CASE
        WHEN (SELECT n FROM emails) + (SELECT n FROM documentos)
           + (SELECT n FROM telefones) + (SELECT n FROM placas) = 0
        THEN 'SEGURO PARA DEPLOY'
        ELSE 'ATENCAO: corrigir dados antes do deploy'
    END AS status_deploy;
