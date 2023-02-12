using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectorNet.Generator;

public static class StringExtensions
{
    private static readonly Regex PascalizeRegex = new Regex("(?:^|_| +)(.)", RegexOptions.Compiled);

    /// <summary>
    /// By default, pascalize converts strings to UpperCamelCase also removing underscores
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Pascalize(this string input)
    {
        return PascalizeRegex.Replace(input, match => match.Groups[1].Value.ToUpper());
    }

    /// <summary>
    /// Same as Pascalize except that the first character is lower case
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Camelize(this string input)
    {
        var word = input.Pascalize();
        return word.Length > 0 ? word.Substring(0, 1).ToLower() + word.Substring(1) : word;
    }
}
