using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics; // Ensure this using is present for BigInteger
using System.Reflection;
using System.Xml;
using Celeste;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

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
    // 常量 
    private const string NODE_LIST = "List";
    private const string NODE_DICTIONARY = "Dictionary";
    private const string NODE_ILIST2 = "IList2";
    private const string NODE_ILIST3 = "IList3";
    private const string NODE_ILIST4 = "IList4";
    private const string NODE_IDICT2 = "IDictionary2";
    private const string NODE_IDICT3 = "IDictionary3";
    private const string NODE_IDICT4 = "IDictionary4";

    private const string ATTR_NAME = "name";
    private const string ATTR_KEY_TYPE = "keyType";
    private const string ATTR_VALUE_TYPE = "valueType";
    private const string ATTR_ELEMENT_TYPE = "elementType";
    private const string ATTR_T1 = "T1";
    private const string ATTR_T2 = "T2";
    private const string ATTR_T3 = "T3";
    private const string ATTR_T4 = "T4";

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


    public void LoadAll()
    {
        // 1. Group members by their save path
        var pathGroups = _memberToPath
            .GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

        // 2. Set defaults (your added code) - This runs regardless

        // 3. Loop through each file path
        foreach (var (filePath, memberNames) in pathGroups)
        {
            // 4. Check if file exists
            if (!File.Exists(filePath))
            {
                Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Save file not found, skipping load: {filePath}");
                continue;
            }

            try
            {
                // 5. Load XML document
                var doc = new XmlDocument();
                doc.Load(filePath);

                // 6. Find root node
                XmlNode rootNode = doc.SelectSingleNode("/ChroniaHelperGlobalData");
                if (rootNode == null)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Malformed XML in {filePath}: Missing root element 'ChroniaHelperGlobalData'. Skipping.");
                    continue;
                }

                // 7. Loop through members assigned to this file
                foreach (string memberName in memberNames)
                {
                    XmlNode node = null;

                    // --- 修改查找逻辑 Start ---
                    // 遍历根节点的所有直接子节点
                    foreach (XmlNode potentialNode in rootNode.ChildNodes)
                    {
                        // 检查节点是否有 'name' 属性，并且其值等于我们要找的 memberName
                        XmlAttribute nameAttr = potentialNode.Attributes?["name"];
                        if (nameAttr != null && nameAttr.Value == memberName)
                        {
                            node = potentialNode;
                            break; // 找到了就停止循环
                        }
                    }
                    // --- 修改查找逻辑 End ---

                    if (node != null)
                    {
                        try
                        {
                            Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Found node for member '{memberName}' in {filePath}.");
                            object value = DeserializeNode(node, memberName);

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

                        // === 修改点：传递 memberName 给 SerializeValue ===
                        // 我们传递 memberName 两次：
                        // 第一次作为节点的“类型”指示符（虽然现在 SerializeValue 内部会决定实际节点名）
                        // 第二次作为 nodeNameForAttribute，告诉它应该把这个值作为 'name' 属性
                        XmlElement valueNode = SerializeValue(doc, memberName, value, memberName);
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

    // === 修改方法签名，添加 nodeNameForAttribute 参数 ===
    private XmlElement SerializeValue(XmlDocument doc, string name, object value, string nodeNameForAttribute = null)
    {
        // ValidateXmlElementName(name); // 注意：这里的 'name' 是传进来的 memberName，不一定适合作为 XML 节点名了。
        // 我们将在创建具体节点时再验证或确定节点名。
        // 如果需要，可以验证 nodeNameForAttribute

        // --- Handle Null Values ---
        if (value == null)
        {
            // 对于 null 值，我们仍然创建一个通用节点，并设置 name 属性和 isNull 属性
            var elem = doc.CreateElement("Value"); // 使用通用名称

            // === 添加 name 属性 ===
            if (!string.IsNullOrEmpty(nodeNameForAttribute))
            {
                elem.SetAttribute("name", nodeNameForAttribute);
            }

            elem.SetAttribute("isNull", "true");
            return elem;
        }

        Type type = value.GetType();

        // --- Handle Generic Collection Types ---
        if (type.IsGenericType)
        {
            Type genericTypeDef = type.GetGenericTypeDefinition();

            // Lists and Sets
            if (genericTypeDef == typeof(List<>) || genericTypeDef == typeof(HashSet<>))
            {
                // === 修改点：传递 nodeNameForAttribute ===
                return SerializeList(doc, name, value, nodeNameForAttribute);
            }

            // Custom ILists
            if (genericTypeDef == typeof(IList2<,>))
            {
                return SerializeIList2(doc, name, value, nodeNameForAttribute); // 假设你也会相应修改这些方法
            }
            else if (genericTypeDef == typeof(IList3<,,>))
            {
                return SerializeIList3(doc, name, value, nodeNameForAttribute);
            }
            else if (genericTypeDef == typeof(IList4<,,,>))
            {
                return SerializeIList4(doc, name, value, nodeNameForAttribute);
            }

            // Dictionaries
            if (genericTypeDef == typeof(Dictionary<,>))
            {
                // === 修改点：传递 nodeNameForAttribute ===
                return SerializeDictionary(doc, name, value, nodeNameForAttribute);
            }

            // Custom IDictionaries
            if (genericTypeDef == typeof(IDictionary2<,,>))
            {
                return SerializeIDictionary2(doc, name, value, nodeNameForAttribute);
            }
            else if (genericTypeDef == typeof(IDictionary3<,,,>))
            {
                return SerializeIDictionary3(doc, name, value, nodeNameForAttribute);
            }
            else if (genericTypeDef == typeof(IDictionary4<,,,,>))
            {
                return SerializeIDictionary4(doc, name, value, nodeNameForAttribute);
            }

            // If it's a generic type not handled above, fall through...
            // (e.g., a custom generic class MyGeneric<T>)
            // We'll treat it like any other class at the end.
        }

        // --- Handle All Other Types (Non-Generic, Primitives, String, Classes, Structs) ---
        // This 'else' block executes if the type is NOT a handled generic type.
        {
            // === 修改点：创建通用节点名 "Value" ===
            var elem = doc.CreateElement("Value");

            // === 添加 name 属性 ===
            if (!string.IsNullOrEmpty(nodeNameForAttribute))
            {
                elem.SetAttribute("name", nodeNameForAttribute);
            }

            string typeCodeStr = null; // Will hold the type attribute value
            bool setValueText = false; // Flag to indicate if InnerText should be set
            string valueText = "";     // The text content to set

            // --- Special Handling for Known Complex Types ---
            // Check if it's a class or struct that needs field-by-field serialization.
            // Crucially, 'string' is a class, but we treat it as a primitive value.
            if (type.IsClass && type != typeof(string))
            {
                // === 修改点：传递 nodeNameForAttribute ===
                return SerializeClass(doc, name, value, nodeNameForAttribute); // 假设你也会相应修改这个方法
            }
            else
            {
                // This 'else' branch handles:
                // - Primitive types (int, bool, char, etc.)
                // - Enum types
                // - The string type (because of the check above)
                // - Potentially some value types (structs) that map to simple TypeCodes
                // We determine how to represent them by their TypeCode.

                // Handle BigInteger specially
                if (type == typeof(BigInteger))
                {
                    typeCodeStr = "biginteger";
                    setValueText = true;
                    valueText = value.ToString();
                }
                else
                {
                    TypeCode typeCode = Type.GetTypeCode(type);

                    // TypeCode.Object usually means class/struct/complex type.
                    // However, TypeCode.String specifically identifies the string type.
                    if (typeCode != TypeCode.Object)
                    {
                        // This covers: Boolean, Char, SByte, Byte, Int16, UInt16,
                        // Int32, UInt32, Int64, UInt64, Single, Double, Decimal, DateTime, String
                        // It also covers Enum types, whose underlying type will have a code.
                        typeCodeStr = typeCode.ToString().ToLowerInvariant();
                        setValueText = true;
                        valueText = value.ToString();
                    }
                    else
                    {
                        // This branch handles:
                        // - Custom classes not caught by IsClass check above (shouldn't happen due to order)
                        // - Custom structs (value types) not mapping to simple codes.
                        // For these, defaulting to SerializeClass is often the best option.
                        // Log a warning as it might be unintended.
                        Log.Warn($"Serializing unhandled non-primitive, non-class type '{type.FullName}' using class serializer.");
                        // === 修改点：传递 nodeNameForAttribute ===
                        return SerializeClass(doc, name, value, nodeNameForAttribute); // Fallback to class serialization
                    }
                }
            }

            // Apply the determined text content if needed
            if (setValueText)
            {
                elem.InnerText = valueText;
            }

            // Apply the type attribute if determined
            if (!string.IsNullOrEmpty(typeCodeStr))
            {
                elem.SetAttribute("type", typeCodeStr);
            }

            return elem;
        }

        // --- Ultimate Fallback (Defensive Programming) ---
        // Should be unreachable with the logic above, but ensures compilation safety.
        Logger.Log(LogLevel.Error, "ChroniaHelper", $"CRITICAL: Unreachable code reached in SerializeValue for type {type.FullName}. This indicates a logical flaw.");
        var fallbackElem = doc.CreateElement("Value"); // === 修改点：通用节点名 ===

        // === 添加 name 属性 ===
        if (!string.IsNullOrEmpty(nodeNameForAttribute))
        {
            fallbackElem.SetAttribute("name", nodeNameForAttribute);
        }

        fallbackElem.InnerText = value.ToString() ?? "";
        fallbackElem.SetAttribute("type", "unknown_fallback");
        return fallbackElem;
    }

    // ===== 反序列化节点 =====
    private object DeserializeNode(XmlNode node, string memberName = "")
    {
        if (node.Attributes?["isNull"]?.Value == "true")
            return null;

        // 按顺序检查，先检查更具体的类型
        if (node.Name == NODE_ILIST4)
        {
            return DeserializeIList4(node);
        }
        if (node.Name == NODE_ILIST3)
        {
            return DeserializeIList3(node);
        }
        if (node.Name == NODE_ILIST2)
        {
            return DeserializeIList2(node);
        }

        if (node.Name == NODE_IDICT4)
        {
            return DeserializeIDictionary4(node);
        }
        if (node.Name == NODE_IDICT3)
        {
            return DeserializeIDictionary3(node);
        }
        if (node.Name == NODE_IDICT2)
        {
            return DeserializeIDictionary2(node);
        }

        if (node.Name == NODE_LIST)
        {
            return DeserializeList(node);
        }
        if (node.Name == NODE_DICTIONARY)
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
    private XmlElement SerializeDictionary(XmlDocument doc, string name, object dict, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name); // Validate wrapper element name
        var elem = doc.CreateElement("Dictionary");
        elem.SetAttribute("name", nodeNameForAttribute);

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
                var member = doc.CreateElement("DictionaryMember"); // <DictionaryMember>

                object keyValue = key ?? throw new InvalidOperationException("Dictionary key cannot be null");
                object valueValue = indexer.GetValue(dict, new object[] { keyValue });

                // 将键和值直接作为属性
                member.SetAttribute("key", keyValue.ToString());
                member.SetAttribute("value", valueValue.ToString());

                elem.AppendChild(member); // <Dictionary> -> <DictionaryMember .../>
            }
        }
        return elem;
    }

    // ===== 反序列化 Dictionary =====
    private object DeserializeDictionary(XmlNode node)
    {
        string keyTypeName = node.Attributes?["keyType"]?.Value ?? "string";
        string valueTypeName = node.Attributes?["valueType"]?.Value ?? "object";

        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(string);
        Type valueType = GetTypeFromName(valueTypeName) ?? typeof(object);

        Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var dict = Activator.CreateInstance(dictType);
        var addMethod = dictType.GetMethod("Add");

        var memberNodes = node.SelectNodes("DictionaryMember"); // 选择所有 <DictionaryMember> 子节点
        if (memberNodes != null)
        {
            foreach (XmlNode member in memberNodes.Cast<XmlNode>())
            {
                // 直接从属性获取
                string keyStr = member.Attributes?["key"]?.Value;
                string valueStr = member.Attributes?["value"]?.Value;

                if (string.IsNullOrEmpty(keyStr)) continue; // 跳过无效项

                try
                {
                    object key = ParseBasicType(keyStr, keyType);
                    object value = ParseBasicType(valueStr, valueType);
                    addMethod?.Invoke(dict, new[] { key, value });
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse Dictionary entry: key='{keyStr}', value='{valueStr}'. Error: {ex.Message}");
                    // 根据需要决定是否跳过或使用默认值
                }
            }
        }
        return dict;
    }

    // ===== 序列化 Class =====
    private XmlElement SerializeClass(XmlDocument doc, string name, object obj, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name); // Validate wrapper element name

        var elem = doc.CreateElement("Class");
        elem.SetAttribute("name", nodeNameForAttribute);
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

        Type classType = GetTypeFromName(classTypeName)
                     ?? GetType().Assembly.GetType(classTypeName)
                     ?? typeof(object);

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

    // === 修改方法签名，添加 nodeNameForAttribute 参数 ===
    private XmlElement SerializeList(XmlDocument doc, string name, object value, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name); // Validate the member name itself if needed elsewhere?

        IList list = (IList)value;
        Type listType = value.GetType();
        Type elementType = listType.GetGenericArguments()[0];

        // === 修改点：使用固定的 "List" 作为节点名 ===
        XmlElement listNode = doc.CreateElement("List");

        // === 添加 name 属性 ===
        if (!string.IsNullOrEmpty(nodeNameForAttribute))
        {
            listNode.SetAttribute("name", nodeNameForAttribute);
        }

        // Add elementType attribute
        listNode.SetAttribute("elementType", elementType.Name);

        foreach (var item in list)
        {
            // === 修改点：递归调用 SerializeValue 时，传递 "Item" 作为 name，且不设置 name 属性 (或设置为 "Item"? 看你需求) ===
            // 如果 Item 也需要 name 属性，可以传递 "Item"；如果只是列表项，可以不传或传 null。
            // 这里我们假设 Item 不需要额外的 name 属性，或者它的 name 就是 "Item"
            XmlElement itemNode = SerializeValue(doc, "Item", item /*, "Item" 或 null */);
            listNode.AppendChild(itemNode);
        }

        return listNode;
    }

    // ===== 反序列化 List<T> =====
    private object DeserializeList(XmlNode node)
    {
        string elementTypeName = node.Attributes?["elementType"]?.Value ?? "object";

        // 使用现有的 GetTypeFromName 方法，它能处理 System 前缀和当前程序集查找
        Type elementType = GetTypeFromName(elementTypeName) ?? typeof(object);

        // 创建一个 List<T> 的实例
        Type listType = typeof(List<>).MakeGenericType(elementType);
        var list = Activator.CreateInstance(listType);

        // 获取 List<T>.Add 方法
        var addMethod = listType.GetMethod("Add");
        if (addMethod == null)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed to find 'Add' method on List<{elementType.Name}>");
            return list;
        }

        // 遍历所有子节点（即 <Item> 节点）
        foreach (XmlNode itemNode in node.ChildNodes)
        {
            // itemNode 就是之前 SerializeValue 生成的完整节点，如 <Item type="string">...</Item>
            object item = DeserializeSingleNode(itemNode, elementType); // 使用 DeserializeSingleNode 处理
            addMethod?.Invoke(list, new[] { item });
        }
        return list;
    }

    // ===== 序列化 IList2<T1, T2> =====
    private XmlElement SerializeIList2(XmlDocument doc, string name, object list2, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name);
        var elem = doc.CreateElement(NODE_ILIST2);
        elem.SetAttribute(ATTR_NAME, nodeNameForAttribute);

        Type listType = list2.GetType();
        Type[] genArgs = listType.GetGenericArguments(); // [T1, T2]
        elem.SetAttribute("item1Type", genArgs[0].Name); // 使用 item1Type/item2Type
        elem.SetAttribute("item2Type", genArgs[1].Name);

        FieldInfo itemsField = listType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        if (itemsField == null) return elem;

        var items = itemsField.GetValue(list2) as System.Collections.IEnumerable;
        if (items == null) return elem;

        foreach (var item in items)
        {
            var valueElem = doc.CreateElement("Value"); // <Value>

            FieldInfo field1 = item.GetType().GetField("Item1");
            FieldInfo field2 = item.GetType().GetField("Item2");

            if (field1 == null || field2 == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to find fields in List2Item");
                continue;
            }

            // 将每个字段作为属性
            valueElem.SetAttribute("item1", (field1.GetValue(item) ?? "").ToString());
            valueElem.SetAttribute("item2", (field2.GetValue(item) ?? "").ToString());

            elem.AppendChild(valueElem); // <IList2> -> <Value .../>
        }
        return elem;
    }

    // ===== 反序列化 IList2<T1, T2> =====
    private object DeserializeIList2(XmlNode node)
    {
        string t1Name = node.Attributes?["item1Type"]?.Value ?? "object"; // 从 item1Type 获取
        string t2Name = node.Attributes?["item2Type"]?.Value ?? "object";

        Type t1 = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2 = GetTypeFromName(t2Name) ?? typeof(object);

        Type listItemType = typeof(List2Item<,>).MakeGenericType(t1, t2);
        Type listType = typeof(IList2<,>).MakeGenericType(t1, t2);
        var list = Activator.CreateInstance(listType);

        FieldInfo itemsField = listType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        if (itemsField == null) return list;

        var itemsListObject = itemsField.GetValue(list);
        if (itemsListObject == null) return list;

        Type itemsListType = itemsListObject.GetType();
        MethodInfo addMethod = itemsListType.GetMethod("Add");
        if (addMethod == null) return list;

        foreach (XmlNode itemNode in node.ChildNodes) // 遍历 <Value> 节点
        {
            if (itemNode.Name != "Value") continue; // 确保是正确的节点

            string val1Str = itemNode.Attributes?["item1"]?.Value;
            string val2Str = itemNode.Attributes?["item2"]?.Value;

            try
            {
                object val1 = ParseBasicType(val1Str, t1);
                object val2 = ParseBasicType(val2Str, t2);

                var listItem = Activator.CreateInstance(listItemType);
                FieldInfo item1Field = listItemType.GetField("Item1");
                FieldInfo item2Field = listItemType.GetField("Item2");
                if (item1Field != null) item1Field.SetValue(listItem, val1);
                if (item2Field != null) item2Field.SetValue(listItem, val2);

                addMethod.Invoke(itemsListObject, new object[] { listItem });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse IList2 item: val1='{val1Str}', val2='{val2Str}'. Error: {ex.Message}");
            }
        }
        return list;
    }

    // ===== 序列化 IList3<T1, T2, T3> =====
    private XmlElement SerializeIList3(XmlDocument doc, string name, object list3, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name);
        var elem = doc.CreateElement(NODE_ILIST3);
        elem.SetAttribute(ATTR_NAME, nodeNameForAttribute);

        Type listType = list3.GetType();
        Type[] genArgs = listType.GetGenericArguments(); // [T1, T2, T3]
        elem.SetAttribute("item1Type", genArgs[0].Name); // 使用 item1Type/item2Type/item3Type
        elem.SetAttribute("item2Type", genArgs[1].Name);
        elem.SetAttribute("item3Type", genArgs[2].Name);

        FieldInfo itemsField = listType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        if (itemsField == null) return elem;

        var items = itemsField.GetValue(list3) as System.Collections.IEnumerable;
        if (items == null) return elem;

        foreach (var item in items)
        {
            var valueElem = doc.CreateElement("Value"); // <Value>

            FieldInfo field1 = item.GetType().GetField("Item1");
            FieldInfo field2 = item.GetType().GetField("Item2");
            FieldInfo field3 = item.GetType().GetField("Item3");

            if (field1 == null || field2 == null || field3 == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to find fields in List3Item");
                continue;
            }

            // 将每个字段作为属性
            valueElem.SetAttribute("item1", (field1.GetValue(item) ?? "").ToString());
            valueElem.SetAttribute("item2", (field2.GetValue(item) ?? "").ToString());
            valueElem.SetAttribute("item3", (field3.GetValue(item) ?? "").ToString());

            elem.AppendChild(valueElem); // <IList3> -> <Value .../>
        }
        return elem;
    }

    // ===== 反序列化 IList3<T1, T2, T3> =====
    private object DeserializeIList3(XmlNode node)
    {
        string t1Name = node.Attributes?["item1Type"]?.Value ?? "object"; // 从 item1Type 获取
        string t2Name = node.Attributes?["item2Type"]?.Value ?? "object";
        string t3Name = node.Attributes?["item3Type"]?.Value ?? "object";

        Type t1 = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2 = GetTypeFromName(t2Name) ?? typeof(object);
        Type t3 = GetTypeFromName(t3Name) ?? typeof(object);

        Type listItemType = typeof(List3Item<,,>).MakeGenericType(t1, t2, t3);
        Type listType = typeof(IList3<,,>).MakeGenericType(t1, t2, t3);
        var list = Activator.CreateInstance(listType);

        FieldInfo itemsField = listType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        if (itemsField == null) return list;

        var itemsListObject = itemsField.GetValue(list);
        if (itemsListObject == null) return list;

        Type itemsListType = itemsListObject.GetType();
        MethodInfo addMethod = itemsListType.GetMethod("Add");
        if (addMethod == null) return list;

        foreach (XmlNode itemNode in node.ChildNodes) // 遍历 <Value> 节点
        {
            if (itemNode.Name != "Value") continue; // 确保是正确的节点

            string val1Str = itemNode.Attributes?["item1"]?.Value;
            string val2Str = itemNode.Attributes?["item2"]?.Value;
            string val3Str = itemNode.Attributes?["item3"]?.Value;

            try
            {
                object val1 = ParseBasicType(val1Str, t1);
                object val2 = ParseBasicType(val2Str, t2);
                object val3 = ParseBasicType(val3Str, t3);

                var listItem = Activator.CreateInstance(listItemType);
                FieldInfo item1Field = listItemType.GetField("Item1");
                FieldInfo item2Field = listItemType.GetField("Item2");
                FieldInfo item3Field = listItemType.GetField("Item3");
                if (item1Field != null) item1Field.SetValue(listItem, val1);
                if (item2Field != null) item2Field.SetValue(listItem, val2);
                if (item3Field != null) item3Field.SetValue(listItem, val3);

                addMethod.Invoke(itemsListObject, new object[] { listItem });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse IList3 item: val1='{val1Str}', val2='{val2Str}', val3='{val3Str}'. Error: {ex.Message}");
            }
        }
        return list;
    }

    // ===== 序列化 IList4<T1, T2, T3, T4> =====
    private XmlElement SerializeIList4(XmlDocument doc, string name, object list4, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name);
        var elem = doc.CreateElement(NODE_ILIST4);
        elem.SetAttribute(ATTR_NAME, nodeNameForAttribute);

        Type listType = list4.GetType();
        Type[] genArgs = listType.GetGenericArguments(); // [T1, T2, T3, T4]
        elem.SetAttribute("item1Type", genArgs[0].Name); // 使用 item1Type/item2Type/item3Type/item4Type
        elem.SetAttribute("item2Type", genArgs[1].Name);
        elem.SetAttribute("item3Type", genArgs[2].Name);
        elem.SetAttribute("item4Type", genArgs[3].Name);

        FieldInfo itemsField = listType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        if (itemsField == null) return elem;

        var items = itemsField.GetValue(list4) as System.Collections.IEnumerable;
        if (items == null) return elem;

        foreach (var item in items)
        {
            var valueElem = doc.CreateElement("Value"); // <Value>

            FieldInfo field1 = item.GetType().GetField("Item1");
            FieldInfo field2 = item.GetType().GetField("Item2");
            FieldInfo field3 = item.GetType().GetField("Item3");
            FieldInfo field4 = item.GetType().GetField("Item4");

            if (field1 == null || field2 == null || field3 == null || field4 == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to find fields in List4Item");
                continue;
            }

            // 将每个字段作为属性
            valueElem.SetAttribute("item1", (field1.GetValue(item) ?? "").ToString());
            valueElem.SetAttribute("item2", (field2.GetValue(item) ?? "").ToString());
            valueElem.SetAttribute("item3", (field3.GetValue(item) ?? "").ToString());
            valueElem.SetAttribute("item4", (field4.GetValue(item) ?? "").ToString());

            elem.AppendChild(valueElem); // <IList4> -> <Value .../>
        }
        return elem;
    }

    // ===== 反序列化 IList4<T1, T2, T3, T4> =====
    private object DeserializeIList4(XmlNode node)
    {
        string t1Name = node.Attributes?["item1Type"]?.Value ?? "object"; // 从 item1Type 获取
        string t2Name = node.Attributes?["item2Type"]?.Value ?? "object";
        string t3Name = node.Attributes?["item3Type"]?.Value ?? "object";
        string t4Name = node.Attributes?["item4Type"]?.Value ?? "object";

        Type t1 = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2 = GetTypeFromName(t2Name) ?? typeof(object);
        Type t3 = GetTypeFromName(t3Name) ?? typeof(object);
        Type t4 = GetTypeFromName(t4Name) ?? typeof(object);

        Type listItemType = typeof(List4Item<,,,>).MakeGenericType(t1, t2, t3, t4);
        Type listType = typeof(IList4<,,,>).MakeGenericType(t1, t2, t3, t4);
        var list = Activator.CreateInstance(listType);

        FieldInfo itemsField = listType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        if (itemsField == null) return list;

        var itemsListObject = itemsField.GetValue(list);
        if (itemsListObject == null) return list;

        Type itemsListType = itemsListObject.GetType();
        MethodInfo addMethod = itemsListType.GetMethod("Add");
        if (addMethod == null) return list;

        foreach (XmlNode itemNode in node.ChildNodes) // 遍历 <Value> 节点
        {
            if (itemNode.Name != "Value") continue; // 确保是正确的节点

            string val1Str = itemNode.Attributes?["item1"]?.Value;
            string val2Str = itemNode.Attributes?["item2"]?.Value;
            string val3Str = itemNode.Attributes?["item3"]?.Value;
            string val4Str = itemNode.Attributes?["item4"]?.Value;

            try
            {
                object val1 = ParseBasicType(val1Str, t1);
                object val2 = ParseBasicType(val2Str, t2);
                object val3 = ParseBasicType(val3Str, t3);
                object val4 = ParseBasicType(val4Str, t4);

                var listItem = Activator.CreateInstance(listItemType);
                FieldInfo item1Field = listItemType.GetField("Item1");
                FieldInfo item2Field = listItemType.GetField("Item2");
                FieldInfo item3Field = listItemType.GetField("Item3");
                FieldInfo item4Field = listItemType.GetField("Item4");
                if (item1Field != null) item1Field.SetValue(listItem, val1);
                if (item2Field != null) item2Field.SetValue(listItem, val2);
                if (item3Field != null) item3Field.SetValue(listItem, val3);
                if (item4Field != null) item4Field.SetValue(listItem, val4);

                addMethod.Invoke(itemsListObject, new object[] { listItem });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse IList4 item: val1='{val1Str}', val2='{val2Str}', val3='{val3Str}', val4='{val4Str}'. Error: {ex.Message}");
            }
        }
        return list;
    }

    // ===== 序列化 IDictionary2<TKey, T1, T2> =====
    private XmlElement SerializeIDictionary2(XmlDocument doc, string name, object dict2, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name);
        var elem = doc.CreateElement(NODE_IDICT2);
        elem.SetAttribute(ATTR_NAME, nodeNameForAttribute);

        Type dictType = dict2.GetType();
        Type[] genArgs = dictType.GetGenericArguments(); // [TKey, T1, T2]

        elem.SetAttribute(ATTR_KEY_TYPE, genArgs[0].Name);
        elem.SetAttribute(ATTR_T1, genArgs[1].Name);
        elem.SetAttribute(ATTR_T2, genArgs[2].Name);

        FieldInfo dictField = dictType.GetField("_dict", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dictField == null) return elem;

        var dictionary = dictField.GetValue(dict2) as System.Collections.IDictionary;
        if (dictionary == null) return elem;

        foreach (System.Collections.DictionaryEntry entry in dictionary)
        {
            var member = doc.CreateElement("DictionaryMember"); // <DictionaryMember>

            object keyValue = entry.Key ?? throw new InvalidOperationException("Dictionary key cannot be null");
            var valueItem = entry.Value; // This is a Dict2Item<T1, T2>

            // 获取 Dict2Item 的字段
            FieldInfo field1 = valueItem.GetType().GetField("Item1");
            FieldInfo field2 = valueItem.GetType().GetField("Item2");

            if (field1 == null || field2 == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to find fields in Dict2Item for key '{keyValue}'");
                continue;
            }

            // 将键和值的各个部分作为属性
            member.SetAttribute("key", keyValue.ToString());
            member.SetAttribute("value1", (field1.GetValue(valueItem) ?? "").ToString());
            member.SetAttribute("value2", (field2.GetValue(valueItem) ?? "").ToString());

            elem.AppendChild(member); // <IDictionary2> -> <DictionaryMember .../>
        }
        return elem;
    }

    // ===== 反序列化 IDictionary2<TKey, T1, T2> =====
    private object DeserializeIDictionary2(XmlNode node)
    {
        string keyTypeName = node.Attributes?[ATTR_KEY_TYPE]?.Value ?? "string";
        string t1Name = node.Attributes?[ATTR_T1]?.Value ?? "object";
        string t2Name = node.Attributes?[ATTR_T2]?.Value ?? "object";

        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(string);
        Type t1 = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2 = GetTypeFromName(t2Name) ?? typeof(object);

        Type dictItemType = typeof(Dict2Item<,>).MakeGenericType(t1, t2);
        Type dictType = typeof(IDictionary2<,,>).MakeGenericType(keyType, t1, t2);
        var dict = Activator.CreateInstance(dictType);

        FieldInfo dictField = dictType.GetField("_dict", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dictField == null) return dict;

        var targetDict = dictField.GetValue(dict) as System.Collections.IDictionary;
        if (targetDict == null) return dict;

        var memberNodes = node.SelectNodes("DictionaryMember");
        if (memberNodes != null)
        {
            foreach (XmlNode member in memberNodes.Cast<XmlNode>())
            {
                string keyStr = member.Attributes?["key"]?.Value;
                string val1Str = member.Attributes?["value1"]?.Value;
                string val2Str = member.Attributes?["value2"]?.Value;

                if (string.IsNullOrEmpty(keyStr)) continue;

                try
                {
                    object key = ParseBasicType(keyStr, keyType);
                    object val1 = ParseBasicType(val1Str, t1);
                    object val2 = ParseBasicType(val2Str, t2);

                    // 创建一个新的 Dict2Item<T1, T2>
                    var dictItem = Activator.CreateInstance(dictItemType);
                    FieldInfo item1Field = dictItemType.GetField("Item1");
                    FieldInfo item2Field = dictItemType.GetField("Item2");
                    if (item1Field != null) item1Field.SetValue(dictItem, val1);
                    if (item2Field != null) item2Field.SetValue(dictItem, val2);

                    targetDict[key] = dictItem;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse IDictionary2 entry: key='{keyStr}', val1='{val1Str}', val2='{val2Str}'. Error: {ex.Message}");
                }
            }
        }
        return dict;
    }

    // ===== 序列化 IDictionary3<TKey, T1, T2, T3> =====
    private XmlElement SerializeIDictionary3(XmlDocument doc, string name, object dict3, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name);
        var elem = doc.CreateElement(NODE_IDICT3);
        elem.SetAttribute(ATTR_NAME, nodeNameForAttribute);

        Type dictType = dict3.GetType();
        Type[] genArgs = dictType.GetGenericArguments(); // [TKey, T1, T2, T3]

        elem.SetAttribute(ATTR_KEY_TYPE, genArgs[0].Name);
        elem.SetAttribute(ATTR_T1, genArgs[1].Name);
        elem.SetAttribute(ATTR_T2, genArgs[2].Name);
        elem.SetAttribute(ATTR_T3, genArgs[3].Name);

        FieldInfo dictField = dictType.GetField("_dict", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dictField == null) return elem;

        var dictionary = dictField.GetValue(dict3) as System.Collections.IDictionary;
        if (dictionary == null) return elem;

        foreach (System.Collections.DictionaryEntry entry in dictionary)
        {
            var member = doc.CreateElement("DictionaryMember"); // <DictionaryMember>

            object keyValue = entry.Key ?? throw new InvalidOperationException("Dictionary key cannot be null");
            var valueItem = entry.Value; // This is a Dict3Item<T1, T2, T3>

            // 获取 Dict3Item 的字段
            FieldInfo field1 = valueItem.GetType().GetField("Item1");
            FieldInfo field2 = valueItem.GetType().GetField("Item2");
            FieldInfo field3 = valueItem.GetType().GetField("Item3");

            if (field1 == null || field2 == null || field3 == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to find fields in Dict3Item for key '{keyValue}'");
                continue;
            }

            // 将键和值的各个部分作为属性
            member.SetAttribute("key", keyValue.ToString());
            member.SetAttribute("value1", (field1.GetValue(valueItem) ?? "").ToString());
            member.SetAttribute("value2", (field2.GetValue(valueItem) ?? "").ToString());
            member.SetAttribute("value3", (field3.GetValue(valueItem) ?? "").ToString());

            elem.AppendChild(member); // <IDictionary3> -> <DictionaryMember .../>
        }
        return elem;
    }

    // ===== 反序列化 IDictionary3<TKey, T1, T2, T3> =====
    private object DeserializeIDictionary3(XmlNode node)
    {
        string keyTypeName = node.Attributes?[ATTR_KEY_TYPE]?.Value ?? "string";
        string t1Name = node.Attributes?[ATTR_T1]?.Value ?? "object";
        string t2Name = node.Attributes?[ATTR_T2]?.Value ?? "object";
        string t3Name = node.Attributes?[ATTR_T3]?.Value ?? "object";

        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(string);
        Type t1 = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2 = GetTypeFromName(t2Name) ?? typeof(object);
        Type t3 = GetTypeFromName(t3Name) ?? typeof(object);

        Type dictItemType = typeof(Dict3Item<,,>).MakeGenericType(t1, t2, t3);
        Type dictType = typeof(IDictionary3<,,,>).MakeGenericType(keyType, t1, t2, t3);
        var dict = Activator.CreateInstance(dictType);

        FieldInfo dictField = dictType.GetField("_dict", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dictField == null) return dict;

        var targetDict = dictField.GetValue(dict) as System.Collections.IDictionary;
        if (targetDict == null) return dict;

        var memberNodes = node.SelectNodes("DictionaryMember");
        if (memberNodes != null)
        {
            foreach (XmlNode member in memberNodes.Cast<XmlNode>())
            {
                string keyStr = member.Attributes?["key"]?.Value;
                string val1Str = member.Attributes?["value1"]?.Value;
                string val2Str = member.Attributes?["value2"]?.Value;
                string val3Str = member.Attributes?["value3"]?.Value;

                if (string.IsNullOrEmpty(keyStr)) continue;

                try
                {
                    object key = ParseBasicType(keyStr, keyType);
                    object val1 = ParseBasicType(val1Str, t1);
                    object val2 = ParseBasicType(val2Str, t2);
                    object val3 = ParseBasicType(val3Str, t3);

                    // 创建一个新的 Dict3Item<T1, T2, T3>
                    var dictItem = Activator.CreateInstance(dictItemType);
                    FieldInfo item1Field = dictItemType.GetField("Item1");
                    FieldInfo item2Field = dictItemType.GetField("Item2");
                    FieldInfo item3Field = dictItemType.GetField("Item3");
                    if (item1Field != null) item1Field.SetValue(dictItem, val1);
                    if (item2Field != null) item2Field.SetValue(dictItem, val2);
                    if (item3Field != null) item3Field.SetValue(dictItem, val3);

                    targetDict[key] = dictItem;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse IDictionary3 entry: key='{keyStr}', val1='{val1Str}', val2='{val2Str}', val3='{val3Str}'. Error: {ex.Message}");
                }
            }
        }
        return dict;
    }

    // ===== 序列化 IDictionary4<TKey, T1, T2, T3, T4> =====
    private XmlElement SerializeIDictionary4(XmlDocument doc, string name, object dict4, string nodeNameForAttribute = null)
    {
        ValidateXmlElementName(name);
        var elem = doc.CreateElement(NODE_IDICT4);
        elem.SetAttribute(ATTR_NAME, nodeNameForAttribute);

        Type dictType = dict4.GetType();
        Type[] genArgs = dictType.GetGenericArguments(); // [TKey, T1, T2, T3, T4]

        elem.SetAttribute(ATTR_KEY_TYPE, genArgs[0].Name);
        elem.SetAttribute(ATTR_T1, genArgs[1].Name);
        elem.SetAttribute(ATTR_T2, genArgs[2].Name);
        elem.SetAttribute(ATTR_T3, genArgs[3].Name);
        elem.SetAttribute(ATTR_T4, genArgs[4].Name);

        FieldInfo dictField = dictType.GetField("_dict", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dictField == null) return elem;

        var dictionary = dictField.GetValue(dict4) as System.Collections.IDictionary;
        if (dictionary == null) return elem;

        foreach (System.Collections.DictionaryEntry entry in dictionary)
        {
            var member = doc.CreateElement("DictionaryMember"); // <DictionaryMember>

            object keyValue = entry.Key ?? throw new InvalidOperationException("Dictionary key cannot be null");
            var valueItem = entry.Value; // This is a Dict4Item<T1, T2, T3, T4>

            // 获取 Dict4Item 的字段
            FieldInfo field1 = valueItem.GetType().GetField("Item1");
            FieldInfo field2 = valueItem.GetType().GetField("Item2");
            FieldInfo field3 = valueItem.GetType().GetField("Item3");
            FieldInfo field4 = valueItem.GetType().GetField("Item4");

            if (field1 == null || field2 == null || field3 == null || field4 == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to find fields in Dict4Item for key '{keyValue}'");
                continue;
            }

            // 将键和值的各个部分作为属性
            member.SetAttribute("key", keyValue.ToString());
            member.SetAttribute("value1", (field1.GetValue(valueItem) ?? "").ToString());
            member.SetAttribute("value2", (field2.GetValue(valueItem) ?? "").ToString());
            member.SetAttribute("value3", (field3.GetValue(valueItem) ?? "").ToString());
            member.SetAttribute("value4", (field4.GetValue(valueItem) ?? "").ToString());

            elem.AppendChild(member); // <IDictionary4> -> <DictionaryMember .../>
        }
        return elem;
    }

    // ===== 反序列化 IDictionary4<TKey, T1, T2, T3, T4> =====
    private object DeserializeIDictionary4(XmlNode node)
    {
        string keyTypeName = node.Attributes?[ATTR_KEY_TYPE]?.Value ?? "string";
        string t1Name = node.Attributes?[ATTR_T1]?.Value ?? "object";
        string t2Name = node.Attributes?[ATTR_T2]?.Value ?? "object";
        string t3Name = node.Attributes?[ATTR_T3]?.Value ?? "object";
        string t4Name = node.Attributes?[ATTR_T4]?.Value ?? "object";

        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(string);
        Type t1 = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2 = GetTypeFromName(t2Name) ?? typeof(object);
        Type t3 = GetTypeFromName(t3Name) ?? typeof(object);
        Type t4 = GetTypeFromName(t4Name) ?? typeof(object);

        Type dictItemType = typeof(Dict4Item<,,,>).MakeGenericType(t1, t2, t3, t4);
        Type dictType = typeof(IDictionary4<,,,,>).MakeGenericType(keyType, t1, t2, t3, t4);
        var dict = Activator.CreateInstance(dictType);

        FieldInfo dictField = dictType.GetField("_dict", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dictField == null) return dict;

        var targetDict = dictField.GetValue(dict) as System.Collections.IDictionary;
        if (targetDict == null) return dict;

        var memberNodes = node.SelectNodes("DictionaryMember");
        if (memberNodes != null)
        {
            foreach (XmlNode member in memberNodes.Cast<XmlNode>())
            {
                string keyStr = member.Attributes?["key"]?.Value;
                string val1Str = member.Attributes?["value1"]?.Value;
                string val2Str = member.Attributes?["value2"]?.Value;
                string val3Str = member.Attributes?["value3"]?.Value;
                string val4Str = member.Attributes?["value4"]?.Value;

                if (string.IsNullOrEmpty(keyStr)) continue;

                try
                {
                    object key = ParseBasicType(keyStr, keyType);
                    object val1 = ParseBasicType(val1Str, t1);
                    object val2 = ParseBasicType(val2Str, t2);
                    object val3 = ParseBasicType(val3Str, t3);
                    object val4 = ParseBasicType(val4Str, t4);

                    // 创建一个新的 Dict4Item<T1, T2, T3, T4>
                    var dictItem = Activator.CreateInstance(dictItemType);
                    FieldInfo item1Field = dictItemType.GetField("Item1");
                    FieldInfo item2Field = dictItemType.GetField("Item2");
                    FieldInfo item3Field = dictItemType.GetField("Item3");
                    FieldInfo item4Field = dictItemType.GetField("Item4");
                    if (item1Field != null) item1Field.SetValue(dictItem, val1);
                    if (item2Field != null) item2Field.SetValue(dictItem, val2);
                    if (item3Field != null) item3Field.SetValue(dictItem, val3);
                    if (item4Field != null) item4Field.SetValue(dictItem, val4);

                    targetDict[key] = dictItem;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse IDictionary4 entry: key='{keyStr}', val1='{val1Str}', val2='{val2Str}', val3='{val3Str}', val4='{val4Str}'. Error: {ex.Message}");
                }
            }
        }
        return dict;
    }

    /// <summary>
    /// 根据字符串名称获取 Type 对象。
    /// </summary>
    private Type GetTypeFromName(string typeName)
    {
        // 尝试直接通过全名获取
        Type type = Type.GetType(typeName);
        if (type != null) return type;

        // 尝试加上 System. 前缀
        type = Type.GetType($"System.{typeName}");
        if (type != null) return type;

        // 尝试在当前程序集查找
        type = GetType().Assembly.GetType(typeName);
        if (type != null) return type;

        // 最后，返回 null
        return null;
    }
}


