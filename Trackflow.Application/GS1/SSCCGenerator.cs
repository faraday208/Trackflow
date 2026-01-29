namespace Trackflow.Application.GS1;

public class SSCCGenerator
{
    private readonly string _companyPrefix;
    private int _serialCounter;
    private readonly object _lock = new();

    /// <summary>
    /// SSCC Generator
    /// </summary>
    /// <param name="companyPrefix">Firma GLN prefix'i (7-10 hane)</param>
    /// <param name="startingSerial">Başlangıç seri numarası</param>
    public SSCCGenerator(string companyPrefix, int startingSerial = 1)
    {
        _companyPrefix = companyPrefix.PadLeft(7, '0');
        _serialCounter = startingSerial;
    }

    /// <summary>
    /// Yeni SSCC kodu üretir
    /// Format: Extension Digit (1) + Company Prefix (7-10) + Serial Reference + Check Digit
    /// Toplam: 18 hane
    /// </summary>
    public string GenerateSSCC()
    {
        lock (_lock)
        {
            // Extension digit (0-9)
            var extensionDigit = "0";

            // Company prefix (7 hane)
            var prefix = _companyPrefix;

            // Serial reference - kalan haneleri doldur (9 hane)
            var serial = _serialCounter.ToString().PadLeft(9, '0');

            // SSCC without check digit (17 hane)
            var ssccWithoutCheck = $"{extensionDigit}{prefix}{serial}";

            // Check digit hesapla
            var checkDigit = CalculateLuhnCheckDigit(ssccWithoutCheck);

            _serialCounter++;

            return $"{ssccWithoutCheck}{checkDigit}";
        }
    }

    /// <summary>
    /// Belirtilen seri numarasıyla SSCC kodu üretir
    /// </summary>
    public string GenerateSSCC(int serialNumber)
    {
        var extensionDigit = "0";
        var prefix = _companyPrefix;
        var serial = serialNumber.ToString().PadLeft(9, '0');
        var ssccWithoutCheck = $"{extensionDigit}{prefix}{serial}";
        var checkDigit = CalculateLuhnCheckDigit(ssccWithoutCheck);

        return $"{ssccWithoutCheck}{checkDigit}";
    }

    /// <summary>
    /// Mod 10 (Luhn) check digit hesaplar
    /// </summary>
    private static int CalculateLuhnCheckDigit(string number)
    {
        var digits = number.Select(c => int.Parse(c.ToString())).ToArray();
        var sum = 0;

        // Sağdan sola, tek pozisyonlar x3, çift pozisyonlar x1
        for (int i = 0; i < digits.Length; i++)
        {
            // GS1 standardında sağdan başlayarak: pozisyon 1 = x3, pozisyon 2 = x1, ...
            var multiplier = (digits.Length - i) % 2 == 0 ? 1 : 3;
            sum += digits[i] * multiplier;
        }

        return (10 - (sum % 10)) % 10;
    }

    /// <summary>
    /// SSCC'nin geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool ValidateSSCC(string sscc)
    {
        if (string.IsNullOrWhiteSpace(sscc) || sscc.Length != 18)
            return false;

        if (!sscc.All(char.IsDigit))
            return false;

        var checkDigit = int.Parse(sscc[^1].ToString());
        var calculatedCheckDigit = CalculateLuhnCheckDigit(sscc[..^1]);

        return checkDigit == calculatedCheckDigit;
    }
}
