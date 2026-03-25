using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Rare;

// 白嫖
// 使另一名玩家的牌堆顶中的一张随机的牌添加至你手牌中。

[Pool(typeof(RegentCardPool))]
public sealed class Freeloading() : CustomCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Upgraded", 0)
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var owner = cardPlay.Card.Owner;
        var allyC = cardPlay.Target;
        var allyP = allyC.Player;
        if (allyP == null)
        {
            return;
        }
        var allyCs = allyP.PlayerCombatState;
        if (allyCs == null)
        {
            return;
        }
        var allyCp = allyCs.AllCards;

        var list = allyCp.ToList();
        if (list.Count == 0)
        {
            return;
        }

        var card = Owner.RunState.Rng.CombatCardSelection.NextItem<CardModel>(list);
        if (card == null) return;
        var newCard = owner.Creature.CombatState.CreateCard(card, base.Owner);
        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(newCard, CardPreviewStyle.None);
        }
        CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, true, CardPilePosition.Bottom);
        if (LocalContext.IsMe(owner))
        {
            CardCmd.PreviewCardPileAdd(result);
        }
    }

    // 使另一名玩家的牌堆顶中的一张随机的牌添加至你手牌中并升级。
    protected override void OnUpgrade()
    {

    }
}