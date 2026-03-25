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

// 感染
// 群攻，五伤，如果伤害未被格挡，为其附加三层中毒

[Pool(typeof(ColorlessCardPool))]
public sealed class Infect() : CustomCardModel(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    // public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new PowerVar<PoisonPower>(3m)
    ];

    public override string PortraitPath => StringExtensions.WrigglerCard();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var owner = cardPlay.Card.Owner;
        var combatState = cardPlay.Card.CombatState;
        var attackResult = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);

        foreach (var result in attackResult.Results)
        {
            if (result.UnblockedDamage > 0)
            {
                await PowerCmd.Apply<PoisonPower>(result.Receiver, base.DynamicVars["PoisonPower"].BaseValue, base.Owner.Creature, this);
            }
        }
    }

// 将三张锻造材料+放入所有玩家的抽牌堆。
    protected override void OnUpgrade()
    {
        DynamicVars["PoisonPower"].UpgradeValueBy(2m);
    }
}