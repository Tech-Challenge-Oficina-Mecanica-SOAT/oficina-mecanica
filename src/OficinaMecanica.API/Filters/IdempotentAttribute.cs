using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Filters;

public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "Idempotency-Key";
    private static readonly TimeSpan Expiracao = TimeSpan.FromHours(24);
    private static readonly TimeSpan IntervaloEspera = TimeSpan.FromMilliseconds(200);
    private const int TentativasEspera = 25; // ~5s aguardando a requisição concorrente concluir

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await next();
            return;
        }

        var store = context.HttpContext.RequestServices.GetRequiredService<IIdempotencyStore>();
        var chave = $"idempotency:{context.HttpContext.Request.Path}:{idempotencyKey}";

        var reservado = await store.TentarReservarAsync(chave, Expiracao);
        if (!reservado)
        {
            var resposta = await AguardarResultadoAsync(store, chave);
            if (resposta is not null)
            {
                context.Result = new ContentResult
                {
                    StatusCode = resposta.StatusCode,
                    Content = resposta.Body,
                    ContentType = resposta.ContentType
                };
                return;
            }

            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status409Conflict,
                Content = "Requisição com a mesma Idempotency-Key ainda está em processamento.",
                ContentType = "text/plain"
            };
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception is not null)
        {
            await store.RemoverAsync(chave);
            return;
        }

        CachedResponse? respostaExecutada = executedContext.Result switch
        {
            ObjectResult objectResult => new CachedResponse(
                objectResult.StatusCode ?? StatusCodes.Status200OK,
                JsonSerializer.Serialize(objectResult.Value),
                "application/json"),
            ContentResult contentResult => new CachedResponse(
                contentResult.StatusCode ?? StatusCodes.Status200OK,
                contentResult.Content ?? string.Empty,
                contentResult.ContentType ?? "text/plain"),
            StatusCodeResult statusCodeResult => new CachedResponse(
                statusCodeResult.StatusCode, string.Empty, "application/json"),
            _ => null
        };

        // Só cacheia respostas de sucesso (2xx). Erros (400, 404 etc.) e resultados
        // não cacheáveis (ex.: FileResult) liberam a reserva, permitindo que o
        // cliente reenvie a mesma Idempotency-Key com um payload corrigido.
        if (respostaExecutada is not null && respostaExecutada.StatusCode is >= 200 and < 300)
            await store.SalvarAsync(chave, JsonSerializer.Serialize(respostaExecutada), Expiracao);
        else
            await store.RemoverAsync(chave);
    }

    private static async Task<CachedResponse?> AguardarResultadoAsync(IIdempotencyStore store, string chave)
    {
        for (var tentativa = 0; tentativa < TentativasEspera; tentativa++)
        {
            await Task.Delay(IntervaloEspera);

            var valor = await store.ObterAsync(chave);
            if (valor is null)
                return null; // reserva foi liberada (falhou) ou expirou

            try
            {
                var resposta = JsonSerializer.Deserialize<CachedResponse>(valor);
                if (resposta is not null)
                    return resposta;
            }
            catch (JsonException)
            {
                // ainda é o marcador de reserva (não é JSON) — a requisição concorrente
                // ainda está processando, continua aguardando.
            }
        }

        return null;
    }

    private record CachedResponse(int StatusCode, string Body, string ContentType);
}
