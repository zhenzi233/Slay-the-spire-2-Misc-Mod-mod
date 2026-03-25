using HarmonyLib;
using System;
using System.Reflection;
// 假设这是游戏的核心命名空间，根据实际项目结构调整
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using Test.Code.Relics;
using MegaCrit.Sts2.Core.Commands;
using Test.Code.Config;

namespace Test.Code.Patches;

public static class GameStartPatchs
{
    [HarmonyPatch(typeof(Player), "PopulateStartingInventory")]
    public class PlayerStartExtraRelicPatch
    {
        // Postfix 表示在原始方法执行后运行
        // __instance 代表当前的 Player 对象
        [HarmonyPostfix]
        public static void AddExtraRelic(Player __instance)
        {
            try
            {
                if (!TestConfig.EnableWrigglerRelic) return;
                RelicCmd.Obtain<Maggot>(__instance);
            }
            catch (Exception ex)
            {
                MainFile.Logger.Info($"[MyMod] Error adding extra relic: {ex.Message}");
            }
        }
    }
}