using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Test.Code.Cards.Common;
using Test.Code.Extensions;

namespace Test.Code.Relics;

// 战斗开始时，若你的生命值低于5。一张自刎归天放入你手牌里。
[Pool(typeof(EventRelicPool))]
public sealed class AllArmiesHeed : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();


    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
		{
			Flash();
			var card = combatState.CreateCard<HeavenlySacrifice>(base.Owner);
            CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true, CardPilePosition.Bottom);
            if (LocalContext.IsMe(base.Owner))
            {
                CardCmd.PreviewCardPileAdd(result);
            }
		}
	}
}