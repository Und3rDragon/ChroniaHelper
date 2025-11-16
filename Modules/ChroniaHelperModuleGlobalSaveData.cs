using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Celeste;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System.Numerics; // Ensure this using is present for BigInteger

namespace ChroniaHelper.Modules;

// ===== 内部成员包装器：统一处理 Field 和 Property =====
internal readonly struct SaveMember
{
    public readonly FieldInfo? _field;
    public readonly PropertyInfo? _property;

    public SaveMember(FieldInfo field)
    {
        _field = field;
        _property = null;
    }

    public SaveMember(PropertyInfo property)
    {
        _property = property;
        _field = null;
    }

    public object GetValue(object instance) =>
        _field?.GetValue(instance) ?? _property!.GetValue(instance);

    public void SetValue(object instance, object value)
    {
        try
        {
            if (_field != null)
                _field.SetValue(instance, value);
            else
                _property!.SetValue(instance, value);
        }
        catch (ArgumentException ae)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to set value for '{_field?.Name ?? _property?.Name}'. Type mismatch: {ae.Message}");
            // Optionally re-throw or handle differently
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Unexpected error setting value for '{_field?.Name ?? _property?.Name}': {e.Message}");
        }
    }
}

// ===== 全局保存数据基类 =====
public abstract class ChroniaHelperModuleGlobalSaveData
{
    // 缓存成员名 -> 路径映射
    private readonly Dictionary<string, string> _memberToPath = new();
    private readonly Dictionary<string, SaveMember> _members = new();

    // 默认保存文件名
    public const string DEFAULT_SAVE_PATH = "ChroniaHelperGlobalSaveData.xml";

    protected ChroniaHelperModuleGlobalSaveData()
    {
        Initialize();
    }

    private void Initialize()
    {
        var type = GetType();

        // === 处理字段（支持 public / non-public / instance）===
        var allFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var validFields = allFields.Where(field =>
            !field.Name.StartsWith("<") // 过滤掉编译器生成的自动属性后台字段
        ).ToArray();

        foreach (var field in validFields)
        {
            var attr = field.GetCustomAttribute<ChroniaGlobalSavePathAttribute>();
            string relativePath = attr?.RelativePath ?? DEFAULT_SAVE_PATH;
            string fullPath = Path.Combine(Everest.PathGame, "Saves", "ChroniaHelper", relativePath);
            _memberToPath[field.Name] = fullPath;
            _members[field.Name] = new SaveMember(field);
        }

        // === 新增：处理属性（必须是 public、instance、可读可写）===
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead && p.CanWrite);
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<ChroniaGlobalSavePathAttribute>();

            string relativePath;
            if (attr != null)
            {
                relativePath = attr.RelativePath;
            }
            else
            {
                // 如果没有指定路径，使用默认路径
                relativePath = DEFAULT_SAVE_PATH;
            }

