using System.Globalization;
using System.Text;

namespace Store.Domain.Catalog;

/// <summary>
/// Builds URL slugs from Spanish names: "Juguetes para Gatos" becomes "juguetes-para-gatos".
/// </summary>
internal static class SlugGenerator
{
    public static string From(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var pendingDash = false;

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(c))
            {
                if (pendingDash && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(c));
                pendingDash = false;
            }
            else
            {
                pendingDash = true;
            }
        }

        return builder.ToString();
    }
}
