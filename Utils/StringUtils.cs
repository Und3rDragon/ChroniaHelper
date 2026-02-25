using System;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ChroniaHelper.Utils.ChroniaSystem;
using YoctoHelper.Cores;
using static Celeste.FancyText;
using static ChroniaHelper.ChroniaHelperModule;

namespace ChroniaHelper.Utils;

public static class StringUtils
{
    public static bool IsNull(this string str)
    {
        return ObjectUtils.IsNull(str);
    }

    public static bool IsNotNull(this string str)
    {
        return ObjectUtils.IsNotNull(str);
    }

    public static bool IsEmpty(this string str)
    {
        return (StringUtils.IsNotNull(str)) && (str.Length <= 0);
    }

    public static bool IsNotEmpty(this string str)
    {
        return (StringUtils.IsNotNull(str)) && (str.Length > 0);
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return (StringUtils.IsNull(str)) || (str.Length <= 0);
    }
    
    public static bool IsNotNullOrEmpty(this string str)
    {
        return !IsNullOrEmpty(str);
    }

    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static bool IsNotWhiteSpace(this string str)
    {
        return (!string.IsNullOrWhiteSpace(str));
    }

    public static bool IsString(this object obj)
    {
        return (obj is string);
    }

    public static void EmptyStringFiller(ref string str, string defaultValue)
    {
        if (StringUtils.IsNullOrEmpty(str))
        {
            str = defaultValue;
        }
    }

    public static string EmptyStringFiller(string str, string defaultValue)
    {
        return (StringUtils.IsNullOrEmpty(str)) ? defaultValue : str;
    }

    public static void WhiteSpaceStringFiller(ref string str, string defaultValue)
    {
        if (StringUtils.IsNullOrWhiteSpace(str))
        {
            str = defaultValue;
        }
    }

    public static string WhiteSpaceStringFiller(string str, string defaultValue)
    {
        return (StringUtils.IsNullOrWhiteSpace(str)) ? defaultValue : str;
    }

    public static string SubStringTilEnd(this string str, int start, int end)
    {
        return str.Substring(start, end - start);
    }

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

    public static int Count(this string str, string find)
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

    public static string[] Split(this string str, string separator = ",", bool removeEmpty = true, bool trim = true)
    {
        return StringUtils.Split(str, [separator], removeEmpty, trim);
    }

