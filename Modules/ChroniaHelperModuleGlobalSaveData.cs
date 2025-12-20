using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml;
using Celeste;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils; // For Logger

namespace ChroniaHelper.Modules;

/// <summary>
/// 全局保存数据基类。继承此类的实例可以自动保存和加载其数据。
/// </summary>
public abstract class ChroniaHelperModuleGlobalSaveData
{
    // 常量
    private const string ROOT_NODE_NAME = "ChroniaHelperGlobalData";
    private const string VALUE_NODE_NAME = "Value";
    private const string CLASS_NODE_NAME = "Class";
    private const string LIST_NODE_NAME = "List";
    private const string HASHSET_NODE_NAME = "HashSet";
    private const string DICTIONARY_NODE_NAME = "Dictionary";
    private const string DICTIONARY_MEMBER_NODE_NAME = "DictionaryMember"; // 用于表示一个键值对
    private const string TUPLE_NODE_NAME = "Tuple"; // 新增：Tuple节点
    private const string VALUE_TUPLE_NODE_NAME = "ValueTuple"; // 新增：ValueTuple节点

    // 默认保存文件名
    public const string DEFAULT_SAVE_PATH = "ChroniaHelperGlobalSaveData.xml";

    // 缓存：成员名 -> (保存路径, 成员信息)
    private readonly Dictionary<string, (string savePath, MemberInfo memberInfo)> _memberCache = new();

    protected ChroniaHelperModuleGlobalSaveData()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化，扫描并缓存所有带有 [ChroniaGlobalSavePath] 属性的成员。
    /// </summary>
    private void Initialize()
    {
        var type = GetType();

        // 缓存：成员名 -> (保存路径, 成员信息)
        _memberCache.Clear(); // 清空之前的缓存

        // 1. 检查类上是否有 [ChroniaGlobalSavePath] 属性
        var classAttr = type.GetCustomAttribute<ChroniaGlobalSavePathAttribute>();
        bool classHasAttribute = classAttr != null;

        // 2. 获取所有实例字段和属性
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // 3. 处理字段
        foreach (var field in fields)
        {
            // 跳过编译器生成的自动属性后台字段
            if (field.Name.StartsWith("<")) continue;

            var fieldAttr = field.GetCustomAttribute<ChroniaGlobalSavePathAttribute>();

            // *** 修正逻辑 ***
            // 如果字段有属性，则使用字段的属性
            // 否则，如果类有属性，则使用类的属性
            // 否则，不处理此字段

            string relativePath = null;
            if (fieldAttr != null)
            {
                relativePath = fieldAttr.RelativePath; // 可能为 null
            }
            else if (classHasAttribute)
            {
                relativePath = classAttr.RelativePath; // 可能为 null
            }
            else
            {
                continue; // 字段和类都没有属性，跳过
            }

            string fullPath = GetFullPath(relativePath);
            _memberCache[field.Name] = (fullPath, field);
        }

        // 4. 处理属性（必须是可读可写的）
        foreach (var prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            var propAttr = prop.GetCustomAttribute<ChroniaGlobalSavePathAttribute>();

            // *** 修正逻辑 ***
            // 如果属性有属性，则使用属性的属性
            // 否则，如果类有属性，则使用类的属性
            // 否则，不处理此属性

            string relativePath = null;
            if (propAttr != null)
            {
                relativePath = propAttr.RelativePath; // 可能为 null
            }
            else if (classHasAttribute)
            {
                relativePath = classAttr.RelativePath; // 可能为 null
            }
            else
            {
                continue; // 属性和类都没有属性，跳过
            }

            string fullPath = GetFullPath(relativePath);
            _memberCache[prop.Name] = (fullPath, prop);
        }

        Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Initialized save data for {GetType().Name}. Found {_memberCache.Count} members.");
    }


    /// <summary>
    /// 根据相对路径获取完整的文件路径。
    /// </summary>
    /// <param name="relativePath">相对路径，或 null 表示使用默认路径。</param>
    /// <returns>完整的文件系统路径。</returns>
    private string GetFullPath(string relativePath)
    {
        string path = relativePath ?? DEFAULT_SAVE_PATH;
        return Path.Combine(Everest.PathSettings, "ChroniaHelper", path);
    }