            string fullPath = Path.Combine(Everest.PathGame, "Saves", "ChroniaHelper", relativePath);
            _memberToPath[prop.Name] = fullPath;
            _members[prop.Name] = new SaveMember(prop);
        }
    }


    // ===== 加载所有数据 =====
    public void LoadAll()
    {
        var pathGroups = _memberToPath
            .GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

        foreach (var (filePath, memberNames) in pathGroups)
        {
            if (!File.Exists(filePath))
            {
                // 首次运行，跳过加载（保留默认值）
                Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Save file not found, skipping load: {filePath}");
                continue;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                XmlNode rootNode = doc.SelectSingleNode("/ChroniaHelperGlobalData");
                if (rootNode == null)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Malformed XML in {filePath}: Missing root element 'ChroniaHelperGlobalData'. Skipping.");
                    continue;
                }

                foreach (string memberName in memberNames)
                {
                    XmlNode node = rootNode.SelectSingleNode(memberName);
                    if (node != null)
                    {
                        try
                        {
                            object value = DeserializeNode(node, memberName); // Pass memberName for context
                            _members[memberName].SetValue(this, value);
                        }
                        catch (Exception innerEx)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error deserializing member '{memberName}' from {filePath}: {innerEx.Message}");
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Node for member '{memberName}' not found in {filePath}. Keeping default value.");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to load {filePath}: {e}");
            }
        }
    }

    // ===== 保存所有数据 =====
    public void SaveAll()
    {
        var pathGroups = _memberToPath
            .GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

        foreach (var (filePath, memberNames) in pathGroups)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                var doc = new XmlDocument();
                XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(decl);

                XmlElement root = doc.CreateElement("ChroniaHelperGlobalData");
                doc.AppendChild(root);

                foreach (string memberName in memberNames)
                {
                    try
                    {
                        object value = _members[memberName].GetValue(this);
                        XmlElement valueNode = SerializeValue(doc, memberName, value);
                        root.AppendChild(valueNode);
                    }
                    catch (Exception innerEx)
                    {
                        Logger.Log(LogLevel.Error, "ChroniaHelper", $"Error serializing member '{memberName}' for {filePath}: {innerEx.Message}");
                        // Consider adding a placeholder node or skipping?
                    }
                }

                doc.Save(filePath);
                Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Successfully saved data to {filePath}");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed to save {filePath}: {e}");
            }
        }
    }

    // ===== 序列化单个值 =====
    private XmlElement SerializeValue(XmlDocument doc, string name, object value)
    {
        // ✅ Add validation for XML element names
        ValidateXmlElementName(name);

        if (value == null)
        {
            var elem = doc.CreateElement(name);
            elem.SetAttribute("isNull", "true");
            return elem;
        }

        Type type = value.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            return SerializeDictionary(doc, name, value);
        }
        else if (type.IsClass && type != typeof(string))
        {
            return SerializeClass(doc, name, value);
        }
        else
        {
            var elem = doc.CreateElement(name);
            elem.InnerText = value.ToString() ?? "";
            string typeCode;
            if (type == typeof(BigInteger))
                typeCode = "biginteger";
            else
                typeCode = Type.GetTypeCode(type).ToString().ToLowerInvariant();
            elem.SetAttribute("type", typeCode);
            return elem;
        }
    }

    // ===== 反序列化节点 =====
    private object DeserializeNode(XmlNode node, string memberName = "")
    {
        if (node.Attributes?["isNull"]?.Value == "true")
            return null;

        if (node.Name == "Dictionary")
        {
            return DeserializeDictionary(node);
        }
        else if (node.Name == "Class")
        {
            return DeserializeClass(node);
        }
        else
        {
            // --- Enhanced Deserialization Logic ---
            string text = node.InnerText;

            // 1. Try to determine target type from the member info
            if (_members.TryGetValue(node.Name, out var member))
            {
                Type targetType = member._property?.PropertyType ?? member._field?.FieldType;
                if (targetType != null)
                {
                    try
                    {
                        return ParseBasicType(text, targetType);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse '{text}' to target type '{targetType.Name}' for member '{node.Name}'. Falling back to type attribute. Error: {ex.Message}");
                        // Fall through to legacy logic below
                    }
                }
            }

            // 2. Fallback: Use type attribute or default to string
            string typeName = node.Attributes?["type"]?.Value ?? "string";

            return typeName.ToLowerInvariant() switch
            {
                "int" => int.Parse(text),
                "float" => float.Parse(text),
                "double" => double.Parse(text),
                "bool" => bool.Parse(text),
                "string" => text,
                "datetime" => DateTime.Parse(text),
                "biginteger" => BigInteger.Parse(text),
                _ => text // final fallback
            };
        }
    }

    // ===== 序列化 Dictionary<K,V> =====
    private XmlElement SerializeDictionary(XmlDocument doc, string name, object dict)
    {
        ValidateXmlElementName(name); // Validate wrapper element name

        var elem = doc.CreateElement("Dictionary");
        elem.SetAttribute("name", name);

        Type dictType = dict.GetType();
        Type keyType = dictType.GetGenericArguments()[0];
        Type valueType = dictType.GetGenericArguments()[1];

        elem.SetAttribute("keyType", keyType.Name);
        elem.SetAttribute("valueType", valueType.Name);

        var keys = dictType.GetProperty("Keys")?.GetValue(dict) as System.Collections.IEnumerable;
        var indexer = dictType.GetProperty("Item");

        if (keys != null && indexer != null)
        {
            foreach (var key in keys)
            {
                var member = doc.CreateElement("DictionaryMember");

                // Key
                object keyValue = key ?? throw new InvalidOperationException("Dictionary key cannot be null");
                XmlElement keyNode = SerializeValue(doc, "Key", keyValue);
                // Append the actual content of the serialized key/value node
                XmlNode keyContent = keyNode.FirstChild ?? keyNode;
                XmlNode importedKeyContent = doc.ImportNode(keyContent, true);
                member.AppendChild(importedKeyContent);

                // Value
                object valueValue = indexer.GetValue(dict, new object[] { keyValue });
                XmlElement valueNode = SerializeValue(doc, "Value", valueValue);
                XmlNode valueContent = valueNode.FirstChild ?? valueNode;
                XmlNode importedValueContent = doc.ImportNode(valueContent, true);
                member.AppendChild(importedValueContent);

                elem.AppendChild(member);
            }
        }

        return elem;
    }

    // ===== 反序列化 Dictionary =====
    private object DeserializeDictionary(XmlNode node)
    {
        string keyTypeName = node.Attributes?["keyType"]?.Value ?? "string";
        string valueTypeName = node.Attributes?["valueType"]?.Value ?? "object";

        Type keyType = keyTypeName switch
        {
            "String" => typeof(string),
            "Int32" => typeof(int),
            "Int64" => typeof(long),
            "Single" => typeof(float),
            "Double" => typeof(double),
            "Boolean" => typeof(bool),
            _ => Type.GetType($"System.{keyTypeName}") ?? typeof(string)
        };

        Type valueType = valueTypeName switch
        {
            "String" => typeof(string),
            "Int32" => typeof(int),
            "Int64" => typeof(long),
            "Single" => typeof(float),
            "Double" => typeof(double),
            "Boolean" => typeof(bool),
            _ => Type.GetType($"System.{valueTypeName}") ?? typeof(object)
        };

        Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var dict = Activator.CreateInstance(dictType);

        var addMethod = dictType.GetMethod("Add");

        var memberNodes = node.SelectNodes("DictionaryMember");
        if (memberNodes != null)
        {
            foreach (XmlNode member in memberNodes.Cast<XmlNode>())
            {
                var keyNode = member.ChildNodes[0];
                var valueNode = member.ChildNodes[1];

                object key = DeserializeSingleNode(keyNode, keyType);
                object value = DeserializeSingleNode(valueNode, valueType);

                addMethod?.Invoke(dict, new[] { key, value });
            }
        }

        return dict;
    }

    // ===== 序列化 Class =====
    private XmlElement SerializeClass(XmlDocument doc, string name, object obj)
    {
        ValidateXmlElementName(name); // Validate wrapper element name

        var elem = doc.CreateElement("Class");
        elem.SetAttribute("name", name);
        elem.SetAttribute("classType", obj.GetType().FullName ?? obj.GetType().Name); // FullName safer

        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            object value = field.GetValue(obj);
            XmlElement attr = doc.CreateElement("Attribute");
            attr.SetAttribute("name", field.Name);
            attr.SetAttribute("type", field.FieldType.FullName ?? field.FieldType.Name); // FullName safer

            if (value == null)
            {
                attr.SetAttribute("isNull", "true");
            }
            else
            {
                // For complex types, recursively serialize
                if (field.FieldType.IsClass && field.FieldType != typeof(string))
                {
                    // Nested Class or Dictionary
                    XmlElement nested = SerializeValue(doc, field.Name, value);
                    XmlNode nestedContent = nested.FirstChild ?? nested;
                    XmlNode importedNestedContent = doc.ImportNode(nestedContent, true);
                    attr.AppendChild(importedNestedContent);
                }
                else
                {
                    attr.InnerText = value.ToString() ?? "";
                }
            }

            elem.AppendChild(attr);
        }

        return elem;
    }

    // ===== 反序列化 Class =====
    private object DeserializeClass(XmlNode node)
    {
        string classTypeName = node.Attributes?["classType"]?.Value;
        if (string.IsNullOrEmpty(classTypeName)) return new object();

        // ⚠️ Simplified: Assume class is in current assembly or a common one
        Type classType = Type.GetType(classTypeName) ??
                         GetType().Assembly.GetType(classTypeName) ??
                         typeof(object);

        if (classType == typeof(object) || classType == null) return new object();

        var obj = Activator.CreateInstance(classType);
        var fields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        var attributeMembers = node.SelectNodes("Attribute");
        if (attributeMembers != null)
        {
            foreach (XmlNode attrNode in attributeMembers.Cast<XmlNode>())
            {
                string fieldName = attrNode.Attributes?["name"]?.Value;
                string typeName = attrNode.Attributes?["type"]?.Value;
                bool isNull = attrNode.Attributes?["isNull"]?.Value == "true";

                var field = fields.FirstOrDefault(f => f.Name == fieldName);
                if (field == null) continue;

                if (isNull)
                {
                    field.SetValue(obj, null);
                }
                else
                {
                    if (field.FieldType.IsClass && field.FieldType != typeof(string))
                    {
                        // Recursively deserialize child node
                        XmlNode childNode = attrNode.FirstChild ?? attrNode;
                        object nested = DeserializeNode(childNode, fieldName ?? "");
                        field.SetValue(obj, nested);
                    }
                    else
                    {
                        string text = attrNode.InnerText;
                        // Prefer parsing based on field type
                        object val = ParseBasicType(text, field.FieldType);
                        field.SetValue(obj, val);
                    }
                }
            }
        }

        return obj;
    }

    // ===== 工具方法 =====
    private object DeserializeSingleNode(XmlNode node, Type targetType)
    {
        if (targetType == typeof(string)) return node.InnerText;
        if (node.Name == "Class") return DeserializeClass(node);
        if (node.Name == "Dictionary") return DeserializeDictionary(node);

        return ParseBasicType(node.InnerText, targetType);
    }

    private object ParseBasicType(string text, Type targetType)
    {
        if (targetType == typeof(string)) return text;
        if (string.IsNullOrEmpty(text)) return GetDefaultValue(targetType);

        try
        {
            if (targetType == typeof(int) || targetType == typeof(int?)) return int.Parse(text);
            if (targetType == typeof(long) || targetType == typeof(long?)) return long.Parse(text);
            if (targetType == typeof(float) || targetType == typeof(float?)) return float.Parse(text);
            if (targetType == typeof(double) || targetType == typeof(double?)) return double.Parse(text);
            if (targetType == typeof(bool) || targetType == typeof(bool?)) return bool.Parse(text);
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?)) return DateTime.Parse(text);
            if (targetType == typeof(BigInteger)) return BigInteger.Parse(text);
            // Add more types as needed...
        }
        catch (FormatException fe)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse '{text}' to type '{targetType.Name}': Format error. Returning default.");
            return GetDefaultValue(targetType);
        }
        catch (OverflowException oe)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse '{text}' to type '{targetType.Name}': Overflow error. Returning default.");
            return GetDefaultValue(targetType);
        }

        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"No parser defined for type '{targetType.Name}', returning raw text.");
        return text; // Final fallback
    }

    private object GetDefaultValue(Type t)
    {
        if (t.IsValueType)
        {
            return Activator.CreateInstance(t);
        }
        return null;
    }

    // Helper method to validate XML element names
    private static void ValidateXmlElementName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("XML element name cannot be null or whitespace.", nameof(name));
        }

        if (!IsValidXmlName(name))
        {
            throw new ArgumentException($"Invalid characters found in XML element name: '{name}'", nameof(name));
        }
    }

    // A simple check for valid XML 1.0 NameStartChar and NameChar (simplified)
    private static bool IsValidXmlName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        char firstChar = name[0];
        if (!((firstChar >= 'A' && firstChar <= 'Z') ||
              (firstChar >= 'a' && firstChar <= 'z') ||
              firstChar == '_' || firstChar == ':'))
        {
            return false;
        }

        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (!((c >= 'A' && c <= 'Z') ||
                  (c >= 'a' && c <= 'z') ||
                  (c >= '0' && c <= '9') ||
                  c == '_' || c == '-' || c == '.' || c == ':' ||
                  c == 0xB7 || (c >= 0xC0 && c <= 0xD6) || (c >= 0xD8 && c <= 0xF6) || (c >= 0xF8 && c <= 0x2FF) ||
                  (c >= 0x370 && c <= 0x37D) || (c >= 0x37F && c <= 0x1FFF) ||
                  (c >= 0x200C && c <= 0x200D) || (c >= 0x2070 && c <= 0x218F) ||
                  (c >= 0x2C00 && c <= 0x2FEF) || (c >= 0x3001 && c <= 0xD7FF) ||
                  (c >= 0xF900 && c <= 0xFDCF) || (c >= 0xFDF0 && c <= 0xFFFD) ||
                  (c >= 0x10000 && c <= 0xEFFFF))) // Simplified range check
            {
                return false;
            }
        }
        return true;
    }
}