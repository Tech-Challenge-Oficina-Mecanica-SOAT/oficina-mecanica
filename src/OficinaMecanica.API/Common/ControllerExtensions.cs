using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.API.Common;

public static class ControllerExtensions
{
    public static IActionResult MapError<T>(this ControllerBase controller, Result<T> result) =>
        result.ErrorType switch
        {
            ResultErrorType.Validation   => controller.BadRequest(new { message = result.Error }),
            ResultErrorType.NotFound     => controller.NotFound(new { message = result.Error }),
            ResultErrorType.Conflict     => controller.Conflict(new { message = result.Error }),
            ResultErrorType.Unauthorized => controller.Unauthorized(new { message = result.Error }),
            _                            => controller.StatusCode(500, new { message = "Erro inesperado." })
        };
}
