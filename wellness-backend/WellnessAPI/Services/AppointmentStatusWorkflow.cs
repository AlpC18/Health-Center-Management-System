using WellnessAPI.Models.Domain;

namespace WellnessAPI.Services;

public static class AppointmentStatusWorkflow
{
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> Allowed = new()
    {
        [AppointmentStatus.Planifikuar] = new[] { AppointmentStatus.Planifikuar, AppointmentStatus.Konfirmuar, AppointmentStatus.NdryshimPropozuar, AppointmentStatus.Anuluar },
        [AppointmentStatus.Konfirmuar] = new[] { AppointmentStatus.Konfirmuar, AppointmentStatus.NdryshimPropozuar, AppointmentStatus.Perfunduar, AppointmentStatus.Anuluar },
        [AppointmentStatus.NdryshimPropozuar] = new[] { AppointmentStatus.NdryshimPropozuar, AppointmentStatus.Konfirmuar, AppointmentStatus.Planifikuar, AppointmentStatus.Anuluar },
        [AppointmentStatus.Perfunduar] = new[] { AppointmentStatus.Perfunduar },
        [AppointmentStatus.Anuluar] = new[] { AppointmentStatus.Anuluar },
    };

    public static AppointmentStatus ParseOrDefault(string? value, AppointmentStatus fallback = AppointmentStatus.Planifikuar)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        if (Enum.TryParse<AppointmentStatus>(value, ignoreCase: true, out var parsed)) return parsed;
        return value.Trim().ToLowerInvariant() switch
        {
            "proposed" or "rescheduleproposed" or "ndryshim" or "ndryshim_propozuar" => AppointmentStatus.NdryshimPropozuar,
            _ => throw new ArgumentException($"Status i panjohur i terminit: {value}.")
        };
    }

    public static bool CanTransition(AppointmentStatus current, AppointmentStatus next)
        => Allowed.TryGetValue(current, out var allowed) && allowed.Contains(next);

    public static void EnsureTransition(AppointmentStatus current, AppointmentStatus next)
    {
        if (!CanTransition(current, next))
            throw new InvalidOperationException($"Kalim i palejuar i statusit: {current} -> {next}.");
    }
}
