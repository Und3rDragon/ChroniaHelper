using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics; // 必须引入这个命名空间
using System.Reflection;
using Celeste;
using Celeste.Mod;
using ChroniaHelper.Cores;
using Monocle;

namespace ChroniaHelper.Modules
{
    public abstract class ChroniaHelperModuleGlobalSaveData
    {
        // 缓存：文件绝对路径 -> 该文件需要保存的成员信息字典 (成员名 -> MemberInfo)
        private readonly Dictionary<string, Dictionary<string, MemberInfo>> _saveFileMap = new();

        /// <summary>
        /// 获取默认的数据保存根目录
        /// </summary>
        protected virtual string GetDefaultPath()
        {
            // 默认保存在 Saves/ChroniaHelper/ 目录下
            return Path.Combine(Everest.PathSettings, "ChroniaHelper");
        }

        /// <summary>
        /// 构造函数：在实例化时通过反射扫描所有需要保存的成员并分组
        /// </summary>
        protected ChroniaHelperModuleGlobalSaveData()
        {
            InitializeSaveMap();
        }

        /// <summary>
        /// 初始化保存映射表
        /// </summary>
        private void InitializeSaveMap()
        {
            _saveFileMap.Clear();
            Type type = GetType();
            string defaultDir = GetDefaultPath();
            string defaultFileName = defaultFileName = "ChroniaHelperGlobalSaveData.yaml"; 
            string defaultFilePath = Path.Combine(defaultDir, defaultFileName);

            // 获取所有可读可写的公共实例属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in properties)
            {
                string targetFilePath;
                var attr = prop.GetCustomAttribute<ChroniaGlobalSavePathAttribute>();

                if (attr == null || string.IsNullOrWhiteSpace(attr.RelativePath))
                {
                    // 情况 A 和 B：保存到默认文件
                    targetFilePath = defaultFilePath;
                }
                else
                {
                    // 情况 C：保存到指定路径，并强制修正后缀为 .yaml
                    string cleanPath = Path.ChangeExtension(attr.RelativePath, ".yaml");
                    targetFilePath = Path.Combine(defaultDir, cleanPath);
                }

                if (!_saveFileMap.ContainsKey(targetFilePath))
                {
                    _saveFileMap[targetFilePath] = new Dictionary<string, MemberInfo>();
                }
                _saveFileMap[targetFilePath][prop.Name] = prop;
            }
        }

        /// <summary>
        /// 保存所有数据
        /// </summary>
        public void SaveAll()
        {
            foreach (var fileGroup in _saveFileMap)
            {
                string filePath = fileGroup.Key;
                var members = fileGroup.Value;

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    var dataToSave = new Dictionary<string, object>();
                    foreach (var memberKvp in members)
                    {
                        string memberName = memberKvp.Key;
                        MemberInfo memberInfo = memberKvp.Value;
                        
                        object value = GetMemberValue(memberInfo);

                        // 【特殊处理】BigInteger 序列化
                        // 如果值是 BigInteger，将其转换为字符串存储，防止 YamlDotNet 无法识别或序列化为复杂对象
                        if (value is BigInteger bigIntValue)
                        {
                            dataToSave[memberName] = bigIntValue.ToString();
                        }
                        else
                        {
                            dataToSave[memberName] = value;
                        }
                    }

                    // 使用 YamlHelper 序列化
                    using (var writer = new StreamWriter(filePath))
                    {
                        YamlHelper.Serializer.Serialize(writer, dataToSave);
                    }
                    
                    Logger.Log("ChroniaHelper", $"Saved global data to: {filePath}");
                }
                catch (Exception ex)
                {
                    Logger.Log("ChroniaHelper", $"Failed to save global data to {filePath}: {ex}");
                }
            }
        }

        /// <summary>
        /// 加载所有数据
        /// </summary>
        public void LoadAll()
        {
            foreach (var fileGroup in _saveFileMap)
            {
                string filePath = fileGroup.Key;
                var members = fileGroup.Value;

                if (!File.Exists(filePath))
                {
                    continue;
                }

                try
                {
                    Dictionary<string, object> loadedData;
                    using (var reader = new StreamReader(filePath))
                    {
                        loadedData = YamlHelper.Deserializer.Deserialize<Dictionary<string, object>>(reader);
                    }

                    if (loadedData == null) continue;

                    foreach (var memberKvp in members)
                    {
                        string memberName = memberKvp.Key;
                        MemberInfo memberInfo = memberKvp.Value;

                        if (loadedData.TryGetValue(memberName, out object value))
                        {
                            SetMemberValue(memberInfo, value);
                        }
                    }

                    Logger.Log("ChroniaHelper", $"Loaded global data from: {filePath}");
                }
                catch (Exception ex)
                {
                    Logger.Log("ChroniaHelper", $"Failed to load global data from {filePath}: {ex}");
                }
            }
        }

        private object GetMemberValue(MemberInfo member)
        {
            return member switch
            {
                PropertyInfo prop => prop.GetValue(this),
                FieldInfo field => field.GetValue(this),
                _ => null
            };
        }

        /// <summary>
        /// 设置成员的值，包含类型转换逻辑
        /// </summary>
        private void SetMemberValue(MemberInfo member, object value)
        {
            try
            {
                Type targetType = member switch
                {
                    PropertyInfo prop => prop.PropertyType,
                    FieldInfo field => field.FieldType,
                    _ => null
                };

                if (targetType == null || value == null) return;

                // 【特殊处理】BigInteger 反序列化
                // 如果目标类型是 BigInteger，我们需要手动解析
                if (targetType == typeof(BigInteger))
                {
                    BigInteger parsedValue;
                    if (value is string strValue)
                    {
                        // 如果是字符串（我们在保存时特意转成了字符串），直接解析
                        parsedValue = BigInteger.Parse(strValue);
                    }
                    else
                    {
                        // 如果 YAML 解析器自作聪明把它当成了数字（long/double等），转为字符串后再解析
                        // 这样可以保证精度不丢失
                        parsedValue = BigInteger.Parse(value.ToString());
                    }
                    
                    switch (member)
                    {
                        case PropertyInfo prop:
                            prop.SetValue(this, parsedValue);
                            break;
                        case FieldInfo field:
                            field.SetValue(this, parsedValue);
                            break;
                    }
                    return;
                }

                // 处理常规类型的转换（如 int, float, double 之间的兼容）
                if (value.GetType() != targetType)
                {
                    Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    
                    if (underlyingType.IsEnum)
                    {
                        value = Enum.ToObject(underlyingType, Convert.ToInt32(value));
                    }
                    else
                    {
                        value = Convert.ChangeType(value, underlyingType);
                    }
                }

                switch (member)
                {
                    case PropertyInfo prop:
                        prop.SetValue(this, value);
                        break;
                    case FieldInfo field:
                        field.SetValue(this, value);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("ChroniaHelper", $"Failed to set value for member '{member.Name}': {ex}");
            }
        }
    }
}