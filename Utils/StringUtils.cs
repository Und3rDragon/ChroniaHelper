using System;
using System.Security.Cryptography;
using System.Text;

namespace ChroniaHelper.Utils;

public static class StringUtils
{

    public static string[] CharArrayToStringArray(char[] charArray)
    {
        string[] stringArray = new string[charArray.Length];
        for (int i = 0; i < charArray.Length; i++)
        {
            stringArray[i] = charArray[i].ToString();
        }
        return stringArray;
    }

    public static char[] StringArrayToCharArray(string[] stringArray)
    {
        int length = 0;
        foreach (string str in stringArray)
        {
            length += str.Length;
        }
        char[] charArray = new char[length];
        int index = 0;
        foreach (string str in stringArray)
        {
            foreach (char character in str)
            {
                charArray[index] = character;
                index++;
            }
        }
        return charArray;
    }

    public static int Count(string str, string find)
    {
        int count = 0;
        for (int i = 0; (i = str.IndexOf(find, i)) != -1; i++)
        {
            count++;
        }
        return count;
    }

    public static string[] Split(string str, char[] separator, bool removeEmpty = true, bool trim = true)
    {
        return Split(str, StringUtils.CharArrayToStringArray(separator), removeEmpty, trim);
    }

    public static string[] Split(string str, string separator, bool removeEmpty = true, bool trim = true)
    {
        return Split(str, [separator], removeEmpty, trim);
    }

    public static string[] Split(string str, string[] separator, bool removeEmpty = true, bool trim = true)
    {
        string[] split = str.Split(separator, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        if (trim)
        {
            for (int i = 0; i < split.Length; i++)
            {
                split[i] = split[i].Trim();
            }
        }
        return split;
    }

    public static int[] SplitToIntArray(string str, bool removeEmpty = true)
    {
        return SplitToIntArray(str, ",", removeEmpty);
    }

    public static int[] SplitToIntArray(string str, char[] separator, bool removeEmpty = true)
    {
        return SplitToIntArray(str, StringUtils.CharArrayToStringArray(separator), removeEmpty);
    }

    public static int[] SplitToIntArray(string str, string separator, bool removeEmpty = true)
    {
        return SplitToIntArray(str, [separator], removeEmpty);
    }

    public static int[] SplitToIntArray(string str, string[] separator, bool removeEmpty = true)
    {
        string[] split = Split(str, separator, removeEmpty);
        int[] intArray = new int[split.Length];
        for (int i = 0; i < split.Length; i++)
        {
            intArray[i] = split[i].ParseInt();
        }
        return intArray;
    }

    public static string SubstringByLength(string str, int start, int length)
    {
        return str.Substring(start, length);
    }

    public static string SubstringByIndex(string str, int start, int end)
    {
        return str.Substring(start, end - start);
    }

    // flag string utils
    public static string AddAsPrefix(this string baseString, string prefix)
    {
        return prefix + baseString;
    }

    public static string AddAsSuffix(this string baseString, string suffix) 
    {
        return baseString + suffix;
    }

    public static string RemoveFirst(this string baseString, string newString)
    {
        int n = baseString.IndexOf(newString);
        return baseString.Remove(n, n + newString.Length);
    }

    public static string RemoveLast(this string baseString, string newString)
    {
        string a = baseString;
        int index = a.IndexOf(newString);
        int start = a.IndexOf(newString) + newString.Length;
        a = a.Substring(start);
        while (a.Contains(newString))
        {
            start = a.IndexOf(newString) + newString.Length;
            index += start;
            a = a.Substring(start);
        }
        return baseString.Remove(index, newString.Length);
    }

    public static string RemoveAll(this string baseString, string newString)
    {
        return baseString.Replace(newString, "");
    }

    public static string ReplaceFirst(this string baseString, string newString)
    {
        int n = baseString.IndexOf(newString);
        string s1 = baseString.Substring(0, n + newString.Length);
        string s2 = baseString.Substring(n + newString.Length);
        string f = s1.Replace(baseString, newString) + s2;
        return f;
    }

    public static string ReplaceLast(this string baseString, string newString)
    {
        string a = baseString;
        int index = a.IndexOf(newString);
        int start = a.IndexOf(newString) + newString.Length;
        a = a.Substring(start);
        while (a.Contains(newString))
        {
            start = a.IndexOf(newString) + newString.Length;
            index += start;
            a = a.Substring(start);
        }
        string s1 = baseString.Substring(0, index);
        string s2 = baseString.Substring(index);
        return s1 + s2.Replace(baseString, newString);
    }

    public static string ReplaceAll(this string baseString, string newString)
    {
        return baseString.Replace(baseString, newString);
    }

    public static string Backspace(this string baseString)
    {
        string s = baseString;
        if (s.Length <= 1) { return string.Empty; }
        s = s.Substring(0, s.Length - 1);
        return s;
    }

    public static string Delete(this string baseString)
    {
        string s = baseString;
        if (s.Length <= 1) { return string.Empty; }
        s = s.Substring(1, s.Length - 1);
        return s;
    }

    // Hash code algorithm from Aurora Aquir
    public static byte[] GetHash(string inputString)
    {
        HashAlgorithm algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static byte[] GetHash(string inputString, string id)
    {
        HashAlgorithm algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(id + inputString));
    }

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
        {
            sb.Append(b.ToString("X2"));
        }
            
        return sb.ToString().ToLower();
    }

    public static string GetHashString(string inputString, string id)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString, id))
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString().ToLower();
    }
}
