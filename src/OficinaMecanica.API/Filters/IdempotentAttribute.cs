using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Filters;

public class IdempotentAttribute : Attribute, IAsyncActionFilter, IAsyncResultFilter
{
    private const string HeaderName = "Idempotency-Key";
    private const string ItemPendente = "Idempotent.PendenteDeGravacao";
    private static readonly TimeSpan Expiracao = TimeSpan.FromHours(24);
    private static readonly TimeSpan IntervaloEspera = TimeSpan.FromMilliseconds(200);
    private const int TentativasEspera = 25; // ~5s aguardando a requisição concorrente concluir

    // Headers gerenciados pelo próprio pipeline HTTP/Kestrel — não fazem sentido
    // (ou não podem) ser reaplicados manualmente numa resposta de cache.
    private static readonly HashSet<string> HeadersIgnorados = new(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Type", "Content-Length", "Date", "Server", "Transfer-Encoding", "Connection"
    };

    /// <summary>
    /// Nome de um parâmetro da action (rota/query) a ser usado como Idempotency-Key
    /// quando o header não está presente — necessário para endpoints acessados por
    /// link (ex.: GET de aprovação por e-mail), onde o cliente não envia headers
    /// customizados. O próprio valor (ex.: um token de uso único) já identifica a
    /// requisição de forma única, então serve como chave de idempotência.
    /// </summary>
    public string? ChaveDeArgumento { get; init; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var idempotencyKey = ObterIdempotencyKey(context);
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await next();
            return;
        }

        var store = context.HttpContext.RequestServices.GetRequiredService<IIdempotencyStore>();
        var chave = $"idempotency:{context.HttpContext.Request.Path}:{idempotencyKey}";
        var corpoHash = CalcularHashCorpo(context.ActionArguments);

        var entradaInicial = new CacheEntry(false, corpoHash, 0, null, null, null);
        var reservado = await store.TentarReservarAsync(chave, JsonSerializer.Serialize(entradaInicial), Expiracao);

        if (!reservado)
        {
            var valorAtual = await store.ObterAsync(chave);
            var entradaAtual = valorAtual is null ? null : JsonSerializer.Deserialize<CacheEntry>(valorAtual);

            if (entradaAtual is not null && entradaAtual.CorpoHash != corpoHash)
            {
                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    Content = "A mesma Idempotency-Key foi usada com um corpo de requisição diferente.",
                    ContentType = "text/plain"
                };
                return;
            }

            var entradaConcluida = await AguardarConclusaoAsync(store, chave);
            if (entradaConcluida is { Concluido: true })
            {
                AplicarHeaders(context.HttpContext.Response, entradaConcluida.Headers);
                context.Result = new ContentResult
                {
                    StatusCode = entradaConcluida.StatusCode,
                    Content = entradaConcluida.Body,
                    ContentType = entradaConcluida.ContentType
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

        CacheEntry? entradaExecutada = executedContext.Result switch
        {
            ObjectResult objectResult => new CacheEntry(
                true, corpoHash,
                objectResult.StatusCode ?? StatusCodes.Status200OK,
                JsonSerializer.Serialize(objectResult.Value),
                "application/json", null),
            ContentResult contentResult => new CacheEntry(
                true, corpoHash,
                contentResult.StatusCode ?? StatusCodes.Status200OK,
                contentResult.Content ?? string.Empty,
                contentResult.ContentType ?? "text/plain", null),
            StatusCodeResult statusCodeResult => new CacheEntry(
                true, corpoHash, statusCodeResult.StatusCode, string.Empty, "application/json", null),
            _ => null
        };

        // Só cacheia respostas de sucesso (2xx). Erros (400, 404 etc.) e resultados
        // não cacheáveis (ex.: FileResult) liberam a reserva, permitindo que o
        // cliente reenvie a mesma Idempotency-Key com um payload corrigido.
        if (entradaExecutada is not null && entradaExecutada.StatusCode is >= 200 and < 300)
        {
            // A gravação em si acontece em OnResultExecutionAsync: alguns resultados
            // (ex.: CreatedAtActionResult) só calculam headers como Location durante a
            // execução do resultado, que ocorre depois deste action filter terminar.
            context.HttpContext.Items[ItemPendente] = (chave, entradaExecutada, store);
        }
        else
        {
            await store.RemoverAsync(chave);
        }
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        if (context.HttpContext.Items.TryGetValue(ItemPendente, out var pendenteObj) &&
            pendenteObj is (string chave, CacheEntry entrada, IIdempotencyStore store))
        {
            var headers = CapturarHeaders(context.HttpContext.Response.Headers);
            var entradaComHeaders = entrada with { Headers = headers };
            await store.SalvarAsync(chave, JsonSerializer.Serialize(entradaComHeaders), Expiracao);
        }
    }

    private static Dictionary<string, string[]>? CapturarHeaders(IHeaderDictionary headers)
    {
        var capturados = headers
            .Where(h => !HeadersIgnorados.Contains(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.Select(v => v ?? string.Empty).ToArray());

        return capturados.Count > 0 ? capturados : null;
    }

    private static void AplicarHeaders(HttpResponse response, Dictionary<string, string[]>? headers)
    {
        if (headers is null)
            return;

        foreach (var (nome, valores) in headers)
            response.Headers[nome] = valores;
    }

    private string? ObterIdempotencyKey(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var header) &&
            !string.IsNullOrWhiteSpace(header))
            return header.ToString();

        if (ChaveDeArgumento is not null &&
            context.ActionArguments.TryGetValue(ChaveDeArgumento, out var valor) &&
            valor is not null)
            return valor.ToString();

        return null;
    }

    private static string CalcularHashCorpo(IDictionary<string, object?> actionArguments)
    {
        var json = JsonSerializer.Serialize(actionArguments);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }

    private static async Task<CacheEntry?> AguardarConclusaoAsync(IIdempotencyStore store, string chave)
    {
        for (var tentativa = 0; tentativa < TentativasEspera; tentativa++)
        {
            await Task.Delay(IntervaloEspera);

            var valor = await store.ObterAsync(chave);
            if (valor is null)
                return null; // reserva foi liberada (falhou) ou expirou

            var entrada = JsonSerializer.Deserialize<CacheEntry>(valor);
            if (entrada is { Concluido: true })
                return entrada;
        }

        return null;
    }

    private record CacheEntry(
        bool Concluido, string CorpoHash, int StatusCode, string? Body, string? ContentType,
        Dictionary<string, string[]>? Headers);
}
