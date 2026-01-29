namespace Trackflow.Application.GS1;

public class GS1Service
{
    /// <summary>
    /// GS1-128 barkod string'i oluşturur
    /// Format: (01){GTIN}(21){SeriNo}(17){SKT}(10){Lot}
    /// </summary>
    public string GenerateGS1Barcode(string gtin, string serialNumber, DateTime expiryDate, string lotNumber)
    {
        // GTIN (01) - 14 karakter
        var gtinPadded = gtin.PadLeft(14, '0');

        // Serial Number (21) - değişken uzunluk
        var serial = serialNumber;

        // Expiry Date (17) - YYMMDD format
        var expiry = expiryDate.ToString("yyMMdd");

        // Lot Number (10) - değişken uzunluk
        var lot = lotNumber;

        return $"(01){gtinPadded}(21){serial}(17){expiry}(10){lot}";
    }

    /// <summary>
    /// GTIN check digit hesaplar (Mod 10 algoritması)
    /// </summary>
    public int CalculateGTINCheckDigit(string gtinWithoutCheckDigit)
    {
        var digits = gtinWithoutCheckDigit.PadLeft(13, '0').Select(c => int.Parse(c.ToString())).ToArray();
        var sum = 0;

        for (int i = 0; i < digits.Length; i++)
        {
            sum += digits[i] * (i % 2 == 0 ? 1 : 3);
        }

        var checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit;
    }

    /// <summary>
    /// GTIN'in geçerli olup olmadığını kontrol eder
    /// </summary>
    public bool ValidateGTIN(string gtin)
    {
        if (string.IsNullOrWhiteSpace(gtin) || gtin.Length < 8 || gtin.Length > 14)
            return false;

        if (!gtin.All(char.IsDigit))
            return false;

        var gtinPadded = gtin.PadLeft(14, '0');
        var checkDigit = int.Parse(gtinPadded[^1].ToString());
        var calculatedCheckDigit = CalculateGTINCheckDigit(gtinPadded[..^1]);

        return checkDigit == calculatedCheckDigit;
    }
}
