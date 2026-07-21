using FluentValidation;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Validations;

public class SayaclarValidator : AbstractValidator<Sayaclar>
{
    public SayaclarValidator()
    {
        RuleFor(x => x.Durum)
            .IsInEnum()
            .WithMessage("Durum değeri geçersiz. Lütfen 1(DEPODA), 2(TAKILI), 3(DEGISIM_BEKLIYOR), 4(SOKULMUS), 5(ARIZALI) veya 6(IPTAL) gönderiniz.");
    }
}
