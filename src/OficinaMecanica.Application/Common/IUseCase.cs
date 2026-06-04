namespace OficinaMecanica.Application.Common;

public interface IUseCase<TRequest, TResponse>
{
    Task<Result<TResponse>> ExecutarAsync(TRequest request);
}
