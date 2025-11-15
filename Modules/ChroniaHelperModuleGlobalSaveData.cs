using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Celeste;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Modules;

// ===== 内部成员包装器：统一处理 Field 和 Property =====
internal readonly struct SaveMember
{
    private readonly FieldInfo? _field;
    private readonly PropertyInfo? _property;

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
        if (_field != null)
            _field.SetValue(instance, value);
        else
            _property!.SetValue(instance, value);
    }
}

// ===== 全局保存数据基类 =====
public abstract class ChroniaHelperModuleGlobalSaveData
{
    // 缓存成员名 -> 路径映射
    private readonly Dictionary<string, string> _memberToPath = new();
    private readonly Dictionary<string, SaveMember> _members = new();

    protected ChroniaHelperModuleGlobalSaveData()
    {
        Initialize();
    }

    private void Initialize()
    {
        var type = GetType();

        // === 处理字段（支持 public / non-public / instance）===
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var attr = field.GetCustomAttribute<ChroniaGlobalSavePathAttribute>();
            if (attr == null) continue; // 仅处理标记了 Attribute 的字段

            string relativePath = attr.RelativePath;
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
            if (attr == null) continue; // 仅处理标记了 Attribute 的属性

            string relativePath = attr.RelativePath;
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
                continue;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                foreach (string memberName in memberNames)
                {
                    var node = doc.SelectSingleNode($"/Root/{memberName}");
                    if (node != null)
                    {
                        object value = DeserializeNode(node);
                        _members[memberName].SetValue(this, value);
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

                XmlElement root = doc.CreateElement("Root");
                doc.AppendChild(root);

                foreach (string memberName in memberNames)
                {
                    object value = _members[memberName].GetValue(this);
                    XmlElement valueNode = SerializeValue(doc, memberName, value);
                    root.AppendChild(valueNode);
                }

                doc.Save(filePath);
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
            elem.InnerText = value.ToString();
            string typeCode;
            if (type == typeof(System.Numerics.BigInteger))
                typeCode = "biginteger";
            else
                typeCode = Type.GetTypeCode(type).ToString().ToLowerInvariant();
            elem.SetAttribute("type", typeCode);
            return elem;
        }
    }

    // ===== 反序列化节点 =====
    private object DeserializeNode(XmlNode node)
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
            // 基本类型
            string typeName = node.Attributes?["type"]?.Value ?? "string";
            string text = node.InnerText;

            return typeName switch
            {
                "int" => int.Parse(text),
                "float" => float.Parse(text),
                "double" => double.Parse(text),
                "bool" => bool.Parse(text),
                "string" => text,
                "datetime" => DateTime.Parse(text),
                _ => text // fallback
            };
        }
    }

    // ===== 序列化 Dictionary<K,V> =====
    private XmlElement SerializeDictionary(XmlDocument doc, string name, object dict)
    {
        var elem = doc.CreateElement("Dictionary");
        elem.SetAttribute("name", name);

        Type dictType = dict.GetType();
        Type keyType = dictType.GetGenericArguments()[0];
        Type valueType = dictType.GetGenericArguments()[1];

        elem.SetAttribute("keyType", keyType.Name);
        elem.SetAttribute("valueType", valueType.Name);

        var keys = dictType.GetProperty("Keys")?.GetValue(dict) as System.Collections.IEnumerable;
        var indexer = dictType.GetProperty("Item");

        foreach (var key in keys!)
        {
            var member = doc.CreateElement("DictionaryMember");

            // Key
            object keyValue = key;
            if (keyValue == null) throw new InvalidOperationException("Dictionary key cannot be null");
            XmlElement keyNode = SerializeValue(doc, "Key", keyValue);
            member.AppendChild(keyNode.FirstChild ?? keyNode); // 如果是基本类型，直接取 InnerText 包装

            // Value
            object valueValue = indexer!.GetValue(dict, new object[] { keyValue });
            XmlElement valueNode = SerializeValue(doc, "Value", valueValue);
            member.AppendChild(valueNode.FirstChild ?? valueNode);

            elem.AppendChild(member);
        }

        return elem;
    }

    // ===== 反序列化 Dictionary =====
    private object DeserializeDictionary(XmlNode node)
    {
        string keyTypeName = node.Attributes["keyType"]?.Value ?? "string";
        string valueTypeName = node.Attributes["valueType"]?.Value ?? "object";

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
        var elem = doc.CreateElement("Class");
        elem.SetAttribute("name", name);
        elem.SetAttribute("classType", obj.GetType().Name);

        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            object value = field.GetValue(obj);
            XmlElement attr = doc.CreateElement("Attribute");
            attr.SetAttribute("name", field.Name);
            attr.SetAttribute("type", field.FieldType.Name);

            if (value == null)
            {
                attr.SetAttribute("isNull", "true");
            }
            else
            {
                // 对于复杂类型，递归序列化
                if (field.FieldType.IsClass && field.FieldType != typeof(string))
                {
                    // 嵌套 Class 或 Dictionary
                    XmlElement nested = SerializeValue(doc, field.Name, value);
                    attr.AppendChild(nested.FirstChild ?? nested);
                }
                else
                {
                    attr.InnerText = value.ToString();
                }
            }

            elem.AppendChild(attr);
        }

        return elem;
    }

    // ===== 反序列化 Class =====
    private object DeserializeClass(XmlNode node)
    {
        string className = node.Attributes["classType"]?.Value;
        if (string.IsNullOrEmpty(className)) return new object();

        // ⚠️ 简化：假设类在当前程序集，且有无参构造函数
        Type classType = GetType().Assembly.GetType($"ChroniaHelper.Save.{className}")
                     ?? Type.GetType(className)
                     ?? typeof(object);

        if (classType == typeof(object)) return new object();

        var obj = Activator.CreateInstance(classType);
        var fields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        var attributeMember = node.SelectNodes("Attribute");
        if (attributeMember != null)
        {
            foreach (XmlNode attrNode in attributeMember.Cast<XmlNode>())
            {
                string fieldName = attrNode.Attributes["name"]?.Value;
                string typeName = attrNode.Attributes["type"]?.Value;
                bool isNull = attrNode.Attributes["isNull"]?.Value == "true";

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
                        // 递归反序列化子节点
                        object nested = DeserializeNode(attrNode.FirstChild ?? attrNode);
                        field.SetValue(obj, nested);
                    }
                    else
                    {
                        string text = attrNode.InnerText;
                        object val = ParseBasicType(text, typeName ?? field.FieldType.Name);
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

        return ParseBasicType(node.InnerText, targetType.Name);
    }

    private object ParseBasicType(string text, string typeName)
    {
        return typeName.ToLowerInvariant() switch
        {
            "int32" or "int" => int.Parse(text),
            "int64" or "long" => long.Parse(text),
            "single" or "float" => float.Parse(text),
            "double" => double.Parse(text),
            "boolean" or "bool" => bool.Parse(text),
            "string" => text,
            "datetime" => DateTime.Parse(text),
            "biginteger" => System.Numerics.BigInteger.Parse(text),
            _ => text
        };
    }
}