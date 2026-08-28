# Diagrama de sequência — Autenticação por CPF

Reflete o desenho real implementado na API .NET (branch do PR #44) mais o fluxo planejado da Lambda, que ainda não existe (`oficina-lambda-auth` está vazio). Os passos 1, 2 e 8, 9 estão marcados como pendentes; os passos 3 a 7 já existem e são testados no `oficina-mecanica` (`AutenticarPorCpfUseCaseTests`, `AuthControllerTests`).

```mermaid
sequenceDiagram
    autonumber
    actor C as Cliente
    participant AG as API Gateway (pendente)
    participant L as Lambda auth-cpf (pendente)
    participant SM as Secrets Manager
    participant API as API .NET (InternalAuthController)
    participant DB as RDS Postgres

    C->>AG: POST /auth/cpf {cpf: "12345678900"}
    AG->>L: invoke(event)

    L->>L: Valida formato do CPF (dígitos verificadores)

    alt CPF com formato inválido
        L-->>AG: 400 Bad Request
        AG-->>C: 400 {error: "CPF inválido"}
    else CPF com formato válido
        L->>SM: GetSecretValue internal-api-key
        SM-->>L: X-Internal-Api-Key

        L->>API: POST /internal/auth/cpf-verify<br/>header X-Internal-Api-Key<br/>body {cpf}

        API->>API: Valida X-Internal-Api-Key (esquema ApiKey)

        alt API Key inválida ou ausente
            API-->>L: 401 Unauthorized
            L-->>AG: 401
            AG-->>C: 401 {error: "não autorizado"}
        else API Key válida
            API->>DB: SELECT Cliente WHERE Documento = :cpf
            DB-->>API: cliente ou null

            alt Cliente não encontrado ou inativo
                API-->>L: 404 Not Found
                L-->>AG: 404
                AG-->>C: 404 {error: "Cliente não encontrado"}
            else Cliente ativo
                API->>DB: SELECT Usuario WHERE ClienteId = :id
                DB-->>API: usuario ou null

                alt Cliente sem conta de acesso vinculada
                    API-->>L: 404 Not Found
                    L-->>AG: 404
                    AG-->>C: 404 {error: "sem conta de acesso"}
                else Usuario encontrado
                    API->>API: Gera JWT via ITokenGenerator
                    API-->>L: 200 {token, expiresIn}
                    L-->>AG: 200 {token, expiresIn}
                    AG-->>C: 200 {token, expiresIn}
                end
            end
        end
    end
```

## Diferenças em relação ao plano original

O plano-05 desenhava a Lambda acessando o RDS e o `jwt-secret-key` diretamente. O código real move essa responsabilidade inteira para a API .NET: a Lambda só chama um endpoint interno (`/internal/auth/cpf-verify`) autenticado por API Key própria (não o `jwt-secret-key`), e quem consulta o banco e assina o token é a API. Ver `docs/arquitetura-fase3.md` para o raciocínio completo.

## Pendências

- Implementação da Lambda `auth-cpf` e do API Gateway (`oficina-lambda-auth`, hoje vazio)
- Secret `internal-api-key` no Secrets Manager (ainda não provisionado por nenhum dos repos de infra)
- Validação de formato de CPF na camada da Lambda (hoje só existe validação no `Documento` value object, do lado da API)
