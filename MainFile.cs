using System.Reflection;
using BaseLib.Config;
using Godot;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using Test.Code.Config;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace Test;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Test"; //At the moment, this is used only for the Logger and harmony names.

    public static Logger Logger { get; } =
        new(ModId, LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        ModConfigRegistry.Register(ModId, new TestConfig());

        var assembly = Assembly.GetExecutingAssembly();

        ScriptManagerBridge.LookupScriptsInAssembly(assembly);

        harmony.PatchAll();
    }
}