    /// <summary>
    /// 从所有关联的文件中加载数据。
    /// </summary>
    public void LoadAll()
    {
        // 按保存路径分组成员
        var pathGroups = _memberCache.Values
            .GroupBy(x => x.savePath)
            .ToDictionary(g => g.Key, g => g.Select(x => x.memberInfo).ToList());

        foreach (var (filePath, members) in pathGroups)
        {
            if (!File.Exists(filePath))
            {
                Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Save file not found: {filePath}");
                continue;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                XmlNode rootNode = doc.SelectSingleNode($"/{ROOT_NODE_NAME}");
                if (rootNode == null)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Root element '{ROOT_NODE_NAME}' not found in {filePath}");
                    continue;
                }

                // 为每个成员在文件中查找对应的节点
                foreach (var member in members)
                {
                    string memberName = member.Name;
                    XmlNode node = FindNodeByName(rootNode, memberName);
                    if (node != null)
                    {
                        try
                        {
                            object value = DeserializeNode(node);
                            SetMemberValue(member, value);
                            Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Loaded '{memberName}' from {filePath}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error loading '{memberName}': {ex.Message}");
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Node for '{memberName}' not found in {filePath}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed to load {filePath}: {e}");
            }
        }
    }

    /// <summary>
    /// 将所有数据保存到各自的文件中。
    /// </summary>
    public void SaveAll()
    {
        var pathGroups = _memberCache.Values
            .GroupBy(x => x.savePath)
            .ToDictionary(g => g.Key, g => g.Select(x => x.memberInfo).ToList());

        foreach (var (filePath, members) in pathGroups)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                var doc = new XmlDocument();
                var decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(decl);

                var root = doc.CreateElement(ROOT_NODE_NAME);
                doc.AppendChild(root);

                foreach (var member in members)
                {
                    try
                    {
                        object value = GetMemberValue(member);
                        XmlElement valueNode = SerializeValue(doc, member.Name, value);
                        root.AppendChild(valueNode);
                        Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Serialized '{member.Name}' for {filePath}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "ChroniaHelper", $"Error serializing '{member.Name}': {ex.Message}");
                    }
                }

                doc.Save(filePath);
                Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Saved data to {filePath}");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed to save {filePath}: {e}");
            }
        }
    }

    // ======== 序列化方法 (Serialize) ========

    /// <summary>
    /// 将任意对象序列化为一个 XmlElement。
    /// </summary>
    /// <param name="doc">要创建元素的 XmlDocument。</param>
    /// <param name="name">节点的名称（通常为成员名）。</param>
    /// <param name="value">要序列化的值。</param>
    /// <returns>表示该值的 XmlElement。</returns>
    private XmlElement SerializeValue(XmlDocument doc, string name, object value)
    {
        if (value == null)
        {
            var elem = doc.CreateElement(VALUE_NODE_NAME);
            elem.SetAttribute("name", name);
            elem.SetAttribute("isNull", "true");
            return elem;
        }

        Type type = value.GetType();

        // --- 处理泛型集合 ---
        if (type.IsGenericType)
        {
            Type genericTypeDef = type.GetGenericTypeDefinition();

            // List<T> / HashSet<T>
            if (genericTypeDef == typeof(List<>) || genericTypeDef == typeof(HashSet<>))
            {
                return SerializeList(doc, name, value);
            }
            // Dictionary<K,V>
            else if (genericTypeDef == typeof(Dictionary<,>))
            {
                return SerializeDictionary(doc, name, value);
            }
            // Tuple<...>
            else if (IsTupleType(type))
            {
                return SerializeTuple(doc, name, value, TUPLE_NODE_NAME);
            }
            // ValueTuple<...>
            else if (IsValueTupleType(type))
            {
                return SerializeTuple(doc, name, value, VALUE_TUPLE_NODE_NAME);
            }
        }

        // --- 处理基本类型和字符串 ---
        if (IsPrimitiveOrString(type))
        {
            var elem = doc.CreateElement(VALUE_NODE_NAME);
            elem.SetAttribute("name", name);
            elem.SetAttribute("type", GetTypeNameForAttribute(type));
            elem.InnerText = value.ToString();
            return elem;
        }

        // --- 处理 BigInteger ---
        if (type == typeof(BigInteger))
        {
            var elem = doc.CreateElement(VALUE_NODE_NAME);
            elem.SetAttribute("name", name);
            elem.SetAttribute("type", "biginteger");
            elem.InnerText = value.ToString();
            return elem;
        }

        // --- 处理自定义类 ---
        if (type.IsClass)
        {
            return SerializeClass(doc, name, value);
        }

        // --- 未处理的类型 ---
        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Cannot serialize unhandled type: {type.FullName}");
        var fallbackElem = doc.CreateElement(VALUE_NODE_NAME);
        fallbackElem.SetAttribute("name", name);
        fallbackElem.SetAttribute("type", "unknown");
        fallbackElem.InnerText = value.ToString() ?? "";
        return fallbackElem;
    }

    private XmlElement SerializeList(XmlDocument doc, string name, object list)
    {
        Type listType = list.GetType();
        Type elementType = listType.GetGenericArguments()[0];

        string nodeName = listType.Name.StartsWith("HashSet") ? HASHSET_NODE_NAME : LIST_NODE_NAME;

        var listNode = doc.CreateElement(nodeName); // 使用动态节点名
        listNode.SetAttribute("name", name);
        listNode.SetAttribute("elementType", elementType.Name);

        foreach (var item in (IEnumerable)list)
        {
            XmlElement itemNode = SerializeValue(doc, "Item", item);
            listNode.AppendChild(itemNode);
        }

        return listNode;
    }

    /// <summary>
    /// 序列化一个 Dictionary<K,V>。
    /// </summary>
    private XmlElement SerializeDictionary(XmlDocument doc, string name, object dict)
    {
        var dictNode = doc.CreateElement(DICTIONARY_NODE_NAME);
        dictNode.SetAttribute("name", name);
        Type dictType = dict.GetType();
        Type keyType = dictType.GetGenericArguments()[0];
        Type valueType = dictType.GetGenericArguments()[1];
        dictNode.SetAttribute("keyType", keyType.Name);
        dictNode.SetAttribute("valueType", valueType.Name);

        var keys = dictType.GetProperty("Keys")?.GetValue(dict) as IEnumerable;
        var indexer = dictType.GetProperty("Item");
        if (keys != null && indexer != null)
        {
            foreach (var key in keys)
            {
                var memberNode = doc.CreateElement("DictionaryMember");
                object keyValue = key;
                object valueValue = indexer.GetValue(dict, new[] { keyValue });

                memberNode.SetAttribute("key", keyValue.ToString());
                memberNode.SetAttribute("value", valueValue?.ToString()); // 注意：这里只存字符串，复杂对象会丢失结构
                dictNode.AppendChild(memberNode);
            }
        }

        return dictNode;
    }

    /// <summary>
    /// 序列化 Tuple 或 ValueTuple。
    /// </summary>
    private XmlElement SerializeTuple(XmlDocument doc, string name, object tuple, string nodeName)
    {
        Type tupleType = tuple.GetType();
        var genericArgs = tupleType.GetGenericArguments();

        var tupleNode = doc.CreateElement(nodeName);
        tupleNode.SetAttribute("name", name);

        // 设置每个元素的类型属性
        for (int i = 0; i < genericArgs.Length; i++)
        {
            tupleNode.SetAttribute($"elementType{i + 1}", genericArgs[i].Name);
        }

        // 获取 Item1, Item2, ... 属性的值
        for (int i = 0; i < genericArgs.Length; i++)
        {
            string propName = $"Item{i + 1}";
            var prop = tupleType.GetProperty(propName);
            if (prop != null)
            {
                try
                {
                    object propValue = prop.GetValue(tuple);
                    XmlElement itemNode = SerializeValue(doc, propName, propValue);
                    tupleNode.AppendChild(itemNode);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to serialize {propName} of tuple: {ex.Message}");
                    // 创建空节点作为占位符
                    var emptyNode = doc.CreateElement(propName);
                    emptyNode.InnerText = "";
                    tupleNode.AppendChild(emptyNode);
                }
            }
        }

        return tupleNode;
    }

    /// <summary>
    /// 序列化一个自定义类（Class）。
    /// 将类的所有公共字段和属性序列化为其子节点。
    /// </summary>
    private XmlElement SerializeClass(XmlDocument doc, string name, object obj)
    {
        var classNode = doc.CreateElement(CLASS_NODE_NAME);
        classNode.SetAttribute("name", name);
        classNode.SetAttribute("type", obj.GetType().AssemblyQualifiedName);

        // 获取所有公共的实例字段
        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                object value = field.GetValue(obj);
                // 使用字段名作为子节点的 name
                var valueNode = SerializeValue(doc, field.Name, value);
                classNode.AppendChild(valueNode);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to serialize field '{field.Name}' of class '{obj.GetType().Name}': {ex.Message}");
            }
        }

        // 获取所有公共的实例属性
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            // 检查属性是否有 getter
            if (prop.GetMethod != null)
            {
                try
                {
                    object value = prop.GetValue(obj);
                    // 使用属性名作为子节点的 name
                    var valueNode = SerializeValue(doc, prop.Name, value);
                    classNode.AppendChild(valueNode);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to serialize property '{prop.Name}' of class '{obj.GetType().Name}': {ex.Message}");
                }
            }
        }

        return classNode;
    }

    // ======== 反序列化方法 (Deserialize) ========

    /// <summary>
    /// 从一个 XmlNode 反序列化出一个对象。
    /// </summary>
    /// <param name="node">要反序列化的 XmlNode。</param>
    /// <returns>反序列化得到的对象。</returns>
    private object DeserializeNode(XmlNode node)
    {
        if (node.Attributes?["isNull"]?.Value == "true")
        {
            return null;
        }

        switch (node.Name)
        {
            case LIST_NODE_NAME:
                return DeserializeList(node);
            case DICTIONARY_NODE_NAME:
                return DeserializeDictionary(node);
            case CLASS_NODE_NAME:
                return DeserializeClass(node);
            case TUPLE_NODE_NAME:
                return DeserializeTuple(node, false);
            case VALUE_TUPLE_NODE_NAME:
                return DeserializeTuple(node, true);
            default:
                // 处理基本类型、字符串、BigInteger
                return DeserializeValue(node);
        }
    }

    /// <summary>
    /// 反序列化一个 List<T>。
    /// </summary>
    private object DeserializeList(XmlNode node)
    {
        string elementTypeName = node.Attributes?["elementType"]?.Value ?? "object";
        Type elementType = GetTypeFromName(elementTypeName) ?? typeof(object);

        Type collectionBaseType = node.Name switch
        {
            HASHSET_NODE_NAME => typeof(HashSet<>),
            _ => typeof(List<>) // 默认为 List
        };

        Type concreteCollectionType = collectionBaseType.MakeGenericType(elementType);
        var collection = Activator.CreateInstance(concreteCollectionType);

        var addMethod = concreteCollectionType.GetMethod("Add");

        foreach (XmlNode itemNode in node.ChildNodes)
        {
            object item = DeserializeNode(itemNode);
            addMethod?.Invoke(collection, new[] { item });
        }

        return collection;
    }

    /// <summary>
    /// 反序列化一个 Dictionary<K,V>。
    /// </summary>
    private object DeserializeDictionary(XmlNode node)
    {
        string keyTypeName = node.Attributes?["keyType"]?.Value ?? "string";
        string valueTypeName = node.Attributes?["valueType"]?.Value ?? "object";
        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(string);
        Type valueType = GetTypeFromName(valueTypeName) ?? typeof(object);
        Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var dict = Activator.CreateInstance(dictType);
        var addMethod = dictType.GetMethod("Add");

        // *** 修正语法错误 ***
        XmlNodeList memberNodes = node.SelectNodes("DictionaryMember");
        if (memberNodes != null) // 检查 SelectNodes 是否返回了 null
        {
            foreach (XmlNode memberNode in memberNodes)
            {
                string keyStr = memberNode.Attributes?["key"]?.Value;
                string valueStr = memberNode.Attributes?["value"]?.Value;
                if (string.IsNullOrEmpty(keyStr)) continue;

                try
                {
                    object key = ParseBasicType(keyStr, keyType);
                    object value = ParseBasicType(valueStr, valueType); // Limitation: Only works for basic types stored as strings
                    addMethod?.Invoke(dict, new[] { key, value });
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse dictionary entry: {ex.Message}");
                }
            }
        }

        return dict;
    }

    /// <summary>
    /// 反序列化 Tuple 或 ValueTuple。
    /// </summary>
    /// <param name="node">XML节点</param>
    /// <param name="isValueTuple">是否为ValueTuple</param>
    /// <returns>反序列化的Tuple对象</returns>
    private object DeserializeTuple(XmlNode node, bool isValueTuple)
    {
        try
        {
            // 收集元素类型
            var elementTypes = new List<Type>();
            int i = 1;
            while (true)
            {
                string typeAttrName = $"elementType{i}";
                string typeName = node.Attributes?[typeAttrName]?.Value;
                if (string.IsNullOrEmpty(typeName))
                    break;

                Type elementType = GetTypeFromName(typeName);
                if (elementType == null)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Cannot resolve element type '{typeName}' for tuple element {i}");
                    elementType = typeof(object); // 回退到object
                }
                elementTypes.Add(elementType);
                i++;
            }

            if (elementTypes.Count == 0)
            {
                throw new Exception("No element types found for tuple");
            }

            // 创建Tuple类型
            Type tupleType;
            if (isValueTuple)
            {
                // ValueTuple有特定的创建方式
                switch (elementTypes.Count)
                {
                    case 1: tupleType = typeof(ValueTuple<>); break;
                    case 2: tupleType = typeof(ValueTuple<,>); break;
                    case 3: tupleType = typeof(ValueTuple<,,>); break;
                    case 4: tupleType = typeof(ValueTuple<,,,>); break;
                    case 5: tupleType = typeof(ValueTuple<,,,,>); break;
                    case 6: tupleType = typeof(ValueTuple<,,,,,>); break;
                    case 7: tupleType = typeof(ValueTuple<,,,,,,>); break;
                    case 8: tupleType = typeof(ValueTuple<,,,,,,,>); break;
                    default: throw new Exception($"Unsupported ValueTuple arity: {elementTypes.Count}");
                }
            }
            else
            {
                // Tuple
                switch (elementTypes.Count)
                {
                    case 1: tupleType = typeof(Tuple<>); break;
                    case 2: tupleType = typeof(Tuple<,>); break;
                    case 3: tupleType = typeof(Tuple<,,>); break;
                    case 4: tupleType = typeof(Tuple<,,,>); break;
                    case 5: tupleType = typeof(Tuple<,,,,>); break;
                    case 6: tupleType = typeof(Tuple<,,,,,>); break;
                    case 7: tupleType = typeof(Tuple<,,,,,,>); break;
                    case 8: tupleType = typeof(Tuple<,,,,,,,>); break;
                    default: throw new Exception($"Unsupported Tuple arity: {elementTypes.Count}");
                }
            }

            Type concreteTupleType = tupleType.MakeGenericType(elementTypes.ToArray());

            // 收集元素值
            var elementValues = new List<object>();
            for (int j = 1; j <= elementTypes.Count; j++)
            {
                string itemName = $"Item{j}";
                XmlNode itemNode = FindNodeByName(node, itemName);

                if (itemNode != null)
                {
                    object itemValue = DeserializeNode(itemNode);
                    elementValues.Add(itemValue);
                }
                else
                {
                    // 如果没有找到节点，使用默认值
                    elementValues.Add(GetDefaultValue(elementTypes[j - 1]));
                }
            }

            // 创建Tuple实例
            object tuple = Activator.CreateInstance(concreteTupleType, elementValues.ToArray());
            return tuple;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed to deserialize {(isValueTuple ? "ValueTuple" : "Tuple")}: {ex.Message}");

            // 对于ValueTuple，返回默认的ValueTuple<string, string>("", "")
            if (isValueTuple)
            {
                return ValueTuple.Create("", "");
            }

            // 对于普通Tuple，返回null
            return null;
        }
    }

    /// <summary>
    /// 反序列化一个自定义类（Class）。
    /// 从子节点中重建对象的字段和属性。
    /// </summary>
    private object DeserializeClass(XmlNode node)
    {
        string typeName = node.Attributes?["type"]?.Value;
        if (string.IsNullOrEmpty(typeName))
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "Class node is missing 'type' attribute.");
            return null;
        }

        Type type = Type.GetType(typeName);
        if (type == null)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Cannot resolve type '{typeName}' for deserialization.");
            return null;
        }

        // 创建该类型的实例
        object instance;
        try
        {
            instance = Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed to create instance of type '{typeName}': {ex.Message}");
            return null;
        }

        // 获取所有公共的实例字段和属性，用于后续赋值
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var fieldMap = fields.ToDictionary(f => f.Name, f => f);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertyMap = properties.ToDictionary(p => p.Name, p => p);

        // 遍历 <Class> 的所有子节点
        foreach (XmlNode childNode in node.ChildNodes)
        {
            if (childNode.NodeType != XmlNodeType.Element) continue;

            string valueName = childNode.Attributes?["name"]?.Value;
            if (string.IsNullOrEmpty(valueName)) continue;

            object deserializedValue = DeserializeNode(childNode);

            // 尝试匹配字段
            if (fieldMap.TryGetValue(valueName, out var field))
            {
                try
                {
                    // 检查反序列化值的类型是否与字段类型兼容
                    if (deserializedValue == null || field.FieldType.IsAssignableFrom(deserializedValue.GetType()) || IsAssignableToGenericType(deserializedValue.GetType(), field.FieldType))
                    {
                        field.SetValue(instance, deserializedValue);
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized value type '{deserializedValue.GetType().Name}' is not assignable to field '{valueName}' of type '{field.FieldType.Name}' in class '{type.Name}'.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to set field '{valueName}' on class '{type.Name}': {ex.Message}");
                }
            }
            // 如果不是字段，则尝试匹配属性
            else if (propertyMap.TryGetValue(valueName, out var prop))
            {
                // 检查属性是否有 setter
                if (prop.SetMethod != null)
                {
                    try
                    {
                        // 检查反序列化值的类型是否与属性类型兼容
                        if (deserializedValue == null || prop.PropertyType.IsAssignableFrom(deserializedValue.GetType()) || IsAssignableToGenericType(deserializedValue.GetType(), prop.PropertyType))
                        {
                            prop.SetValue(instance, deserializedValue);
                        }
                        else
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized value type '{deserializedValue.GetType().Name}' is not assignable to property '{valueName}' of type '{prop.PropertyType.Name}' in class '{type.Name}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to set property '{valueName}' on class '{type.Name}': {ex.Message}");
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"Property '{valueName}' on class '{type.Name}' has no setter, skipping.");
                }
            }
            else
            {
                // 既不是字段也不是属性
                Logger.Log(LogLevel.Verbose, "ChroniaHelper", $"No public field or property named '{valueName}' found in class '{type.Name}'. Skipping this value.");
            }
        }

        return instance;
    }

    /// <summary>
    /// 反序列化一个基本类型的值（int, float, bool, string, etc.）。
    /// </summary>
    private object DeserializeValue(XmlNode node)
    {
        string text = node.InnerText;
        string typeName = node.Attributes?["type"]?.Value ?? "string";

        // Try to get the target type from the node's name (which is the member name)
        Type targetType = null;
        if (_memberCache.TryGetValue(node.Attributes?["name"]?.Value, out var cacheEntry))
        {
            targetType = GetMemberType(cacheEntry.memberInfo);
        }

        // First, try to parse using the target type if available
        if (targetType != null)
        {
            try
            {
                return ParseBasicType(text, targetType);
            }
            catch
            {
                // Fall back to type attribute
            }
        }

        // Fall back to type attribute
        return typeName.ToLowerInvariant() switch
        {
            "int32" => int.Parse(text),
            "single" => float.Parse(text),
            "double" => double.Parse(text),
            "boolean" => bool.Parse(text),
            "string" => text,
            "datetime" => DateTime.Parse(text),
            "biginteger" => BigInteger.Parse(text),
            _ => text // Default fallback
        };
    }

    // ======== 工具方法 (Helper Methods) ========

    /// <summary>
    /// 判断类型是否为Tuple类型。
    /// </summary>
    private bool IsTupleType(Type type)
    {
        if (!type.IsGenericType) return false;

        Type genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(Tuple<>) ||
               genericType == typeof(Tuple<,>) ||
               genericType == typeof(Tuple<,,>) ||
               genericType == typeof(Tuple<,,,>) ||
               genericType == typeof(Tuple<,,,,>) ||
               genericType == typeof(Tuple<,,,,,>) ||
               genericType == typeof(Tuple<,,,,,,>) ||
               genericType == typeof(Tuple<,,,,,,,>);
    }

    /// <summary>
    /// 判断类型是否为ValueTuple类型。
    /// </summary>
    private bool IsValueTupleType(Type type)
    {
        if (!type.IsGenericType) return false;

        Type genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(ValueTuple<>) ||
               genericType == typeof(ValueTuple<,>) ||
               genericType == typeof(ValueTuple<,,>) ||
               genericType == typeof(ValueTuple<,,,>) ||
               genericType == typeof(ValueTuple<,,,,>) ||
               genericType == typeof(ValueTuple<,,,,,>) ||
               genericType == typeof(ValueTuple<,,,,,,>) ||
               genericType == typeof(ValueTuple<,,,,,,,>);
    }

    /// <summary>
    /// 根据名称在父节点中查找子节点。
    /// </summary>
    private XmlNode FindNodeByName(XmlNode parentNode, string name)
    {
        foreach (XmlNode child in parentNode.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                XmlAttribute nameAttr = child.Attributes?["name"];
                if (nameAttr != null && nameAttr.Value == name)
                {
                    return child;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 从 MemberInfo 获取其值。
    /// </summary>
    private object GetMemberValue(MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.GetValue(this),
            PropertyInfo prop => prop.GetValue(this),
            _ => throw new ArgumentException("Unsupported MemberInfo type.")
        };
    }

    /// <summary>
    /// 设置 MemberInfo 的值。
    /// </summary>
    private void SetMemberValue(MemberInfo member, object value)
    {
        try
        {
            switch (member)
            {
                case FieldInfo field:
                    field.SetValue(this, value);
                    break;
                case PropertyInfo prop:
                    prop.SetValue(this, value);
                    break;
                default:
                    throw new ArgumentException("Unsupported MemberInfo type.");
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to set value for '{member.Name}': {e.Message}");
        }
    }

    /// <summary>
    /// 获取 MemberInfo 的类型。
    /// </summary>
    private Type GetMemberType(MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo prop => prop.PropertyType,
            _ => throw new ArgumentException("Unsupported MemberInfo type.")
        };
    }

    /// <summary>
    /// 判断类型是否为基本类型或字符串。
    /// </summary>
    private bool IsPrimitiveOrString(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
    }

    /// <summary>
    /// 获取用于 XML 属性的类型名称。
    /// </summary>
    private string GetTypeNameForAttribute(Type type)
    {
        if (type == typeof(int)) return "int32";
        if (type == typeof(float)) return "single";
        if (type == typeof(double)) return "double";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(string)) return "string";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(DateTime)) return "datetime";
        return type.Name.ToLowerInvariant();
    }

    /// <summary>
    /// 根据名称解析 Type 对象。
    /// </summary>
    private Type GetTypeFromName(string typeName)
    {
        Type type = Type.GetType(typeName);
        if (type != null) return type;

        type = Type.GetType($"System.{typeName}");
        if (type != null) return type;

        type = GetType().Assembly.GetType(typeName);
        return type;
    }

    /// <summary>
    /// 将字符串解析为指定类型的基本值。
    /// </summary>
    private object ParseBasicType(string text, Type targetType)
    {
        if (string.IsNullOrEmpty(text)) return GetDefaultValue(targetType);
        if (targetType == typeof(string)) return text;

        try
        {
            if (targetType == typeof(int) || targetType == typeof(int?)) return int.Parse(text);
            if (targetType == typeof(long) || targetType == typeof(long?)) return long.Parse(text);
            if (targetType == typeof(float) || targetType == typeof(float?)) return float.Parse(text);
            if (targetType == typeof(double) || targetType == typeof(double?)) return double.Parse(text);
            if (targetType == typeof(bool) || targetType == typeof(bool?)) return bool.Parse(text);
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?)) return DateTime.Parse(text);
            if (targetType == typeof(BigInteger)) return BigInteger.Parse(text);
            if (targetType == typeof(decimal) || targetType == typeof(decimal?)) return decimal.Parse(text);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to parse '{text}' to '{targetType.Name}': {ex.Message}");
        }

        return GetDefaultValue(targetType);
    }

    /// <summary>
    /// 获取类型的默认值。
    /// </summary>
    private object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type baseType = givenType.BaseType;
        if (baseType == null) return false;

        return IsAssignableToGenericType(baseType, genericType);
    }
}