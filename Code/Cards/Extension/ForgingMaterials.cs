using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Extension;

// 锻造材料
// 消耗。抽两张牌。
 
[Pool(typeof(RegentCardPool))]
public sealed class ForgingMaterials() : CustomCardModel(0, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    public override HashSet<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, cardPlay.Card.Owner);
    }
// 抽三张牌。
    protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}

    public static IEnumerable<ForgingMaterials> Create(Player owner, int amount, CombatState combatState, bool Upgraded)
	{
		List<ForgingMaterials> list = new List<ForgingMaterials>();
		for (int i = 0; i < amount; i++)
		{
            var card = combatState.CreateCard<ForgingMaterials>(owner);
            if (Upgraded)
            {
                CardCmd.Upgrade(card);
            }
			list.Add(card);
		}
		return list;
	}


}