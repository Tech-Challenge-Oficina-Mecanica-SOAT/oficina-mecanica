using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.RejeitarOS;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOrcamentoPorEmail;

public record AprovarOrcamentoPorEmailRequest(string Token, bool Aprovado);

public interface IAprovarOrcamentoPorEmailUseCase
{
    Task<Result<string>> ExecutarAsync(AprovarOrcamentoPorEmailRequest request);
}

public class AprovarOrcamentoPorEmailUseCase : IAprovarOrcamentoPorEmailUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IAprovarOSUseCase _aprovarUseCase;
    private readonly IRejeitarOSUseCase _rejeitarUseCase;

    public AprovarOrcamentoPorEmailUseCase(
        IOrdemServicoRepository repository,
        IAprovarOSUseCase aprovarUseCase,
        IRejeitarOSUseCase rejeitarUseCase)
    {
        _repository = repository;
        _aprovarUseCase = aprovarUseCase;
        _rejeitarUseCase = rejeitarUseCase;
    }

    public async Task<Result<string>> ExecutarAsync(AprovarOrcamentoPorEmailRequest request)
    {
        // 1. Buscar OS pelo token
        var os = await _repository.ObterPorTokenAsync(request.Token);
        if (os == null)
            return Result<string>.NotFound("Token inválido");

        // 2. Verificar se token já foi usado
        if (os.TokenUsado)
            return Result<string>.Validation("Link já foi utilizado");

        // 3. Verificar status correto
        if (os.StatusOS.ToString() != "AguardandoAprovacao")
            return Result<string>.Validation($"Ordem de serviço está com status: {os.StatusOS}");

        // 4. Processar aprovação ou recusa
        if (request.Aprovado)
        {
            var result = await _aprovarUseCase.ExecutarAsync(new AprovarOSRequest(os.Id, "ClienteEmail"));
            if (!result.IsSuccess)
                return Result<string>.Validation(result.Error ?? "Erro desconhecido ao aprovar");
            
            os.TokenUsado = true;
            await _repository.UpdateAsync(os);
            
            return Result<string>.Success("aprovado");
        }
        else
        {
            var result = await _rejeitarUseCase.ExecutarAsync(new RejeitarOSUseCaseRequest(os.Id, "ClienteEmail", "Recusado via e-mail"));
            if (!result.IsSuccess)
                return Result<string>.Validation(result.Error ?? "Erro desconhecido ao rejeitar");
            
            os.TokenUsado = true;
            await _repository.UpdateAsync(os);
            
            return Result<string>.Success("recusado");
        }
    }
}