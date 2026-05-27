using Microsoft.AspNetCore.DataProtection;

namespace WellnessAPI.Services;

public class PhiProtectionService
{
    private const string Prefix = "phi:v1:";
    private readonly IDataProtector _protector;

    public PhiProtectionService(IDataProtectionProvider provider)
        => _protector = provider.CreateProtector("WellnessAPI.PHI.KlientShenim.v1");

    public string Protect(string value)
    {
        if (string.IsNullOrEmpty(value) || value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        return Prefix + _protector.Protect(value);
    }

    public string Unprotect(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        try
        {
            return _protector.Unprotect(value[Prefix.Length..]);
        }
        catch
        {
            return "[E dhene klinike e enkriptuar nuk mund te lexohet]";
        }
    }
}
