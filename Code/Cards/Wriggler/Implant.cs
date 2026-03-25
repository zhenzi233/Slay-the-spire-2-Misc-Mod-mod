using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.CardPools;
using Test.Code.Cards.Extension;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Wriggler;

// 植入
// 一费。另一名玩家获得一点能量。随机升级手中一张牌。

[Pool(typeof(ColorlessCardPool))]
public sealed class Implant() : CustomCardModel(1, CardType.Skill, CardRarity.Common, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new DynamicVar("Card", 1)
    ];

    public override HashSet<CardKeyword> CanonicalKeywords =>
    [
    ];

    public override string PortraitPath => StringExtensions.WrigglerCard();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var allyP = cardPlay.Target.Player;

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, allyP);
        
        IReadOnlyList<CardModel> cards = base.Owner.PlayerCombatState.Hand.Cards;
        var canUpgradeCards = new List<CardModel>();
        foreach (CardModel card in cards)
        {
            if (!card.IsUpgraded)
            {
                canUpgradeCards.Add(card);
            }
        }
        if (canUpgradeCards.Count == 0)
        {
            return;
        }

        IEnumerable<CardModel> enumerable = canUpgradeCards.ToList().UnstableShuffle(base.Owner.RunState.Rng.CombatCardSelection).Take(1);

        foreach (CardModel card in enumerable)
		{
			if (card.IsUpgradable)
			{
				CardCmd.Upgrade(card);
			}
		}
    }

// 将三张锻造材料+放入所有玩家的抽牌堆。
    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}