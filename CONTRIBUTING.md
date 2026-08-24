# Contribuindo com o oficina-mecanica-api

## Disciplina de agregados

Este projeto segue Clean Architecture com múltiplos agregados de domínio (`Cliente`, `Veiculo`, `OrdemServico`, `Peca`, `Servico`, `Usuario`). Para manter os agregados desacoplados — pré-requisito para a evolução a microsserviços na Fase 4 — as seguintes regras se aplicam a todo Pull Request:

- **Um use case nunca chama o repositório de outro agregado diretamente.** Um use case do agregado `OrdemServico` não deve injetar ou chamar `IClienteRepository`, `IVeiculoRepository` etc. Ele deve operar apenas sobre o repositório do seu próprio agregado.
- **Comunicação cross-agregado é feita via Domain Events.** Quando uma ação em um agregado precisa notificar ou reagir em outro (ex.: `OrdemServico` concluída deve notificar o cliente), isso é modelado como um evento de domínio (`Domain/Events`) e tratado por um `EventHandler` (`Application/EventHandlers`) — nunca por uma chamada direta entre repositórios de agregados diferentes.
- **Domain Events carregam apenas dados primitivos.** Eventos devem conter somente `Guid`, `string`, `decimal`, `DateTime` (ou outros primitivos/value objects simples) — nunca uma referência a uma entidade completa. Isso garante que os eventos possam futuramente ser publicados em um broker de mensageria (Fase 4) sem exigir refactor.

Essas regras valem como critério de code review: um PR que viole a disciplina de agregados deve ser recusado ou ajustado antes do merge.

## Rodando os testes

```bash
dotnet test tests/OficinaMecanica.Tests.Unit/OficinaMecanica.Tests.Unit.csproj
dotnet test tests/OficinaMecanica.Tests.Integration/OficinaMecanica.Tests.Integration.csproj
```

Os testes de integração usam Testcontainers e exigem Docker em execução.
