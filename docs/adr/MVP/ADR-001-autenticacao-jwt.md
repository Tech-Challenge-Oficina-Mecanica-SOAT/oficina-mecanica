# ADR-001 — Autenticação JWT com Argon2id

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito                         |
| **Data**     | 2026-04-18                     |
| **Autores**  | Time M4                        |
| **Contexto** | Fase 1 — Tech Challenge FIAP   |

---

## Contexto

O sistema de oficina mecânica necessita de um mecanismo de autenticação para proteger as rotas administrativas (gestão de clientes, veículos, ordens de serviço) e permitir que mecânicos e administradores operem o sistema com segurança. Clientes da oficina **não autenticam** nesta fase: eles consultam o status de suas OS via endpoint público.

---

## Decisão

Adotar **JWT (JSON Web Token)** com **HS256** para autenticação stateless, combinado com **Argon2id + HMAC-SHA256** para hash de senhas.

---

## Consequências

**Positivas:**
- Stateless: sem sessão no servidor, escala horizontalmente
- Perfil (`Admin`, `Mecanico`) embutido no token como claim `role`, eliminando consulta ao banco por requisição
- Argon2id é resistente a ataques de GPU/ASIC, superior ao BCrypt para hardware moderno
- HMAC-SHA256 na senha antes do Argon2id adiciona defesa contra vazamento de banco (pepper)

**Negativas/Trade-offs:**
- Tokens não podem ser revogados antes da expiração sem infraestrutura adicional (blacklist/Redis)
- A `SecretKey` precisa ser rotacionada fora de banda: todos os tokens em circulação são invalidados na rotação

---

## Alternativas consideradas

| Alternativa | Motivo de rejeição |
|---|---|
| BCrypt | Vulnerável a ataques de GPU modernos; Argon2id é o padrão recomendado pelo OWASP desde 2023 |
| HMAC-SHA256 puro | Não é um KDF: não tem fator de custo adaptável; descartado conforme revisão de segurança |

---

## Parâmetros de configuração

```json
"Jwt": {
  "SecretKey": "<mínimo 32 caracteres, variável de ambiente em produção>",
  "Issuer":    "mecanica-api",
  "Audience":  "mecanica-cliente",
  "ExpiracaoMinutos": 480
}
```

```json
"Seguranca": {
  "PasswordKey": "<pepper, variável de ambiente em produção>"
}
```

**Argon2id:** `m=9216` (9 MiB), `t=4` iterações, `p=1` thread, hash 32 bytes  
**Formato do hash armazenado:** `$argon2id$v=19$m=9216,t=4,p=1$<salt_b64>$<hash_b64>`

---

## Fluxo de autenticação

### Login

```mermaid
flowchart TD
    A([Cliente HTTP]) -->|POST /auth/login\nemail + senha| B[AuthController]

    B --> C[UsuarioService.AutenticarAsync]
    C --> D[(Banco de dados\nTabela Usuarios)]
    D -->|Usuário não encontrado| E[null]
    E --> F[401 Unauthorized]

    D -->|Usuário encontrado| G[VerificarSenha]
    G --> G1["HMAC-SHA256(senha, passwordKey)\n→ keyedPassword"]
    G1 --> G2["Argon2id(keyedPassword, salt\nm=9216, t=4, p=1)"]
    G2 --> G3{"ConstantTimeEquals\n(hash calculado\nvs hash armazenado)"}
    G3 -->|Inválido| F
    G3 -->|Válido| H[JwtService.GerarToken]

    H --> I["Claims:\n• sub = usuario.Id\n• email = usuario.Email\n• role = usuario.Perfil\n• jti = Guid.NewGuid()"]
    I --> J["Assina com HS256\n+ SecretKey\nExpira em 480 min"]
    J --> K[200 OK\ntoken + expiracao]
    K --> A
```

### Registro de usuário (uso interno)

```mermaid
flowchart TD
    A([Admin HTTP]) -->|POST /auth/registrar\nemail + senha + perfil| B[AuthController]

    B --> C[UsuarioService.RegistrarAsync]
    C --> D[(Banco de dados\nTabela Usuarios)]
    D -->|Email já existe| E[InvalidOperationException]
    E --> F[409 Conflict]

    D -->|Email livre| G[HashSenha]
    G --> G1["RandomNumberGenerator\n32 bytes → salt"]
    G1 --> G2["HMAC-SHA256(senha, passwordKey)\n→ keyedPassword"]
    G2 --> G3["Argon2id(keyedPassword, salt\nm=9216, t=4, p=1)\n→ hash 32 bytes"]
    G3 --> G4["Formata PHC:\n$argon2id$v=19$m=...$salt$hash"]
    G4 --> H["new Usuario(email, hash, perfil)"]
    H --> I[(Persiste no banco)]
    I --> J[201 Created\nid + email + perfil]
    J --> A
```

### Acesso a rota protegida

```mermaid
flowchart TD
    A([Usuário autenticado]) -->|"GET /clientes\nAuthorization: Bearer {token}"| B[Middleware JWT\nUseAuthentication]

    B --> C{"Token\nválido?"}
    C -->|"Não: expirado\nou assinatura inválida"| D[401 Unauthorized]
    C -->|Sim| E[Popula HttpContext.User\ncom claims do token]

    E --> F[Middleware\nUseAuthorization]
    F --> G{"Perfil na claim 'role'\natende a policy\ndo endpoint?"}
    G -->|"Não\n(ex: Mecanico\nacessando rota Admin)"| H[403 Forbidden]
    G -->|Sim| I[Controller executa]
    I --> J[200 OK + dados]
    J --> A
```

---

## Âncora para autenticação futura de clientes

A entidade `Usuario` possui `ClienteId (Guid?)` e o enum `Perfil` inclui `Cliente = 2`. Quando o portal do cliente for implementado:

1. Um `Usuario` com `Perfil.Cliente` será criado e vinculado ao `Cliente` pelo `ClienteId`
2. `JwtService.GerarToken` emitirá a claim `clienteId` quando `ClienteId.HasValue`
3. Rotas do portal usarão `[Authorize(Policy = Policies.Cliente)]`

Não haverá migration adicional: a estrutura já existe.

---

## Arquivos relevantes

| Arquivo | Responsabilidade |
|---|---|
| `src/OficinaMecanica.Domain/Entities/Usuario.cs` | Entidade de domínio |
| `src/OficinaMecanica.Domain/Enums/Perfil.cs` | Enum de perfis (`Admin=0`, `Mecanico=1`, `Cliente=2`) |
| `src/OficinaMecanica.Application/Services/UsuarioService.cs` | Hash Argon2id + autenticação |
| `src/OficinaMecanica.Application/Services/JwtService.cs` | Geração do token JWT |
| `src/OficinaMecanica.Application/AuthorizationPolicies.cs` | Constantes de políticas |
| `src/OficinaMecanica.API/Controllers/AuthController.cs` | Endpoints `/auth/login` e `/auth/registrar` |
| `src/OficinaMecanica.API/Program.cs` | Configuração JWT + políticas de autorização |
| `src/OficinaMecanica.Infrastructure/Migrations/` | Schema do banco incluindo `Usuarios.ClienteId` |
