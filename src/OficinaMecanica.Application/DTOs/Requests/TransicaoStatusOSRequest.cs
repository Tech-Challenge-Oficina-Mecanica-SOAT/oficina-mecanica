using OficinaMecanica.Domain.Entities;
namespace OficinaMecanica.Application.DTOs.Requests;
public record TransicaoStatusOSRequest(EnumStatusOS NovoStatus, string Motivo);
