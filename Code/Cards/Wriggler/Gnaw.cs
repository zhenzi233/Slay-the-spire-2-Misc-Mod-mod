using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.CardPools;
using Test.Code.Cards.Extension;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Wriggler;

// 啃咬
// 单攻，造成三点伤害并恢复三点血量

[Pool(typeof(ColorlessCardPool))]
public sealed class Gnaw() : CustomCardModel(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    // public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Heal", 3)
    ];

    public override string PortraitPath => StringExtensions.WrigglerCard();



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var owner = cardPlay.Card.Owner;
        var combatState = cardPlay.Card.CombatState;
        var attackResult = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);

        await CreatureCmd.Heal(base.Owner.Creature, DynamicVars["Heal"].BaseValue);
    }

    // 将三张锻造材料+放入所有玩家的抽牌堆。
    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(2m);
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}