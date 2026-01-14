using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

[ModImportName("SpeedrunTool.SaveLoad")]
public static class SpeedRunToolImports
{
    /// <summary>
    /// Ignore the entities when saving state. They will be removed before saving state and then added into level after loading state.
    /// </summary>
    /// <param name="entity">Ignored entity</param>
    /// <param name="based">The Added/Removed method of the entity will not be triggered when based is true</param>
    public static void IgnoreSaveState(this Entity entity, bool based = false)
    {
        speedrunTool_IgnoreSaveState(entity, based);
    }
    public delegate void SpeedrunTool_IgnoreSaveState(Entity entity, bool based = false);
    public static SpeedrunTool_IgnoreSaveState speedrunTool_IgnoreSaveState;

    /// <summary>
    /// <para> Register SaveLoadAction. </para>
    /// <para> Please save your values into the dictionary, otherwise multi saveslots will not be supported. </para>
    /// <para> DON'T pass in method calls of a singleton instance, use static methods instead. </para>
    /// not recommended to pass in method calls of a singleton
    /// </summary>
    /// <param name="saveState"></param>
    /// <param name="loadState"></param>
    /// <param name="clearState"></param>
    /// <param name="beforeSaveState"></param>
    /// <param name="beforeLoadState"></param>
    /// <param name="preCloneEntities"></param>
    /// <returns>SaveLoadAction instance, used for unregister when your mod unloads </returns>
    public static object RegisterSaveLoadAction(Action<Dictionary<Type, Dictionary<string, object>>, Level> saveState, 
        Action<Dictionary<Type, Dictionary<string, object>>, Level> loadState, Action clearState,
        Action<Level> beforeSaveState, Action<Level> beforeLoadState, Action preCloneEntities)
    {
        return speedrunTool_RegisterSaveLoadAction(saveState, loadState, clearState, beforeSaveState, beforeSaveState, preCloneEntities);
    }
    public delegate object SpeedrunTool_RegisterSaveLoadAction(Action<Dictionary<Type, Dictionary<string, object>>, Level> saveState,
        Action<Dictionary<Type, Dictionary<string, object>>, Level> loadState, Action clearState,
        Action<Level> beforeSaveState, Action<Level> beforeLoadState, Action preCloneEntities);
    public static SpeedrunTool_RegisterSaveLoadAction speedrunTool_RegisterSaveLoadAction;

    /// <summary>
    /// Unregister the SaveLoadAction return from RegisterStaticTypes()/RegisterSaveLoadAction()
    /// </summary>
    /// <param name="obj">The object return from RegisterStaticTypes()/RegisterSaveLoadAction()</param>
    public static void Unregister(this object obj)
    {
        speedrunTool_Unregister(obj);
    }
    public delegate void SpeedrunTool_Unregister(object obj);
    public static SpeedrunTool_Unregister speedrunTool_Unregister;

    /// <summary>
    /// Name of the currently using save slot, appear after v3.25.0
    /// </summary>
    public static string GetSlotName() => speedrunTool_GetSlotName();
    public delegate string SpeedrunTool_GetSlotName();
    public static SpeedrunTool_GetSlotName speedrunTool_GetSlotName;
}
