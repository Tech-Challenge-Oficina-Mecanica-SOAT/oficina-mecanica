namespace OficinaMecanica.Tests.Integration.TestHelpers;

public static class TestDataGenerator
{
    private static int _cpfBase = 100_000_000;
    private static int _placaCounter = 1000;

    /// <summary>
    /// Generates a unique valid CPF (Brazilian individual tax ID) on each call.
    /// Uses an atomic counter as the 9-digit base and computes the two check digits.
    /// </summary>
    public static string NextCpf()
    {
        var n = Interlocked.Increment(ref _cpfBase);
        var s = n.ToString().PadLeft(9, '0');
        var d = s[..9].Select(c => c - '0').ToArray();

        var sum1 = 0;
        for (int i = 0; i < 9; i++) sum1 += d[i] * (10 - i);
        var r1 = sum1 % 11;
        var c1 = r1 < 2 ? 0 : 11 - r1;

        var sum2 = 0;
        for (int i = 0; i < 9; i++) sum2 += d[i] * (11 - i);
        sum2 += c1 * 2;
        var r2 = sum2 % 11;
        var c2 = r2 < 2 ? 0 : 11 - r2;

        return string.Concat(d) + c1 + c2;
    }

    /// <summary>
    /// Generates a unique valid Brazilian license plate (old format AAA9999) on each call.
    /// </summary>
    public static string NextPlaca()
    {
        var n = Interlocked.Increment(ref _placaCounter);
        return $"TST{n:D4}";
    }

    public static string NextEmail(string prefix = "seed") =>
        $"{prefix}_{Guid.NewGuid():N}@oficina.com";
}
