using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace ChroniaHelper.Settings;

[WorkingInProgress]
public class EmbeddedChoice
{
    public struct DialogPair
    {
        public string PromptText;
        public string TargetText;
    }
    
    // [LoadHook]
    public static void Load()
    {
        On.Celeste.Textbox.RunRoutine += BeforeRunRoutine;
        IL.Celeste.FancyText.Parse += FancyTextParse;
        On.Celeste.Textbox.Close += AfterClosed;
    }
    // [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Textbox.RunRoutine -= BeforeRunRoutine;
        IL.Celeste.FancyText.Parse -= FancyTextParse;
        On.Celeste.Textbox.Close -= AfterClosed;
    }

    public static IEnumerator BeforeRunRoutine(On.Celeste.Textbox.orig_RunRoutine orig, Textbox self)
    {
        // Create textbox session here
        
        return orig(self);
    }

    public static void FancyTextParse(ILContext il)
    {
        ILCursor cur = new(il);

        if (cur.TryGotoNext(MoveType.Before,
                ins => ins.MatchLdcR4(0f),
                ins => ins.MatchStloc(out int n)))
        {
            cur.Emit(OpCodes.Ldloc_S);

            cur.EmitDelegate<Action<List<string>>>(i =>
            {
                // Get string list here for parameters
            });
        
        }

        if (cur.TryGotoNext(MoveType.Before,
                ins => ins.MatchLdloc(out int n),
                ins => ins.MatchLdstr("break"),
                ins => ins.MatchCall<object>("Equals"),
                ins => ins.MatchBrfalse(out ILLabel label)))
        {
            // 移动到 ldloc 之后
            cur.Index++;
    
            // 加载局部变量
            //cur.Emit(OpCodes.Ldloc_S);
    
            // 使用委托处理自定义命令
            cur.EmitDelegate<Action<string>>((string s) =>
            {
                if (s == "chronia_choice")
                {
                    // Store logics here for processing
                }
                else if (s == "chronia_label")
                {
                    // Create a jump label
                }
                else if (s == "chronia_if")
                {
                    // Create a jump point to the label
                }
            });
    
            // 此时执行顺序是：
            // 1. 原 ldloc (加载 s)
            // 2. 的自定义检查 (使用 s)
            // 3. 原 ldstr "break"
            // 4. 原 Equals
            // 5. 原 Brfalse
    
            // EmitDelegate 会消费掉栈上的 s
            // 而原有的 ldstr "break" 需要 s 在栈上才能比较
            // 所以在 EmitDelegate 之后重新加载 s
            cur.Emit(OpCodes.Ldloc_S);
            // 然后继续执行原有的 ldstr "break" 等指令
        }
    }

    public static void AfterClosed(On.Celeste.Textbox.orig_Close orig, Textbox self)
    {
        orig(self);
        
        // Create cutscene here
    }
}
