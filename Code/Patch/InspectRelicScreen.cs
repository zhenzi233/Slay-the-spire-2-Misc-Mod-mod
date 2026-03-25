using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi; // 假设使用游戏内的标准按钮类，或者使用 Godot 原生 Button
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens; // NInspectRelicScreen 所在的命名空间
using System.Reflection;
using Test.Code.Relics;

namespace YourModName.Patches;

[HarmonyPatch(typeof(NInspectRelicScreen))]
public static class InspectRelicScreen_ButtonPatch
{
    // 用于存储动态创建的按钮引用，以便在关闭屏幕时移除
    // 使用 WeakReference 或直接存储在实例中可能更好，但这里为了简单演示，我们利用 Godot 节点树查找或静态字典
    // 更稳健的方法是给按钮设置一个独特的 Name，然后在需要时通过 GetNode 查找

    private const string CustomButtonName = "ModCustomActionButton";

    [HarmonyPatch(nameof(NInspectRelicScreen.Open))]
    [HarmonyPostfix]
    public static void Open_Postfix(NInspectRelicScreen __instance, IReadOnlyList<RelicModel> relics, RelicModel relic)
    {
        if (!(relic is Maggot)) return;
        // 获取弹窗容器 (%Popup)
        var popup = __instance.GetNodeOrNull<Control>("%Popup");
        if (popup == null) return;

        // 检查是否已存在旧按钮，如果有，先移除它（确保状态重置）
        var existingButton = popup.GetNodeOrNull(CustomButtonName);
        if (existingButton != null)
        {
            existingButton.QueueFree();
        }

        // 创建新按钮
        var godotButton = new Button();
        godotButton.Name = CustomButtonName;
        godotButton.Text = "我的功能"; // 这里可以改成动态文本

        // 设置字体以匹配游戏风格
        // var font = PreloadManager.Cache.GetAsset<Font>("res://themes/kreon_bold_shared.tres");
        // if (font != null)
        // {
        //     godotButton.AddThemeFontOverride("font", font);
        // }

        // 设置大小和锚点 (右下角)
        godotButton.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
        godotButton.SetOffset(Side.Left, -210);  // 左边距：- (按钮宽 + 间距)
        godotButton.SetOffset(Side.Top, -70);    // 上边距：- (按钮高 + 间距)
        godotButton.SetOffset(Side.Right, -10);  // 右边距
        godotButton.SetOffset(Side.Bottom, -10); // 下边距

        // 添加到弹窗
        popup.AddChild(godotButton);

        // 连接点击事件
        godotButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnCustomButtonPressed(__instance)));

        // 确保按钮可见 (防止之前的状态影响)
        godotButton.Visible = true;
        godotButton.Show();
    }

    [HarmonyPatch(nameof(NInspectRelicScreen.Close))]
    [HarmonyPostfix]
    public static void Close_Postfix(NInspectRelicScreen __instance)
    {
        // 清理按钮，防止下次打开时重复或引用丢失
        var popup = __instance.GetNodeOrNull<Control>("%Popup");
        if (popup != null)
        {
            var existingButton = popup.GetNodeOrNull(CustomButtonName);
            if (existingButton != null)
            {
                existingButton.QueueFree(); // 安全移除
            }
        }
    }

    private static void OnCustomButtonPressed(NInspectRelicScreen screen)
    {
        // 在这里编写你的按钮点击逻辑
        Godot.GD.Print("点击了自定义遗物检视按钮！");

        // 示例：获取当前显示的遗物模型 (需要通过反射访问私有字段 _relics 和 _index)
        // 注意：访问私有字段需要 Harmony 的 AccessTools 或反射
        var type = typeof(NInspectRelicScreen);
        var relicsField = AccessTools.Field(type, "_relics");
        var indexField = AccessTools.Field(type, "_index");

        if (relicsField != null && indexField != null)
        {
            var relics = (System.Collections.Generic.IReadOnlyList<MegaCrit.Sts2.Core.Models.RelicModel>)relicsField.GetValue(screen);
            var index = (int)indexField.GetValue(screen);

            if (relics != null && index >= 0 && index < relics.Count)
            {
                var currentRelic = relics[index];
                Godot.GD.Print($"当前检视的遗物是: {currentRelic.Title}");
                // 在这里执行你的模组逻辑
            }
        }
    }
}