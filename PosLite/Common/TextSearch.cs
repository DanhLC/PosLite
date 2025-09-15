using System.Globalization;
using System.Text;

namespace PosLite.Common;

public static class TextSearch
{
    /// <summary>
    /// Normalize a string for text search by removing diacritics and converting to lowercase.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
    }
}
