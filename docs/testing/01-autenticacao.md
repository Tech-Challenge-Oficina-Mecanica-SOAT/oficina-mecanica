# 01. Autenticação

Cria um usuário Admin e gera um JWT que será usado nos demais fluxos.

> **Perfis disponíveis** (enum `Perfil`): `0 = Admin`, `1 = Mecanico`, `2 = Cliente`.

## Pré-condições

- API rodando em `http://localhost:5000` (`docker compose up -d`).

---

## Caminho feliz

### 1. Registrar Admin: `POST /Auth/registrar`

```json
{
  "email": "admin@oficina.com",
  "senha": "Senha@123",
  "perfil": 0
}
```

**`201 Created`** com `{ id, email, perfil }`.

### 2. Login: `POST /Auth/login`

```json
{
  "email": "admin@oficina.com",
  "senha": "Senha@123"
}
```

**`200 OK`**

```json
{
  "token": "eyJhbGciOi...",
  "expiracao": "2026-04-25T03:00:00Z"
}
```

### 3. Autenticar no Scalar

Copie o `token` e cole no botão *Authentication* do Scalar (canto superior direito) → esquema *Bearer*.

---

## Caminho triste

| Cenário | Requisição | Resposta esperada |
|---------|-----------|-------------------|
| E-mail já cadastrado | repetir o `POST /Auth/registrar` com o mesmo e-mail | `409 Conflict` com `{ "mensagem": "..." }` |
| Senha incorreta | `POST /Auth/login` com senha errada | `401 Unauthorized` com `{ "mensagem": "Credenciais inválidas." }` |
| Acessar endpoint protegido sem token | `GET /api/Clientes` sem header `Authorization` | `401 Unauthorized` |
| Token inválido ou expirado | `GET /api/Clientes` com `Authorization: Bearer abc.123` | `401 Unauthorized` |

## Próximo passo

[02. Gerenciar clientes](./02-gerenciar-clientes.md)
