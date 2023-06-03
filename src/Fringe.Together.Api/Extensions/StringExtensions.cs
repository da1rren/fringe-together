namespace Fringe.Together.Api.Extensions;

using AngleSharp.Text;
using System.Text;

public static class StringExtensions
{
    public static string SanitizeWhitespace(this string str)
    {
        var builder = new StringBuilder();
        var lastCharacterWasSpace = false;

        foreach (var character in str)
        {
            if (character.IsWhiteSpaceCharacter())
            {
                if (lastCharacterWasSpace)
                {
                    continue;
                }

                builder.Append(character);
                lastCharacterWasSpace = true;
            }
            else
            {
                builder.Append(character);
                lastCharacterWasSpace = false;
            }
        }

        return builder.ToString();
    }
}
