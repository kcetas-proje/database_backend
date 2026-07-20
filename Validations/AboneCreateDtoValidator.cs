using FluentValidation;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Validations;

public class AboneCreateDtoValidator : AbstractValidator<AboneCreateDto>
{
    public AboneCreateDtoValidator()
    {
        RuleFor(x => x.AboneTipi).NotEmpty().WithMessage("Abone Tipi boş bırakılamaz.");
        RuleFor(x => x.Telefon).NotEmpty().WithMessage("Telefon numarası zorunludur.")
                               .Matches(@"^\d+$").WithMessage("Telefon numarası sadece rakamlardan oluşabilir.");

        When(x => x.AboneTipi == "BIREYSEL", () =>
        {
            RuleFor(x => x.Ad).NotEmpty().WithMessage("Bireysel abonelerde Ad zorunludur.");
            RuleFor(x => x.Soyad).NotEmpty().WithMessage("Bireysel abonelerde Soyad zorunludur.");
            RuleFor(x => x.Tckn).NotEmpty().WithMessage("Bireysel abonelerde TCKN zorunludur.")
                                .Length(11).WithMessage("TCKN 11 haneli olmalıdır.");
        });

        When(x => x.AboneTipi == "KURUMSAL", () =>
        {
            RuleFor(x => x.Unvan).NotEmpty().WithMessage("Kurumsal abonelerde Ünvan zorunludur.");
            RuleFor(x => x.Vkn).NotEmpty().WithMessage("Kurumsal abonelerde VKN zorunludur.")
                               .Length(10).WithMessage("VKN 10 haneli olmalıdır.");
        });
    }
}
