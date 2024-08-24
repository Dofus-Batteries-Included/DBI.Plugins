using System.Collections.Generic;
using System.Text;

namespace DofusBatteriesIncluded.Core.Extensions;

public static class StringExtensions
{
    public static string RemoveAccents(this string str)
    {
        Dictionary<char, char> replacements = new()
        {
            { 'à', 'a' },
            { 'ç', 'c' },
            { 'é', 'e' },
            { 'è', 'e' },
            { 'ê', 'e' },
            { 'ë', 'e' },
            { 'ô', 'o' },
            { 'û', 'u' },
            { 'ù', 'u' }
        };

        Dictionary<char, string> strReplacements = new()
        {
            { 'œ', "oe" },
            { 'Œ', "Oe" }
        };

        StringBuilder stringBuilder = new();

        foreach (char c in str)
        {
            if (replacements.TryGetValue(c, out char r))
            {
                stringBuilder.Append(r);
            }
            else if (strReplacements.TryGetValue(c, out string s))
            {
                stringBuilder.Append(s);
            }
            else
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }
}
