using FluentValidation;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Validations;

public class FaturaCreateDtoValidator : AbstractValidator<FaturaCreateDto>
{
    public FaturaCreateDtoValidator()
    {
        RuleFor(x => x.Durum)
            .IsInEnum()
            .WithMessage("Fatura Durumu sadece 1 ile 8 arasında bir değer alabilir. (1: TASLAK, 2: HESAPLANDI, 3: ONAYLANDI, 4: GONDERILDI, 5: HATALI, 6: IPTAL, 7: ODENMEDI, 8: ODENDI)");
    }
}
