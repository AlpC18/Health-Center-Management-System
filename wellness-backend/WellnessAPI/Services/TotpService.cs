using System.Security.Cryptography;
using System.Text;

namespace WellnessAPI.Services;

public class TotpService
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private const int StepSeconds = 30;
    private const int CodeDigits = 6;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        return ToBase32(bytes);
    }

    public string BuildOtpAuthUri(string issuer, string accountName, string secret)
        => $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits={CodeDigits}&period={StepSeconds}";

    public bool VerifyCode(string secret, string code)
    {
        var normalized = new string((code ?? "").Where(char.IsDigit).ToArray());
        if (normalized.Length != CodeDigits) return false;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;
        for (var offset = -1; offset <= 1; offset++)
        {
            var expected = GenerateCode(secret, now + offset);
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(expected),
                    Encoding.ASCII.GetBytes(normalized)))
                return true;
        }

        return false;
    }

    private static string GenerateCode(string secret, long timestep)
    {
        var key = FromBase32(secret);
        var counter = BitConverter.GetBytes(timestep);
        if (BitConverter.IsLittleEndian) Array.Reverse(counter);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counter);
        var offset = hash[^1] & 0x0f;
        var binary =
            ((hash[offset] & 0x7f) << 24) |
            ((hash[offset + 1] & 0xff) << 16) |
            ((hash[offset + 2] & 0xff) << 8) |
            (hash[offset + 3] & 0xff);

        var otp = binary % (int)Math.Pow(10, CodeDigits);
        return otp.ToString(new string('0', CodeDigits));
    }

    private static string ToBase32(byte[] bytes)
    {
        var output = new StringBuilder();
        var bits = 0;
        var value = 0;

        foreach (var b in bytes)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                output.Append(Alphabet[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }

        if (bits > 0)
            output.Append(Alphabet[(value << (5 - bits)) & 31]);

        return output.ToString();
    }

    private static byte[] FromBase32(string input)
    {
        var cleaned = (input ?? "").Trim().Replace(" ", "").TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>();
        var bits = 0;
        var value = 0;

        foreach (var c in cleaned)
        {
            var idx = Alphabet.IndexOf(c);
            if (idx < 0) continue;
            value = (value << 5) | idx;
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)((value >> (bits - 8)) & 0xff));
                bits -= 8;
            }
        }

        return bytes.ToArray();
    }
}
