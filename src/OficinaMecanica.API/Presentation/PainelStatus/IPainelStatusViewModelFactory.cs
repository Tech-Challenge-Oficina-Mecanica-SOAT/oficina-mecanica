using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.API.Presentation.PainelStatus;

public interface IPainelStatusViewModelFactory
{
    PainelStatusViewModel CriarViewModel(OrdemServico os);
}
