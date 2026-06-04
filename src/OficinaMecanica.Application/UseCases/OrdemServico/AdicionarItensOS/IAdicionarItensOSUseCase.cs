using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;

public interface IAdicionarItensOSUseCase
    : IUseCase<AdicionarItensOSRequest, IEnumerable<OrdemServicoItemResponse>> { }
