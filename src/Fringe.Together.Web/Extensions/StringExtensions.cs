namespace Fringe.Together.Web.Extensions;

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
    
    public static string CreateMD5(this string input)
    {
        using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes); 
    }
}
