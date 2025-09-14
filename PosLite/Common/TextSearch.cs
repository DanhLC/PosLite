using System.Globalization;
using System.Text;

namespace PosLite.Common;

public static class TextSearch
{
    // Bỏ dấu + về lowercase (ví dụ: "THỨC ĂN CHÓ" -> "thuc an cho")
    public static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
