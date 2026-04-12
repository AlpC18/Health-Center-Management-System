using FluentValidation;
using WellnessAPI.DTOs;

namespace WellnessAPI.Validators;

public class KlientValidators
{
    public class Create : AbstractValidator<KlientCreateDto>
    {
        public Create()
        {
            RuleFor(x => x.Emri).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Mbiemri).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Telefoni).NotEmpty().Matches(@"^\+?[0-9\s-]{8,20}$")
                .WithMessage("Format i pasaktë i telefonit.");
            RuleFor(x => x.DataLindjes).LessThan(DateTime.UtcNow.AddYears(-10))
                .WithMessage("Klienti duhet të jetë të paktën 10 vjeç.");
            RuleFor(x => x.Gjinia).NotEmpty().Must(g => g == "M" || g == "F")
                .WithMessage("Gjinia duhet të jetë 'M' ose 'F'.");
        }
    }

    public class Update : AbstractValidator<KlientUpdateDto>
    {
        public Update()
        {
            RuleFor(x => x.Emri).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Mbiemri).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Gjinia).NotEmpty().Must(g => g == "M" || g == "F");
        }
    }
}

public class AuthValidators
{
    public class Login : AbstractValidator<LoginDto>
    {
        public Login()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }

    public class Register : AbstractValidator<RegisterDto>
    {
        public Register()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }
}

public class SherbimValidators
{
    public class Create : AbstractValidator<SherbimCreateDto>
    {
        public Create()
        {
            RuleFor(x => x.EmriSherbimit).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Cmimi).GreaterThanOrEqualTo(0);
            RuleFor(x => x.KohezgjatjaMin).InclusiveBetween(5, 480);
        }
    }
}

public class TerapistValidators
{
    public class Create : AbstractValidator<TerapistCreateDto>
    {
        public Create()
        {
            RuleFor(x => x.Emri).NotEmpty();
            RuleFor(x => x.Mbiemri).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}

public class TerminValidators
{
    public class Create : AbstractValidator<TerminCreateDto>
    {
        public Create()
        {
            RuleFor(x => x.KlientId).NotEmpty();
            RuleFor(x => x.SherbimId).NotEmpty();
            RuleFor(x => x.TerapistId).NotEmpty();
            RuleFor(x => x.DataTerminit).GreaterThanOrEqualTo(DateTime.Today);
            RuleFor(x => x.OraFillimit).NotEmpty();
            RuleFor(x => x.OraMbarimit).GreaterThan(x => x.OraFillimit)
                .WithMessage("Ora e mbarimit duhet të jetë pas orës së fillimit.");
        }
    }
}
