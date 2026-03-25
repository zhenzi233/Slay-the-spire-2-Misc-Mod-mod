using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;

namespace Test.Code.Patches;

public static class ParasiteTransformPatchesA
{
    // 存储变身期间的临时数据
    private static readonly Dictionary<Creature, TransformData> _activeTransforms = new();

    public class TransformData
    {
        public MonsterModel Model;
        public NCreatureVisuals MonsterVisuals; // 新的怪物视觉
        public CreatureAnimator MonsterAnimator; // 新的怪物动画器
        // 不需要存 PlayerVisuals，因为它本来就在 node 里，Patch 失效后自动会用
    }

    /// <summary>
    /// 启动变身：创建怪物视觉并注册 Patch
    /// </summary>
    public static void StartTransformation(Creature creature, MonsterModel model, NCreatureVisuals monsterVisuals, CreatureAnimator monsterAnimator)
    {
        _activeTransforms[creature] = new TransformData
        {
            Model = model,
            MonsterVisuals = monsterVisuals,
            MonsterAnimator = monsterAnimator
        };
        
        // 关键：将怪物视觉加入场景树，但先隐藏，防止穿模或闪烁
        // 假设调用者已经 add 过了，这里确保它是可见的（通过 Patch 控制逻辑上的可见性）
        // 实际上，只要 Patch 生效，NCreature 就会用它，我们只需确保它在树里
        if (monsterVisuals.GetParent() == null)
        {
             GD.PrintErr("[Harmony] MonsterVisuals not added to tree before StartTransformation!");
        }
        
        GD.Print($"[Harmony] Transformation STARTED for {creature.Name}");
    }

    /// <summary>
    /// 结束变身：仅移除记录，Patch 自动失效，视觉自动切回玩家
    /// </summary>
    public static void EndTransformation(Creature creature)
    {
        if (_activeTransforms.Remove(creature))
        {
            GD.Print($"[Harmony] Transformation ENDED for {creature.Name}. Reverted to default visuals.");
            // 不需要做任何销毁操作！原来的玩家 Visuals 自动生效。
            // 怪物 Visuals 还挂在树上，可以选择不删（等场景重置），或者异步删除
        }
    }

    public static bool IsTransformed(Creature c) => _activeTransforms.ContainsKey(c);
    public static TransformData GetData(Creature c) => _activeTransforms.TryGetValue(c, out var d) ? d : null;

    // ---------------------------------------------------------
    // Patch 1: 拦截 Visuals (核心)
    // ---------------------------------------------------------
    [HarmonyPatch(typeof(NCreature), "get_Visuals")]
    public static class NCreature_GetVisuals_Patch
    {
        public static bool Prefix(NCreature __instance, ref NCreatureVisuals __result)
        {
            if (_activeTransforms.TryGetValue(__instance.Entity, out var data))
            {
                // 只有当怪物视觉有效时才返回
                if (data.MonsterVisuals != null && GodotObject.IsInstanceValid(data.MonsterVisuals))
                {
                    __result = data.MonsterVisuals;
                    return false; // 跳过原方法
                }
            }
            // 如果没有变身数据，或数据无效，执行原方法（返回玩家 Visuals）
            return true;
        }
    }

    // ---------------------------------------------------------
    // Patch 2: 拦截 SpineController
    // ---------------------------------------------------------
    // [HarmonyPatch(typeof(NCreature), "get_SpineController")]
    // public static class NCreature_GetSpineController_Patch
    // {
    //     public static bool Prefix(NCreature __instance, ref GodotObject? __result)
    //     {
    //         if (_activeTransforms.TryGetValue(__instance.Entity, out var data))
    //         {
    //             if (data.MonsterVisuals?.SpineBody != null && GodotObject.IsInstanceValid(data.MonsterVisuals.SpineBody.BoundObject))
    //             {
    //                 __result = data.MonsterVisuals.SpineBody.BoundObject;
    //                 return false;
    //             }
    //         }
    //         return true;
    //     }
    // }

    // ---------------------------------------------------------
    // Patch 3: 拦截 SetAnimationTrigger
    // ---------------------------------------------------------
    [HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
    public static class NCreature_SetAnimationTrigger_Patch
    {
        public static bool Prefix(NCreature __instance, string trigger)
        {
            if (_activeTransforms.TryGetValue(__instance.Entity, out var data))
            {
                if (data.MonsterAnimator != null)
                {
                    data.MonsterAnimator.SetTrigger(trigger);
                    return false;
                }
            }
            return true;
        }
    }
    
    // ---------------------------------------------------------
    // Patch 4: 拦截 VfxSpawnPosition (防止访问旧节点)
    // ---------------------------------------------------------
    [HarmonyPatch(typeof(NCreature), "get_VfxSpawnPosition")]
    public static class NCreature_GetVfxSpawnPosition_Patch
    {
        public static bool Prefix(NCreature __instance, ref Vector2 __result)
        {
            if (_activeTransforms.TryGetValue(__instance.Entity, out var data))
            {
                if (data.MonsterVisuals != null && GodotObject.IsInstanceValid(data.MonsterVisuals))
                {
                    var marker = data.MonsterVisuals.GetNodeOrNull<Marker2D>("VfxSpawnPosition");
                    if (marker != null && GodotObject.IsInstanceValid(marker))
                    {
                        __result = marker.GlobalPosition;
                        return false;
                    }
                }
                // Fallback
                __result = __instance.GlobalPosition;
                return false;
            }
            return true;
        }
    }
}