using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.DTOs;

public record TransicaoStatusOSDto(EnumStatusOS NovoStatus, string Motivo);
