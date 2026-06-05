using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.API.Common;

public static class ControllerExtensions
{
    public static IActionResult MapError<T>(this ControllerBase controller, Result<T> result) =>
        result.ErrorType switch
        {
            ResultErrorType.Validation   => controller.BadRequest(new { mensagem = result.Error }),
            ResultErrorType.NotFound     => controller.NotFound(new { mensagem = result.Error }),
            ResultErrorType.Conflict     => controller.Conflict(new { mensagem = result.Error }),
            ResultErrorType.Unauthorized => controller.Unauthorized(new { mensagem = result.Error }),
            _                            => controller.StatusCode(500, new { mensagem = "Erro inesperado." })
        };
}
