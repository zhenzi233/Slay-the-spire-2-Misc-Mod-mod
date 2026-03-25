using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

// 蛮食
// 另一名玩家选择一张牌消耗（并抽一张牌），你回复等同该牌的费用三倍的血量

[Pool(typeof(ColorlessCardPool))]
public sealed class SavageFeast() : CustomCardModel(1, CardType.Skill, CardRarity.Common, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Multi", 3m)
    ];

    public override string PortraitPath => StringExtensions.WrigglerCard();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var allyP = cardPlay.Target.Player;

        CardModel exhaustedCard = (await CardSelectCmd.FromHand(choiceContext, allyP, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), filter: null, this)).FirstOrDefault();
        if (exhaustedCard == null)
        {
            return;
        }

        var cardCost = 0;
        if (exhaustedCard.EnergyCost.CostsX)
        {
            cardCost = allyP.PlayerCombatState.Energy;
        }
        else
        {
            cardCost = exhaustedCard.EnergyCost.Canonical;
        }

        var healValue = cardCost * 3;

        await CardCmd.Exhaust(choiceContext, exhaustedCard);

        if (base.IsUpgraded)
        {
            await CardPileCmd.Draw(choiceContext, allyP);
        }

        await CreatureCmd.Heal(base.Owner.Creature, (decimal)healValue);
    }

    // 将三张锻造材料+放入所有玩家的抽牌堆。
    protected override void OnUpgrade()
    {
    }
}