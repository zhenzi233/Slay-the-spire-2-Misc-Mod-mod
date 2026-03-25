using System.Numerics;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Test.Code.Cards.Common;
using Test.Code.Cards.Wriggler;
using Test.Code.Config;
using Test.Code.Extensions;
using Test.Code.Patches;

namespace Test.Code.Relics;

// 战斗开始时，若你的生命值低于5。一张自刎归天放入你手牌里。

[Pool(typeof(EventRelicPool))]
public sealed class Maggot : CustomRelicModel
{
    public enum Direction
    {
        Right,
        Left
    }

    public override RelicRarity Rarity => RelicRarity.Rare;

    private bool died = false;

    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    private Direction _facing;
    private decimal maxHp;

    public Direction Facing
    {
        get
        {
            return _facing;
        }
        private set
        {
            AssertMutable();
            _facing = value;
        }
    }

    public string vfxPath = "vfx/common/vfx_smoke_flipbook";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Die", 0m)
    ];

    public override async Task AfterObtained()
    {
        maxHp = base.Owner.Creature.MaxHp;
    }

    public override bool ShouldDie(Creature creature)
    {
        GD.Print(died);
        if (creature != base.Owner.Creature)
        {
            return true;
        }
        if (creature == base.Owner.Creature && died)
        {
            return true;
        }
        maxHp = base.Owner.Creature.MaxHp;
        died = true;
        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        GD.Print($"[Maggot] Death prevented. Executing transformation with Harmony...");

        var player = base.Owner;
        var configHp = TestConfig.WrigglerHp;
        var configchangeHp = (int)TestConfig.WrigglerHpWave;
        var changeHp = player.PlayerRng.Transformations.NextInt(-configchangeHp, configchangeHp);
        var MaxHp = (decimal)Math.Max(configHp + changeHp, 1);
        await CreatureCmd.SetMaxHp(player.Creature, MaxHp);
        await CreatureCmd.Heal(player.Creature, MaxHp);

        VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, vfxPath);

        
        var newMonster = ModelDb.Monster<Wriggler>().ToMutable();
        TransformDeadPlayerToMonster(creature, newMonster);

        await Cmd.CustomScaledWait(0.15f, 0.25f);

        CardModel[] cards = new CardPile[5]
        {
            player.PlayerCombatState.Hand,
            player.PlayerCombatState.DrawPile,
            player.PlayerCombatState.DiscardPile,
            player.PlayerCombatState.ExhaustPile,
            player.PlayerCombatState.PlayPile
        }.SelectMany((CardPile p) => p.Cards).ToArray();
        await CardPileCmd.RemoveFromCombat(cards);
        await PlayerCmd.SetStars(0m, player);

        await Cmd.CustomScaledWait(0.15f, 0.25f);

        List<CardModel> list = new List<CardModel>();
        AddCards<Squirm>(list, 4);
        AddCards<SavageFeast>(list, 4);
        AddCards<Infect>(list, 4);
        AddCards<Implant>(list, 4);
        AddCards<Gnaw>(list, 4);

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
        if (LocalContext.IsMe(base.Owner.Creature))
        {
            CardCmd.PreviewCardPileAdd(results);
        }
    }

    public override async Task AfterCombatVictoryEarly(CombatRoom room)
    {
        if (!died) return;

        died = false;
        await CreatureCmd.SetMaxHp(base.Owner.Creature, maxHp);

        if (ParasiteTransformPatchesA.IsTransformed(base.Owner.Creature))
        {
            GD.Print("[Maggot] Combat ended. Reverting player model...");

            var data = ParasiteTransformPatchesA.GetData(base.Owner.Creature);

            VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, vfxPath);

            // 【关键】只需调用这一行！
            ParasiteTransformPatchesA.EndTransformation(base.Owner.Creature);

            // Patch 失效后：
            // 1. node.get_Visuals 自动返回原来的玩家 Visuals (它一直在树里，只是之前被隐藏了)
            // 2. node.get_SpineController 自动返回玩家的 Spine
            // 3. node.SetAnimationTrigger 自动调用玩家的 Animator

            // 你只需要做一些善后工作：
            NCreature node = NCombatRoom.Instance?.GetCreatureNode(base.Owner.Creature);
            if (node != null && data != null && data.MonsterVisuals != null)
            {
                if (GodotObject.IsInstanceValid(data.MonsterVisuals))
                {
                    Node parent = data.MonsterVisuals.GetParent();
                    if (parent != null)
                    {
                        parent.RemoveChild(data.MonsterVisuals);
                        GD.Print($"[Maggot] Removed monster visuals from tree.");
                    }

                    // 标记为待销毁 (Godot 会在帧结束时真正释放内存)
                    data.MonsterVisuals.QueueFreeSafely();
                    GD.Print($"[Maggot] Monster visuals queued for deletion.");
                }

                // A. 显示玩家 Visuals (如果之前被隐藏了)
                if (node.Visuals != null)
                {
                    node.Visuals.Visible = true;
                    node.MoveChild(node.Visuals, 0); // 移到顶层
                    node.Visuals.Scale = Godot.Vector2.One; // 重置朝向
                }

                // B. 清理怪物 Visuals (可选，也可以留着等场景重置)
                // 如果想立即清理：
                // var data = ParasiteTransformPatchesA.GetData(base.Owner.Creature); 
                // if (data.MonsterVisuals != null) { node.RemoveChild(data.MonsterVisuals); data.MonsterVisuals.QueueFreeSafely(); }

                // C. 重建 OrbManager (因为变身时删了)
                if (node.OrbManager == null)
                {
                    var orbManager = NOrbManager.Create(node, LocalContext.IsMe(base.Owner.Creature));
                    if (orbManager != null) node.AddChildSafely(orbManager);
                }

                // D. 刷新一下血条和碰撞箱 (确保基于玩家模型重新计算)
                // 触发一次 UpdateBounds
                var method = typeof(NCreature).GetMethod("UpdateBounds", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Node) }, null);
                if (node.Visuals != null) method?.Invoke(node, new object[] { node.Visuals });

                // 刷新血条
                // ... (调用之前的刷新血条逻辑) ...
            }
        }
    }
    private async void TransformDeadPlayerToMonster(Creature creature, MonsterModel newMonster)
    {

        // 获取节点
        NCreature node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node == null) return;

        // 【关键修改】不再销毁 node.Visuals！
        // 让它留在场景树里，只是会被 Patch 覆盖掉，不会被访问到。
        // 如果担心层级问题，可以把它的 Visible 设为 false，或者 MoveChild 到最底层
        if (node.Visuals != null)
        {
            node.Visuals.Visible = false; // 隐藏旧视觉，防止穿模
            node.MoveChild(node.Visuals, node.GetChildCount() - 1); // 移到底层
        }

        // 2. 创建新怪物 Visuals
        var monsterVisuals = newMonster.CreateVisuals();
        if (monsterVisuals == null) return;

        node.AddChildSafely(monsterVisuals);
        node.MoveChild(monsterVisuals, 0); // 放在最上层
        monsterVisuals.Position = Godot.Vector2.Zero;
        monsterVisuals.Visible = true; // 确保新视觉可见

        // 确保 VfxSpawnPosition 存在
        if (monsterVisuals.GetNodeOrNull<Marker2D>("VfxSpawnPosition") == null)
        {
            var vfxPos = new Marker2D();
            vfxPos.Name = "VfxSpawnPosition";
            monsterVisuals.AddChild(vfxPos);
        }

        var monsterAnimator = newMonster.GenerateAnimator(monsterVisuals.SpineBody);
        monsterVisuals.SetUpSkin(newMonster);

        // 4. 【关键】注册到 Harmony
        // 从现在开始，任何对 node.Visuals, node.SpineController, node.SetAnimationTrigger 的调用
        // 都会自动指向我们刚创建的 monsterVisuals 和 monsterAnimator
        ParasiteTransformPatchesA.StartTransformation(creature, newMonster, monsterVisuals, monsterAnimator);

        // 5. 设置朝向 & 更新碰撞箱
        // 注意：UpdateBounds 内部会访问 Visuals，现在它会通过 Harmony 拿到新的 Visuals
        await FaceDirection(Direction.Left, monsterVisuals.Body);

        // 手动调用一次 UpdateBounds 以确保 Hitbox 立即更新
        // 由于我们 Patch 了 get_Visuals，这里调用 node.UpdateBounds(node.Visuals) 会自动使用新对象
        // 但 UpdateBounds 是 private，我们需要反射调用，或者直接触发信号
        // 简单做法：手动触发一下 BoundsUpdated 信号（如果 Animator 连接了的话），或者反射调用
        var method = typeof(NCreature).GetMethod("UpdateBounds",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null, new Type[] { typeof(Node) }, null);
        method?.Invoke(node, new object[] { monsterVisuals });

        // 6. 播放动画
        if (monsterAnimator.HasTrigger("Spawn"))
            monsterAnimator.SetTrigger("Spawn"); // 这会触发 Patched 的 SetAnimationTrigger
        else
            monsterAnimator.SetTrigger("Idle");

        // 7. UI 修正
        node.ToggleIsInteractable(true);

        fixHitbox(node, monsterVisuals);

        // 7. 强制更新布局
        // healthBar.ForceUpdateLayout();
        // hitbox.ForceUpdateLayout();
    }

    private void fixHitbox(NCreature node, NCreatureVisuals monsterVisuals)
    {
        var stateDisplayField = typeof(NCreature).GetField("_stateDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
        var healthBarField = typeof(NCreatureStateDisplay).GetField("_healthBar", BindingFlags.NonPublic | BindingFlags.Instance);
        var hitboxProp = typeof(NCreature).GetProperty("Hitbox", BindingFlags.Public | BindingFlags.Instance);

        var stateDisplay = stateDisplayField?.GetValue(node) as NCreatureStateDisplay;
        var hitbox = hitboxProp?.GetValue(node) as Control;
        var healthBar = healthBarField?.GetValue(stateDisplay) as NHealthBar;

        if (hitbox != null && healthBar != null && monsterVisuals != null)
        {
            Godot.Vector2 calculatedSize = Godot.Vector2.Zero;
            Godot.Vector2 calculatedPos = node.GlobalPosition;

            // 1. 尝试从 "Bounds" 节点获取大小
            var boundsNode = monsterVisuals.GetNodeOrNull<Control>("Bounds");

            if (boundsNode != null)
            {
                float tempScale = 1f;
                Godot.Vector2 size = boundsNode.Size * monsterVisuals.Scale / tempScale;
                Godot.Vector2 vector = (boundsNode.GlobalPosition - node.GlobalPosition) / tempScale;

                calculatedSize = size;
                calculatedPos = node.GlobalPosition + vector;

                // 【关键检查】如果获取到的宽度为 0，说明模型里没配好 Bounds，使用默认值！
                if (calculatedSize.X <= 0f)
                {
                    GD.Print($"[Maggot] WARNING: Bounds node found but Width is 0! Using fallback size.");
                    // Fallback: 假设怪物标准宽度为 100，高度保持原样或也设为默认
                    calculatedSize = new Godot.Vector2(100f, Math.Max(calculatedSize.Y, 100f));
                }
            }
            else
            {
                // 2. 如果连 "Bounds" 节点都没有，直接使用默认值
                GD.Print($"[Maggot] WARNING: No 'Bounds' node found in monster visuals! Using fallback size.");
                // Fallback: 根据怪物类型或通用默认值
                calculatedSize = new Godot.Vector2(100f, 148f); // 148 是你日志里的高度，宽度给个默认 100
                calculatedPos = node.GlobalPosition;
            }

            // 3. 应用修正后的大小和位置
            hitbox.Size = calculatedSize;
            hitbox.GlobalPosition = calculatedPos;

            GD.Print($"[Maggot] Hitbox Fixed! Final Size: {hitbox.Size}, Pos: {hitbox.GlobalPosition}");

            // 4. 同步 SelectionReticle
            var reticleField = typeof(NCreature).GetField("_selectionReticle", BindingFlags.NonPublic | BindingFlags.Instance);
            var reticle = reticleField?.GetValue(node) as Control;
            if (reticle != null)
            {
                reticle.Size = hitbox.Size;
                reticle.GlobalPosition = hitbox.GlobalPosition;
                reticle.PivotOffset = reticle.Size / 2f;
            }

            // 5. 通知 HealthBar 重新计算布局 (此时 bounds.Size.X 已经是正确的非零值了)
            healthBar.UpdateLayoutForCreatureBounds(hitbox);

            // 6. 刷新血条数值
            healthBar.RefreshValues();
        }
    }

    /// <summary>
    /// 执行实际的变身操作 (视觉替换、字段注入、UI 更新)
    /// </summary>
    // --- 辅助方法 ---
    private void AddCards<T>(List<CardModel> targetList, int count) where T : CardModel
    {
        for (int i = 0; i < count; i++)
        {
            targetList.Add(base.Owner.Creature.CombatState.CreateCard<T>(base.Owner));
        }
    }

    // 来自SurroundedPower.cs 的函数
    private async Task FaceDirection(Direction direction, Node2D body)
    {
        if (body == null)
        {
            return;
        }
        Facing = direction;
        await FlipScale(body);
    }

    private Task FlipScale(Node2D? body)
    {
        if (body == null)
        {
            return Task.CompletedTask;
        }
        float x = body.Scale.X;
        if ((Facing == Direction.Right && x < 0f) || (Facing == Direction.Left && x > 0f))
        {
            body.Scale *= new Godot.Vector2(-1f, 1f);
        }
        return Task.CompletedTask;
    }
}