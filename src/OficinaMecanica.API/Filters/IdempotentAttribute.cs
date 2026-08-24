using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Filters;

public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "Idempotency-Key";
    private static readonly TimeSpan Expiracao = TimeSpan.FromHours(24);

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

        var cacheado = await store.ObterAsync(chave);
        if (cacheado is not null)
        {
            var resposta = JsonSerializer.Deserialize<CachedResponse>(cacheado)!;
            context.Result = new ContentResult
            {
                StatusCode = resposta.StatusCode,
                Content = resposta.Body,
                ContentType = resposta.ContentType
            };
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception is not null)
            return;

        if (executedContext.Result is ObjectResult objectResult)
        {
            var resposta = new CachedResponse(
                objectResult.StatusCode ?? StatusCodes.Status200OK,
                JsonSerializer.Serialize(objectResult.Value),
                "application/json");

            await store.SalvarAsync(chave, JsonSerializer.Serialize(resposta), Expiracao);
        }
        else if (executedContext.Result is ContentResult contentResult)
        {
            var resposta = new CachedResponse(
                contentResult.StatusCode ?? StatusCodes.Status200OK,
                contentResult.Content ?? string.Empty,
                contentResult.ContentType ?? "text/plain");

            await store.SalvarAsync(chave, JsonSerializer.Serialize(resposta), Expiracao);
        }
        else if (executedContext.Result is StatusCodeResult statusCodeResult)
        {
            var resposta = new CachedResponse(statusCodeResult.StatusCode, string.Empty, "application/json");
            await store.SalvarAsync(chave, JsonSerializer.Serialize(resposta), Expiracao);
        }
    }

    private record CachedResponse(int StatusCode, string Body, string ContentType);
}
