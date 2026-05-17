using Konscious.Security.Cryptography;
using OficinaMecanica.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace OficinaMecanica.Infrastructure.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    private const int Argon2Memory = 9216;
    private const int Argon2Iterations = 4;
    private const int Argon2Parallelism = 1;
    private const int Argon2HashLength = 32;
    private const int SaltLength = 32;

    private readonly byte[] _passwordKey;

    public Argon2PasswordHasher(IPasswordSettings settings)
    {
        _passwordKey = Encoding.UTF8.GetBytes(settings.PasswordKey);
    }

    public string Hash(string senha)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltLength);
        byte[] keyedPassword = ComputeHmac(senha);
        byte[] hash = ComputeArgon2id(keyedPassword, salt);

        var b64Salt = Convert.ToBase64String(salt);
        var b64Hash = Convert.ToBase64String(hash);

        return $"$argon2id$v=19$m={Argon2Memory},t={Argon2Iterations},p={Argon2Parallelism}${b64Salt}${b64Hash}";
    }

    public bool Verificar(string senha, string senhaHash)
    {
        var parts = senhaHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5 || parts[0] != "argon2id") return false;

        var paramParts = parts[2].Split(',');
        if (paramParts.Length != 3) return false;

        if (!TryParseArgon2Params(paramParts, out int m, out int t, out int p)) return false;

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expectedHash = Convert.FromBase64String(parts[4]);
        }
        catch { return false; }

        byte[] keyedPassword = ComputeHmac(senha);
        byte[] actualHash = ComputeArgon2id(keyedPassword, salt, m, t, p);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private byte[] ComputeHmac(string senha)
    {
        using var hmac = new HMACSHA256(_passwordKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));
    }

    private static byte[] ComputeArgon2id(byte[] password, byte[] salt,
        int m = Argon2Memory, int t = Argon2Iterations, int p = Argon2Parallelism)
    {
        using var argon2 = new Argon2id(password)
        {
            Salt = salt,
            MemorySize = m,
            Iterations = t,
            DegreeOfParallelism = p
        };
        return argon2.GetBytes(Argon2HashLength);
    }

    private static bool TryParseArgon2Params(string[] parts, out int m, out int t, out int p)
    {
        m = t = p = 0;
        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (kv.Length != 2 || !int.TryParse(kv[1], out int val)) return false;
            switch (kv[0])
            {
                case "m": m = val; break;
                case "t": t = val; break;
                case "p": p = val; break;
                default: return false;
            }
        }
        return m > 0 && t > 0 && p > 0;
    }
}
