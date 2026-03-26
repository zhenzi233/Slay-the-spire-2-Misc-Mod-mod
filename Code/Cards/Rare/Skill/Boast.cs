using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Cards.Extension;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Rare.Skill;

// 吹牛
// 消耗。眩晕你的队友和所有敌人。

[Pool(typeof(ColorlessCardPool))]
public sealed class Boast() : CustomCardModel(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.SummonDynamic, base.DynamicVars.Summon)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        var enemies = CombatState.Enemies;
        var allies = CombatState.Allies;
        foreach (var enemy in enemies)
        {
            await CreatureCmd.Stun(enemy);
        }

        foreach (var ally in allies)
        {
            PlayerCmd.EndTurn(ally.Player, false);
        }
        
    }

    // 另一名玩家召唤等同你血量一半的数值。
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}