    public static string[] Split(this string str, string[] separator, bool removeEmpty = true, bool trim = true)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new string[0];
        }
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

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
        {
            sb.Append(b.ToString("X2"));
        }
            
        return sb.ToString().ToLower();
    }

    public static string GetHashString(string inputString, bool caseSensitive)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(caseSensitive? inputString : inputString.ToLower()))
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString().ToLower();
    }

    public static string GetWildcardPart(this string input, string wildcardMatch, string wildcardSymbol, string compensateSplitSymbol = "|")
    {
        string[] divided = wildcardMatch.Split(wildcardSymbol);

        string s = input;
        for(int i = 0; i < divided.Length; i++)
        {
            if (!s.Contains(divided[i])) { return ""; }

            s = s.Replace(divided[i], compensateSplitSymbol);
        }
        string[] wildcards = s.Split(compensateSplitSymbol);

        string wildcardPart = "";
        for(int i = 0; i < wildcards.Length; i++)
        {
            if (wildcards[i] == wildcards[0]) { wildcardPart = wildcards[0]; }
            else { wildcardPart = ""; }
        }

        return wildcardPart;
    }

    public static string GetWildcardPart(this string input, string wildcardMatch, string wildcardSymbol, out bool success, string compensateSplitSymbol = "|")
    {
        success = true;
        string[] divided = wildcardMatch.Split(wildcardSymbol, StringSplitOptions.RemoveEmptyEntries);
        
        string s = input;
        for (int i = 0; i < divided.Length; i++)
        {
            if (!s.Contains(divided[i])) { success = false; return ""; }

            s = s.Replace(divided[i], compensateSplitSymbol);
        }
        string[] wildcards = s.Split(compensateSplitSymbol, StringSplitOptions.RemoveEmptyEntries);

        string wildcardPart = "";
        for (int i = 0; i < wildcards.Length; i++)
        {
            if (wildcards[i] == wildcards[0]) { wildcardPart = wildcards[0]; }
            else { wildcardPart = ""; success = false; }
        }

        if (string.IsNullOrEmpty(wildcardPart))
        {
            success = false;
        }
        return wildcardPart;
    }

    public static bool IsBool(this string input)
    {
        return input.ToLower() == "true" || input.ToLower() == "false";
    }

    public static bool IsInt(this string input)
    {
        int n = 0;
        return int.TryParse(input, out n);
    }

    public static bool IsFloat(this string input)
    {
        float n = 0;
        return float.TryParse(input, out n);
    }

    public static int ToIntOrCounter(this string input)
    {
        if (int.TryParse(input, out int n))
        {
            return n;
        }

        return MaP.level?.Session?.GetCounter(input) ?? 0;
    }

    public static float ToFloatOrSlider(this string input)
    {
        if (float.TryParse(input, out float n))
        {
            return n;
        }

        return MaP.level?.Session?.GetSlider(input) ?? 0;
    }

    public static string[][] ParseSquaredString(this string source, string firstSeparator = ";", string secondSeparator = ",", StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        string[] _s = source.Split(firstSeparator, split);
        string[][] r = new string[_s.Length][];
        for(int i = 0; i < _s.Length; i++)
        {
            r[i] = _s[i].Split(secondSeparator, split);
        }
        
        return r;
    }
    
    public static Dictionary<string, string> SquareStringToDictionary(this string[][] enter, bool defaultBehaviour = true, int indexStringIndex = 0)
    {
        Dictionary<string, string> s = new();
        
        foreach(var item in enter)
        {
            if (defaultBehaviour)
            {
                if (item.Length < 1) { continue; }

                if (item.Length == 1) { s.Enter(item[0], string.Empty); continue; }

                s.Enter(item[0], item[1]);
            }
            else
            {
                if (item.Length < indexStringIndex + 1) { continue; }

                string indexString, valueString;
                indexString = item[0];

                if (item.Length == indexStringIndex + 1)
                {
                    for(int i = 1; i < item.Length; i++)
                    {
                        indexString += item[i];
                    }

                    s.Enter(indexString, string.Empty);

                    continue;
                }

                for (int i = 1; i <= indexStringIndex; i++)
                {
                    indexString += item[i];
                }

                valueString = item[indexStringIndex + 1];
                
                for(int i = indexStringIndex + 2; i < item.Length; i++)
                {
                    valueString += item[i];
                }

                s.Enter(indexString, valueString);
            }
        }

        return s;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="getDialog">Use Dialog.Get to fetch the original string data</param>
    /// <param name="language"></param>
    /// <returns></returns>
    public static string ParseDialogMetaToString(this string getDialog, Language language = null)
    {
        string[] array = Regex.Split(getDialog, (language ?? Dialog.Language).SplitRegex);
        string[] array2 = new string[array.Length];
        int num = 0;
        for (int k = 0; k < array.Length; k++)
        {
            if (!string.IsNullOrEmpty(array[k]))
            {
                array2[num++] = array[k];
            }
        }

        // Split the strings into substrings

        List<string> resultList = new List<string>();
        string currentSegment = "";
        int i = 0;

        while (i < array2.Length)
        {
            string token = array2[i];

            // 处理普通文本
            if (token != "{" && token != "}")
            {
                currentSegment += token;
                i++;
                continue;
            }

            // 处理指令开始
            if (token == "{")
            {
                i++;
                if (i >= array2.Length) break;

                string command = array2[i];

                // 检查是否是闭合的大括号
                if (command == "}")
                {
                    i++;
                    continue;
                }

                // 收集完整的指令内容直到遇到闭合大括号
                List<string> commandParts = new List<string> { command };
                i++;

                while (i < array2.Length && array2[i] != "}")
                {
                    if (array2[i] != "{") // 避免嵌套的大括号
                    {
                        commandParts.Add(array2[i]);
                    }
                    i++;
                }

                // 跳过闭合的 }
                if (i < array2.Length && array2[i] == "}")
                {
                    i++;
                }

                // 处理指令
                string fullCommand = string.Join("", commandParts);
                string[] parts = fullCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    string cmd = parts[0];

                    // 处理 {break}
                    if (cmd == "break")
                    {
                        currentSegment += "\r\n";
                    }
                    // 处理 {n}
                    else if (cmd == "n")
                    {
                        currentSegment += "\n";
                    }
                    // 处理 {counter xxx n}
                    else if (cmd == "counter" && parts.Length >= 2)
                    {
                        string counterName = parts[1];
                        int minDigits = 0;

                        if (parts.Length >= 3 && int.TryParse(parts[2], out int parsedDigits))
                        {
                            minDigits = parsedDigits;
                        }

                        int value = counterName.GetCounter();
                        currentSegment += value.ToString($"D{minDigits}");
                    }
                    // 处理 {slider xxx m n}
                    else if (cmd == "slider" && parts.Length >= 2)
                    {
                        string sliderName = parts[1];
                        int intMinDigits = 0;
                        int decimalMaxDigits = 2;

                        if (parts.Length >= 3 && int.TryParse(parts[2], out int parsedIntDigits))
                        {
                            intMinDigits = parsedIntDigits;
                        }

                        if (parts.Length >= 4 && int.TryParse(parts[3], out int parsedDecimalDigits))
                        {
                            decimalMaxDigits = parsedDecimalDigits;
                        }

                        float value = sliderName.GetSlider();

                        // 格式化浮点数
                        string format = $"F{decimalMaxDigits}";
                        string formattedValue = value.ToString(format);

                        // 处理整数部分的最小位数
                        string[] numberParts = formattedValue.Split('.');
                        string intPart = numberParts[0];

                        if (intPart.Length < intMinDigits)
                        {
                            intPart = intPart.PadLeft(intMinDigits, '0');
                        }

                        if (numberParts.Length > 1)
                        {
                            currentSegment += intPart + "." + numberParts[1];
                        }
                        else
                        {
                            currentSegment += intPart;
                        }
                    }
                    // 处理 {savedata name}
                    else if (cmd == "savedata" && parts.Length >= 2)
                    {
                        string dataName = parts[1];

                        // 这里可以根据不同的dataName返回不同的值
                        // 示例中要求将{savedata name}替换成"Madeline"
                        if (dataName == "Name")
                        {
                            currentSegment += (Celeste.SaveData.Instance?.Name ?? "Madeline");
                        }
                        else
                        {
                            // 可以根据需要处理其他savedata类型
                            currentSegment += ""; // 或者其他默认值
                        }
                    }
                    // 其他大括号指令都替换为空
                    else
                    {
                        // 不做任何添加，相当于替换为""
                    }
                }
            }
        }

        // 添加最后一个段落到结果中
        if (!string.IsNullOrEmpty(currentSegment))
        {
            resultList.Add(currentSegment);
        }

        return string.Concat(resultList);
    }

    public static string ParseDialogToString(this string id, Language lang = null)
    {
        return Dialog.Get(id, lang).ParseDialogMetaToString(lang);
    }
}
