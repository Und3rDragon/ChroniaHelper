using System;
using System.Collections.Generic;

namespace ChroniaHelper.Utils;

public static class MapDataUtils
{

    public static List<EntityData> GetMapDataEntities(Level level)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, false);
    }

    public static List<EntityData> GetMapDataTriggers(Level level)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, true);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, (entityData) => true, triggers);
    }

    public static List<EntityData> GetMapDataEntities(Level level, string name)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, false);
    }

    public static List<EntityData> GetMapDataTriggers(Level level, string name)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, true);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, string name, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, (entityData) => entityData.Name == name, triggers);
    }

    public static List<EntityData> GetMapDataEntities(Level level, string name, Func<EntityData, bool> predicate)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, predicate, false);
    }

    public static List<EntityData> GetMapDataTriggers(Level level, string name, Func<EntityData, bool> predicate)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, predicate, true);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, string name, Func<EntityData, bool> predicate, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, (entityData) => (entityData.Name == name) && (predicate(entityData)), triggers);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, Func<EntityData, bool> predicate, bool triggers = false)
    {
        List<EntityData> entities = new List<EntityData>();
        foreach (LevelData levelData in level.Session.MapData.Levels)
        {
            List<EntityData> entityDataList = (triggers ? levelData.Triggers : levelData.Entities);
            foreach (EntityData entityData in entityDataList)
            {
                if (predicate(entityData))
                {
                    entities.Add(entityData);
                }
            }
        }
        return entities;
    }

    public static EntityData GetMapDataEntity(Level level, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, index, false);
    }

    public static EntityData GetMapDataTrigger(Level level, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, index, true);
    }

    public static EntityData GetMapDataEntityData(Level level, int index = 0, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityData(level, (entityData) => true, index, triggers);
    }

    public static EntityData GetMapDataEntity(Level level, string name, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, index, false);
    }

    public static EntityData GetMapDataTrigger(Level level, string name, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, index, true);
    }

    public static EntityData GetMapDataEntityData(Level level, string name, int index = 0, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityData(level, (entityData) => entityData.Name == name, index, triggers);
    }

    public static EntityData GetMapDataEntity(Level level, string name, Func<EntityData, bool> predicate, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, predicate, index, false);
    }

    public static EntityData GetMapDataTrigger(Level level, string name, Func<EntityData, bool> predicate, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, predicate, index, true);
    }

    public static EntityData GetMapDataEntityData(Level level, string name, Func<EntityData, bool> predicate, int index = 0, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityData(level, (entityData) => (entityData.Name == name) && (predicate(entityData)), index, triggers);
    }

    public static EntityData GetMapDataEntityData(Level level, Func<EntityData, bool> predicate, int index = 0, bool triggers = false)
    {
        List<EntityData> entityDataList = MapDataUtils.GetMapDataEntityDataList(level, predicate, triggers);
        if (entityDataList.Count == 0)
        {
            return null;
        }
        return entityDataList[(index < 0 || index >= entityDataList.Count) ? (entityDataList.Count - 1) : index];
    }

}
