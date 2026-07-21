using FluentValidation;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Validations;

public class SozlesmelerValidator : AbstractValidator<Sozlesmeler>
{
    public SozlesmelerValidator()
    {
        RuleFor(x => x.Durum)
            .IsInEnum()
            .WithMessage("Durum değeri geçersiz. Lütfen 1(AKTIF), 2(PASIF), 3(FESHEDILDI) veya 4(ASKIDA) gönderiniz.");
    }
}
