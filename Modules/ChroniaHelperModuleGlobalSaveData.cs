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
    private const string DICTIONARY_NODE_NAME = "Dictionary";
    private const string ILIST2_NODE_NAME = "IList2";
    private const string ILIST3_NODE_NAME = "IList3";
    private const string ILIST4_NODE_NAME = "IList4";
    private const string IDICTIONARY2_NODE_NAME = "IDictionary2";
    private const string DICTIONARY_MEMBER_NODE_NAME = "DictionaryMember"; // 用于表示一个键值对
    private const string DICT2_ITEM_NODE_NAME = "Dict2Item"; // 用于包装 Dict2Item 的值
    private const string IDICTIONARY3_NODE_NAME = "IDictionary3";
    private const string IDICTIONARY4_NODE_NAME = "IDictionary4";
    private const string DICT3_ITEM_NODE_NAME = "Dict3Item"; // 用于包装 Dict3Item 的值
    private const string DICT4_ITEM_NODE_NAME = "Dict4Item"; // 用于包装 Dict4Item 的值

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
        return Path.Combine(Everest.PathGame, "Saves", "ChroniaHelper", path);
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
            // IList2
            else if (genericTypeDef == typeof(IList2<,>))
            {
                return SerializeIList2(doc, name, value);
            }
            else if (genericTypeDef == typeof(IList3<,,>))
            {
                return SerializeIList3(doc, name, value);
            }
            else if (genericTypeDef == typeof(IList4<,,,>))
            {
                return SerializeIList4(doc, name, value);
            }
            else if (genericTypeDef == typeof(IDictionary2<,,>))
            {
                return SerializeIDictionary2(doc, name, value);
            }
            else if (genericTypeDef == typeof(IDictionary3<,,,>))
            {
                return SerializeIDictionary3(doc, name, value);
            }
            else if (genericTypeDef == typeof(IDictionary4<,,,,>))
            {
                return SerializeIDictionary4(doc, name, value);
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

    /// <summary>
    /// 序列化一个 List<T> 或 HashSet<T>。
    /// </summary>
    private XmlElement SerializeList(XmlDocument doc, string name, object list)
    {
        var listNode = doc.CreateElement(LIST_NODE_NAME);
        listNode.SetAttribute("name", name);
        Type listType = list.GetType();
        Type elementType = listType.GetGenericArguments()[0];
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
    /// 序列化一个自定义类的实例。
    /// </summary>
    private XmlElement SerializeClass(XmlDocument doc, string name, object obj)
    {
        var classNode = doc.CreateElement(CLASS_NODE_NAME);
        classNode.SetAttribute("name", name);
        classNode.SetAttribute("type", obj.GetType().FullName ?? obj.GetType().Name);

        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.Name.StartsWith("<")) continue; // Skip compiler-generated fields

            var attrNode = doc.CreateElement("Field");
            attrNode.SetAttribute("name", field.Name);
            attrNode.SetAttribute("type", field.FieldType.FullName ?? field.FieldType.Name);

            object fieldValue = field.GetValue(obj);
            if (fieldValue == null)
            {
                attrNode.SetAttribute("isNull", "true");
            }
            else
            {
                // Recursively serialize the field value
                XmlElement fieldValueNode = SerializeValue(doc, field.Name, fieldValue);
                // Import and append the *content* of the serialized value
                XmlNode importedNode = doc.ImportNode(fieldValueNode, true);
                attrNode.AppendChild(importedNode);
            }
            classNode.AppendChild(attrNode);
        }

        return classNode;
    }

    /// <summary>
    /// [修正版] 序列化一个 IList2<T1, T2>。
    /// </summary>
    private XmlElement SerializeIList2(XmlDocument doc, string name, object list)
    {
        var listNode = doc.CreateElement(ILIST2_NODE_NAME);
        listNode.SetAttribute("name", name);
        Type listType = list.GetType();
        Type[] genericArgs = listType.GetGenericArguments();
        Type t1 = genericArgs[0];
        Type t2 = genericArgs[1];
        listNode.SetAttribute("elementType1", t1.Name);
        listNode.SetAttribute("elementType2", t2.Name);

        // --- 关键修正：将 list 视为 IEnumerable 并遍历 ---
        // 因为 IList2<T1, T2> 实现了 IEnumerable<List2Item<T1, T2>>
        if (list is System.Collections.IEnumerable enumerable)
        {
            foreach (var itemObj in enumerable)
            {
                // itemObj 的实际类型是 List2Item<T1, T2>
                if (itemObj != null)
                {
                    Type itemType = itemObj.GetType(); // 获取具体的 List2Item<T1, T2> 类型

                    // 为每个 List2Item 创建一个 Value 节点
                    var itemNode = doc.CreateElement(VALUE_NODE_NAME);
                    itemNode.SetAttribute("name", "Item");
                    itemNode.SetAttribute("type", itemType.Name); // 例如 "List2Item`2"

                    // 使用反射获取 A 和 B 属性的值
                    var propertyA = itemType.GetProperty("A");
                    var propertyB = itemType.GetProperty("B");

                    if (propertyA != null && propertyB != null)
                    {
                        try
                        {
                            object aValue = propertyA.GetValue(itemObj);
                            object bValue = propertyB.GetValue(itemObj);

                            // 序列化 A 和 B 的值
                            var aNode = SerializeValue(doc, "A", aValue);
                            var bNode = SerializeValue(doc, "B", bValue);

                            itemNode.AppendChild(aNode);
                            itemNode.AppendChild(bNode);
                            listNode.AppendChild(itemNode);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error accessing A/B properties of List2Item during serialization: {ex.Message}");
                            // 即使单个项失败，也继续处理下一个
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", "Could not find A or B property on List2Item struct during serialization.");
                    }
                }
            }
        }
        else
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "IList2 instance is not IEnumerable. Cannot serialize items.");
        }
        // --- 修正结束 ---

        return listNode;
    }

    /// <summary>
    /// [修正版] 序列化一个 IList3<T1, T2, T3>。
    /// </summary>
    private XmlElement SerializeIList3(XmlDocument doc, string name, object list)
    {
        var listNode = doc.CreateElement(ILIST3_NODE_NAME);
        listNode.SetAttribute("name", name);
        Type listType = list.GetType();
        Type[] genericArgs = listType.GetGenericArguments();
        Type t1 = genericArgs[0];
        Type t2 = genericArgs[1];
        Type t3 = genericArgs[2];
        listNode.SetAttribute("elementType1", t1.Name);
        listNode.SetAttribute("elementType2", t2.Name);
        listNode.SetAttribute("elementType3", t3.Name);

        // --- 与 IList2 相同的遍历逻辑 ---
        if (list is System.Collections.IEnumerable enumerable)
        {
            foreach (var itemObj in enumerable)
            {
                if (itemObj != null)
                {
                    Type itemType = itemObj.GetType();

                    // 为每个 List3Item 创建一个 Value 节点
                    var itemNode = doc.CreateElement(VALUE_NODE_NAME);
                    itemNode.SetAttribute("name", "Item");
                    itemNode.SetAttribute("type", itemType.Name); // 例如 "List3Item`3"

                    // 使用反射获取 A, B, C 属性的值
                    var propertyA = itemType.GetProperty("A");
                    var propertyB = itemType.GetProperty("B");
                    var propertyC = itemType.GetProperty("C");

                    if (propertyA != null && propertyB != null && propertyC != null)
                    {
                        try
                        {
                            object aValue = propertyA.GetValue(itemObj);
                            object bValue = propertyB.GetValue(itemObj);
                            object cValue = propertyC.GetValue(itemObj);

                            // 序列化 A, B, C 的值
                            var aNode = SerializeValue(doc, "A", aValue);
                            var bNode = SerializeValue(doc, "B", bValue);
                            var cNode = SerializeValue(doc, "C", cValue);

                            itemNode.AppendChild(aNode);
                            itemNode.AppendChild(bNode);
                            itemNode.AppendChild(cNode);
                            listNode.AppendChild(itemNode);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error accessing A/B/C properties of List3Item during serialization: {ex.Message}");
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", "Could not find A, B or C property on List3Item struct during serialization.");
                    }
                }
            }
        }
        else
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "IList3 instance is not IEnumerable. Cannot serialize items.");
        }

        return listNode;
    }

    /// <summary>
    /// [修正版] 序列化一个 IList4<T1, T2, T3, T4>。
    /// </summary>
    private XmlElement SerializeIList4(XmlDocument doc, string name, object list)
    {
        var listNode = doc.CreateElement(ILIST4_NODE_NAME);
        listNode.SetAttribute("name", name);
        Type listType = list.GetType();
        Type[] genericArgs = listType.GetGenericArguments();
        Type t1 = genericArgs[0];
        Type t2 = genericArgs[1];
        Type t3 = genericArgs[2];
        Type t4 = genericArgs[3];
        listNode.SetAttribute("elementType1", t1.Name);
        listNode.SetAttribute("elementType2", t2.Name);
        listNode.SetAttribute("elementType3", t3.Name);
        listNode.SetAttribute("elementType4", t4.Name);

        // --- 与 IList2 相同的遍历逻辑 ---
        if (list is System.Collections.IEnumerable enumerable)
        {
            foreach (var itemObj in enumerable)
            {
                if (itemObj != null)
                {
                    Type itemType = itemObj.GetType();

                    // 为每个 List4Item 创建一个 Value 节点
                    var itemNode = doc.CreateElement(VALUE_NODE_NAME);
                    itemNode.SetAttribute("name", "Item");
                    itemNode.SetAttribute("type", itemType.Name); // 例如 "List4Item`4"

                    // 使用反射获取 A, B, C, D 属性的值
                    var propertyA = itemType.GetProperty("A");
                    var propertyB = itemType.GetProperty("B");
                    var propertyC = itemType.GetProperty("C");
                    var propertyD = itemType.GetProperty("D");

                    if (propertyA != null && propertyB != null && propertyC != null && propertyD != null)
                    {
                        try
                        {
                            object aValue = propertyA.GetValue(itemObj);
                            object bValue = propertyB.GetValue(itemObj);
                            object cValue = propertyC.GetValue(itemObj);
                            object dValue = propertyD.GetValue(itemObj);

                            // 序列化 A, B, C, D 的值
                            var aNode = SerializeValue(doc, "A", aValue);
                            var bNode = SerializeValue(doc, "B", bValue);
                            var cNode = SerializeValue(doc, "C", cValue);
                            var dNode = SerializeValue(doc, "D", dValue);

                            itemNode.AppendChild(aNode);
                            itemNode.AppendChild(bNode);
                            itemNode.AppendChild(cNode);
                            itemNode.AppendChild(dNode);
                            listNode.AppendChild(itemNode);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error accessing A/B/C/D properties of List4Item during serialization: {ex.Message}");
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", "Could not find A, B, C or D property on List4Item struct during serialization.");
                    }
                }
            }
        }
        else
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "IList4 instance is not IEnumerable. Cannot serialize items.");
        }

        return listNode;
    }

    /// <summary>
    /// 序列化一个 IDictionary2<TKey, T1, T2>。
    /// 使用新的通用 Key 序列化方式，并通过稳健的反射处理 Entry (ValueTuple)。
    /// </summary>
    private XmlElement SerializeIDictionary2(XmlDocument doc, string name, object dict)
    {
        var dictNode = doc.CreateElement(IDICTIONARY2_NODE_NAME);
        dictNode.SetAttribute("name", name);

        Type dictType = dict.GetType();
        Type[] genericArgs = dictType.GetGenericArguments();
        Type keyType = genericArgs[0];
        Type valueType1 = genericArgs[1];
        Type valueType2 = genericArgs[2];

        dictNode.SetAttribute("keyType", keyType.Name);
        dictNode.SetAttribute("valueType1", valueType1.Name);
        dictNode.SetAttribute("valueType2", valueType2.Name);

        // 获取 Entries 属性，它返回 IEnumerable<(TKey Key, Dict2Item<T1, T2> Value)>
        var entriesProperty = dictType.GetProperty("Entries");
        if (entriesProperty == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "IDictionary2 does not have an 'Entries' property.");
            return dictNode;
        }

        var entries = entriesProperty.GetValue(dict) as System.Collections.IEnumerable; // 强制转换为非泛型 IEnumerable 以便使用 foreach
        if (entries == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "Failed to get Entries from IDictionary2.");
            return dictNode;
        }

        // --- 使用反射手动枚举 Entries ---
        var enumerator = entries.GetEnumerator();
        if (enumerator == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "Failed to get Enumerator for Entries.");
            return dictNode;
        }

        try
        {
            var moveNextMethod = enumerator.GetType().GetMethod("MoveNext");
            var currentProperty = enumerator.GetType().GetProperty("Current");

            if (moveNextMethod == null || currentProperty == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", "Enumerator does not have MoveNext or Current.");
                return dictNode;
            }

            while ((bool)(moveNextMethod.Invoke(enumerator, null) ?? false))
            {
                var entryObj = currentProperty.GetValue(enumerator);
                if (entryObj == null) continue;

                Type entryType = entryObj.GetType();

                // --- 安全地获取 Entry 的 Key (Item1) 和 Value (Item2) ---
                // ValueTuple<TKey, TValue> 的元素是公共字段 Item1 和 Item2
                object key = null;
                object dict2ItemValue = null;
                bool entryValid = false;

                // 1. 尝试获取字段 (ValueTuple 默认是字段)
                var keyField = entryType.GetField("Item1");
                var valueField = entryType.GetField("Item2");

                if (keyField != null && valueField != null)
                {
                    key = keyField.GetValue(entryObj);
                    dict2ItemValue = valueField.GetValue(entryObj);
                    entryValid = true;
                }
                else
                {
                    // 2. 如果字段不存在，尝试获取属性 (不太常见，但以防万一)
                    var keyProp = entryType.GetProperty("Item1");
                    var valueProp = entryType.GetProperty("Item2");

                    if (keyProp != null && valueProp != null)
                    {
                        key = keyProp.GetValue(entryObj);
                        dict2ItemValue = valueProp.GetValue(entryObj);
                        entryValid = true;
                    }
                }

                if (!entryValid)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Entry object of type {entryType.FullName} does not have accessible Item1 or Item2 (field or property).");
                    continue; // 跳过无效的 entry
                }

                // --- 序列化 Key 和 Value ---

                // 创建一个 DictionaryMember 节点来包裹这个键值对
                var memberNode = doc.CreateElement(DICTIONARY_MEMBER_NODE_NAME);

                // --- 序列化 Key 作为子 Value 节点 ---
                var keyNode = SerializeValue(doc, "Key", key); // 复用通用序列化
                memberNode.AppendChild(keyNode);

                // --- 序列化 Value (Dict2Item<T1, T2>) ---
                if (dict2ItemValue != null)
                {
                    var dict2ItemNode = doc.CreateElement(DICT2_ITEM_NODE_NAME);

                    // 获取 Dict2Item 的 X 和 Y 属性
                    var propertyX = dict2ItemValue.GetType().GetProperty("X");
                    var propertyY = dict2ItemValue.GetType().GetProperty("Y");

                    if (propertyX != null && propertyY != null)
                    {
                        try
                        {
                            object xValue = propertyX.GetValue(dict2ItemValue);
                            object yValue = propertyY.GetValue(dict2ItemValue);

                            // 序列化 X 和 Y 的值
                            var xNode = SerializeValue(doc, "X", xValue);
                            var yNode = SerializeValue(doc, "Y", yValue);

                            dict2ItemNode.AppendChild(xNode);
                            dict2ItemNode.AppendChild(yNode);

                            // 将 Dict2Item 节点添加到 Member 节点
                            memberNode.AppendChild(dict2ItemNode);

                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error accessing X/Y properties of Dict2Item during serialization: {ex.Message}");
                            // 即使 Dict2Item 序列化出错，也跳过这个条目
                            continue;
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", "Could not find X or Y property on Dict2Item struct during serialization.");
                        // Dict2Item 结构不匹配，跳过这个条目
                        continue;
                    }

                }
                else
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", "Dict2Item value is null for key. Skipping entry.");
                    // Value 为 null，根据策略可以选择跳过或序列化一个特殊标记，这里选择跳过
                    continue;
                }

                // --- 如果 Key 和 Value 都成功序列化，将 Member 节点添加到主字典节点 ---
                dictNode.AppendChild(memberNode);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Unexpected error during IDictionary2 serialization: {ex}");
        }
        finally
        {
            // 确保释放枚举器资源
            (enumerator as IDisposable)?.Dispose();
        }

        return dictNode;
    }

    /// <summary>
    /// 序列化一个 IDictionary3<TKey, T1, T2, T3>。
    /// 结构与 IDictionary2 类似。
    /// </summary>
    private XmlElement SerializeIDictionary3(XmlDocument doc, string name, object dict)
    {
        var dictNode = doc.CreateElement(IDICTIONARY3_NODE_NAME);
        dictNode.SetAttribute("name", name);

        Type dictType = dict.GetType();
        Type[] genericArgs = dictType.GetGenericArguments();
        Type keyType = genericArgs[0];
        Type valueType1 = genericArgs[1];
        Type valueType2 = genericArgs[2];
        Type valueType3 = genericArgs[3];

        dictNode.SetAttribute("keyType", keyType.Name);
        dictNode.SetAttribute("valueType1", valueType1.Name);
        dictNode.SetAttribute("valueType2", valueType2.Name);
        dictNode.SetAttribute("valueType3", valueType3.Name);

        var entriesProperty = dictType.GetProperty("Entries");
        if (entriesProperty == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "IDictionary3 does not have an 'Entries' property.");
            return dictNode;
        }

        var entries = entriesProperty.GetValue(dict) as System.Collections.IEnumerable;
        if (entries == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "Failed to get Entries from IDictionary3.");
            return dictNode;
        }

        var enumerator = entries.GetEnumerator();
        if (enumerator == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "Failed to get Enumerator for Entries (IDictionary3).");
            return dictNode;
        }

        try
        {
            var moveNextMethod = enumerator.GetType().GetMethod("MoveNext");
            var currentProperty = enumerator.GetType().GetProperty("Current");

            if (moveNextMethod == null || currentProperty == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", "Enumerator (IDictionary3) does not have MoveNext or Current.");
                return dictNode;
            }

            while ((bool)(moveNextMethod.Invoke(enumerator, null) ?? false))
            {
                var entryObj = currentProperty.GetValue(enumerator);
                if (entryObj == null) continue;

                Type entryType = entryObj.GetType();

                object key = null;
                object dict3ItemValue = null;
                bool entryValid = false;

                var keyField = entryType.GetField("Item1");
                var valueField = entryType.GetField("Item2");

                if (keyField != null && valueField != null)
                {
                    key = keyField.GetValue(entryObj);
                    dict3ItemValue = valueField.GetValue(entryObj);
                    entryValid = true;
                }
                else
                {
                    var keyProp = entryType.GetProperty("Item1");
                    var valueProp = entryType.GetProperty("Item2");

                    if (keyProp != null && valueProp != null)
                    {
                        key = keyProp.GetValue(entryObj);
                        dict3ItemValue = valueProp.GetValue(entryObj);
                        entryValid = true;
                    }
                }

                if (!entryValid)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Entry object of type {entryType.FullName} does not have accessible Item1 or Item2 (IDictionary3).");
                    continue;
                }

                var memberNode = doc.CreateElement(DICTIONARY_MEMBER_NODE_NAME);
                var keyNode = SerializeValue(doc, "Key", key);
                memberNode.AppendChild(keyNode);

                if (dict3ItemValue != null)
                {
                    var dict3ItemNode = doc.CreateElement(DICT3_ITEM_NODE_NAME);

                    var propertyX = dict3ItemValue.GetType().GetProperty("X");
                    var propertyY = dict3ItemValue.GetType().GetProperty("Y");
                    var propertyZ = dict3ItemValue.GetType().GetProperty("Z");

                    if (propertyX != null && propertyY != null && propertyZ != null)
                    {
                        try
                        {
                            object xValue = propertyX.GetValue(dict3ItemValue);
                            object yValue = propertyY.GetValue(dict3ItemValue);
                            object zValue = propertyZ.GetValue(dict3ItemValue);

                            var xNode = SerializeValue(doc, "X", xValue);
                            var yNode = SerializeValue(doc, "Y", yValue);
                            var zNode = SerializeValue(doc, "Z", zValue);

                            dict3ItemNode.AppendChild(xNode);
                            dict3ItemNode.AppendChild(yNode);
                            dict3ItemNode.AppendChild(zNode);

                            memberNode.AppendChild(dict3ItemNode);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error accessing X/Y/Z properties of Dict3Item during serialization: {ex.Message}");
                            continue;
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", "Could not find X, Y, or Z property on Dict3Item struct during serialization.");
                        continue;
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", "Dict3Item value is null for key. Skipping entry.");
                    continue;
                }

                dictNode.AppendChild(memberNode);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Unexpected error during IDictionary3 serialization: {ex}");
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        return dictNode;
    }

    /// <summary>
    /// 序列化一个 IDictionary4<TKey, T1, T2, T3, T4>。
    /// 结构与 IDictionary2/3 类似。
    /// </summary>
    private XmlElement SerializeIDictionary4(XmlDocument doc, string name, object dict)
    {
        var dictNode = doc.CreateElement(IDICTIONARY4_NODE_NAME);
        dictNode.SetAttribute("name", name);

        Type dictType = dict.GetType();
        Type[] genericArgs = dictType.GetGenericArguments();
        Type keyType = genericArgs[0];
        Type valueType1 = genericArgs[1];
        Type valueType2 = genericArgs[2];
        Type valueType3 = genericArgs[3];
        Type valueType4 = genericArgs[4];

        dictNode.SetAttribute("keyType", keyType.Name);
        dictNode.SetAttribute("valueType1", valueType1.Name);
        dictNode.SetAttribute("valueType2", valueType2.Name);
        dictNode.SetAttribute("valueType3", valueType3.Name);
        dictNode.SetAttribute("valueType4", valueType4.Name);

        var entriesProperty = dictType.GetProperty("Entries");
        if (entriesProperty == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "IDictionary4 does not have an 'Entries' property.");
            return dictNode;
        }

        var entries = entriesProperty.GetValue(dict) as System.Collections.IEnumerable;
        if (entries == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "Failed to get Entries from IDictionary4.");
            return dictNode;
        }

        var enumerator = entries.GetEnumerator();
        if (enumerator == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", "Failed to get Enumerator for Entries (IDictionary4).");
            return dictNode;
        }

        try
        {
            var moveNextMethod = enumerator.GetType().GetMethod("MoveNext");
            var currentProperty = enumerator.GetType().GetProperty("Current");

            if (moveNextMethod == null || currentProperty == null)
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", "Enumerator (IDictionary4) does not have MoveNext or Current.");
                return dictNode;
            }

            while ((bool)(moveNextMethod.Invoke(enumerator, null) ?? false))
            {
                var entryObj = currentProperty.GetValue(enumerator);
                if (entryObj == null) continue;

                Type entryType = entryObj.GetType();

                object key = null;
                object dict4ItemValue = null;
                bool entryValid = false;

                var keyField = entryType.GetField("Item1");
                var valueField = entryType.GetField("Item2");

                if (keyField != null && valueField != null)
                {
                    key = keyField.GetValue(entryObj);
                    dict4ItemValue = valueField.GetValue(entryObj);
                    entryValid = true;
                }
                else
                {
                    var keyProp = entryType.GetProperty("Item1");
                    var valueProp = entryType.GetProperty("Item2");

                    if (keyProp != null && valueProp != null)
                    {
                        key = keyProp.GetValue(entryObj);
                        dict4ItemValue = valueProp.GetValue(entryObj);
                        entryValid = true;
                    }
                }

                if (!entryValid)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Entry object of type {entryType.FullName} does not have accessible Item1 or Item2 (IDictionary4).");
                    continue;
                }

                var memberNode = doc.CreateElement(DICTIONARY_MEMBER_NODE_NAME);
                var keyNode = SerializeValue(doc, "Key", key);
                memberNode.AppendChild(keyNode);

                if (dict4ItemValue != null)
                {
                    var dict4ItemNode = doc.CreateElement(DICT4_ITEM_NODE_NAME);

                    var propertyX = dict4ItemValue.GetType().GetProperty("X");
                    var propertyY = dict4ItemValue.GetType().GetProperty("Y");
                    var propertyZ = dict4ItemValue.GetType().GetProperty("Z");
                    var propertyW = dict4ItemValue.GetType().GetProperty("W");

                    if (propertyX != null && propertyY != null && propertyZ != null && propertyW != null)
                    {
                        try
                        {
                            object xValue = propertyX.GetValue(dict4ItemValue);
                            object yValue = propertyY.GetValue(dict4ItemValue);
                            object zValue = propertyZ.GetValue(dict4ItemValue);
                            object wValue = propertyW.GetValue(dict4ItemValue);

                            var xNode = SerializeValue(doc, "X", xValue);
                            var yNode = SerializeValue(doc, "Y", yValue);
                            var zNode = SerializeValue(doc, "Z", zValue);
                            var wNode = SerializeValue(doc, "W", wValue);

                            dict4ItemNode.AppendChild(xNode);
                            dict4ItemNode.AppendChild(yNode);
                            dict4ItemNode.AppendChild(zNode);
                            dict4ItemNode.AppendChild(wNode);

                            memberNode.AppendChild(dict4ItemNode);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Error accessing X/Y/Z/W properties of Dict4Item during serialization: {ex.Message}");
                            continue;
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", "Could not find X, Y, Z, or W property on Dict4Item struct during serialization.");
                        continue;
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", "Dict4Item value is null for key. Skipping entry.");
                    continue;
                }

                dictNode.AppendChild(memberNode);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Unexpected error during IDictionary4 serialization: {ex}");
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        return dictNode;
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
            case ILIST2_NODE_NAME:
                return DeserializeIList2(node);
            case ILIST3_NODE_NAME:
                return DeserializeIList3(node);
            case ILIST4_NODE_NAME:
                return DeserializeIList4(node);
            case IDICTIONARY2_NODE_NAME:
                return DeserializeIDictionary2(node);
            case IDICTIONARY3_NODE_NAME:
                return DeserializeIDictionary3(node);
            case IDICTIONARY4_NODE_NAME:
                return DeserializeIDictionary4(node);
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
        Type listType = typeof(List<>).MakeGenericType(elementType);
        var list = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add");

        foreach (XmlNode itemNode in node.ChildNodes)
        {
            object item = DeserializeNode(itemNode);
            addMethod?.Invoke(list, new[] { item });
        }

        return list;
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
    /// 反序列化一个自定义类的实例。
    /// </summary>
    private object DeserializeClass(XmlNode node)
    {
        string typeName = node.Attributes?["type"]?.Value;
        if (string.IsNullOrEmpty(typeName)) return new object();

        Type type = GetTypeFromName(typeName) ?? GetType().Assembly.GetType(typeName) ?? typeof(object);
        if (type == typeof(object)) return new object();

        var obj = Activator.CreateInstance(type);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        XmlNodeList attrNodes = node.SelectNodes("Field");
        if (attrNodes != null) // 检查 SelectNodes 是否返回了 null
        {
            foreach (XmlNode attrNode in attrNodes)
            {
                string fieldName = attrNode.Attributes?["name"]?.Value;
                if (string.IsNullOrEmpty(fieldName)) continue;

                var field = fields.FirstOrDefault(f => f.Name == fieldName);
                if (field == null) continue;

                if (attrNode.Attributes?["isNull"]?.Value == "true")
                {
                    field.SetValue(obj, null);
                }
                else
                {
                    // Find the first child node that represents the value
                    XmlNode valueContentNode = attrNode.FirstChild;
                    if (valueContentNode != null)
                    {
                        object value = DeserializeNode(valueContentNode);
                        field.SetValue(obj, value);
                    }
                }
            }
        }

        return obj;
    }

    /// <summary>
    /// [修正版] 反序列化一个 IList2<T1, T2>。
    /// </summary>
    private object DeserializeIList2(XmlNode node)
    {
        string t1Name = node.Attributes?["elementType1"]?.Value ?? "object";
        string t2Name = node.Attributes?["elementType2"]?.Value ?? "object";
        Type t1Type = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2Type = GetTypeFromName(t2Name) ?? typeof(object);

        // 构造具体的 IList2<T1, T2> 类型
        Type list2Type = typeof(IList2<,>).MakeGenericType(t1Type, t2Type);
        var list2 = Activator.CreateInstance(list2Type);

        // 获取 IList2 的 Add 方法 (void Add(T1 a, T2 b))
        var addMethod = list2Type.GetMethod("Add", new Type[] { t1Type, t2Type });
        if (addMethod == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"IList2<{t1Type.Name},{t2Type.Name}> does not have an Add(T1, T2) method.");
            return list2; // 返回空列表
        }

        // --- 关键修正：正确解析 Item 节点并调用 Add 方法 ---
        // 遍历所有子节点，它们应该是 <Value name="Item" ...> ...
        foreach (XmlNode itemNode in node.ChildNodes)
        {
            if (itemNode.Name == VALUE_NODE_NAME && itemNode.Attributes?["name"]?.Value == "Item")
            {
                object aValue = null;
                object bValue = null;
                bool aFound = false, bFound = false;

                // 遍历 Item 节点的子节点，寻找 A 和 B
                foreach (XmlNode fieldNode in itemNode.ChildNodes)
                {
                    string fieldName = fieldNode.Attributes?["name"]?.Value;
                    if (fieldName == "A")
                    {
                        aValue = DeserializeNode(fieldNode);
                        aFound = true;
                    }
                    else if (fieldName == "B")
                    {
                        bValue = DeserializeNode(fieldNode);
                        bFound = true;
                    }
                }

                if (aFound && bFound)
                {
                    try
                    {
                        // 确保类型匹配后再调用 Add
                        if ((aValue == null || t1Type.IsAssignableFrom(aValue.GetType()) || IsAssignableToGenericType(aValue.GetType(), t1Type)) &&
                            (bValue == null || t2Type.IsAssignableFrom(bValue.GetType()) || IsAssignableToGenericType(bValue.GetType(), t2Type)))
                        {
                            // 调用 Add(T1 a, T2 b)
                            addMethod.Invoke(list2, new object[] { aValue, bValue });
                        }
                        else
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized types ({aValue?.GetType().Name}, {bValue?.GetType().Name}) do not match expected types ({t1Type.Name}, {t2Type.Name}) for IList2 item.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to add item to IList2: {ex.Message}");
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", "Incomplete List2Item found during deserialization.");
                }
            }
        }
        // --- 修正结束 ---

        return list2;
    }

    /// <summary>
    /// [修正版] 反序列化一个 IList3<T1, T2, T3>。
    /// </summary>
    private object DeserializeIList3(XmlNode node)
    {
        string t1Name = node.Attributes?["elementType1"]?.Value ?? "object";
        string t2Name = node.Attributes?["elementType2"]?.Value ?? "object";
        string t3Name = node.Attributes?["elementType3"]?.Value ?? "object";
        Type t1Type = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2Type = GetTypeFromName(t2Name) ?? typeof(object);
        Type t3Type = GetTypeFromName(t3Name) ?? typeof(object);

        // 构造具体的 IList3<T1, T2, T3> 类型
        Type list3Type = typeof(IList3<,,>).MakeGenericType(t1Type, t2Type, t3Type);
        var list3 = Activator.CreateInstance(list3Type);

        // 获取 IList3 的 Add 方法 (void Add(T1 a, T2 b, T3 c))
        var addMethod = list3Type.GetMethod("Add", new Type[] { t1Type, t2Type, t3Type });
        if (addMethod == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"IList3<{t1Type.Name},{t2Type.Name},{t3Type.Name}> does not have an Add(T1, T2, T3) method.");
            return list3; // 返回空列表
        }

        // --- 与 IList2 相同的解析逻辑 ---
        foreach (XmlNode itemNode in node.ChildNodes)
        {
            if (itemNode.Name == VALUE_NODE_NAME && itemNode.Attributes?["name"]?.Value == "Item")
            {
                object aValue = null;
                object bValue = null;
                object cValue = null;
                bool aFound = false, bFound = false, cFound = false;

                foreach (XmlNode fieldNode in itemNode.ChildNodes)
                {
                    string fieldName = fieldNode.Attributes?["name"]?.Value;
                    if (fieldName == "A")
                    {
                        aValue = DeserializeNode(fieldNode);
                        aFound = true;
                    }
                    else if (fieldName == "B")
                    {
                        bValue = DeserializeNode(fieldNode);
                        bFound = true;
                    }
                    else if (fieldName == "C")
                    {
                        cValue = DeserializeNode(fieldNode);
                        cFound = true;
                    }
                }

                if (aFound && bFound && cFound)
                {
                    try
                    {
                        // 确保类型匹配
                        if ((aValue == null || t1Type.IsAssignableFrom(aValue.GetType()) || IsAssignableToGenericType(aValue.GetType(), t1Type)) &&
                            (bValue == null || t2Type.IsAssignableFrom(bValue.GetType()) || IsAssignableToGenericType(bValue.GetType(), t2Type)) &&
                            (cValue == null || t3Type.IsAssignableFrom(cValue.GetType()) || IsAssignableToGenericType(cValue.GetType(), t3Type)))
                        {
                            addMethod.Invoke(list3, new object[] { aValue, bValue, cValue });
                        }
                        else
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized types ({aValue?.GetType().Name}, {bValue?.GetType().Name}, {cValue?.GetType().Name}) do not match expected types ({t1Type.Name}, {t2Type.Name}, {t3Type.Name}) for IList3 item.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to add item to IList3: {ex.Message}");
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", "Incomplete List3Item found during deserialization.");
                }
            }
        }

        return list3;
    }

    /// <summary>
    /// [修正版] 反序列化一个 IList4<T1, T2, T3, T4>。
    /// </summary>
    private object DeserializeIList4(XmlNode node)
    {
        string t1Name = node.Attributes?["elementType1"]?.Value ?? "object";
        string t2Name = node.Attributes?["elementType2"]?.Value ?? "object";
        string t3Name = node.Attributes?["elementType3"]?.Value ?? "object";
        string t4Name = node.Attributes?["elementType4"]?.Value ?? "object";
        Type t1Type = GetTypeFromName(t1Name) ?? typeof(object);
        Type t2Type = GetTypeFromName(t2Name) ?? typeof(object);
        Type t3Type = GetTypeFromName(t3Name) ?? typeof(object);
        Type t4Type = GetTypeFromName(t4Name) ?? typeof(object);

        // 构造具体的 IList4<T1, T2, T3, T4> 类型
        Type list4Type = typeof(IList4<,,,>).MakeGenericType(t1Type, t2Type, t3Type, t4Type);
        var list4 = Activator.CreateInstance(list4Type);

        // 获取 IList4 的 Add 方法 (void Add(T1 a, T2 b, T3 c, T4 d))
        var addMethod = list4Type.GetMethod("Add", new Type[] { t1Type, t2Type, t3Type, t4Type });
        if (addMethod == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"IList4<{t1Type.Name},{t2Type.Name},{t3Type.Name},{t4Type.Name}> does not have an Add(T1, T2, T3, T4) method.");
            return list4; // 返回空列表
        }

        // --- 与 IList2 相同的解析逻辑 ---
        foreach (XmlNode itemNode in node.ChildNodes)
        {
            if (itemNode.Name == VALUE_NODE_NAME && itemNode.Attributes?["name"]?.Value == "Item")
            {
                object aValue = null;
                object bValue = null;
                object cValue = null;
                object dValue = null;
                bool aFound = false, bFound = false, cFound = false, dFound = false;

                foreach (XmlNode fieldNode in itemNode.ChildNodes)
                {
                    string fieldName = fieldNode.Attributes?["name"]?.Value;
                    if (fieldName == "A")
                    {
                        aValue = DeserializeNode(fieldNode);
                        aFound = true;
                    }
                    else if (fieldName == "B")
                    {
                        bValue = DeserializeNode(fieldNode);
                        bFound = true;
                    }
                    else if (fieldName == "C")
                    {
                        cValue = DeserializeNode(fieldNode);
                        cFound = true;
                    }
                    else if (fieldName == "D")
                    {
                        dValue = DeserializeNode(fieldNode);
                        dFound = true;
                    }
                }

                if (aFound && bFound && cFound && dFound)
                {
                    try
                    {
                        // 确保类型匹配
                        if ((aValue == null || t1Type.IsAssignableFrom(aValue.GetType()) || IsAssignableToGenericType(aValue.GetType(), t1Type)) &&
                            (bValue == null || t2Type.IsAssignableFrom(bValue.GetType()) || IsAssignableToGenericType(bValue.GetType(), t2Type)) &&
                            (cValue == null || t3Type.IsAssignableFrom(cValue.GetType()) || IsAssignableToGenericType(cValue.GetType(), t3Type)) &&
                            (dValue == null || t4Type.IsAssignableFrom(dValue.GetType()) || IsAssignableToGenericType(dValue.GetType(), t4Type)))
                        {
                            addMethod.Invoke(list4, new object[] { aValue, bValue, cValue, dValue });
                        }
                        else
                        {
                            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized types ({aValue?.GetType().Name}, {bValue?.GetType().Name}, {cValue?.GetType().Name}, {dValue?.GetType().Name}) do not match expected types ({t1Type.Name}, {t2Type.Name}, {t3Type.Name}, {t4Type.Name}) for IList4 item.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to add item to IList4: {ex.Message}");
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", "Incomplete List4Item found during deserialization.");
                }
            }
        }

        return list4;
    }

    /// <summary>
    /// 反序列化一个 IDictionary2<TKey, T1, T2>。
    /// 使用新的通用 Key 反序列化方式。
    /// </summary>
    private object DeserializeIDictionary2(XmlNode node)
    {
        string keyTypeName = node.Attributes?["keyType"]?.Value ?? "object";
        string valueType1Name = node.Attributes?["valueType1"]?.Value ?? "object";
        string valueType2Name = node.Attributes?["valueType2"]?.Value ?? "object";

        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(object);
        Type valueType1 = GetTypeFromName(valueType1Name) ?? typeof(object);
        Type valueType2 = GetTypeFromName(valueType2Name) ?? typeof(object);

        // 构造具体的 IDictionary2<TKey, T1, T2> 类型
        Type dict2Type = typeof(IDictionary2<,,>).MakeGenericType(keyType, valueType1, valueType2);
        var dict2 = Activator.CreateInstance(dict2Type);

        // 获取 IDictionary2 的 Add 方法 (void Add(TKey key, T1 x, T2 y))
        var addMethod = dict2Type.GetMethod("Add", new Type[] { keyType, valueType1, valueType2 });
        if (addMethod == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"IDictionary2<{keyType.Name},{valueType1.Name},{valueType2.Name}> does not have an Add(TKey, T1, T2) method.");
            return dict2; // 返回空字典
        }

        // 选择所有 <DictionaryMember> 节点
        XmlNodeList memberNodes = node.SelectNodes(DICTIONARY_MEMBER_NODE_NAME);
        if (memberNodes == null)
        {
            Logger.Log(LogLevel.Verbose, "ChroniaHelper", "No DictionaryMember nodes found in IDictionary2.");
            return dict2;
        }

        foreach (XmlNode memberNode in memberNodes)
        {
            object key = null;
            object xValue = null;
            object yValue = null;
            bool keyFound = false, xFound = false, yFound = false;

            // 遍历 DictionaryMember 的子节点
            foreach (XmlNode childNode in memberNode.ChildNodes)
            {
                if (childNode.NodeType != XmlNodeType.Element) continue;

                string childName = childNode.Name;
                string valueName = childNode.Attributes?["name"]?.Value;

                if (childName == VALUE_NODE_NAME && valueName == "Key")
                {
                    key = DeserializeNode(childNode); // 使用通用的 DeserializeNode
                    keyFound = true;
                }
                else if (childName == DICT2_ITEM_NODE_NAME)
                {
                    // 在 Dict2Item 节点中查找 X 和 Y
                    foreach (XmlNode fieldNode in childNode.ChildNodes)
                    {
                        if (fieldNode.NodeType != XmlNodeType.Element) continue;
                        string fieldName = fieldNode.Attributes?["name"]?.Value;
                        if (fieldName == "X")
                        {
                            xValue = DeserializeNode(fieldNode);
                            xFound = true;
                        }
                        else if (fieldName == "Y")
                        {
                            yValue = DeserializeNode(fieldNode);
                            yFound = true;
                        }
                    }
                }
            }


            if (keyFound && xFound && yFound)
            {
                try
                {
                    // 确保类型匹配
                    if ((key == null || keyType.IsAssignableFrom(key.GetType()) || IsAssignableToGenericType(key.GetType(), keyType)) &&
                        (xValue == null || valueType1.IsAssignableFrom(xValue.GetType()) || IsAssignableToGenericType(xValue.GetType(), valueType1)) &&
                        (yValue == null || valueType2.IsAssignableFrom(yValue.GetType()) || IsAssignableToGenericType(yValue.GetType(), valueType2)))
                    {
                        // 调用 Add(TKey key, T1 x, T2 y)
                        addMethod.Invoke(dict2, new object[] { key, xValue, yValue });
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized types (Key:{key?.GetType().Name}, X:{xValue?.GetType().Name}, Y:{yValue?.GetType().Name}) do not match expected types (Key:{keyType.Name}, X:{valueType1.Name}, Y:{valueType2.Name}) for IDictionary2 entry.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to add entry to IDictionary2: {ex.Message}");
                }
            }
            else
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", "Incomplete DictionaryMember found during deserialization (missing Key, X, or Y).");
            }
        }

        return dict2;
    }

    /// <summary>
    /// 反序列化一个 IDictionary3<TKey, T1, T2, T3>。
    /// </summary>
    private object DeserializeIDictionary3(XmlNode node)
    {
        string keyTypeName = node.Attributes?["keyType"]?.Value ?? "object";
        string valueType1Name = node.Attributes?["valueType1"]?.Value ?? "object";
        string valueType2Name = node.Attributes?["valueType2"]?.Value ?? "object";
        string valueType3Name = node.Attributes?["valueType3"]?.Value ?? "object";

        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(object);
        Type valueType1 = GetTypeFromName(valueType1Name) ?? typeof(object);
        Type valueType2 = GetTypeFromName(valueType2Name) ?? typeof(object);
        Type valueType3 = GetTypeFromName(valueType3Name) ?? typeof(object);

        Type dict3Type = typeof(IDictionary3<,,,>).MakeGenericType(keyType, valueType1, valueType2, valueType3);
        var dict3 = Activator.CreateInstance(dict3Type);

        var addMethod = dict3Type.GetMethod("Add", new Type[] { keyType, valueType1, valueType2, valueType3 });
        if (addMethod == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"IDictionary3<{keyType.Name},{valueType1.Name},{valueType2.Name},{valueType3.Name}> does not have an Add(TKey, T1, T2, T3) method.");
            return dict3;
        }

        XmlNodeList memberNodes = node.SelectNodes(DICTIONARY_MEMBER_NODE_NAME);
        if (memberNodes == null)
        {
            Logger.Log(LogLevel.Verbose, "ChroniaHelper", "No DictionaryMember nodes found in IDictionary3.");
            return dict3;
        }

        foreach (XmlNode memberNode in memberNodes)
        {
            object key = null;
            object xValue = null;
            object yValue = null;
            object zValue = null;
            bool keyFound = false, xFound = false, yFound = false, zFound = false;

            foreach (XmlNode childNode in memberNode.ChildNodes)
            {
                if (childNode.NodeType != XmlNodeType.Element) continue;

                string childName = childNode.Name;
                string valueName = childNode.Attributes?["name"]?.Value;

                if (childName == VALUE_NODE_NAME && valueName == "Key")
                {
                    key = DeserializeNode(childNode);
                    keyFound = true;
                }
                else if (childName == DICT3_ITEM_NODE_NAME)
                {
                    foreach (XmlNode fieldNode in childNode.ChildNodes)
                    {
                        if (fieldNode.NodeType != XmlNodeType.Element) continue;
                        string fieldName = fieldNode.Attributes?["name"]?.Value;
                        if (fieldName == "X")
                        {
                            xValue = DeserializeNode(fieldNode);
                            xFound = true;
                        }
                        else if (fieldName == "Y")
                        {
                            yValue = DeserializeNode(fieldNode);
                            yFound = true;
                        }
                        else if (fieldName == "Z")
                        {
                            zValue = DeserializeNode(fieldNode);
                            zFound = true;
                        }
                    }
                }
            }

            if (keyFound && xFound && yFound && zFound)
            {
                try
                {
                    if ((key == null || keyType.IsAssignableFrom(key.GetType()) || IsAssignableToGenericType(key.GetType(), keyType)) &&
                        (xValue == null || valueType1.IsAssignableFrom(xValue.GetType()) || IsAssignableToGenericType(xValue.GetType(), valueType1)) &&
                        (yValue == null || valueType2.IsAssignableFrom(yValue.GetType()) || IsAssignableToGenericType(yValue.GetType(), valueType2)) &&
                        (zValue == null || valueType3.IsAssignableFrom(zValue.GetType()) || IsAssignableToGenericType(zValue.GetType(), valueType3)))
                    {
                        addMethod.Invoke(dict3, new object[] { key, xValue, yValue, zValue });
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized types (Key:{key?.GetType()?.Name}, X:{xValue?.GetType()?.Name}, Y:{yValue?.GetType()?.Name}, Z:{zValue?.GetType()?.Name}) do not match expected types (Key:{keyType.Name}, X:{valueType1.Name}, Y:{valueType2.Name}, Z:{valueType3.Name}) for IDictionary3 entry.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to add entry to IDictionary3: {ex.Message}");
                }
            }
            else
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", "Incomplete DictionaryMember found during deserialization for IDictionary3 (missing Key, X, Y, or Z).");
            }
        }

        return dict3;
    }

    /// <summary>
    /// 反序列化一个 IDictionary4<TKey, T1, T2, T3, T4>。
    /// </summary>
    private object DeserializeIDictionary4(XmlNode node)
    {
        string keyTypeName = node.Attributes?["keyType"]?.Value ?? "object";
        string valueType1Name = node.Attributes?["valueType1"]?.Value ?? "object";
        string valueType2Name = node.Attributes?["valueType2"]?.Value ?? "object";
        string valueType3Name = node.Attributes?["valueType3"]?.Value ?? "object";
        string valueType4Name = node.Attributes?["valueType4"]?.Value ?? "object";

        Type keyType = GetTypeFromName(keyTypeName) ?? typeof(object);
        Type valueType1 = GetTypeFromName(valueType1Name) ?? typeof(object);
        Type valueType2 = GetTypeFromName(valueType2Name) ?? typeof(object);
        Type valueType3 = GetTypeFromName(valueType3Name) ?? typeof(object);
        Type valueType4 = GetTypeFromName(valueType4Name) ?? typeof(object);

        Type dict4Type = typeof(IDictionary4<,,,,>).MakeGenericType(keyType, valueType1, valueType2, valueType3, valueType4);
        var dict4 = Activator.CreateInstance(dict4Type);

        var addMethod = dict4Type.GetMethod("Add", new Type[] { keyType, valueType1, valueType2, valueType3, valueType4 });
        if (addMethod == null)
        {
            Logger.Log(LogLevel.Warn, "ChroniaHelper", $"IDictionary4<{keyType.Name},{valueType1.Name},{valueType2.Name},{valueType3.Name},{valueType4.Name}> does not have an Add(TKey, T1, T2, T3, T4) method.");
            return dict4;
        }

        XmlNodeList memberNodes = node.SelectNodes(DICTIONARY_MEMBER_NODE_NAME);
        if (memberNodes == null)
        {
            Logger.Log(LogLevel.Verbose, "ChroniaHelper", "No DictionaryMember nodes found in IDictionary4.");
            return dict4;
        }

        foreach (XmlNode memberNode in memberNodes)
        {
            object key = null;
            object xValue = null;
            object yValue = null;
            object zValue = null;
            object wValue = null;
            bool keyFound = false, xFound = false, yFound = false, zFound = false, wFound = false;

            foreach (XmlNode childNode in memberNode.ChildNodes)
            {
                if (childNode.NodeType != XmlNodeType.Element) continue;

                string childName = childNode.Name;
                string valueName = childNode.Attributes?["name"]?.Value;

                if (childName == VALUE_NODE_NAME && valueName == "Key")
                {
                    key = DeserializeNode(childNode);
                    keyFound = true;
                }
                else if (childName == DICT4_ITEM_NODE_NAME)
                {
                    foreach (XmlNode fieldNode in childNode.ChildNodes)
                    {
                        if (fieldNode.NodeType != XmlNodeType.Element) continue;
                        string fieldName = fieldNode.Attributes?["name"]?.Value;
                        if (fieldName == "X")
                        {
                            xValue = DeserializeNode(fieldNode);
                            xFound = true;
                        }
                        else if (fieldName == "Y")
                        {
                            yValue = DeserializeNode(fieldNode);
                            yFound = true;
                        }
                        else if (fieldName == "Z")
                        {
                            zValue = DeserializeNode(fieldNode);
                            zFound = true;
                        }
                        else if (fieldName == "W")
                        {
                            wValue = DeserializeNode(fieldNode);
                            wFound = true;
                        }
                    }
                }
            }

            if (keyFound && xFound && yFound && zFound && wFound)
            {
                try
                {
                    if ((key == null || keyType.IsAssignableFrom(key.GetType()) || IsAssignableToGenericType(key.GetType(), keyType)) &&
                        (xValue == null || valueType1.IsAssignableFrom(xValue.GetType()) || IsAssignableToGenericType(xValue.GetType(), valueType1)) &&
                        (yValue == null || valueType2.IsAssignableFrom(yValue.GetType()) || IsAssignableToGenericType(yValue.GetType(), valueType2)) &&
                        (zValue == null || valueType3.IsAssignableFrom(zValue.GetType()) || IsAssignableToGenericType(zValue.GetType(), valueType3)) &&
                        (wValue == null || valueType4.IsAssignableFrom(wValue.GetType()) || IsAssignableToGenericType(wValue.GetType(), valueType4)))
                    {
                        addMethod.Invoke(dict4, new object[] { key, xValue, yValue, zValue, wValue });
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Deserialized types (Key:{key?.GetType()?.Name}, X:{xValue?.GetType()?.Name}, Y:{yValue?.GetType()?.Name}, Z:{zValue?.GetType()?.Name}, W:{wValue?.GetType()?.Name}) do not match expected types (Key:{keyType.Name}, X:{valueType1.Name}, Y:{valueType2.Name}, Z:{valueType3.Name}, W:{valueType4.Name}) for IDictionary4 entry.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "ChroniaHelper", $"Failed to add entry to IDictionary4: {ex.Message}");
                }
            }
            else
            {
                Logger.Log(LogLevel.Warn, "ChroniaHelper", "Incomplete DictionaryMember found during deserialization for IDictionary4 (missing Key, X, Y, Z, or W).");
            }
        }

        return dict4;
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