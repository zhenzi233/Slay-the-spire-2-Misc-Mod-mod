// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using Godot;
// using HarmonyLib;
// using MegaCrit.Sts2.Core.Entities.Creatures;
// using MegaCrit.Sts2.Core.Models.Monsters;
// using MegaCrit.Sts2.Core.Nodes.Combat;
// using MegaCrit.Sts2.Core.Animation;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Bindings.MegaSpine;
// using MegaCrit.Sts2.Core.Helpers;
// using BaseLib;

// public static class ParasiteTransformPatches
// {
//     // 存储变身映射：Creature -> (NewMonsterModel, NewVisuals, NewAnimator)
//     private static readonly Dictionary<Creature, TransformData> _transformedCreatures = new();

//     private class TransformData
//     {
//         public MonsterModel Model;
//         public NCreatureVisuals Visuals;
//         public CreatureAnimator Animator;
//     }

//     /// <summary>
//     /// 注册变身数据。在 AfterPreventingDeath 中调用此方法。
//     /// </summary>
//     public static void RegisterTransformation(Creature creature, MonsterModel model, NCreatureVisuals visuals, CreatureAnimator animator)
//     {
//         _transformedCreatures[creature] = new TransformData
//         {
//             Model = model,
//             Visuals = visuals,
//             Animator = animator
//         };
//         GD.Print($"[Harmony] Registered transformation for {creature.Name}");
//     }

//     /// <summary>
//     /// 清理数据
//     /// </summary>
//     public static void UnregisterTransformation(Creature creature)
//     {
//         if (_transformedCreatures.Remove(creature))
//         {
//             GD.Print($"[Harmony] Unregistered transformation for {creature.Name}. Reverting to default visuals.");
//         }
//     }

//     public static bool IsTransformed(Creature c) => _transformedCreatures.ContainsKey(c);

//     // ---------------------------------------------------------
//     // Patch 1: 拦截 Visuals 属性
//     // 确保任何访问 node.Visuals 的地方都拿到新的对象
//     // ---------------------------------------------------------
//     [HarmonyPatch(typeof(NCreature), "get_Visuals")]
//     public static class NCreature_GetVisuals_Patch
//     {
//         public static bool Prefix(NCreature __instance, ref NCreatureVisuals __result)
//         {
//             if (_transformedCreatures.TryGetValue(__instance.Entity, out var data))
//             {
//                 // 检查新对象是否有效
//                 if (data.Visuals != null && GodotObject.IsInstanceValid(data.Visuals))
//                 {
//                     __result = data.Visuals;
//                     return false; // 跳过原方法
//                 }
//             }
//             return true; // 执行原方法
//         }
//     }

//     // ---------------------------------------------------------
//     // Patch 2: 拦截 _spineAnimator 相关逻辑 (通过 SetAnimationTrigger 或直接访问)
//     // 由于 _spineAnimator 是私有字段，我们很难直接 Patch 它的 getter。
//     // 但是我们可以 Patch 使用它的方法，比如 SetAnimationTrigger
//     // 或者更好的方法：Patch NCreature 的构造函数或 _Ready？不，那是实例化时。
//     // 最简单的方法：Patch SetAnimationTrigger，强制使用我们的 Animator
//     // ---------------------------------------------------------
//     [HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
//     public static class NCreature_SetAnimationTrigger_Patch
//     {
//         public static bool Prefix(NCreature __instance, string trigger)
//         {
//             if (_transformedCreatures.TryGetValue(__instance.Entity, out var data))
//             {
//                 if (data.Animator != null)
//                 {
//                     data.Animator.SetTrigger(trigger);
//                     return false; // 跳过原方法（原方法会使用旧的 _spineAnimator）
//                 }
//             }
//             return true;
//         }
//     }
    
//     // 如果需要访问 SpineController (MegaSprite)，也可以 Patch 对应的 getter
//     [HarmonyPatch(typeof(NCreature), "get_SpineController")]
//     public static class NCreature_GetSpineController_Patch
//     {
//         public static bool Prefix(NCreature __instance, ref MegaSprite? __result)
//         {
//             if (_transformedCreatures.TryGetValue(__instance.Entity, out var data))
//             {
//                 if (data.Visuals?.SpineBody != null && GodotObject.IsInstanceValid(data.Visuals.SpineBody.BoundObject))
//                 {
//                     __result = data.Visuals.SpineBody;
//                     return false;
//                 }
//             }
//             return true;
//         }
//     }

//     // ---------------------------------------------------------
//     // Patch 3: 拦截 UpdateIntent (让变身怪显示意图)
//     // ---------------------------------------------------------
//     // ---------------------------------------------------------
//     // Patch 4: 拦截 VfxSpawnPosition (防止访问已销毁的 Marker2D)
//     // ---------------------------------------------------------
//     [HarmonyPatch(typeof(NCreature), "get_VfxSpawnPosition")]
//     public static class NCreature_GetVfxSpawnPosition_Patch
//     {
//         public static bool Prefix(NCreature __instance, ref Vector2 __result)
//         {
//             // 检查是否是变身过的生物
//             if (ParasiteTransformPatches._transformedCreatures.TryGetValue(__instance.Entity, out var data))
//             {
//                 // 1. 检查 Visuals 容器是否有效
//                 if (data.Visuals != null && GodotObject.IsInstanceValid(data.Visuals))
//                 {
//                     // 2. 尝试获取 Marker2D 节点
//                     var marker = data.Visuals.GetNodeOrNull<Marker2D>("VfxSpawnPosition");
                    
//                     // 3. 【关键修复】必须同时检查 marker 不为 null 且 实例有效！
//                     if (marker != null && GodotObject.IsInstanceValid(marker))
//                     {
//                         __result = marker.GlobalPosition;
//                         return false; // 跳过原方法，成功返回
//                     }
                    
//                     // 如果 marker 无效（可能刚被销毁或还没添加），不要尝试访问它的属性
//                     // 而是 fallback 到一个安全的位置，防止崩溃
//                     MainFile.Logger.Warn($"[Harmony] VfxSpawnPosition marker invalid for {__instance.Entity.Name}. Using fallback position.");
//                     __result = __instance.GlobalPosition; // fallback 到生物中心
//                     return false;
//                 }
                
//                 // 如果 Visuals 无效，也 fallback
//                 MainFile.Logger.Warn($"[Harmony] Visuals invalid for {__instance.Entity.Name}. Using fallback position.");
//                 __result = __instance.GlobalPosition;
//                 return false;
//             }

//             // 如果不是变身生物，执行原方法
//             return true;
//         }
//     }
// }